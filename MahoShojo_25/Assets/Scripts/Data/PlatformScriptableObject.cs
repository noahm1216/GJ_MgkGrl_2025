using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Level_Num_BossName", menuName = "ScriptableObjects/LevelPlatforms", order = 0)]
public class PlatformScriptableObject : ScriptableObject
{
    public string levelName;
    public bool levelUnlocked;
    public bool levelBeat;
    public Sprite monsterImage;
    public Sprite levelRewardImage;
    [Tooltip("The monsters added here will spawn in the order they are placed")]
    public Transform[] monsterBosses;
    [Tooltip("When not set to 0, these conditions must be met before the next boss will show up in a level")]
    public float distanceBeforeBossShowsUp, timeBeforeBossShowsUp, scoreBeforeBossShowsUp;
    // maybe we have additional variables for
    // - obstacles we can spawn
    // - text the game will play/show
    [Header("Levels To Load")]
    public List<CustomPlatformData> listOfSpawnablePlatforms = new List<CustomPlatformData>();

    [Space]
    [Header("Text To Show")]
    public List<CustomTextMessageData> listOfTextToShow = new List<CustomTextMessageData>();
}



// the custom data for platforms
[System.Serializable]
public class CustomTextMessageData
{
    public string messageNickname;
    [TextArea] [Tooltip("The text that will show on screen")]
    public string textToSay;
    [Tooltip("The character icon image for who will show up when the line is being spoken")]
    public Sprite characterIconSpeaking;
    [Tooltip("When assigned, this text will not play before the assigned text even if it's condition is met")]
    public CustomTextMessageData requiredTextBefore;
    [Tooltip("When not set to 0, these conditions must be met before this text will show up in a level")]
    public float distanceBeforeTextShows, timeBeforeTextShows, scoreBeforeTextShows;
    [Tooltip("When not set to 0, these conditions must be met before this text will show up in a level")]
    public int monstersSpawnedBeforeTextShows, monstersCapturedBeforeTextShows;
    [HideInInspector]
    public bool hasPlayed;
    
    //public CustomTextMessageData(string _newName,)
    //{
    //    //abilityNickname = _newName;
    //}

}//end of data text messages
