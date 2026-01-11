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
}
