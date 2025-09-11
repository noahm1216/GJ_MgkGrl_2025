using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;


/// <summary>
/// <para>
/// Manages the functionality to call steam api (using facepunch). All tracking stats and achievements should be seperate objects or scripts from this one. (childed to this object to remain singleton)
/// </para>
/// </summary>
public class Manager_Steam : MonoBehaviour
{
    public static Manager_Steam Instance { get; private set; }

    public string steamName { get; private set; } = "Not connected";
    public ulong steamId { get; private set; } = 0;
    public uint steamAppId { get; private set; } = 3673730;
    public bool appConnected { get; private set; }  // may want to check " if (SteamClient.IsValid) " but this lets us check faster without calling steam API
    public Steamworks.Data.Image playerImage { get; private set; }




    private void Awake()
    {
        if (Instance != null && Instance != this) // If there is an instance, and it's not me, delete myself.
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);

        SteamUserStats.OnAchievementProgress += AchievementChanged;
    }

    private void Start()
    {
        appConnected = InitializeSteam();

        if (appConnected)
        {
            print("steam connected");
            CacheAvatarImage();
        }
        else print("Steam didnt connect");
    }

    private bool InitializeSteam()
    {
        try
        {
            //Steamworks.SteamClient.Init(480); // Steam's public testable app to see if connectivity is working
            Steamworks.SteamClient.Init(steamAppId); // Maho App ID
            if (SteamClient.IsValid)
            {
                steamName = SteamClient.Name;
                steamId = SteamClient.SteamId;
                Debug.Log($"Connected to Steam as {steamName} ({steamId})");
                return true;
            }
            else
            {
                Debug.LogError("Steam client not valid!");
            }

            return false;
        }
        catch (System.Exception e)
        {
            // Something went wrong - it's one of these:
            //
            //     Steam is closed?
            //     Can't find steam_api dll?
            //     Don't have permission to play app?
            //
            Debug.LogError($"Steam init failed: {e.Message}");
            return false;
        }
    }

    private void OnApplicationQuit() // when we close play/editor we will stop "playing" the game
    {
        if (appConnected)
        {
            StoreStats();
            SteamClient.Shutdown();
        }
    }

    private void LateUpdate()
    {
        Steamworks.SteamClient.RunCallbacks();
    }

    //private void OnGUI()
    //{
    //    // Simple on-screen text
    //    GUI.Label(new Rect(10, 10, 400, 30), $"Steam User: {steamName}");
    //    GUI.Label(new Rect(10, 30, 400, 30), $"Steam ID: {steamId}");
    //}


    #region STEAM AVATAR

    public Texture2D GetAvatarPlayerImage()
    {
        if (appConnected) return Cache.Avatar;
        else return null;
    }

    // Cache storage (Facepunch forgot to include this)
    public static class Cache
    {
        public static Texture2D Avatar;
    }

    public async void CacheAvatarImage() // if only need one image
    {
        // Get the task
        var avatarImage = await GetAvatar();

        // Cache Items (only if not null)
        if (avatarImage != null)
        {
            Cache.Avatar = avatarImage.Value.Covert();
        }
    }

    public async void CacheAvatarImages(List<ulong> steamIds) // if need multiple images
    {
        // Start all avatar fetch tasks
        var avatarTasks = steamIds.Select(id => SteamFriends.GetLargeAvatarAsync(id)).ToArray();

        // Await them all
        var avatars = await Task.WhenAll(avatarTasks);

        // Cache them (example: just caching the first one here)
        if (avatars[0] != null)
        {
            Cache.Avatar = avatars[0].Value.Covert();
        }
    }

    private static async Task<Image?> GetAvatar()
    {
        try
        {
            // Get Avatar using await
            return await SteamFriends.GetLargeAvatarAsync(SteamClient.SteamId);
        }
        catch (Exception e)
        {
            // If something goes wrong, log it
            Debug.Log(e);
            return null;
        }
    }


    #endregion steam avatar


    #region STATS

    public int GetStatInt(string _statName)
    {
        if (appConnected) return SteamUserStats.GetStatInt(_statName);
        else return -1;
    }

    public float GetStatFloat(string _statName)
    {
        if (appConnected) return SteamUserStats.GetStatFloat(_statName);
        else return -1;
    }

    public void SetStat(string _statName, int _amountSet)
    {
        if (appConnected)
        {
            SteamUserStats.SetStat(_statName, _amountSet);
            SteamUserStats.StoreStats();
        }
    }

    public void AddStat(string _statName, int _amountChange)
    {
        if (appConnected)
        {
            SteamUserStats.AddStat(_statName, _amountChange);
            SteamUserStats.StoreStats();
        }
    }

    public void StoreStats()
    {
        if (appConnected)
            SteamUserStats.StoreStats();
    }

    public void ResetStats(bool _andAchievements)
    {
        if (appConnected)
            SteamUserStats.ResetAll(_andAchievements);
    }

    #endregion stats


    #region ACHIEVEMENTS

    public void AchievementChanged(Achievement ach, int currentProgress, int progress)
    {
        if (ach.State)
        {
            Debug.Log($"{ach.Name} WAS UNLOCKED!");
        }
    }

    #endregion achievements


    #region LEADERBOARDS // https://wiki.facepunch.com/steamworks/Leaderboards

    public static async Task<Steamworks.Data.Leaderboard?> CreateOrFindLeaderboard(string _boardName, LeaderboardSort _sortType, LeaderboardDisplay _displayType)
    {
        try
        {
            return await SteamUserStats.FindOrCreateLeaderboardAsync(_boardName, _sortType, _displayType);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        return null;
    }

    public static async Task<Steamworks.Data.Leaderboard?> OnlyFindLeaderboard(string _boardName)
    {
        try
        {
            return await SteamUserStats.FindLeaderboardAsync(_boardName);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        return null;
    }

    /// <summary>
    ///     This function will only replace your last score if the new one is better.
    /// </summary>
    /// <param name="leaderboard"></param>
    /// <param name="value"></param>
    /// <param name="details"></param>
    public static async Task SubmitLeaderboardUpdate(Steamworks.Data.Leaderboard _leaderboard, int _value, int[] details = null)
    {
        var leaderboardUpdate = await _leaderboard.SubmitScoreAsync(_value, details ?? Array.Empty<int>());
        if (!leaderboardUpdate.HasValue)
        {
            Debug.LogError("leaderboardUpdate is null");
            return;
        }

        Debug.Log(leaderboardUpdate.Value);
    }

    /// <summary>
    ///     Force your score to be replaced
    /// </summary>
    /// <param name="leaderboard"></param>
    /// <param name="value"></param>
    /// <param name="details"></param>
    public static async Task ForceLeaderboardReplace(Steamworks.Data.Leaderboard _leaderboard, int _value, int[] details = null)
    {
        var leaderboardUpdate = await _leaderboard.ReplaceScore(_value, details ?? Array.Empty<int>());
        if (!leaderboardUpdate.HasValue)
        {
            Debug.LogError("leaderboardUpdate is null");
            return;
        }

        Debug.Log(leaderboardUpdate.Value);
    }    

    public float ConvertScoreToInt(int _score, int _multiplier)
    {
        return _score * _multiplier;
    }

    public int ConvertScoreToFloat(int _score, int _multiplier)
    {
        return Mathf.RoundToInt(_score * _multiplier);
    }
    #endregion leaderboards


    #region SCREENSHOTS

    public void TakeScreenshot()
    {
        SteamScreenshots.TriggerScreenshot();
    }

    #endregion screenshots


}

public static class SteamImageExtensions
{
    // Make 100% sure we extend the Facepunch type:
    public static Texture2D Covert(this Steamworks.Data.Image image)
    {
        var avatar = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.ARGB32, false);
        avatar.filterMode = FilterMode.Trilinear;

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                var p = image.GetPixel(x, y);
                avatar.SetPixel(x, (int)image.Height - y,
                    new UnityEngine.Color(p.r / 255f, p.g / 255f, p.b / 255f, p.a / 255f));
            }
        }

        avatar.Apply();
        return avatar;
    }
}


public struct LeaderboardScore // Struct to hold data for ease-of-use and re-use-ability
{
    public readonly int Score;
    public readonly int SomeOtherValue;
    public readonly int[] Details;

    public LeaderboardScore(int score, int someOtherValue)
    {
        Score = score;
        SomeOtherValue = someOtherValue;
        Details = new[] { someOtherValue };
    }
}

