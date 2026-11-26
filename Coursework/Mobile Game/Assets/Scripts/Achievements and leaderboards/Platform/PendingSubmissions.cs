// CMP5402 Mobile Development - Week 9: Leaderboards, Achievements & Platform Services
// PendingSubmissions: the offline queue between the game and a platform ("The offline queue" slide).
//
// Every score and every achievement report is written to disk FIRST, then sent when a back end is signed in.
// Sign-in later, airplane mode, a killed app: nothing is lost, and nothing is sent twice in a harmful way, because
// every entry is absolute (a best score, a highest step count, an unlock) rather than a delta:
//   - "score"     keeps the best value per board (highest, or lowest when the board is lowerIsBetter)
//   - "progress"  keeps the highest absolute step count per achievement; an unlock is progress 1 of 0
// Flush sends one item at a time: one submission, one callback, then the next. A failure stops the flush until the
// next trigger (sign-in, reachability, resume) and counts an attempt; past MaxAttempts the item is parked, logged,
// and left on disk so RetryParked() can bring it back. The player's local progress is never affected by any of this.
//
// File rules are Week 7's: temp file then File.Replace with a .bak, a version field, stray temp files ignored, a
// corrupt main file falls back to the .bak with a warning (a queue is a cache of outbound work, not a ledger).

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CMP5402.Week9
{
    [Serializable]
    public class Submission
    {
        public string kind;          // "score" or "progress"
        public string platformId;
        public long value;           // score, or absolute steps
        public int total;            // steps in the achievement; 0 = standard
        public bool lowerIsBetter;   // time boards
        public int attempts;
        public bool parked;

        public string Key => kind + ":" + platformId;

        public static Submission Score(string platformId, long score, bool lowerIsBetter = false) =>
            new Submission { kind = "score", platformId = platformId, value = score, lowerIsBetter = lowerIsBetter };
        public static Submission Progress(string platformId, int steps, int totalSteps) =>
            new Submission { kind = "progress", platformId = platformId, value = steps, total = totalSteps };
    }

    public class PendingSubmissions
    {
        [Serializable]
        class FileModel { public int version = 1; public List<Submission> items = new List<Submission>(); }

        public const int MaxAttempts = 5;

        readonly string path;
        readonly List<Submission> items = new List<Submission>();
        bool flushing;

        public string Path_ => path;
        public int Count => items.Count;
        public int ParkedCount { get { int n = 0; foreach (var s in items) if (s.parked) n++; return n; } }
        public IReadOnlyList<Submission> Items => items;
        public bool IsFlushing => flushing;

        /// Raised after every successful send (kind, platformId, value).
        public event Action<Submission> Sent;
        public event Action<Submission> Parked;

        public PendingSubmissions(string directory, string fileName = "pending_submissions.json")
        {
            Directory.CreateDirectory(directory);
            path = System.IO.Path.Combine(directory, fileName);
            Load();
        }

        // ---- queueing ----------------------------------------------------------------------------------------

        /// De-duplicates by kind + platform id, keeps the better value, then writes the file. Returns true if the
        /// queue changed (the hub flushes only then).
        public bool Enqueue(Submission s)
        {
            if (s == null || string.IsNullOrEmpty(s.platformId)) return false;
            int i = items.FindIndex(x => x.Key == s.Key);
            if (i < 0) { items.Add(s); Save(); return true; }

            var e = items[i];
            bool better = s.kind == "progress" ? s.value > e.value
                        : (e.lowerIsBetter ? s.value < e.value : s.value > e.value);
            if (!better) return false;
            e.value = s.value; e.total = s.total; e.attempts = 0; e.parked = false;
            Save();
            return true;
        }

        /// Bring parked items back into play (the test menu button; also sensible after a sign-in).
        public void RetryParked()
        {
            bool any = false;
            foreach (var s in items) if (s.parked) { s.parked = false; s.attempts = 0; any = true; }
            if (any) Save();
        }

        public void Clear() { items.Clear(); Save(); }

        // ---- sending -----------------------------------------------------------------------------------------

        /// Send the next unparked item, then the next, until the queue is empty or a send fails.
        /// Safe to call from anywhere, any number of times: only one submission is ever in flight.
        public void Flush(IPlatformServices svc)
        {
            if (svc == null || !svc.IsSignedIn || flushing) return;
            var s = items.Find(x => !x.parked);
            if (s == null) return;
            flushing = true;
            Send(svc, s, ok =>
            {
                flushing = false;
                if (ok)
                {
                    items.Remove(s);
                    Save();
                    Sent?.Invoke(s);
                    Flush(svc);
                }
                else
                {
                    s.attempts++;
                    if (s.attempts >= MaxAttempts)
                    {
                        s.parked = true;
                        Debug.LogWarning("[Platform] parked after " + s.attempts + " attempts: " + s.Key);
                        Parked?.Invoke(s);
                    }
                    Save();
                    // stop here: the next trigger (sign-in, reachability, resume) calls Flush again
                }
            });
        }

        static void Send(IPlatformServices svc, Submission s, Action<bool> done)
        {
            bool called = false;
            Action<bool> once = ok => { if (called) return; called = true; done(ok); };
            try
            {
                if (s.kind == "score") svc.SubmitScore(s.platformId, s.value, once);
                else if (s.kind == "progress") svc.ReportProgress(s.platformId, (int)s.value, s.total, once);
                else once(true);   // unknown kind from a future version: drop it rather than wedge the queue
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Platform] send threw: " + ex.Message);
                once(false);
            }
        }

        // ---- file --------------------------------------------------------------------------------------------

        void Load()
        {
            items.Clear();
            if (TryRead(path, out var model) || TryRead(path + ".bak", out model))
            {
                foreach (var s in model.items) if (s != null && !string.IsNullOrEmpty(s.platformId)) items.Add(s);
            }
        }

        bool TryRead(string file, out FileModel model)
        {
            model = null;
            if (!File.Exists(file)) return false;
            try
            {
                var m = JsonUtility.FromJson<FileModel>(File.ReadAllText(file));
                if (m == null || m.items == null) throw new InvalidDataException("empty");
                model = m;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Platform] could not read " + file + ": " + ex.Message);
                return false;
            }
        }

        /// Atomic: write a temp file, then swap it in; the previous file becomes .bak. A crash between the two leaves
        /// a stray temp file that the next Load ignores. Returns false (and logs) on any I/O failure; the queue in
        /// memory is unchanged either way.
        protected virtual bool Save()
        {
            string tmp = path + "." + Guid.NewGuid().ToString("N").Substring(0, 8) + ".tmp";
            try
            {
                var model = new FileModel { items = items };
                File.WriteAllText(tmp, JsonUtility.ToJson(model, true));
                if (File.Exists(path)) File.Replace(tmp, path, path + ".bak");
                else File.Move(tmp, path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Platform] could not save the queue: " + ex.Message);
                try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
                return false;
            }
        }
    }
}
