// CMP5402 Mobile Development - Week 9: Leaderboards, Achievements & Platform Services
// AchievementTracker: gameplay events in, logical ids out ("AchievementTracker" slide).
//
// The tracker is the ONLY place that knows what earns what. Gameplay raises events (OnLevelCompleted,
// OnEnemyDefeated); the tracker updates counters in ProgressRecord, decides which achievement that touches, and asks
// PlatformHub with a logical id. Nothing here knows which store it is on.
//
// Local progress is the truth: counters and the unlocked set live in ProgressRecord, which you should fold into your
// Week 7 SaveData (one more [Serializable] field) so it is versioned, written atomically and cloud-synced like the
// rest of the save. The platform only mirrors it; Resync() re-sends every unlock and counter after a cloud restore
// or a first sign-in on a new device, which is harmless because every report is absolute.
//
// The example rules are deliberately small. Replace them with yours; keep the shape.

using System;
using System.Collections.Generic;

namespace CMP5402.Week9
{
    /// The part of the save that achievements need. Add a field of this type to your Week 7 SaveData.
    [Serializable]
    public class ProgressRecord
    {
        public int levelsCompleted;
        public int enemiesDefeated;
        public long bestLevelTimeMs = long.MaxValue;
        public List<string> unlocked = new List<string>();

        public bool IsUnlocked(string logicalId) => unlocked.Contains(logicalId);
        public bool Unlock(string logicalId)
        {
            if (unlocked.Contains(logicalId)) return false;
            unlocked.Add(logicalId);
            return true;
        }
    }

    public class AchievementTracker
    {
        public const string FirstClear = "first_clear";
        public const string WorldOne = "world_one";
        public const string SpeedRun = "speed_run";
        public const string Hunter = "hunter";            // incremental: totalSteps 100 in PlatformIds
        public const string FastestLevel = "fastest_level";   // leaderboard, lowerIsBetter

        public const int HunterBatch = 10;

        readonly PlatformHub hub;
        readonly ProgressRecord progress;
        readonly Action save;   // your Week 7 SaveManager.RequestSave or similar

        /// Raised when a NEW achievement is unlocked locally (for your own toast; the platform banner is separate).
        public event Action<string> Unlocked;

        public AchievementTracker(PlatformHub hub, ProgressRecord progress, Action save = null)
        {
            this.hub = hub;
            this.progress = progress;
            this.save = save;
        }

        public void OnLevelCompleted(int level, float seconds)
        {
            progress.levelsCompleted++;
            if (progress.levelsCompleted == 1) Unlock(FirstClear);
            if (level == 10) Unlock(WorldOne);
            if (seconds < 30f) Unlock(SpeedRun);

            long ms = (long)(seconds * 1000f);
            if (ms < progress.bestLevelTimeMs) progress.bestLevelTimeMs = ms;
            hub.SubmitScore(FastestLevel, ms);   // the queue and the platform both keep the best
            save?.Invoke();
        }

        public void OnEnemyDefeated()
        {
            progress.enemiesDefeated++;
            if (progress.enemiesDefeated % HunterBatch == 0)          // batch, never per kill
            {
                hub.ReportProgress(Hunter, progress.enemiesDefeated);   // absolute, idempotent
                save?.Invoke();
            }
        }

        void Unlock(string logicalId)
        {
            if (!progress.Unlock(logicalId)) return;   // local truth first; second call is a no-op
            Unlocked?.Invoke(logicalId);
            hub.UnlockAchievement(logicalId);
        }

        /// After a cloud restore or a sign-in on a new device: mirror the local truth back to the platform.
        public void Resync()
        {
            foreach (var id in progress.unlocked) hub.UnlockAchievement(id);
            if (progress.enemiesDefeated > 0) hub.ReportProgress(Hunter, progress.enemiesDefeated);
            if (progress.bestLevelTimeMs != long.MaxValue) hub.SubmitScore(FastestLevel, progress.bestLevelTimeMs);
        }
    }
}
