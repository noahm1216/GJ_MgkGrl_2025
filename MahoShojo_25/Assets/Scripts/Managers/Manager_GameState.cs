using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class Manager_GameState : MonoBehaviour
{
    public static Manager_GameState Instance { get; private set; }

    public enum GAMESTATE {Menu, Playing, Paused, Lost, Won }
    public enum GAMEMODE {None, Story, Endless }
    public GAMESTATE currentState;// { get; private set; }
    private GAMESTATE stateWhenPaused; // store whatever game state we were in
    public GAMEMODE currentMode { get; private set; }

    public KeyCode key_Pause1 = KeyCode.P, key_Pause2 = KeyCode.Escape;

    public int scoreTotal { get; private set; } // the score we get from capturing & other things
    public int obstaclePoints { get; private set; } // TODO: Break this out into the different obstacles points and amounts so we can track all of the points and amounts as stats?
    public int capturedCreatues_Unique { get; private set; }
    public int capturedPoints { get; private set; }
    public float timeOfCurrentGameRun { get; private set; }// how long we have been in play-mode of our current run
    public float distanceOfCurrentGameRun { get; private set; } // how long we have been traveling in our current run

    public CustomConditionVariables dataSinceLevelStarted { get; private set; }
    public CustomConditionVariables dataSinceMonsterSpawn { get; private set; }


    public Transform[] objectsToResetPositions;
    private Vector3[] startPositions, startScales;
    private Quaternion[] startRotations;
    private bool[] wasEnabled;
   [HideInInspector] public List<Transform> objectsSpawnedDuringRuntime = new List<Transform>();


    public int mostRecentLevel {get; private set; } = 0; 

    [Space]
    [Header("Hit Points \n______________")]
    public int hitpoints = 1;
    private int currentHitPoints;

    [Space]
    public UnityEvent onPauseInMainMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this) // If there is an instance, and it's not me, delete myself.
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        SaveObjectDataForRestart();
        RestartVariables(true);      
    }


    public void ChangeState(GAMESTATE _newState)
    {     
        switch (_newState)
        {
            case GAMESTATE.Menu:
                // load main menu canvas and scene
                break;
            case GAMESTATE.Playing:
                // update any code so we can begin early tutorial
                break;
            case GAMESTATE.Paused:
                // pause the game and open the menu for pause
                break;
            case GAMESTATE.Lost:
                // show the screen when we lose and offer restart || or go after X time
                break;
            case GAMESTATE.Won:
                // show end game cutscene
                break;
            default:
                Debug.Log($"WARNING: Case for Gamestate '{currentState}' - not found");
                break;
        }
        currentState = _newState;
    }

    public void ChangeMode(GAMEMODE _newMode)
    {       
        currentMode = _newMode;
    }

    // Update is called once per frame
    private void Update()
    {
        StateChecker();
        CheckForInputs();
    }

    private void StateChecker()
    {
        switch (currentState)
        {
            case GAMESTATE.Menu:
                break;
            case GAMESTATE.Playing:
                timeOfCurrentGameRun += Time.deltaTime;
                dataSinceMonsterSpawn.timePassed += Time.deltaTime;
                dataSinceLevelStarted.timePassed += Time.deltaTime;
                break;
            case GAMESTATE.Paused:
                break;
            case GAMESTATE.Lost:
                break;
            case GAMESTATE.Won:
                break;
            default:
                Debug.Log($"WARNING: Case for Gamestate '{currentState}' - not found");
                break;
        }
    }

    private void CheckForInputs()
    {
        //cheatcode
        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Space))
        {
            CaptureChange(6, 9999);
            Debug.Log("Cheated your way to winning... now you just have to fall off the map");
        }

        if(Input.GetKeyDown(key_Pause1) || Input.GetKeyDown(key_Pause2)) // pause toggle
        {
            if(Manager_GameState.Instance && Manager_GameState.Instance.currentState == GAMESTATE.Menu) { onPauseInMainMenu?.Invoke(); return; }
            PauseToggle();
        }
    }

    public void PauseToggle()
    {
        print("calling game pause");
        if (currentState != GAMESTATE.Paused)
        {
            stateWhenPaused = currentState;
            currentState = GAMESTATE.Paused;
            if (Manager_UI.Instance)
                Manager_UI.Instance.PauseToggle();

            Time.timeScale = 0;
        }
        else
        {
            currentState = stateWhenPaused;
            if (Manager_UI.Instance)
                Manager_UI.Instance.PauseToggle();

            Time.timeScale = 1;
        }
    }

    public void CaptureChange(int _amountChange, int _changePoints)
    {
        capturedCreatues_Unique += _amountChange;
        dataSinceMonsterSpawn.monstersCaptured += _amountChange;
        dataSinceLevelStarted.monstersCaptured += _amountChange;

        capturedPoints += _changePoints;
        dataSinceMonsterSpawn.scoreAchieved += _changePoints;
        dataSinceLevelStarted.scoreAchieved += _changePoints;

        TallyPoints();

        if (Manager_TutorialUI.Instance)
        { Manager_TutorialUI.Instance.SetCaptureShowcase(capturedCreatues_Unique); }
    }

    public void ObstaclePointChange(int _changePoints)
    {
        obstaclePoints += _changePoints;
        dataSinceMonsterSpawn.scoreAchieved += _changePoints;
        dataSinceLevelStarted.scoreAchieved += _changePoints;
        TallyPoints();
    }

    public void TallyPoints()
    {
        scoreTotal = capturedPoints + obstaclePoints;
    }

    public bool ChangeHitPoints(int _amountChange)
    {
        currentHitPoints += _amountChange;
        dataSinceMonsterSpawn.hitPointsCurrent = currentHitPoints;
        dataSinceMonsterSpawn.hitPointsChange += _amountChange;
        dataSinceLevelStarted.hitPointsCurrent = currentHitPoints;
        dataSinceLevelStarted.hitPointsChange += _amountChange;

        if (currentHitPoints > hitpoints)
            currentHitPoints = hitpoints;
        if (currentHitPoints <= 0)
        { GameOver(); return true; } // returns true to say we did die

        return false; // returns false to say we didnt die
    }

    public void ChangeDistanceTraveled(float _amountChange)
    {
        distanceOfCurrentGameRun += _amountChange;
        dataSinceMonsterSpawn.distanceTravel += _amountChange;
        dataSinceLevelStarted.distanceTravel += _amountChange;
    }

    public bool CheckMetConditions(CustomConditionVariables _chkConASource, CustomConditionVariables _chkConBCompare)
    { // this checks the requirements of the monster data and returns how many are NOT zero and > variable

        if (_chkConASource == null || _chkConBCompare == null) { print("Manager GameState: Can't Compare Null Conditions"); return false; }

        //print($"Comparing Conditions: A:{_chkConASource.conditionNickname} - B:{_chkConBCompare.conditionNickname}");
        bool atLeastOneCondition = (_chkConBCompare.distanceTravel > 0 || _chkConBCompare.timePassed > 0 || _chkConBCompare.scoreAchieved > 0 ||
            _chkConBCompare.monstersSpawned > 0 || _chkConBCompare.monstersCaptured > 0 || _chkConBCompare.hitPointsCurrent > 0 || _chkConBCompare.hitPointsChange > 0);
        if (!atLeastOneCondition) { print("Manager GameState: No Conditions Possible"); return false; }

        int metConditionsFromA = 0;
        if (_chkConBCompare.distanceTravel > 0 && _chkConASource.distanceTravel >= _chkConBCompare.distanceTravel) metConditionsFromA++;
        if (_chkConBCompare.timePassed > 0 && _chkConASource.timePassed >= _chkConBCompare.timePassed) metConditionsFromA++;
        if (_chkConBCompare.scoreAchieved > 0 && _chkConASource.scoreAchieved >= _chkConBCompare.scoreAchieved) metConditionsFromA++;
        if (_chkConBCompare.monstersSpawned > 0 && _chkConASource.monstersSpawned >= _chkConBCompare.monstersSpawned) metConditionsFromA++;
        if (_chkConBCompare.monstersCaptured > 0 && _chkConASource.monstersCaptured >= _chkConBCompare.monstersCaptured) metConditionsFromA++;
        if (_chkConBCompare.hitPointsCurrent > 0 && _chkConASource.hitPointsCurrent >= _chkConBCompare.hitPointsCurrent) metConditionsFromA++;
        if (_chkConBCompare.hitPointsChange > 0 && _chkConASource.hitPointsChange >= _chkConBCompare.hitPointsChange) metConditionsFromA++;
        //print($"Conditions Possible: {possibleConditions} \nConditions Met: {metConditionsFromA} out of {_chkConBCompare.numberOfRequiredConditions} total required" +
        //    $"\n CanSpawn = {metConditionsFromA >= _chkConBCompare.numberOfRequiredConditions} + {possibleConditions > 0} = {metConditionsFromA >= _chkConBCompare.numberOfRequiredConditions && possibleConditions > 0}");
        return (metConditionsFromA >= _chkConBCompare.numberOfRequiredConditions && atLeastOneCondition);
    }

    public void ResetDataSinceMonsterSpawn()
    {
        dataSinceMonsterSpawn = ResetCustomConditionsData(dataSinceMonsterSpawn);
    }

    public CustomConditionVariables ResetCustomConditionsData(CustomConditionVariables _dataToReset)
    { // reset the data of our variables for tracking monster data. This will allow each monster to have its own conditions if called on capture/despawn

        CustomConditionVariables _newData = new CustomConditionVariables(
            "ResetDataTracker", 0, 0, 0, 0, 0, 0, currentHitPoints, 0);

        if (_dataToReset != null)
        { _newData.conditionNickname = _dataToReset.conditionNickname; _newData.numberOfRequiredConditions = _dataToReset.numberOfRequiredConditions; }

        return _newData;
    }



    #region GameOver And Restarts

    public void UpdateRecentLevel(int _levelId)
    {
        mostRecentLevel = _levelId;
    }

    public void WonTheGame() // TODO: rename to "WonTheLevel" (and update any functions calling it)
    {
        ChangeState(GAMESTATE.Won);
        if (Manager_Levels.Instance)
        { Manager_Levels.Instance.BeatLevel(mostRecentLevel); Manager_Levels.Instance.UnlockLevel(mostRecentLevel+1); }
        if (Manager_TutorialUI.Instance)
            Manager_TutorialUI.Instance.ShowWinGameScreen();
        if (Manager_Audio.Instance)
            Manager_Audio.Instance.SwitchClip(Manager_Audio.Instance.aSourceMusic, Manager_Audio.Instance.clipMusic_Win);
    }

    public void StartGameButton()
    {
        timeOfCurrentGameRun = 0; // Reset Timer
        distanceOfCurrentGameRun = 0; // Reset distance traveled
        ChangeState(GAMESTATE.Playing);
        if (Manager_Platforms.Instance) Manager_Platforms.Instance.ResetMonsterTimers();
    }

    public void RestartGameButton()
    {
        RestartGame(false);
    }

    public void GameOver()
    {
        ChangeState(GAMESTATE.Lost);
        if (Manager_TutorialUI.Instance)
            Manager_TutorialUI.Instance.ShowGameOverScreen();
    }


    public void RestartGame(bool _totalRestart)
    {
        if (currentState == GAMESTATE.Paused)
            PauseToggle();
        ChangeState(GAMESTATE.Menu);
        RestartVariables(false);
        //if (Manager_Platforms.Instance)
        //    Manager_Platforms.Instance.ChangeMonsterVariables(true, false, false);

        if (_totalRestart)
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
        else
            LoadbjectDataForRestart();
    }


    private void RestartVariables(bool _callingFromStart)
    {       
        currentHitPoints = hitpoints;
        if (dataSinceLevelStarted == null) { dataSinceLevelStarted = ResetCustomConditionsData(null); }
        if (dataSinceMonsterSpawn == null) { dataSinceMonsterSpawn = ResetCustomConditionsData(null); }
        dataSinceMonsterSpawn.hitPointsCurrent = currentHitPoints;
        dataSinceLevelStarted.hitPointsCurrent = currentHitPoints;
        CaptureChange(-capturedCreatues_Unique, -capturedPoints);

        if (!_callingFromStart)
        {
            ChangeState(GAMESTATE.Playing);
            StartCoroutine(ClearReferences());
            if (Manager_TutorialUI.Instance)
                Manager_TutorialUI.Instance.ResetTutorial();
        }
    }



    private void SaveObjectDataForRestart()
    {
        if (objectsToResetPositions.Length > 0) // store references for fast restarts
        {
            startPositions = new Vector3[objectsToResetPositions.Length];
            startScales = new Vector3[objectsToResetPositions.Length];
            startRotations = new Quaternion[objectsToResetPositions.Length];
            wasEnabled = new bool[objectsToResetPositions.Length];

            for (int i = 0; i < objectsToResetPositions.Length; i++)
            {
                startPositions[i] = objectsToResetPositions[i].position;
                startScales[i] = objectsToResetPositions[i].localScale;
                startRotations[i] = objectsToResetPositions[i].rotation;
                wasEnabled[i] = objectsToResetPositions[i].gameObject.activeSelf;
            }
        }
    }

    private void LoadbjectDataForRestart()
    {
        if (objectsSpawnedDuringRuntime.Count > 0)
        {
            Transform deleteRef = null;
            for (int i = 0; i < objectsSpawnedDuringRuntime.Count; i++)
            {
                if (objectsSpawnedDuringRuntime[i] != null)
                {
                    deleteRef = objectsSpawnedDuringRuntime[i];
                    objectsSpawnedDuringRuntime.RemoveAt(i);
                    Destroy(deleteRef.gameObject);
                }               
            }

        }     

        if (objectsToResetPositions.Length > 0) // retrieve references for fast restarts
        {           
            for (int i = 0; i < objectsToResetPositions.Length; i++)
            {
                objectsToResetPositions[i].position = startPositions[i];
                objectsToResetPositions[i].localScale = startScales[i];
                objectsToResetPositions[i].rotation = startRotations[i];
                objectsToResetPositions[i].gameObject.SetActive(wasEnabled[i]);
            }
        }       
    }

    private IEnumerator ClearReferences()
    {
        yield return new WaitForSeconds(0.5f);

        if (Manager_Platforms.Instance)
            Manager_Platforms.Instance.RemoveAnyNullPlatforms();
    }
    #endregion GameOver And Restarts
}
