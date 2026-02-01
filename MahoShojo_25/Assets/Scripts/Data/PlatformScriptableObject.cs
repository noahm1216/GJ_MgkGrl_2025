using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Level_Num_BossName", menuName = "ScriptableObjects/LevelPlatforms", order = 0)]
public class PlatformScriptableObject : ScriptableObject
{
    public string levelName;
    public bool levelUnlocked;
    public bool levelBeat;
    public Sprite buttonImage;
    public Sprite levelRewardImage;
    public CustomConditionVariables levelWinConditions;

    [Space]
    [Header("Monsters To Spawn")]
    public bool spawnMonstersEndlessly;
    [Tooltip("The monsters added here will spawn in the order they are placed based on the conditions")]    
    public CustomMonsterSpawner[] monstersToSpawn;
    // maybe we have additional variables for
    // - obstacles we can spawn
    [Header("Levels To Load")]
    public List<CustomPlatformData> listOfSpawnablePlatforms = new List<CustomPlatformData>();

    [Space]
    [Header("Text To Show")]
    public List<CustomTextMessageData> listOfTextToShow = new List<CustomTextMessageData>();
}



// the custom data for messages
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
    public CustomConditionVariables requiredConditions;
    [HideInInspector] public bool hasPlayed;

    //public CustomTextMessageData(string _newName,)
    //{
    //    //messageNickname = _newName;
    //}

}//end of data text messages


// the custom data for conditions
[System.Serializable]
public class CustomConditionVariables
{
    public string conditionNickname;
    [Tooltip("The number of the below conditions that must be met. If enough of these conditions are met then the game will accept the results and proceed" +
        "\n ... If 0 then no conditions are required. If 5 then all conditions are required. (see condition notes about value settings) ")]
    [Range(0,7)] public int numberOfRequiredConditions = 1;
    [Space][Space][Header("Required Conditions")]
    [Tooltip("When NOT set to 0, these conditions must be met before this condition is considered met during a level")]
    [Range(0, 9999)] public float distanceTravel, timePassed, scoreAchieved;
    [Tooltip("When NOT set to 0, these conditions must be met before this condition is considered met during a level")]
    [Range(0, 999)] public int monstersSpawned, monstersCaptured, hitPointsCurrent, hitPointsChange;

    public CustomConditionVariables(string _newName, int _numCond, float _dist, float _time, float _score, int _monSpwn, int _monCptr, int _hpCur, int _hpChng)
    {
        conditionNickname = _newName;
        numberOfRequiredConditions = _numCond;
        distanceTravel = _dist;
        timePassed = _time;
        scoreAchieved = _score;
        monstersSpawned = _monSpwn;
        monstersCaptured = _monCptr;
        hitPointsCurrent = _hpCur;
        hitPointsChange = _hpChng;
    }

}//end of data conditions


// the custom data monster spawner
[System.Serializable]
public class CustomMonsterSpawner
{
    public string monsterNickname;
    public Transform monsterBosses;
    public CustomConditionVariables requiredConditions;

    //public CustomMonsterSpawner(string _newName,)
    //{
    //    //conditionNickname = _newName;
    //}

}//end of data monster spawner
