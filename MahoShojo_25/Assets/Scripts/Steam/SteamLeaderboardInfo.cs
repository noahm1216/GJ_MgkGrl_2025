using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamLeaderboardInfo : MonoBehaviour
{

    private const string LeaderboardName = "Level_0_TimeScore";
    private Steamworks.Data.Leaderboard _lb;

    public async void GetLeaderboard()
    {
        if (Manager_Steam.Instance)
        {
            try
            {
                var lb = await Manager_Steam.CreateOrFindLeaderboard(LeaderboardName, Steamworks.Data.LeaderboardSort.Ascending, Steamworks.Data.LeaderboardDisplay.Numeric);
                if (lb.HasValue) _lb = lb.Value;
                else Debug.LogError("leaderboard did not have value");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading leaderboards: {e.Message}");
                // Optionally, you could handle the exception, e.g., retry logic or fallback behavior
            }
        }
        else Debug.LogError("Missing Steam Manager Instance");
    }

    #region TEST

    //[ContextMenu("ReplaceScoreTest")]
    //private void ReplaceScoreTest()
    //{
    //    var entry = new LeaderboardScore(1234.1829f, 12345789);
    //    ReplaceSpeedScore(entry);
    //}

    //#endregion

    //#region ReplaceScores

    //private void ReplaceSpeedScore(LeaderboardScoreentry)
    //{
    //    _ = SteamLeaderBoardSystem.ReplaceLeaderboard(_lb, entry.TimeI, entry.Details);
    //}

    #endregion
}


