using Steamworks;
using UnityEngine;

public class Manager_Steam : MonoBehaviour
{
    public static Manager_Steam Instance { get; private set; }

    private string steamName = "Not connected";
    private ulong steamId = 0;
    private uint steamAppId = 3673730;
    public bool appConnected { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
        appConnected = InitializeSteam();

        if (appConnected) print("steam connected");
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
            }
            else
            {
                Debug.LogError("Steam client not valid!");
            }

            return true;
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

    // Update is called once per frame
    void LateUpdate()
    {
        Steamworks.SteamClient.RunCallbacks();
    }

    void OnGUI()
    {
        // Simple on-screen text
        GUI.Label(new Rect(10, 10, 400, 30), $"Steam User: {steamName}");
        GUI.Label(new Rect(10, 30, 400, 30), $"Steam ID: {steamId}");
    }

    void OnApplicationQuit() // when we close play/editor we will stop "playing" the game
    {
        SteamClient.Shutdown();
    }
}
