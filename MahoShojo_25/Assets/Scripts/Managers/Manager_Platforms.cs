using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// <para> handles the movement and spawning indefinitely of platforms</para>
/// </summary>
public class Manager_Platforms : MonoBehaviour
{
    public static Manager_Platforms Instance { get; private set; }    

    [Header("Input Keys\n______________")]
    [Tooltip("This keycode will move the maps to the Left (as if we are going right)")]
    public KeyCode key_MovePlatformsLeft = KeyCode.D;
    [Tooltip("This keycode will move the maps to the Right (as if we are going left)")]
    public KeyCode key_MovePlatformsRight = KeyCode.A;
    [Tooltip("When checked, the platforms will move on their own")]
    public bool automaicallyMoveRight = true;

    private bool pressedMove;
    private float timePressedJumpOrMove;
    private float timeUntilShowTutorial = 20;
    [Tooltip("This keycode will move the maps to the Left (as if we are going right)")]
    private KeyCode key_MovePlatformsLeft2 = KeyCode.RightArrow;
    [Tooltip("This keycode will move the maps to the Right (as if we are going left)")]
    private KeyCode key_MovePlatformsRight2 = KeyCode.LeftArrow;

    // dash right/ left ability
    [Space]
    [Space]
    [Header("Dash Rules\n______________")]    
    [Tooltip("When true, dashing requires you to press forward twice within the blow variables")]
    public bool dashRequiresDoubleTap;
    [Tooltip("The amount of time allowed until our double-input of a direction acts as a dash (higher number means more tolerance for slow key-presses)")]
    [Range(0.1f, 3)]
    public float timeWindowForDashing = 0.75f;
    [Tooltip("The multiplier for speed when we dash")]
    [Range(0.1f, 1000)]
    public float dashingSpeedMultiplier = 200f;
    [Tooltip("The amount of time we will continue to dash (or at least our speed will reflect it)")]
    [Range(0.1f, 10)]
    public float dashingLastingTime = 2f;
    [Tooltip("The functions that will occur when we first dash")]
    public UnityEvent onDashEvent;

    private float dashInputStamp;
    private KeyCode lastKeyPressed;
    public bool isDashing { get; private set; }
    private float dashStartTimeStamp;


    [Space]
    [Space]
    [Header("Platform Objects\n______________")]
    [Tooltip("The starting Variable for how our speed is calculated")]
    [Range(0.1f, 30)]
    public float speedBase = 14f;   
    [Tooltip("Change how fast / slow we can go at top speed")]
    [Range(0.1f, 10.0f)] public float speedLimiting = 1f;
    public bool startPlayerSlowly = true;
    [Tooltip("Change how fast / slow we ramp up to full speed")]
    [Range(0.1f, 6f)] public float speedAcceleration = 1;
    [Tooltip("Change how fast / slow we ramp down when we let go of the controls")]
    [Range(0.1f, 6f)] public float speedDeceleration = 1;
    [Tooltip("When changing direction, what percent of speed should we cut? (0.5 means we cut half of our speed)")]
    [Range(0.1f, 1f)] public float speedChangeOnDirectionChange = 0.33f;
    [Tooltip("When not touching anything (in air). our speed will be adjusted by this amound (0.5 means we cut half of our speed)")]
    [Range(0.00f, 2f)] public float speedChangeWhileInAir = 0.66f;

    private float startSpeedAccel;
    private float startSpeedAccelStamp, startSpeedAceelWaitTime = 1;
    private float inputXYTime;
    public int dir { get; private set; } = 1;
    public bool isBlocked { get; private set; }
    public Transform blockerObj { get; private set; }
    private bool checkingBlockerData;
    public bool playerInAir { get; private set; }
    private float blockedTimeStamp;
    private float blockedTimeUntilControlsPopUp = 7;
   

    [Space]
    [Space]
    [Header("Platform Objects\n______________")]   
    [Tooltip("a transform of the object holding all of our children")]
    public Transform parentOfMapModelsToMove;

    [Range(1, 10)] public int platformsToSpawnOnStart = 3;
    [Range(1,20)] public int platformsToKeepOnScreen = 5;

    [Tooltip("A list of platforms we can spawn during runtime. Stats within each platform should allow for some customization")]
    public List<CustomPlatformData> listOfSpawnablePlatforms = new List<CustomPlatformData>();
    public List<BehaviorPlatform> spawnedPlatformsInPlay = new List<BehaviorPlatform>();

    public bool stopSpawningPlatforms { get; private set; }


    [Space]
    [Space]
    [Header("Monster Objects\n______________")]
    [Tooltip("The prefabs of the monsters we want to spawn in the order we want to spawn them")]
    public Transform[] monstersToSpawnInOrder;
    [Tooltip("The acceptable distance of those monsters")]
    public Vector3[] distanceOkayToSpawnFromPlayer;
    public float waitTimeUntilDespawn = 20;
    public float waitTimeAfterCapture = 10;

    public bool readyToSpawn { get; private set; } // decidor when we can spawn
    public bool monsterIsInPlay { get; private set; } // so we dont over spawn
    public int monstersSpawned;//{ get; private set; } // tracking how manywe've spawned
    public Transform spawnedMonster { get; private set; } // the monster we want to track
    private BehaviorMonster spawnedMonsterBehavior;
    [HideInInspector] public bool monsterSignaledCapture; // the monster can signal its capture so it does not despawn by accident while being captured
    //private List<Transform> allSpawnedMonsters = new List<Transform>(); // to pool the monsters, but this should come in later versions
    private float lastCaptureTimeStamp;
    private float timeMonsterSpawned;
    private float waitTimeUntilDespawnDynamic;


    [Space]
    [Space]
    [Header("Pocki Objects\n______________")]
    public Transform pockiBoxObjPrefab;
    public bool playerUnlockedPockiBox;
    public int pockiBoxSticks = 1;
    public float spawnXOffset = 5;
    public LayerMask pockiBoxInteractionLayers;
    public bool spawnPockiBoxOnTimer;
    public float timeUntilPockiBoxSpawn = 10;
    private Transform spawnedPockiBoxObj;
    private float pockiBoxSpawnStamp;


    [Space]
    [Header("Camera\n______________")]
    public BehaviorCameraFollower ref_BehaviorCameraFollower;


    private bool FoundErrors()
    {
        if (listOfSpawnablePlatforms.Count == 0)
        { Debug.Log("ERROR: Missing Platforms To Spawn"); return true; }

        if (!parentOfMapModelsToMove)
        { Debug.Log("ERROR: Missing Platforms To Spawn"); return true; }

        return false;
    }

    private void Awake()
    {        
        if (Instance != null && Instance != this) // If there is an instance, and it's not me, delete myself.
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        if (FoundErrors())
            return;

        SpawnStartingPlatforms();

        lastCaptureTimeStamp = Time.time + (waitTimeAfterCapture * 1.2f); // the first monster shouldnt spawn right away, but the normal pacing is good for most
        ChangeMonsterVariables(true, false, false);

        if (startPlayerSlowly)
            startSpeedAccel = 0;
        else
            startSpeedAccel = 1;

        if (!ref_BehaviorCameraFollower && Camera.main)
            Camera.main.TryGetComponent(out ref_BehaviorCameraFollower);

        waitTimeUntilDespawnDynamic = waitTimeUntilDespawn;
    }

    // Update is called once per frame
    private void Update()
    {
        if (FoundErrors())
            return;

        if(Manager_GameState.Instance) // if we have the game manager then we want things to look a specific way
        {
            ReactToGameManager();
            if (Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing)
                return;
        }
        CheckForTutorial(); // TODO: Maybe we can delay how frequently we check for some of these (instead of every frame)
        CheckTimeStamps();
        CheckForInputs();
        CheckDashing();
        MoveMaps();
        CheckForMonsterSpawn();
        StartSpeedIsRampedUp();
        CheckForObstacles();
        if (spawnPockiBoxOnTimer && Time.time > pockiBoxSpawnStamp + timeUntilPockiBoxSpawn && !playerUnlockedPockiBox)
            SpawnOrMovePockiBox();        
    }


    #region HELPERS
    private void CheckTimeStamps()
    {
        if (!isBlocked)
            blockedTimeStamp = Time.time;
        if (Time.time > blockedTimeStamp + blockedTimeUntilControlsPopUp && Manager_TutorialUI.Instance)
        { Manager_TutorialUI.Instance.QueueMessage("Controls_Jump"); blockedTimeStamp = Time.time; }
    
    }

    private void CheckForTutorial()
    {
        if (Time.time > timePressedJumpOrMove + timeUntilShowTutorial && !pressedMove)
        {
            if (Manager_TutorialUI.Instance)
            {
                Manager_TutorialUI.Instance.QueueMessage("Controls_Move");
                Manager_TutorialUI.Instance.QueueMessage("Controls_Dash");
                timePressedJumpOrMove = Time.time;
            }
        }
    }

    public void ResetToBeginning()
    {
        for(int i = 0; i < spawnedPlatformsInPlay.Count; i++)
        {
            if (spawnedPlatformsInPlay[i] == null)
                spawnedPlatformsInPlay.RemoveAt(i);
            else
                spawnedPlatformsInPlay[i].ResetToStart();
        }

        lastCaptureTimeStamp = Time.time;
        ChangeSpawningPlatforms(true);
        readyToSpawn = true;
        ResetMonsterTimers();
    }

    private void ReactToGameManager()
    {
        // can put code for animations or other objects to enable / disable
        // also code for checking if we are done or finished with the game

        if (Manager_GameState.Instance.currentState == Manager_GameState.GAMESTATE.Menu)
        {
            startSpeedAccelStamp = Time.time;
            timePressedJumpOrMove = Time.time;
            ResetMonsterTimers();
        }
        if (Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing) // make sure that if we are not playing, it doesnt change our spawn timers
        {
           
        }
    }

    public void ResetMonsterTimers()
    {
        lastCaptureTimeStamp = Time.deltaTime;
        timeMonsterSpawned = Time.time;
        waitTimeUntilDespawnDynamic = waitTimeUntilDespawn;
        readyToSpawn = true;
    }


    public void ChangeIsBlocked(bool _isBlocked, Transform _blockerObj)
    {
        //print($"Changin isBlocked to: {_isBlocked}");
        isBlocked = _isBlocked;
        blockerObj = _blockerObj;
    }

    public void ChangePlayerInAir(bool _inAir)
    {
        playerInAir = _inAir;
    }

    public float CurrentSpeed() // calculatedSpeed
    {
        if (isBlocked)
            return 0;
        if (Manager_GameState.Instance && Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing) return 0;        

        float speed = ((speedBase * dir * inputXYTime * Time.deltaTime) * speedLimiting) * startSpeedAccel;
        speed = Mathf.Round(speed * 100f) / 100f; // rounding 2 Decimals for other reliable calculations
        if (playerInAir && !isDashing)
            speed *= speedChangeWhileInAir;
        if (isDashing)
            speed *= (dashingSpeedMultiplier * Time.deltaTime);
        return speed; // if we dont round we dont need to create a new variable
    }

    public bool StartSpeedIsRampedUp() // lets us know if we are at full speed and makes adjustments if needed
    {      
        if(Time.time > startSpeedAccelStamp + startSpeedAceelWaitTime && startSpeedAccel < 2)
        {
            if (startSpeedAceelWaitTime < 20)
                startSpeedAceelWaitTime++;
            startSpeedAccelStamp = Time.time;
            startSpeedAccel += 6f * Time.deltaTime;
            return false;
        }

        if (startSpeedAccel >= 1)
        { print("DONE SPEEDING UP"); startSpeedAccel = 1; return true; }
        else
            return false;              
    }

    private void CheckDashing()
    {
        if (!isDashing)
            dashStartTimeStamp = Time.time;
        if (isDashing && Time.time > dashStartTimeStamp + dashingLastingTime)
            isDashing = false;
    }

    private void DashAction()
    {        
        isDashing = true;
        onDashEvent.Invoke();
    }
    #endregion helpers

    #region INPUTS

    private void CheckForInputs()
    {
        if (Input.GetKey(key_MovePlatformsRight) || Input.GetKey(key_MovePlatformsRight2)) // going left  
        {
            pressedMove = true;

            if (dir < 0) // check and set directions to be LEFT (platforms going right)
            { inputXYTime *= speedChangeOnDirectionChange; dir = 1; }
            inputXYTime += 1 * Time.deltaTime * speedAcceleration;
            if (inputXYTime > 1) { inputXYTime = 1; }

            if (ref_BehaviorCameraFollower)
                ref_BehaviorCameraFollower.StoreChangeState(BehaviorCameraFollower.CameraFocusState.MovingBackwards, false);
        }
        else if (Input.GetKey(key_MovePlatformsLeft) || Input.GetKey(key_MovePlatformsLeft2) || automaicallyMoveRight ) // going right
        {
            if (Input.GetKeyDown(key_MovePlatformsLeft) || Input.GetKey(key_MovePlatformsLeft2)) // pressed forward to dash
            {
                pressedMove = true;

                if (dashRequiresDoubleTap && !isDashing) // if we must press it twice
                    if (lastKeyPressed == key_MovePlatformsLeft && Time.time < dashInputStamp + timeWindowForDashing) // TODO : Check for if dashing so we have to time it, and animation / VFX spot
                        DashAction();
                    else
                    { lastKeyPressed = key_MovePlatformsLeft; dashInputStamp = Time.time; } // reset
                else if (!dashRequiresDoubleTap && !isDashing) // if we only have to press it once
                    DashAction();
            }

            if (dir > 0) // check and set directions to be Right (platforms going left)
            { inputXYTime *= speedChangeOnDirectionChange; dir = -1; }
            inputXYTime += 1 * Time.deltaTime * speedAcceleration;
            if (inputXYTime > 1) { inputXYTime = 1; }

            if (ref_BehaviorCameraFollower)
                ref_BehaviorCameraFollower.StoreChangeState(BehaviorCameraFollower.CameraFocusState.MovingForward, false);
        }      
        else // not pressing left or rights
        {
            inputXYTime -= 1 * Time.deltaTime;
            if (inputXYTime < 0) { inputXYTime = 0; }
        }
    }

    private void CheckForObstacles()
    {
        if (blockerObj && isDashing && !checkingBlockerData)  // check if we're blocked by an obstacle while dashing
        {
            print("TODO: Get blocker obj's script references and apply information");
            checkingBlockerData = true;
            BehaviorObstacles obstacleScript = null;
            blockerObj.TryGetComponent(out obstacleScript);
            if (obstacleScript && obstacleScript.thisObsType == BehaviorObstacles.obstacleType.Senpai)
                obstacleScript.Interacted(transform, null, BehaviorObstacles.signalType.Dash);
            checkingBlockerData = false;
        }
    }

    #endregion INPUTS

    #region PLATFORMS

    public void ChangeSpawningPlatforms(bool _stop)
    {
        stopSpawningPlatforms = _stop;
    }


    public void RemoveAllPlatforms()
    {
        for (int i = 0; i < spawnedPlatformsInPlay.Count; i++)
        {
            if (spawnedPlatformsInPlay[i] != null) { Destroy(spawnedPlatformsInPlay[i].gameObject); }
        }
        spawnedPlatformsInPlay.Clear();
    }

    public void SpawnStartingPlatforms()
    {
        for (int i = 0; i < platformsToKeepOnScreen - 1; i++)
            SpawnOrPoolPlatform(null, true);
    }

    public void SpawnNewPlatformFromEdge() // when player reaches end of env chunk trigger
    {
        if (stopSpawningPlatforms)
        {
            if (spawnedPlatformsInPlay.Count > 1)
            {
                for (int i = 1; i < spawnedPlatformsInPlay.Count; i++)
                    RemoveSpecificPlatform(i, false);
            }
            return; // stop here if we dont want to spawn anymore
        }

        if (Manager_GameState.Instance)
            Manager_GameState.Instance.objectsSpawnedDuringRuntime.Add(SpawnOrPoolPlatform(null, false));
        else
            SpawnOrPoolPlatform(null, false);
    }

    public void SpawnPlatformsOnDelay(float _delay)
    {
        StartCoroutine(SpawnPlatformsOnDelayEnum(_delay));
    }

    public IEnumerator SpawnPlatformsOnDelayEnum(float _delay)
    {
        print($"spawning platforms on delay of: {_delay} seconds");
        yield return new WaitForSeconds(_delay);
        SpawnStartingPlatforms();
    }


    private void MoveMaps()
    {
        if (parentOfMapModelsToMove.childCount > 0) // the code to move the environment
        {
            for (int p = 0; p < parentOfMapModelsToMove.childCount; p++)
                if (parentOfMapModelsToMove.GetChild(p).gameObject.activeSelf == true)
                    parentOfMapModelsToMove.GetChild(p).position += new Vector3(CurrentSpeed(), 0, 0);
        }

        if (Manager_GameState.Instance)
            Manager_GameState.Instance.ChangeDistanceTraveled(-CurrentSpeed());
    }

    public Transform SpawnOrPoolPlatform(string _forceByNickname, bool _forceNewPlatformInstance) // can try to get a platform by name and/or force the code to spawn a new instance
    {
        if (FoundErrors() || listOfSpawnablePlatforms.Count == 0)
        { Debug.LogError("NO PLATFORMS AVAILABLE ||OR|| FoundErros()"); return null; }

        float diceRoller = Random.Range(0.01f, 1); // the odds of a platform being selected
        Transform returnPlatform = null; // the platform we are storing to spawn or enable
        string nicknameRef = ""; // the name the platform will be titled if spawned

        // PROCESS REWRITE
        //  --> bool foundInLibrary = false;
        //  --> bool foundUnusedInGame = false;
        //
        //  --> Did we require a specific name?
        //      --> YES
        //          --> foundInLibrary = check if we have in library
        //          --> foundUnusedInGame = check if we have in-game & unused
        //          --> Did we require new instance?
        //              --> YES 
        //                  --> If foundInLibrary = true --> then set returnPlatform
        //                  --> If NO, but foundUnusedInGame = true --> set returnPlatform
        //
        //      --> If No returnPlatform yet
        //          --> Roll Dice
        //          --> Collect accepted platforms from library
        //          --> Randomly pick from simulated pool and assign nicknameRef
        //          --> foundInLibrary = check if we have in library
        //          --> foundUnusedInGame = check if we have in-game & unused
        //          --> Did we _forceNewPlatformInstance?
        //              --> YES
        //                  --> Assign library version if we have it
        //              --> NO
        //                  --> Assign unused version if we have it
        //      
        //  --> If no level was selected
        //      --> log it
        //      --> if we have any levels in our library set to level 0
        //      --> if it's still null --> create a large cube and assign it


        if (!string.IsNullOrEmpty(_forceByNickname)) // for when we have a specific platform we want
        {
            if (!_forceNewPlatformInstance) // pool from list if possible 
            {
                print("HERE!!! ... We should pick a random platform first, then see if we already have a copy of it");
                for (int p = 0; p < parentOfMapModelsToMove.childCount; p++) // check the platforms we have
                {
                    BehaviorPlatform pooledPlatformBehavior = null;
                    if (parentOfMapModelsToMove.GetChild(p).gameObject.activeSelf == false)
                    {
                        parentOfMapModelsToMove.GetChild(p).TryGetComponent(out pooledPlatformBehavior); // if we get a reference that matches and disabled
                        if (pooledPlatformBehavior && pooledPlatformBehavior.nickname == _forceByNickname || pooledPlatformBehavior.isVisible == false && pooledPlatformBehavior.nickname == _forceByNickname)
                            returnPlatform = parentOfMapModelsToMove.GetChild(p); // use it
                    }                        
                }
            }
        }
               
        if (!returnPlatform)  // if we dont have a plafform yet we will try spawning one or choose a default
        {
            CustomPlatformData newPlatform = null;
            newPlatform = PickOurNextPlatform(_forceByNickname); // try to get a platform, if no nickname then just roll the dice
            if (newPlatform != null) returnPlatform = Instantiate(newPlatform.prefabToSpawn, parentOfMapModelsToMove);           
            nicknameRef = newPlatform.platformNickname;
        }

        // find where it goes & move it over
        BehaviorPlatform newPlatformBehavior = null;
        returnPlatform.TryGetComponent(out newPlatformBehavior);
        if (newPlatformBehavior == null)
        { Debug.Log("ERROR: Platform missing script resources"); return null; }

        if (spawnedPlatformsInPlay.Count > 0 && returnPlatform != null) // move it if we can
        returnPlatform.position = spawnedPlatformsInPlay[spawnedPlatformsInPlay.Count - 1].transform.transform.position + new Vector3((spawnedPlatformsInPlay[spawnedPlatformsInPlay.Count - 1].scale.x / 2) + (newPlatformBehavior.scale.x / 2), 0, 0); 

        //add the platform to the list
        spawnedPlatformsInPlay.Add(newPlatformBehavior);
        if(!string.IsNullOrEmpty(nicknameRef))
        newPlatformBehavior.nickname = nicknameRef;
        // make sure it's visible
        returnPlatform.gameObject.SetActive(true);
        newPlatformBehavior.ShowHideArt(true);
        if (spawnedPlatformsInPlay.Count > platformsToKeepOnScreen) // check if we need to remove any
            RemoveSpecificPlatform(0, false);

        return returnPlatform;
    }

    public void RemoveSpecificPlatform(int _removeId, bool _delete)
    {
        if (_removeId >= 0 && _removeId < spawnedPlatformsInPlay.Count)
        {          
            if (!_delete)
            {                
                spawnedPlatformsInPlay[_removeId].ShowHideArt(false);
                spawnedPlatformsInPlay.RemoveAt(_removeId);
            }
            else
            {              
                GameObject deleteObj = null;
                if (spawnedPlatformsInPlay[_removeId] != null)
                {
                    deleteObj = spawnedPlatformsInPlay[_removeId].gameObject;
                    Destroy(deleteObj);
                }
                spawnedPlatformsInPlay.RemoveAt(_removeId);
            }
        }
        else
            Debug.Log("WARNING: Tried to remove a platform that wasnt in the list");

    }

    public void RemoveAnyNullPlatforms() // this is called when we RESET ... so TODO: make a reset function that calls these things instead
    {
        ChangeMonsterVariables(true, false, false);

        for (int i = 0; i < spawnedPlatformsInPlay.Count; i++)
        {
            if (spawnedPlatformsInPlay[i] == null)
                spawnedPlatformsInPlay.RemoveAt(i);
            else
                spawnedPlatformsInPlay[i].ResetToStart();
        }
    }

    private CustomPlatformData PickOurNextPlatform(string _forceByNickname) // only returns the appropraite platform data we can use to spawn
    {
        if (!string.IsNullOrEmpty(_forceByNickname)) // if we want something specific check for it
        {
            for (int cpd = 0; cpd < listOfSpawnablePlatforms.Count; cpd++)
                if (listOfSpawnablePlatforms[cpd].platformNickname == _forceByNickname)
                    return listOfSpawnablePlatforms[cpd];
        }

        float diceRoller = Random.Range(0.01f, 1);

        List<CustomPlatformData> listPlatformsInRange = new List<CustomPlatformData>();
        for (int cpd = 0; cpd < listOfSpawnablePlatforms.Count; cpd++)
            if (listOfSpawnablePlatforms[cpd].spawnChance >= diceRoller)
                listPlatformsInRange.Add(listOfSpawnablePlatforms[cpd]);
               

        if (listPlatformsInRange.Count > 0) // pick a random one from the options allowed
            return listPlatformsInRange[Random.Range(0, listPlatformsInRange.Count)];
        else
            return listOfSpawnablePlatforms[0]; // if we didnt find any in range then return the default       
    }

    #endregion PLATFORMS

    #region MONSTERS

    public void ChangeMonsterVariables(bool _readyToSpawn, bool _monsterInPlay, bool _successfulCapture)
    {
        if (monsterIsInPlay && !_monsterInPlay)
            lastCaptureTimeStamp = Time.time;        

        readyToSpawn = _readyToSpawn;
        monsterIsInPlay = _monsterInPlay;

        if (_successfulCapture && Manager_TutorialUI.Instance && Manager_GameState.Instance)
        {
            print("successful capture");
            if (Manager_GameState.Instance.capturedCreatues_Unique >= 4)
            { }// do nothing
            else
                Manager_TutorialUI.Instance.QueueMessage("Story_5");

            if (Manager_GameState.Instance.capturedCreatues_Unique > 0)
                waitTimeUntilDespawnDynamic = waitTimeUntilDespawn * (Manager_GameState.Instance.capturedCreatues_Unique * 1.1f); // Adds time to fight each monster  
            else
                waitTimeUntilDespawnDynamic = waitTimeUntilDespawn * (2f); // Adds first monster to spawn/despawn
        }

        if (!_monsterInPlay && ref_BehaviorCameraFollower)
            ref_BehaviorCameraFollower.StoreChangeState(BehaviorCameraFollower.CameraFocusState.MovingForward, true);

        monsterSignaledCapture = false; // reset our capture situation so we can despawn if needed
    }

    private void CheckForMonsterSpawn() // TODO: this needs refactoring... this functions purpose is to spawn monster if ready... COMBINED with "ChangeMonsterVariables" (above) we can have a singular checker
    {
        //if (Manager_GameState.Instance && Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing)
        //{ print($"no spawning monsters during { Manager_GameState.Instance.currentState} mode");  return; }

        if (Time.time > timeMonsterSpawned + waitTimeUntilDespawnDynamic && spawnedMonster != null && monsterIsInPlay)
            DespawnMonster();

        if (readyToSpawn)
        {
            if (!monsterIsInPlay)
            {
                if (isBlocked)
                    lastCaptureTimeStamp += Time.deltaTime; // delays the monster spawning if we are standing still

                if (Time.time > lastCaptureTimeStamp + waitTimeAfterCapture)
                    SpawnMonster();
                //else
                //    print("waiting to spawn a new monster");
            }
        }
    }

    public void SpawnMonster()
    {
        //print("CALLING SPAWN MONSTER");
        if (monstersSpawned < monstersToSpawnInOrder.Length)
        {
            if (Manager_GameState.Instance)
            {
                // spawn monster communicating with Game Manager
                spawnedMonster = Instantiate(monstersToSpawnInOrder[Manager_GameState.Instance.capturedCreatues_Unique]);
                spawnedMonster.transform.position = distanceOkayToSpawnFromPlayer[Manager_GameState.Instance.capturedCreatues_Unique];
                spawnedMonster.TryGetComponent(out spawnedMonsterBehavior);
                Manager_GameState.Instance.objectsSpawnedDuringRuntime.Add(spawnedMonster);

                if (Manager_TutorialUI.Instance)
                {
                    if (Manager_GameState.Instance.capturedCreatues_Unique == 0)
                    { Manager_TutorialUI.Instance.QueueMessage("Story_3"); Manager_TutorialUI.Instance.QueueMessage("Story_4"); }
                    if (Manager_GameState.Instance.capturedCreatues_Unique == 4)
                    { Manager_TutorialUI.Instance.QueueMessage("Story_6"); }
                    if (Manager_GameState.Instance.capturedCreatues_Unique == 5)
                    { Manager_TutorialUI.Instance.QueueMessage("Story_7"); Manager_TutorialUI.Instance.QueueMessage("Story_8"); Manager_TutorialUI.Instance.QueueMessage("Story_9"); }
                }
            }
            else
            {
                // spawn monster locally
                spawnedMonster = Instantiate(monstersToSpawnInOrder[monstersSpawned]);
                spawnedMonster.transform.position = distanceOkayToSpawnFromPlayer[monstersSpawned];
                spawnedMonster.TryGetComponent(out spawnedMonsterBehavior);
            }
            timeMonsterSpawned = Time.time;
            ChangeMonsterVariables(false, true, false);
            monstersSpawned++;                      

            if (ref_BehaviorCameraFollower)
                ref_BehaviorCameraFollower.StoreChangeState(BehaviorCameraFollower.CameraFocusState.FightingMonster, true);

            //print("SPAWNED MONSTER");
        }
        else // we've captured each unique monster
        {
            // spawn a random monster we've already captured one in endless mode
            ChangeSpawningPlatforms(true);
            Debug.LogWarning("WARNING: We should be finished with the demo game and dont want to spawn more monsters");
        }

        //readyToSpawn = false;
        //monsterIsInPlay = true;
    }

    public void RunMonsterBehavior()
    {
        if (spawnedMonsterBehavior)
            spawnedMonsterBehavior.RunMonsterBehavior();
    }

    public void DespawnMonster()
    {
        //print("Checking if we need to despawn monster");

        if(monstersSpawned > 0 && spawnedMonster != null && !monsterSignaledCapture) // if we have more than 1 spawned monster and its active now and we havent captured it yet
        {
            //print("Despawning MONSTER");       
            Destroy(spawnedMonster.gameObject, 1);
            spawnedMonster = null;
            spawnedMonsterBehavior = null;
            monstersSpawned--;
            timeMonsterSpawned = Time.time;
            ChangeMonsterVariables(true, false, false);
            if (Manager_GameState.Instance)
                Manager_GameState.Instance.objectsSpawnedDuringRuntime.Remove(spawnedMonster);

            // spawn pocki if we havent collected it yet
            if (!playerUnlockedPockiBox)
                SpawnOrMovePockiBox();

        }
    }

    public void SpawnOrMovePockiBox()
    {
        pockiBoxSpawnStamp = Time.time;
        spawnPockiBoxOnTimer = true;

        if(!pockiBoxObjPrefab)
        { Debug.Log("WARNING: PlatformManager.cs missing reference to pocki prefab and cannot spawn or move it."); return; }

        // check if we dont have a spawned one
        if (!spawnedPockiBoxObj)
        { spawnedPockiBoxObj = Instantiate(pockiBoxObjPrefab, Camera.main.transform); spawnedPockiBoxObj.gameObject.SetActive(false); }
        
        // move it
        spawnedPockiBoxObj.SetParent(transform);
        spawnedPockiBoxObj.position = Vector3.zero +  new Vector3(spawnXOffset, 20, 0);
        // raycast from the pocki box down onto a platform and place it there
        RaycastHit hit;
        if (Physics.Raycast(spawnedPockiBoxObj.position, -spawnedPockiBoxObj.up, out hit, Mathf.Infinity, pockiBoxInteractionLayers))
        {
            //Debug.DrawRay(spawnedPockiBoxObj.position, -spawnedPockiBoxObj.up * hit.distance, Color.yellow);
            //Debug.Log($"Pocki Box Did Hit: {hit.transform.name} @ {hit.point}");
            spawnedPockiBoxObj.position = hit.point + new Vector3(0, 1, 0);
            spawnedPockiBoxObj.gameObject.SetActive(true);

            // read tutorial line
            Manager_TutorialUI.Instance.QueueMessage("Controls_Pokie");
        }
        else
            Debug.Log("WARNING: Pocki box wasnt able to find surface to land on");              
    }

    #endregion MONSTERS

}// end of manager-platform class



// the custom data for platforms
[System.Serializable]
public class CustomPlatformData
{
    public string platformNickname;

    [Tooltip("The chance our platform can spawn. 1 (100%) means there is ALWAYS a chance it will spawn")]
    [Range(0, 1)]    public float spawnChance; // 100% means it will always have a chance to spawn
    [Tooltip("The Model we will spawn when this is selected")]
    public Transform prefabToSpawn;

    public UnityEvent onSpawnEvents, onMainPlatformEvents, onFinishedEvents;

    //public CustomAbilityOptions(string _newName,)
    //{
    //    //abilityNickname = _newName;
    //}

}//end of data for platforms
