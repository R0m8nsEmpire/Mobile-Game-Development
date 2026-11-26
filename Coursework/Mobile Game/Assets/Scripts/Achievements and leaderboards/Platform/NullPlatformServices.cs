// CMP5402 Mobile Development - Week 9: Leaderboards, Achievements & Platform Services
// NullPlatformServices: the back end used in the Editor, in the desktop harness and on any build without a plug-in.
//
// A null object, not a null reference: the game runs with no "is the platform there?" checks anywhere.
// It records every call so the test menu and the harness can show what WOULD have been sent, and it can be told to
// refuse (SimulateSignedOut / SimulateFailures) to rehearse the declined-sign-in and offline rows of the test matrix.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CMP5402.Week9
{
    public class NullPlatformServices : IPlatformServices
    {
        public string Name => "Null";
        public bool IsAvailable => true;
        public bool IsSignedIn { get; private set; }
        public bool IsUnderage => SimulateUnderage;

        /// Test controls (the test menu flips these).
        public bool SimulateSignedOut;     // SignIn fails while true
        public bool SimulateFailures;      // SubmitScore / ReportProgress fail while true
        public bool SimulateUnderage;

        /// Everything that reached the "platform", oldest first. The harness asserts on this.
        public readonly List<string> Sent = new List<string>();
        public readonly Dictionary<string, long> BestScores = new Dictionary<string, long>();
        public readonly Dictionary<string, int> Progress = new Dictionary<string, int>();

        public void SignIn(bool interactive, Action<bool> done)
        {
            IsSignedIn = !SimulateSignedOut;
            Debug.Log("[Platform/Null] SignIn(" + (interactive ? "interactive" : "silent") + ") -> " + IsSignedIn);
            done?.Invoke(IsSignedIn);
        }

        public void SignOut() { IsSignedIn = false; }

        public void SubmitScore(string platformId, long score, Action<bool> done)
        {
            if (!IsSignedIn || SimulateFailures) { done?.Invoke(false); return; }
            if (!BestScores.TryGetValue(platformId, out var best) || score > best) BestScores[platformId] = score;
            Sent.Add("score:" + platformId + "=" + score);
            Debug.Log("[Platform/Null] score " + score + " -> " + platformId);
            done?.Invoke(true);
        }

        public void ReportProgress(string platformId, int steps, int totalSteps, Action<bool> done)
        {
            if (!IsSignedIn || SimulateFailures) { done?.Invoke(false); return; }
            int pct = totalSteps > 0 ? Mathf.Min(100, (int)(steps * 100L / totalSteps)) : (steps >= 1 ? 100 : 0);
            if (!Progress.TryGetValue(platformId, out var have) || pct > have) Progress[platformId] = pct;
            Sent.Add("progress:" + platformId + "=" + steps + "/" + totalSteps);
            Debug.Log("[Platform/Null] progress " + steps + "/" + totalSteps + " (" + pct + "%) -> " + platformId);
            done?.Invoke(true);
        }

        public void ShowLeaderboard(string platformId) => Debug.Log("[Platform/Null] ShowLeaderboard " + platformId);
        public void ShowAchievements() => Debug.Log("[Platform/Null] ShowAchievements");
    }
}
