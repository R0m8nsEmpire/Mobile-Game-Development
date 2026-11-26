// CMP5402 Mobile Development - Week 9: Leaderboards, Achievements & Platform Services
// PlatformHub: the only object gameplay talks to for platform features ("Logical IDs, and the hub" slide).
//
// One persistent GameObject in the first scene. In Awake it picks the back end for this build:
//   Android with GPGS_INSTALLED            -> PlayGamesServices
//   iOS with APPLE_GAMEKIT_INSTALLED       -> GameCenterServices
//   anything else (Editor, tests, no plug-in) -> NullPlatformServices
// Gameplay calls UnlockAchievement / ReportProgress / SubmitScore with LOGICAL ids. The hub resolves the platform id
// from the PlatformIds asset, writes the submission to the PendingSubmissions queue, and flushes when signed in.
// Flush triggers: sign-in success, reachability regained, app resume (OnApplicationFocus / OnApplicationPause false).
// Sign-in policy: one silent attempt at launch; an interactive attempt only when the player presses a button.

using System;
using UnityEngine;

namespace CMP5402.Week9
{
    [DefaultExecutionOrder(-50)]
    public class PlatformHub : MonoBehaviour
    {
        public static PlatformHub Instance { get; private set; }

        [SerializeField] PlatformIds ids;
        [SerializeField, Tooltip("Try a silent sign-in at launch (recommended). Never show a prompt here.")]
        bool silentSignInAtLaunch = true;
        [SerializeField, Tooltip("Seconds between reachability polls used as a flush trigger. 0 disables; wire Week 7's NetProbe instead.")]
        float reachabilityPollSeconds = 5f;

        public IPlatformServices Services { get; private set; }
        public PendingSubmissions Queue { get; private set; }
        public PlatformIds Ids => ids;
        public bool IsSignedIn => Services != null && Services.IsSignedIn;
        public bool SignInAttempted { get; private set; }

        /// Raised after any sign-in attempt with the result (true = signed in).
        public event Action<bool> SignInChanged;
        /// Raised whenever a submission is queued (logical id, kind).
        public event Action<string, string> Queued;

        bool wasReachable;
        float pollTimer;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Services = CreateServices();
            Queue = new PendingSubmissions(System.IO.Path.Combine(Application.persistentDataPath, "platform"));
            if (ids == null) Debug.LogWarning("[Platform] no PlatformIds asset assigned; every logical id will resolve to nothing on a real platform");
            wasReachable = Application.internetReachability != NetworkReachability.NotReachable;
            Debug.Log("[Platform] back end: " + Services.Name + ", queued: " + Queue.Count);
        }

        /// Overridable for the harness (which supplies a fake). In the game the #if decides.
        protected virtual IPlatformServices CreateServices()
        {
#if UNITY_ANDROID && GPGS_INSTALLED && !UNITY_EDITOR
            return new PlayGamesServices();
#elif UNITY_IOS && APPLE_GAMEKIT_INSTALLED && !UNITY_EDITOR
            return new GameCenterServices();
#else
            return new NullPlatformServices();
#endif
        }

        void Start()
        {
            if (silentSignInAtLaunch) SignIn(false);
        }

        // ---- sign-in -----------------------------------------------------------------------------------------

        /// interactive == false is the silent launch attempt; true is the Sign in button.
        public void SignIn(bool interactive)
        {
            if (Services == null || !Services.IsAvailable) { SignInChanged?.Invoke(false); return; }
            Services.SignIn(interactive, ok =>
            {
                SignInAttempted = true;
                Debug.Log("[Platform] sign-in " + (interactive ? "(interactive)" : "(silent)") + " -> " + ok);
                SignInChanged?.Invoke(ok);
                if (ok) { Queue.RetryParked(); Queue.Flush(Services); }
            });
        }

        // ---- gameplay API (logical ids) ------------------------------------------------------------------------

        /// Standard achievement: unlock it. Idempotent; call it as often as you like.
        public void UnlockAchievement(string logicalId)
        {
            var a = ids != null ? ids.FindAchievement(logicalId) : null;
            if (a != null && a.totalSteps > 0) { ReportProgress(logicalId, a.totalSteps); return; }
            Submit(logicalId, "progress", id => Submission.Progress(id, 1, 0), ResolveAchievement);
        }

        /// Incremental achievement: absolute steps so far. The asset holds totalSteps.
        public void ReportProgress(string logicalId, int stepsSoFar)
        {
            var a = ids != null ? ids.FindAchievement(logicalId) : null;
            int total = a != null ? a.totalSteps : 0;
            if (total <= 0) { Debug.LogWarning("[Platform] " + logicalId + " has no totalSteps in PlatformIds; treating progress as an unlock"); UnlockAchievement(logicalId); return; }
            int steps = Mathf.Clamp(stepsSoFar, 0, total);
            if (steps == 0) return;
            Submit(logicalId, "progress", id => Submission.Progress(id, steps, total), ResolveAchievement);
        }

        /// Leaderboard score. The platform keeps the best; the queue keeps the best while offline.
        public void SubmitScore(string logicalId, long score)
        {
            var l = ids != null ? ids.FindLeaderboard(logicalId) : null;
            bool lower = l != null && l.lowerIsBetter;
            Submit(logicalId, "score", id => Submission.Score(id, score, lower), ResolveLeaderboard);
        }

        public void ShowLeaderboard(string logicalId)
        {
            var id = ResolveLeaderboard(logicalId);
            if (id != null && IsSignedIn) Services.ShowLeaderboard(id);
        }

        public void ShowAchievements() { if (IsSignedIn) Services.ShowAchievements(); }

        /// Week 7 NetProbe users: call this from ReachabilityChanged(true). Otherwise the poll below covers it.
        public void OnReachabilityRegained() => Queue.Flush(Services);

        // ---- internals ---------------------------------------------------------------------------------------

        void Submit(string logicalId, string kind, Func<string, Submission> make, Func<string, string> resolve)
        {
            var platformId = resolve(logicalId);
            if (platformId == null)
            {
                Debug.LogWarning("[Platform] no " + Services.Name + " id for '" + logicalId + "' in PlatformIds; nothing sent");
                return;
            }
            if (Queue.Enqueue(make(platformId))) Queued?.Invoke(logicalId, kind);
            Queue.Flush(Services);
        }

        string ResolveAchievement(string logicalId) => ids == null ? (Services.Name == "Null" ? logicalId : null) : ids.ResolveAchievement(logicalId, Services.Name);
        string ResolveLeaderboard(string logicalId) => ids == null ? (Services.Name == "Null" ? logicalId : null) : ids.ResolveLeaderboard(logicalId, Services.Name);

        void Update()
        {
            if (reachabilityPollSeconds <= 0f) return;
            pollTimer += Time.unscaledDeltaTime;
            if (pollTimer < reachabilityPollSeconds) return;
            pollTimer = 0f;
            bool reachable = Application.internetReachability != NetworkReachability.NotReachable;
            if (reachable && !wasReachable) Queue.Flush(Services);
            wasReachable = reachable;
        }

        void OnApplicationFocus(bool focus) { if (focus) Queue.Flush(Services); }
        void OnApplicationPause(bool paused) { if (!paused) Queue.Flush(Services); }

        /// For tests: inject ids and services without a scene.
        public void ConfigureForTesting(PlatformIds platformIds, IPlatformServices services, string queueDirectory)
        {
            ids = platformIds;
            Services = services;
            Queue = new PendingSubmissions(queueDirectory);
        }
    }
}
