// CMP5402 Mobile Development - Week 9: Leaderboards, Achievements & Platform Services
// PlayGamesServices: the Google Play Games Services adapter ("Play Games: sign in, submit, show" slide).
//
// Compiles only with GPGS_INSTALLED (Project Settings > Player > Scripting Define Symbols) after importing the
// Google Play Games plugin for Unity (v11 or later). On every other build this file is empty.
//
// From the Android Developers Unity pages and the plug-in's PlayGamesPlatform class:
//   - the plug-in signs in automatically at launch; Authenticate(callback) reports the SignInStatus of that attempt
//   - ManuallyAuthenticate(callback) is for a Sign in button
//   - no other Play Games call before SignInStatus.Success
//   - ReportScore(score, leaderboardId, callback); the server discards scores lower than the player's best
//   - ReportProgress(id, 100.0, callback) unlocks a standard achievement (0.0 only reveals it)
//   - SetStepsAtLeast(id, steps, callback) sets an incremental achievement's ABSOLUTE step count; the pack uses it in
//     preference to IncrementAchievement so that a retried submission cannot double-count
//   - ShowLeaderboardUI(id) and ShowAchievementsUI()
//   - RequestServerSideAccess(forceRefresh, callback) yields the one-time auth code Unity Authentication needs
//     (see Identity/AccountLinker.cs)

#if GPGS_INSTALLED
using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

namespace CMP5402.Week9
{
    public class PlayGamesServices : IPlatformServices
    {
        public string Name => "PlayGames";
        public bool IsAvailable => Application.platform == RuntimePlatform.Android;
        public bool IsSignedIn { get; private set; }
        public bool IsUnderage => false;   // Play Games exposes no under-age flag; use the Families policy and your own age gate

        public void SignIn(bool interactive, Action<bool> done)
        {
            Action<SignInStatus> finish = status =>
            {
                IsSignedIn = status == SignInStatus.Success;
                if (!IsSignedIn) Debug.Log("[Platform/PlayGames] sign-in: " + status);
                done?.Invoke(IsSignedIn);
            };
            if (interactive) PlayGamesPlatform.Instance.ManuallyAuthenticate(finish);
            else PlayGamesPlatform.Instance.Authenticate(finish);
        }

        public void SubmitScore(string platformId, long score, Action<bool> done)
        {
            if (!IsSignedIn) { done?.Invoke(false); return; }
            PlayGamesPlatform.Instance.ReportScore(score, platformId, ok => done?.Invoke(ok));
        }

        public void ReportProgress(string platformId, int steps, int totalSteps, Action<bool> done)
        {
            if (!IsSignedIn) { done?.Invoke(false); return; }
            if (totalSteps > 0) PlayGamesPlatform.Instance.SetStepsAtLeast(platformId, steps, ok => done?.Invoke(ok));
            else if (steps >= 1) PlayGamesPlatform.Instance.ReportProgress(platformId, 100.0, ok => done?.Invoke(ok));
            else done?.Invoke(true);
        }

        public void ShowLeaderboard(string platformId)
        {
            if (IsSignedIn) PlayGamesPlatform.Instance.ShowLeaderboardUI(platformId);
        }

        public void ShowAchievements()
        {
            if (IsSignedIn) PlayGamesPlatform.Instance.ShowAchievementsUI();
        }

        /// One-time server auth code for Unity Authentication (SignInWithGooglePlayGamesAsync / LinkWith...).
        public void RequestAuthCode(Action<string> done)
        {
            if (!IsSignedIn) { done?.Invoke(null); return; }
            PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => done?.Invoke(code));
        }
    }
}
#endif
