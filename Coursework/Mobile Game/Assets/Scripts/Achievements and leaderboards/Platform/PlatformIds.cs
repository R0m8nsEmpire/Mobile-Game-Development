// CMP5402 Mobile Development - Week 9: Leaderboards, Achievements & Platform Services
// PlatformIds: the mapping from the logical ids gameplay uses to the ids each store gave you ("Logical IDs" slide).
//
// Create one with Assets > Create > CMP5402 > Platform IDs and fill it from Play Console and App Store Connect.
// Gameplay only ever says "first_clear" or "fastest_level"; the hub resolves the column for the running platform.
// Unknown ids resolve to null and the hub logs a warning and sends nothing: a missing console entry must never throw
// inside gameplay. The Null back end accepts any id, so the Editor never needs real ones (leave the columns blank and
// the logical id is used as the platform id).

using System;
using UnityEngine;

namespace CMP5402.Week9
{
    [CreateAssetMenu(menuName = "CMP5402/Platform IDs", fileName = "PlatformIds")]
    public class PlatformIds : ScriptableObject
    {
        [Serializable]
        public class Achievement
        {
            public string logicalId;
            public string playGamesId;
            public string gameCenterId;
            [Tooltip("0 for a standard achievement; the number of steps for an incremental one (same on both stores)")]
            public int totalSteps;
        }

        [Serializable]
        public class Leaderboard
        {
            public string logicalId;
            public string playGamesId;
            public string gameCenterId;
            [Tooltip("Tick for time boards. The board must ALSO be configured as ascending in both consoles and in UGS.")]
            public bool lowerIsBetter;
            [Tooltip("Optional Unity Gaming Services leaderboard id for the cross-platform board")]
            public string ugsId;
        }

        public Achievement[] achievements = new Achievement[0];
        public Leaderboard[] leaderboards = new Leaderboard[0];

        public Achievement FindAchievement(string logicalId)
        {
            foreach (var a in achievements) if (a != null && a.logicalId == logicalId) return a;
            return null;
        }

        public Leaderboard FindLeaderboard(string logicalId)
        {
            foreach (var l in leaderboards) if (l != null && l.logicalId == logicalId) return l;
            return null;
        }

        /// The platform id for an achievement on the named platform, or null when there is none.
        public string ResolveAchievement(string logicalId, string platform)
        {
            var a = FindAchievement(logicalId);
            return a == null ? null : Column(platform, a.logicalId, a.playGamesId, a.gameCenterId);
        }

        /// The platform id for a leaderboard on the named platform, or null when there is none.
        public string ResolveLeaderboard(string logicalId, string platform)
        {
            var l = FindLeaderboard(logicalId);
            return l == null ? null : Column(platform, l.logicalId, l.playGamesId, l.gameCenterId);
        }

        static string Column(string platform, string logicalId, string playGames, string gameCenter)
        {
            switch (platform)
            {
                case "PlayGames": return string.IsNullOrEmpty(playGames) ? null : playGames;
                case "GameCenter": return string.IsNullOrEmpty(gameCenter) ? null : gameCenter;
                default: return logicalId;   // Null back end: the logical id is good enough
            }
        }
    }
}
