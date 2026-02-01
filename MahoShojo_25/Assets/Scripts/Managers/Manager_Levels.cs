using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Levels : MonoBehaviour
{
    /// <summary>
    /// TODOs:
    /// + working on creating a level manager that can be our source of truth for levels we've completed and havent
    /// --> this means it will probably work with the steam manager
    /// --> benefit from being an instance
    /// --> List all of the scriptable objects we have as levels
    /// --> have a function for changing the environment
    /// 
    /// - things to consider
    /// --> black fade for level image so we can transition environment assets if needed
    /// --> when we beat a level we are shown an animation of an image and scoreboards
    /// --> we can go back to the menu or next level if we like
    /// --> 
    /// </summary>
    /// 

    public static Manager_Levels Instance { get; private set; }

    public PlatformScriptableObject[] levelsList;
    public LevelSelectData[] levelUis;

    private void Awake()
    {
        if (Instance != null && Instance != this) // If there is an instance, and it's not me, delete myself.
            Destroy(this);
        else
            Instance = this;
    }


    // Start is called before the first frame update
    void OnEnable()
    {
        //print("Level Manager: Enabled");
        if (levelsList.Length > 0) levelsList[0].levelUnlocked = true; // unlock the first level
        CheckLevelSaveData();
    }

    public void CheckLevelSaveData() //TODO: get data for levels we have unlocked
    {
        print("Level Manager: Data Loading");
        // load from file data to see what we unlocked or havent unlocked
        UpdateUnlockedLevels();
    }

    public void LoadLevel(int _levelId) // this may be called from a button press
    {
        print("Level Manager: Load Level Attempt");

        if (levelsList.Length == 0 || _levelId < 0) { Debug.LogError("Level Manager: Missing Level data ..."); return; }

        if (!levelsList[_levelId].levelUnlocked) { Debug.LogError("Level Manager: Level not unlocked... and should be unclickable"); return; }

        for (int i = 0; i < levelsList.Length; i++)
        {
            if(i == _levelId)
            {
                if(levelsList[i] == null) { Debug.LogError("Level Manager: Missing Level data Level"); return; }
                print($"Found Level To Load: {levelsList[i].levelName}");
                if (Manager_Platforms.Instance)
                    Manager_Platforms.Instance.UpdateCurrentLoadedLevel(levelsList[i]);               
            }
        }
    }

    public void UpdateUnlockedLevels()
    {
        print("Level Manager: Levels Unlocking");

        if (levelUis.Length == 0) { Debug.LogError("Missing Level data UI"); return; }

        for (int i = 0; i < levelUis.Length; i++)
        {
            if (levelsList.Length > i)
            {
                if (levelUis[i] && levelsList[i])
                {
                    levelUis[i].UpdateLevelButton(levelsList[i].levelName,
                        levelsList[i].levelUnlocked,
                        levelsList[i].buttonImage,
                        levelsList[i].levelBeat);
                }
                else
                    if (levelUis[i]) levelUis[i].UpdateLevelButton("???", false, null, false);
            }
            else
                if (levelUis[i]) levelUis[i].UpdateLevelButton("???", false, null, false);
        }
    }

   


}
