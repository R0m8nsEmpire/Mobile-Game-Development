// CMP5402 Mobile Development - Week 9: Leaderboards, Achievements & Platform Services
// IPlatformServices: the one seam between the game and a platform's social features ("The interface" slide).
//
// Rules of the seam:
//   - no vendor type crosses it (no GooglePlayGames.*, no Apple.GameKit.*); gameplay never imports a plug-in namespace
//   - every call is callback-shaped. Play Games is callback-based already; the Game Center adapter converts its
//     async/await API to this shape. The Null adapter answers at once.
//   - every call takes a PLATFORM id (the Play Console or App Store Connect string). The mapping from the logical id
//     that gameplay uses ("first_clear") to the platform id lives in PlatformIds and is applied by PlatformHub.
//   - ReportProgress is ABSOLUTE: steps so far out of totalSteps. totalSteps == 0 means a standard (non-incremental)
//     achievement, which unlocks when steps >= 1. Absolute values can be re-sent after a crash without double-counting;
//     that is what lets PendingSubmissions retry safely.

using System;

namespace CMP5402.Week9
{
    public interface IPlatformServices
    {
        /// "PlayGames", "GameCenter" or "Null". Used as the column selector in PlatformIds.
        string Name { get; }
        /// True when the plug-in for this platform is compiled in and this is the right OS. Null is always available.
        bool IsAvailable { get; }
        bool IsSignedIn { get; }
        /// Platform's own under-18 signal where it has one (Game Center IsUnderage). False when unknown.
        bool IsUnderage { get; }

        /// interactive == false: the silent attempt at launch. interactive == true: the player pressed Sign in.
        /// done is called exactly once, on the main thread, with the signed-in state.
        void SignIn(bool interactive, Action<bool> done);

        /// Post a score to a platform leaderboard. Platforms keep the best score themselves; resubmitting is harmless.
        void SubmitScore(string platformId, long score, Action<bool> done);

        /// Absolute progress: steps so far out of totalSteps (0 = standard achievement, unlocks when steps >= 1).
        void ReportProgress(string platformId, int steps, int totalSteps, Action<bool> done);

        void ShowLeaderboard(string platformId);
        void ShowAchievements();
    }
}
