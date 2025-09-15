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
    public Transform monsterBoss;
    // maybe we have additional variables for
    // - monsters we can spawn
    // - obstacles we can spawn
    [Header("Levels To Load")]
    public List<CustomPlatformData> listOfSpawnablePlatforms = new List<CustomPlatformData>();
}
