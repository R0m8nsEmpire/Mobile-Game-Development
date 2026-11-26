// CMP5402 Mobile Development - Week 9: Leaderboards, Achievements & Platform Services
// UgsLeaderboard: one cross-platform board through Unity Gaming Services ("Cross-platform boards" slide).
//
// Compiles only with UGS_LEADERBOARDS_INSTALLED after adding the Leaderboards package (com.unity.services.leaderboards)
// and linking the project (Project Settings > Services). Needs UnityServices.InitializeAsync() and a signed-in
// Unity player (Week 7's bootstrap does both). Create the board in the dashboard: Development > Products >
// Leaderboards, with its sort order, score strategy (Best / Latest / Total) and any resets or tiers.
//
// From the UGS Leaderboards documentation:
//   AddPlayerScoreAsync(leaderboardId, score[, AddPlayerScoreOptions { Metadata }])   metadata up to 1024 bytes
//   GetScoresAsync(leaderboardId[, GetScoresOptions { Offset, Limit, IncludeMetadata }])  top 10 by default
//   GetPlayerScoreAsync(leaderboardId)
// This is the CLIENT-SUBMITS path (tier 1 on the fairness slide). For a board that matters, move the AddPlayerScore
// call into a Cloud Code script that checks the evidence first, and deny the client in Access Control.

#if UGS_LEADERBOARDS_INSTALLED
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

namespace CMP5402.Week9
{
    public static class UgsLeaderboard
    {
        [Serializable]
        public class ScoreMetadata { public string build; public string mode; }

        public static async Task<LeaderboardEntry> SubmitAsync(string leaderboardId, double score, string mode = null)
        {
            try
            {
                var options = new AddPlayerScoreOptions { Metadata = new ScoreMetadata { build = Application.version, mode = mode } };
                var entry = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score, options);
                Debug.Log("[UGS] " + leaderboardId + " rank " + entry.Rank + " score " + entry.Score);
                return entry;
            }
            catch (Exception ex) { Debug.LogWarning("[UGS] submit failed: " + ex.Message); return null; }
        }

        public static async Task<List<LeaderboardEntry>> TopAsync(string leaderboardId, int limit = 10, int offset = 0)
        {
            try
            {
                var page = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId,
                    new GetScoresOptions { Limit = limit, Offset = offset, IncludeMetadata = true });
                return page.Results;
            }
            catch (Exception ex) { Debug.LogWarning("[UGS] top failed: " + ex.Message); return null; }
        }

        public static async Task<LeaderboardEntry> MeAsync(string leaderboardId)
        {
            try { return await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId); }
            catch (Exception ex) { Debug.LogWarning("[UGS] me failed: " + ex.Message); return null; }
        }
    }
}
#endif
