using System;
//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
//using Steamworks;
using Steamworks.Data;

public class SteamLeaderboardInfo : MonoBehaviour
{
    private const string LeaderboardName = "Test_Leaderboard"; // TODO: open this up to be changed per level
    private Steamworks.Data.Leaderboard _lb;
    private bool retrivedLeaderboard = false;
    private int scoreToStore;
    private int[] detailsToStore;

    public void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            print("Forcing Get Updated Leaderboard");
            retrivedLeaderboard = false;
            GetLeaderboard();
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            print("Getting Leaderboard & Sending Leaderboard Update");
            GetLeaderboard();
            GetLatestScores();
            UpdateScores();
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            print("Getting Leaderboard & Forcing Leaderboard Update");
            GetLeaderboard();
            GetLatestScores();
            ReplaceScoreTest();
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha4)) // debug.log the scores and data
        {
            print("Reading Leaderboard Score");
            GetLeaderboard();
            if (Manager_Steam.Instance)
            {
                PrintConvertedScore();
            }
        }
    }

    public bool GetLatestScores()
    {
        if (Manager_GameState.Instance)
        {
            scoreToStore = Manager_GameState.Instance.scoreTotal;
            detailsToStore = new int[] { Manager_GameState.Instance.obstaclePoints,
                                           Manager_GameState.Instance.capturedCreatues_Unique,
                                            Manager_GameState.Instance.capturedPoints,
                                            Manager_Steam.ConvertScoreToInt(Manager_GameState.Instance.timeOfCurrentGameRun,100),
                                            Manager_Steam.ConvertScoreToInt(Manager_GameState.Instance.distanceOfCurrentGameRun,100),   };
            return true;
        }
        Debug.LogWarning("Unable to get latest scores... missing GameState Instance");
        return false;
    }

    public async void PrintConvertedScore()
    {
        string userName = "";
        int ourScore = -1;
        int[] details = null;
        LeaderboardEntry[] scores = await Manager_Steam.ReturnScoresAndNeighbors(_lb, -3, 1);
        foreach (LeaderboardEntry e in scores)
        {            
            if (e.User.Name == Manager_Steam.Instance.steamName) { print("Found our score"); userName = e.User.Name; ourScore = e.Score; details = e.Details; }
        }
        print($"{userName}'s Converted Score Is: {ourScore}, \nDetails: ObstaclePoints - {details[0]}, \nUniqueCaptures - {details[1]}, \nCapturePoints - {details[2]}, \nRunTime {Manager_Steam.ConvertScoreToFloat(details[3],100)}, \nDistOfRun - {Manager_Steam.ConvertScoreToFloat(details[4],100)} ,");
    }

    public async void GetLeaderboard() // should eventually take data we want to request (like 'LeaderboardName')
    {
        if (retrivedLeaderboard) return;

        if (Manager_Steam.Instance)
        {
            try
            {
                var lb = await Manager_Steam.CreateOrFindLeaderboard(LeaderboardName, Steamworks.Data.LeaderboardSort.Descending, Steamworks.Data.LeaderboardDisplay.Numeric);
                if (lb.HasValue) { _lb = lb.Value; retrivedLeaderboard = true; }
                else Debug.LogError("leaderboard did not have value");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading leaderboards: {e.Message}"); // Optionally, you could handle the exception, e.g., retry logic or fallback behavior
            }
        }
        else Debug.LogError("Missing Steam Manager Instance");
    }


    private async void UpdateScores()
    {
        if (Manager_Steam.Instance)
            await Manager_Steam.SubmitLeaderboardUpdate(_lb, scoreToStore, detailsToStore);
    }

    [ContextMenu("ReplaceScoreTest")]
    private async void ReplaceScoreTest()
    {
        if (Manager_Steam.Instance)
            await Manager_Steam.ForceLeaderboardReplace(_lb, scoreToStore, detailsToStore);
    }

}



