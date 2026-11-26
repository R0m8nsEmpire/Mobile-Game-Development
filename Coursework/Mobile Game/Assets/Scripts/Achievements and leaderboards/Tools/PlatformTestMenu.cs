// CMP5402 Mobile Development - Week 9: Leaderboards, Achievements & Platform Services
// PlatformTestMenu: on-screen buttons for the platform services test matrix ("The platform services test matrix" slide).
//
// Immediate-mode GUI, no Canvas, Editor and development builds only. Three-finger tap or F4 toggles it.
// In the Editor the hub runs on NullPlatformServices, so this menu can rehearse the first five matrix rows:
//   - "Sign out (fake)" / "Sign in (silent)" / "Sign in (button)": declined and later sign-in
//   - "Go offline (fake)": every send fails, the queue grows and attempts count up; "Go online" lets it drain
//   - "Earn first_clear", "Defeat 10 enemies", "Complete level in 12.3 s": drive the tracker exactly as gameplay would
//   - "Unknown id": asks the hub for an achievement that is not in the asset (warning, nothing sent, no exception)
//   - "Retry parked", "Clear queue", "Resync": queue management and the restore path
// On a device with a plug-in and tester account the same buttons drive the real back end.

using System;
using UnityEngine;

namespace CMP5402.Week9
{
    public class PlatformTestMenu : MonoBehaviour
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        [SerializeField] bool visible = true;
        [SerializeField] int fontSize = 26;
        GUIStyle label, button;
        string lastAction = "";
        ProgressRecord progress = new ProgressRecord();
        AchievementTracker tracker;
        int level = 1;

        void Start()
        {
            if (PlatformHub.Instance == null) { lastAction = "No PlatformHub in the scene"; return; }
            tracker = new AchievementTracker(PlatformHub.Instance, progress, () => lastAction += " (save requested)");
            tracker.Unlocked += id => lastAction = "UNLOCKED locally: " + id;
            PlatformHub.Instance.Queued += (id, kind) => lastAction = "queued " + kind + " for " + id;
            PlatformHub.Instance.Queue.Sent += s => lastAction = "sent " + s.Key + " = " + s.value;
            PlatformHub.Instance.Queue.Parked += s => lastAction = "PARKED " + s.Key + " after " + s.attempts + " attempts";
            PlatformHub.Instance.SignInChanged += ok => lastAction = "sign-in -> " + ok;
        }

        void Update()
        {
            bool toggle = false;
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.f4Key.wasPressedThisFrame) toggle = true;
            var ts = UnityEngine.InputSystem.Touchscreen.current;
            if (ts != null && ts.touches.Count >= 3 && ts.touches[2].press.wasPressedThisFrame) toggle = true;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.F4)) toggle = true;
            if (Input.touchCount >= 3 && Input.GetTouch(2).phase == TouchPhase.Began) toggle = true;
#endif
            if (toggle) visible = !visible;
        }

        void OnGUI()
        {
            if (!visible) return;
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label) { fontSize = fontSize, richText = true };
                button = new GUIStyle(GUI.skin.button) { fontSize = fontSize };
            }
            var hub = PlatformHub.Instance;
            GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, Screen.height - 40));
            if (hub == null) { GUILayout.Label("No PlatformHub", label); GUILayout.EndArea(); return; }

            var svc = hub.Services;
            var nul = svc as NullPlatformServices;
            GUILayout.Label("<b>Platform:</b> " + svc.Name + "  signed in: " + svc.IsSignedIn + "  underage: " + svc.IsUnderage
                + "  queue: " + hub.Queue.Count + " (parked " + hub.Queue.ParkedCount + ")" + (nul != null && nul.SimulateFailures ? "  OFFLINE (fake)" : ""), label);
            GUILayout.Label("<b>Progress:</b> levels " + progress.levelsCompleted + ", enemies " + progress.enemiesDefeated
                + ", best " + (progress.bestLevelTimeMs == long.MaxValue ? "none" : progress.bestLevelTimeMs + " ms") + ", unlocked [" + string.Join(", ", progress.unlocked) + "]", label);
            GUILayout.Label("<b>Last:</b> " + lastAction, label);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Sign in (silent)", button)) hub.SignIn(false);
            if (GUILayout.Button("Sign in (button)", button)) hub.SignIn(true);
            if (nul != null && GUILayout.Button(nul.SimulateSignedOut ? "Allow sign-in (fake)" : "Refuse sign-in (fake)", button)) { nul.SimulateSignedOut = !nul.SimulateSignedOut; if (nul.SimulateSignedOut) nul.SignOut(); }
            if (nul != null && GUILayout.Button(nul.SimulateFailures ? "Go online (fake)" : "Go offline (fake)", button)) { nul.SimulateFailures = !nul.SimulateFailures; if (!nul.SimulateFailures) hub.OnReachabilityRegained(); }
            if (nul != null && GUILayout.Button(nul.SimulateUnderage ? "Adult (fake)" : "Underage (fake)", button)) nul.SimulateUnderage = !nul.SimulateUnderage;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (tracker != null && GUILayout.Button("Complete level " + level + " in 12.3 s", button)) tracker.OnLevelCompleted(level++, 12.3f);
            if (tracker != null && GUILayout.Button("Complete level " + level + " in 45 s", button)) tracker.OnLevelCompleted(level++, 45f);
            if (tracker != null && GUILayout.Button("Defeat 10 enemies", button)) for (int i = 0; i < 10; i++) tracker.OnEnemyDefeated();
            if (GUILayout.Button("Unknown id", button)) hub.UnlockAchievement("not_in_the_asset");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Flush", button)) hub.OnReachabilityRegained();
            if (GUILayout.Button("Retry parked", button)) { hub.Queue.RetryParked(); hub.OnReachabilityRegained(); }
            if (GUILayout.Button("Clear queue", button)) hub.Queue.Clear();
            if (tracker != null && GUILayout.Button("Resync", button)) tracker.Resync();
            if (GUILayout.Button("Show leaderboard", button)) hub.ShowLeaderboard(AchievementTracker.FastestLevel);
            if (GUILayout.Button("Show achievements", button)) hub.ShowAchievements();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            foreach (var s in hub.Queue.Items)
                GUILayout.Label((s.parked ? "<color=red>parked</color> " : "") + s.Key + " = " + s.value + (s.total > 0 ? "/" + s.total : "") + "  attempts " + s.attempts, label);
            GUILayout.Label("<size=" + (fontSize - 8) + ">queue file: " + hub.Queue.Path_ + "</size>", label);
            GUILayout.EndArea();
        }
#endif
    }
}
