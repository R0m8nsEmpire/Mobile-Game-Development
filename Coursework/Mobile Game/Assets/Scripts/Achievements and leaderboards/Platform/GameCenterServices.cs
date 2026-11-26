// CMP5402 Mobile Development - Week 9: Leaderboards, Achievements & Platform Services
// GameCenterServices: the Apple Game Center adapter ("Game Center: sign in, submit, show" slide).
//
// Compiles only with APPLE_GAMEKIT_INSTALLED after importing Apple.Core and Apple.GameKit from the Apple Unity
// Plug-ins (github.com/apple/unityplugins, built with build.py; iOS 15.6 minimum per the repository README).
//
// GameKit's Unity API is async/await and throws GameKitException; this adapter converts every call to the callback
// shape of IPlatformServices and never lets an exception escape. Names used, from the plug-in source:
//   GKLocalPlayer.Authenticate() -> GKLocalPlayer; GKLocalPlayer.Local.IsAuthenticated / IsUnderage / TeamPlayerId
//   GKLeaderboard.LoadLeaderboards(params string[] ids); leaderboard.SubmitScore(long score, long context, GKPlayer)
//   GKAchievement.Init(id); PercentComplete; ShowCompletionBanner; GKAchievement.Report(params GKAchievement[])
//   GKAccessPoint.Shared.Trigger(GKGameCenterViewControllerState) (the enum is top-level in Apple.GameKit)
//   GKLocalPlayer.Local.FetchItems() -> signature, salt, public key URL, timestamp for Unity Authentication

#if APPLE_GAMEKIT_INSTALLED
using System;
using System.Threading.Tasks;
using Apple.GameKit;
using Apple.GameKit.Leaderboards;
using UnityEngine;

namespace CMP5402.Week9
{
    public class GameCenterServices : IPlatformServices
    {
        public string Name => "GameCenter";
        public bool IsAvailable => Application.platform == RuntimePlatform.IPhonePlayer;
        public bool IsSignedIn { get; private set; }
        public bool IsUnderage { get; private set; }

        bool loggedAuthError;

        public async void SignIn(bool interactive, Action<bool> done)
        {
            // Game Center has no silent/interactive split: Authenticate shows Apple's sheet only when it has to.
            // A declined sheet throws; log that once and never re-prompt automatically.
            if (!interactive && loggedAuthError) { done?.Invoke(false); return; }
            try
            {
                var player = await GKLocalPlayer.Authenticate();
                IsSignedIn = player != null && player.IsAuthenticated;
                IsUnderage = IsSignedIn && GKLocalPlayer.Local.IsUnderage;
            }
            catch (GameKitException ex)
            {
                IsSignedIn = false;
                if (!loggedAuthError) Debug.Log("[Platform/GameCenter] sign-in: " + ex.Message);
                loggedAuthError = true;
            }
            done?.Invoke(IsSignedIn);
        }

        public async void SubmitScore(string platformId, long score, Action<bool> done)
        {
            if (!IsSignedIn) { done?.Invoke(false); return; }
            try
            {
                var boards = await GKLeaderboard.LoadLeaderboards(platformId);
                if (boards == null || boards.Count == 0) { Debug.LogWarning("[Platform/GameCenter] no leaderboard " + platformId); done?.Invoke(false); return; }
                await boards[0].SubmitScore(score, 0, GKLocalPlayer.Local);
                done?.Invoke(true);
            }
            catch (GameKitException ex) { Debug.LogWarning("[Platform/GameCenter] score: " + ex.Message); done?.Invoke(false); }
        }

        public async void ReportProgress(string platformId, int steps, int totalSteps, Action<bool> done)
        {
            if (!IsSignedIn) { done?.Invoke(false); return; }
            try
            {
                var a = GKAchievement.Init(platformId);
                a.PercentComplete = totalSteps > 0 ? Math.Min(100.0, steps * 100.0 / totalSteps) : (steps >= 1 ? 100.0 : 0.0);
                a.ShowCompletionBanner = a.PercentComplete >= 100.0;
                await GKAchievement.Report(a);
                done?.Invoke(true);
            }
            catch (GameKitException ex) { Debug.LogWarning("[Platform/GameCenter] achievement: " + ex.Message); done?.Invoke(false); }
        }

        public async void ShowLeaderboard(string platformId)
        {
            if (!IsSignedIn) return;
            try { await GKAccessPoint.Shared.Trigger(GKGameCenterViewControllerState.Leaderboards); }
            catch (GameKitException ex) { Debug.LogWarning("[Platform/GameCenter] show: " + ex.Message); }
        }

        public async void ShowAchievements()
        {
            if (!IsSignedIn) return;
            try { await GKAccessPoint.Shared.Trigger(GKGameCenterViewControllerState.Achievements); }
            catch (GameKitException ex) { Debug.LogWarning("[Platform/GameCenter] show: " + ex.Message); }
        }

        /// Proof for Unity Authentication: valid for ten minutes, fetch it right before linking.
        public async Task<GKIdentityVerificationResponse> FetchIdentityAsync()
        {
            if (!IsSignedIn) return null;
            try { return await GKLocalPlayer.Local.FetchItems(); }
            catch (GameKitException ex) { Debug.LogWarning("[Platform/GameCenter] identity: " + ex.Message); return null; }
        }
    }
}
#endif
