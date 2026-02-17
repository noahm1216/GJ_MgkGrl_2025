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

    public PlatformScriptableObject CurrentLoadedLevel { get; private set; } = null;
    public PlayerCore PlayerReference { get; private set; } = null;

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
    [Space][Space]
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


    [Space][Space]
    [Header("Platform Objects\n______________")]
    [Tooltip("The starting Variable for how our speed is calculated")]
    [Range(0.1f, 30)]
    public float speedBase = 14f;   
    [Tooltip("Change how fast / slow we can go at top speed")]
    [Range(0.1f, 10.0f)] public float speedLimiting = 1f;
    [Tooltip("When true, the player will start at 0 speed and use 'Time Between Aceeleration' + 'Speed Acceleration' to ramp up speed")]
    public bool startPlayerSlowly = true;
    [Tooltip("Change how much time inbetween speed increases/ramp ups")]
    [Range(0.1f, 10f)] public float timeBetweenAceeleration = 1.5f;
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
   

    [Space][Space]
    [Header("Platform Objects\n______________")]   
    [Tooltip("a transform of the object holding all of our children")]
    private Transform parentOfMapModelsToMove;

    [Range(1, 10)] public int platformsToSpawnOnStart = 3;
    [Range(1,20)] public int platformsToKeepOnScreen = 5;

    [Tooltip("A list of platforms we can spawn during runtime. Stats within each platform should allow for some customization")]
    public List<CustomPlatformData> listOfSpawnablePlatforms = new List<CustomPlatformData>();
    public List<BehaviorPlatform> spawnedPlatformsInPlay = new List<BehaviorPlatform>();

    public bool stopSpawningPlatforms { get; private set; }


    [Space][Space]
    [Header("Monster Objects\n______________")]
    [Tooltip("The prefabs of the monsters we want to spawn in the order we want to spawn them")]
    public Transform[] monstersToSpawnInOrder;
    [Tooltip("The acceptable distance of those monsters")]
    public Vector3[] distanceOkayToSpawnFromPlayer;
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
    private int monstersCaptured; // the number of monsters we've captured in a level


    [Space][Space]
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


    [Space][Space]
    [Header("Camera\n______________")]
    public BehaviorCameraFollower ref_BehaviorCameraFollower;


    private bool FoundErrors()
    {
        if (listOfSpawnablePlatforms.Count == 0)
        { Debug.Log("ERROR Platform-Log: Missing Platforms To Spawn"); return true; }

        if (!parentOfMapModelsToMove)
        {
            Debug.Log("Note: Platform-Log: Missing Platform Parent To Spawn Under ... spawning one");
            parentOfMapModelsToMove = new GameObject("ParentOfMapModelsToMove").transform;
            parentOfMapModelsToMove.SetParent(transform);
        }

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
        //ChangeMonsterVariables(true, false, false);

        if (startPlayerSlowly)
            startSpeedAccel = 0;
        else
            startSpeedAccel = 1;

        if (!ref_BehaviorCameraFollower && Camera.main)
            Camera.main.TryGetComponent(out ref_BehaviorCameraFollower);

        //waitTimeUntilDespawnDynamic = waitTimeUntilDespawn;
    }

    // Update is called once per frame
    private void Update()
    {
        if (FoundErrors())
            return;

        if(Manager_GameState.Instance) // if we have the game manager then we want things to look a specific way
            if (Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing)
                return;

        CheckForInputs();
        CheckDashing();   
    }

    private void FixedUpdate() //TODO: refactor function calling: we dont need to check all of these functions every frame, it can be more as-needed
    {
        if (FoundErrors())
            return;

        if (Manager_GameState.Instance) // if we have the game manager then we want things to look a specific way
        {
            ReactToGameManager();
            if (Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing)
                return;
        }

        // TODO: Maybe we can delay how frequently we check for some of these (instead of every frame)
        CheckForTutorial(); // TODO: we will replace this entirely with our new message system 
        CheckLevelMessages();
        CheckTimeStamps();
        MoveMaps();
        CheckMonsterSpawner();
        RunMonsterBehavior();
        
        StartSpeedIsRampedUp();
        CheckForObstacles();
        CheckWinCondition();
        if (spawnPockiBoxOnTimer && Time.time > pockiBoxSpawnStamp + timeUntilPockiBoxSpawn && !playerUnlockedPockiBox)
            SpawnOrMovePockiBox();
    }


    #region HELPERS
    private void CheckTimeStamps()
    {
        if (!isBlocked)
            blockedTimeStamp = Time.time;    
    }

    private void CheckWinCondition()
    {
        if (CurrentLoadedLevel == null) { print("Manager Platforms: No Current Level Loaded"); return; }

        if (Manager_GameState.Instance && Manager_GameState.Instance.currentState == Manager_GameState.GAMESTATE.Playing)
        { 
            if (Manager_GameState.Instance.CheckMetConditions // check win condition
                (
                  Manager_GameState.Instance.dataSinceLevelStarted,
                  CurrentLoadedLevel.levelWinConditions)) // Compare if we met enough win conditions in the level
            {
                print("Triggering Win Condition Naturally");
                if (Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Won)
                    Manager_GameState.Instance.WonTheGame();
            }
        }
    }

    

    public void UpdateCurrentLoadedLevel(PlatformScriptableObject _newLevelToUpdate)
    {
        print("Manager Platform: Updating Current Loaded Level Attempt");

        CurrentLoadedLevel = _newLevelToUpdate; // assign the references   
        
        if (_newLevelToUpdate != null) // update the platforms and data we use
        {   // level data
            listOfSpawnablePlatforms = CurrentLoadedLevel.listOfSpawnablePlatforms;
            RemoveAllPlatforms();
            SpawnPlatformsOnDelay(0);            
            // monsters
            monstersToSpawnInOrder = new Transform[CurrentLoadedLevel.monstersToSpawn.Length];
            for (int i = 0; i < CurrentLoadedLevel.monstersToSpawn.Length; i++)
                monstersToSpawnInOrder[i] = CurrentLoadedLevel.monstersToSpawn[i].monsterBosses;
            monstersCaptured = 0;
            // player
            if (PlayerReference) PlayerReference.transform.position += new Vector3(0, 1, 0);
        }

        if (_newLevelToUpdate) print($"Manager Platform: UpdatedLevel to: {_newLevelToUpdate.levelName}"); // log it
        else print("Manager Platform: Unable To Load Null level");
    }

    public void PopulatePlayerCoreRef(PlayerCore _newPlayerRef)
    {
        PlayerReference = _newPlayerRef;
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
                startSpeedAceelWaitTime+= 1 * timeBetweenAceeleration;
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
        //print($"Platforms To Remove: {spawnedPlatformsInPlay.Count} platforms ");
        for (int i = 0; i <= spawnedPlatformsInPlay.Count; i++)
            RemoveSpecificPlatform(0, true); // they will all be 0 when we remove them

        spawnedPlatformsInPlay.Clear();
    }

    public void SpawnStartingPlatforms()
    {
        for (int i = 0; i < platformsToSpawnOnStart; i++)
            SpawnOrPoolPlatform(null, true);
    }

    public void SpawnNewPlatformFromEdge() // when player reaches end of env chunk trigger
    {
        if (stopSpawningPlatforms)
        {
            if (spawnedPlatformsInPlay.Count > 1)

                for (int i = 1; i < spawnedPlatformsInPlay.Count; i++)
                    RemoveSpecificPlatform(i, false);

            return; // stop here if we dont want to spawn anymore platforms
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

    private IEnumerator SpawnPlatformsOnDelayEnum(float _delay) // NOTE: when public still not easily accessible??
    {
        //print($"spawning platforms on delay of: {_delay} seconds");
        yield return new WaitForSeconds(_delay);
        SpawnStartingPlatforms();
        print("TODO: Raycast Maho's new position when we do this");
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

        CustomPlatformData newPlatform = null; // the data of the platform we will spawn
        Transform returnPlatform = null; // the platform we are storing to spawn or enable
        BehaviorPlatform platformBehavior = null; // the data of the platform we will update when we spawn or pull from existing spawns
        string nicknameRef = ""; // the name the platform will be titled if spawned


        newPlatform = PickOurNextPlatform(_forceByNickname); // try to get a platform, if no nickname then just roll the dice

        if (newPlatform != null) // we either found a match, rolled the dice, or picked the first platform in the list
        {
            nicknameRef = newPlatform.platformNickname;

            if (!_forceNewPlatformInstance) // if we dont want to force, we should first try to pool
            {
                for (int p = 0; p < parentOfMapModelsToMove.childCount; p++) // check the platforms we have disabled
                {
                    if (parentOfMapModelsToMove.GetChild(p).gameObject.activeSelf == false)
                    {
                        parentOfMapModelsToMove.GetChild(p).TryGetComponent(out platformBehavior); // if we get a reference that matches and disabled
                        if (platformBehavior && platformBehavior.nickname == _forceByNickname || platformBehavior.isVisible == false && platformBehavior.nickname == _forceByNickname)
                            returnPlatform = parentOfMapModelsToMove.GetChild(p); // use it
                    }
                }
            }
            // if no platform yet then we instantiate one
            if (returnPlatform == null) returnPlatform = Instantiate(newPlatform.prefabToSpawn, parentOfMapModelsToMove);

        } // else we didn't find anything by the name and tried to roll a dice
        else { Debug.LogError($"ERROR - MISSING DATA: Couldnt Find Platform using current library of options\nObject: {transform.name}"); }

        // find where it goes & move it over
        returnPlatform.TryGetComponent(out platformBehavior);
        if (returnPlatform == null || platformBehavior == null)
        {
            Debug.LogError("ERROR: Platform missing script resources");
            GameObject errorCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!returnPlatform) return errorCube.transform;
            else
            {
                errorCube.transform.SetParent(returnPlatform);
                errorCube.transform.position = new Vector3(0, 0.5f, 0);
                return returnPlatform;
            }
        }

        if (spawnedPlatformsInPlay.Count > 0 && returnPlatform != null) // move the platform to the correct location in the list of platforms we've spawned
            returnPlatform.position = spawnedPlatformsInPlay[spawnedPlatformsInPlay.Count - 1].transform.transform.position + new Vector3((spawnedPlatformsInPlay[spawnedPlatformsInPlay.Count - 1].scale.x / 2) + (platformBehavior.scale.x / 2), 0, 0);

        spawnedPlatformsInPlay.Add(platformBehavior); //add the platform to the list
        if (!string.IsNullOrEmpty(nicknameRef)) platformBehavior.nickname = nicknameRef; // assign nickname
        returnPlatform.gameObject.SetActive(true);  // make sure it's visible
        platformBehavior.ShowHideArt(true); // make sure the platform and its elements are visible
        if (spawnedPlatformsInPlay.Count > platformsToKeepOnScreen) RemoveSpecificPlatform(0, false);  // check if we need to remove any

        return returnPlatform;
    }

    public void RemoveSpecificPlatform(int _removeId, bool _delete)
    {
        //print($"ASKED TO REMOVE: Platform: {_removeId} - Delete: {_delete}");
        if (_removeId >= 0 && _removeId < spawnedPlatformsInPlay.Count)
        {
            if (spawnedPlatformsInPlay[_removeId] != null)
            {
                if(_delete) Destroy(spawnedPlatformsInPlay[_removeId].gameObject, 1); // destroy on delay to ensure we time the removal well
                spawnedPlatformsInPlay[_removeId].ShowHideArt(false);
            }
            spawnedPlatformsInPlay.RemoveAt(_removeId);
        }
        else
            Debug.Log("WARNING: Tried to remove a platform that wasnt in the list");
    }

    public void RemoveAnyNullPlatforms() // this is called when we RESET ... so TODO: make a reset function that calls these things instead
    {
        //ChangeMonsterVariables(true, false, false);

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
            return null;
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

    #region POKKI
    public void SpawnOrMovePockiBox()
    {
        pockiBoxSpawnStamp = Time.time;
        spawnPockiBoxOnTimer = true;

        if (!pockiBoxObjPrefab)
        { Debug.Log("WARNING: PlatformManager.cs missing reference to pocki prefab and cannot spawn or move it."); return; }

        // check if we dont have a spawned one
        if (!spawnedPockiBoxObj)
        { spawnedPockiBoxObj = Instantiate(pockiBoxObjPrefab, Camera.main.transform); spawnedPockiBoxObj.gameObject.SetActive(false); }

        // move it
        spawnedPockiBoxObj.SetParent(transform);
        spawnedPockiBoxObj.position = Vector3.zero + new Vector3(spawnXOffset, 20, 0);
        // raycast from the pocki box down onto a platform and place it there
        RaycastHit hit;
        if (Physics.Raycast(spawnedPockiBoxObj.position, -spawnedPockiBoxObj.up, out hit, Mathf.Infinity, pockiBoxInteractionLayers))
        {
            //Debug.DrawRay(spawnedPockiBoxObj.position, -spawnedPockiBoxObj.up * hit.distance, Color.yellow);
            //Debug.Log($"Pocki Box Did Hit: {hit.transform.name} @ {hit.point}");
            spawnedPockiBoxObj.position = hit.point + new Vector3(0, 1, 0);
            spawnedPockiBoxObj.gameObject.SetActive(true);

            // read tutorial line
            //Manager_TutorialUI.Instance.QueueMessage("Controls_Pokie");
        }
        else
            Debug.Log("WARNING: Pocki box wasnt able to find surface to land on");
    }
    #endregion pokki

    #region MONSTERS
    /// <summary>
    ///  Ideal Monster behavior:
    ///  we have monster(s) we plan to spawn
    ///  one or more requirements are set for their spawning
    ///  once all those requirements are met the platforms should pause for a moment (if story mode)
    ///  while pause we can move the camera to show the animation and abilities (make maho invincible temporarily)
    ///  once the player is the focus again we resume the platforms moving
    ///  the monster will despawn after it's alloted time (this is on each monster's behavior)
    ///  if pocki hasnt been spawned yet, it will 
    ///  the monster will be respawned at half of the required values each time it despawns (so 120 seconds becomes 60 seconds)
    ///  
    /// </summary>

    public void CheckMonsterSpawner()
    {
        //if (isBlocked)
               //lastCaptureTimeStamp += Time.deltaTime; // delays the monster spawning if we are standing still

        if (monstersCaptured < monstersToSpawnInOrder.Length) // if we havent finished the list of monsters to spawn in this level
        {
            if (monstersCaptured == monstersSpawned) // if we have captured all the monster who have spawned...
            {
                print("Checking if Ready To Spawn A Monster!");
                if (Manager_GameState.Instance)
                { // check the requirements it has || returns true if enough conditions are met
                    if (Manager_GameState.Instance.CheckMetConditions
                        (
                        Manager_GameState.Instance.dataSinceMonsterSpawn,
                        CurrentLoadedLevel.monstersToSpawn[monstersCaptured].requiredConditions)) // check we achieved enough conditiond for next monster
                    {
                        print("READY to spawn a monster!");
                        SpawnMonster(monstersSpawned);
                    }
                    else { print("Monster Spawn Conditions Not Yet Met"); }
                }
            }
        }

        if (spawnedMonster != null && spawnedMonster.gameObject.activeSelf == true && Time.time > timeMonsterSpawned + waitTimeUntilDespawnDynamic)
            DespawnMonster();
    }

    public void RemoveMonsterInPlay()
    {
        if (spawnedMonster)
        {
            monstersSpawned--;
            monsterSignaledCapture = false;
            print("Turn Off Monster Stuff In-game");
            spawnedMonster.gameObject.SetActive(false);
            ref_BehaviorCameraFollower.StoreChangeState(BehaviorCameraFollower.CameraFocusState.MovingForward, true);            
        }
    }

    public void CaptureMonsterInPlay()
    {
        if (spawnedMonster)
        {
            print("Captured Monster!!");
            ref_BehaviorCameraFollower.StoreChangeState(BehaviorCameraFollower.CameraFocusState.MovingForward, true);
            spawnedMonster.gameObject.SetActive(false);            
            Destroy(spawnedMonster, 2);
            spawnedMonsterBehavior = null;
            spawnedMonster = null;
            monsterSignaledCapture = false;
        }
    }

    // TODO: REMOVE
    //public void ChangeMonsterVariables(bool _readyToSpawn, bool _monsterInPlay, bool _successfulCapture)
    //{
    //    // NOTE TODO - when we have have captured all the monsters we needed to, thats when we play the end-game screen

    //    if (monsterIsInPlay && !_monsterInPlay)
    //        lastCaptureTimeStamp = Time.time;        

    //    readyToSpawn = _readyToSpawn;
    //    monsterIsInPlay = _monsterInPlay;

    //    if (_successfulCapture && Manager_TutorialUI.Instance && Manager_GameState.Instance)
    //    {
    //        print("successful capture");
    //        if (Manager_GameState.Instance.capturedCreatues_Unique >= 4)
    //        { }// do nothing
    //        else
    //            Manager_TutorialUI.Instance.QueueMessage("Story_5");

    //        if (Manager_GameState.Instance.capturedCreatues_Unique > 0)
    //            waitTimeUntilDespawnDynamic = waitTimeUntilDespawn * (Manager_GameState.Instance.capturedCreatues_Unique * 1.1f); // Adds time to fight each monster  
    //        else
    //            waitTimeUntilDespawnDynamic = waitTimeUntilDespawn * (2f); // Adds first monster to spawn/despawn
    //    }

    //    if (!_monsterInPlay && ref_BehaviorCameraFollower)
    //        ref_BehaviorCameraFollower.StoreChangeState(BehaviorCameraFollower.CameraFocusState.MovingForward, true);

    //    monsterSignaledCapture = false; // reset our capture situation so we can despawn if needed
    //}

    // TODO: REMOVE
    //private void CheckForMonsterSpawn() // TODO: this needs refactoring... this functions purpose is to spawn monster if ready... COMBINED with "ChangeMonsterVariables" (above) we can have a singular checker
    //{
    //    //if (Manager_GameState.Instance && Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing)
    //    //{ print($"no spawning monsters during { Manager_GameState.Instance.currentState} mode");  return; }
       

    //    if (Time.time > timeMonsterSpawned + waitTimeUntilDespawnDynamic && spawnedMonster != null)
    //        DespawnMonster();

    //    print("TODO: Incorporate our level requirements...\n(1) Distance \n(2) Time \n(3) Score");
    //    if (readyToSpawn)
    //    {
    //        if (!monsterIsInPlay)
    //        {
    //            if (isBlocked)
    //                lastCaptureTimeStamp += Time.deltaTime; // delays the monster spawning if we are standing still

    //            //if (Time.time > lastCaptureTimeStamp + waitTimeAfterCapture) // UNCOMMENTED DURING REFACTOR
    //            //    SpawnMonster();
    //            //else
    //            //    print("waiting to spawn a new monster");
    //        }
    //    }
    //}

    public void SpawnMonster(int _idToSpawn)
    {
        // BEGIN with a return that stops if we already spawned all the monsters 
        if (_idToSpawn > CurrentLoadedLevel.monstersToSpawn.Length && CurrentLoadedLevel.spawnMonstersEndlessly == false)
        { print("We have spawned all possible monsters"); return; }

        monstersSpawned++;

        // spawn monster locally
        if (!spawnedMonster)
        {
            spawnedMonster = Instantiate(monstersToSpawnInOrder[_idToSpawn]);
            spawnedMonster.transform.position = distanceOkayToSpawnFromPlayer[_idToSpawn];
            spawnedMonster.TryGetComponent(out spawnedMonsterBehavior);

            print($"Manager Platform: Spawning Monster #{_idToSpawn}");
            if (Manager_GameState.Instance) // communicating with Game Manager
            {
                //spawnedMonster = Instantiate(monstersToSpawnInOrder[Manager_GameState.Instance.capturedCreatues_Unique]);
                //spawnedMonster.transform.position = distanceOkayToSpawnFromPlayer[Manager_GameState.Instance.capturedCreatues_Unique];
                //spawnedMonster.TryGetComponent(out spawnedMonsterBehavior);
                Manager_GameState.Instance.objectsSpawnedDuringRuntime.Add(spawnedMonster);

                //if (Manager_TutorialUI.Instance) // TODO: replace this with check loadedlevel condition for "TextToShow"
                //{
                //    if (Manager_GameState.Instance.capturedCreatues_Unique == 0)
                //    { Manager_TutorialUI.Instance.QueueMessage("Story_3"); Manager_TutorialUI.Instance.QueueMessage("Story_4"); }
                //    if (Manager_GameState.Instance.capturedCreatues_Unique == 4)
                //    { Manager_TutorialUI.Instance.QueueMessage("Story_6"); }
                //    if (Manager_GameState.Instance.capturedCreatues_Unique == 5)
                //    { Manager_TutorialUI.Instance.QueueMessage("Story_7"); Manager_TutorialUI.Instance.QueueMessage("Story_8"); Manager_TutorialUI.Instance.QueueMessage("Story_9"); }
                //}
            }
        }
        else
            spawnedMonster.gameObject.SetActive(true);

        waitTimeUntilDespawnDynamic = spawnedMonsterBehavior.timeUntilDespawn;
        timeMonsterSpawned = Time.time;
        //ChangeMonsterVariables(false, true, false);
        //monstersSpawned++;

        if (ref_BehaviorCameraFollower)
            ref_BehaviorCameraFollower.StoreChangeState(BehaviorCameraFollower.CameraFocusState.FightingMonster, true);

        //print("SPAWNED MONSTER");
    }


    public void RunMonsterBehavior()
    {
        if (spawnedMonsterBehavior && spawnedMonster && spawnedMonster.gameObject.activeSelf == true)
         spawnedMonsterBehavior.RunMonsterBehavior(); 
    }

    public void DespawnMonster()
    {
        //print("Checking if we need to despawn monster");

        // if we have spawned at least 1 monster and its active now and we havent captured it yet
        if (monstersSpawned > 0 && spawnedMonster != null && !monsterSignaledCapture) 
        {
            //print("Despawning MONSTER");       
            Destroy(spawnedMonster.gameObject, 1);
            spawnedMonster = null;
            spawnedMonsterBehavior = null;
            monstersSpawned--;
            timeMonsterSpawned = Time.time;
            //ChangeMonsterVariables(true, false, false);
            if (Manager_GameState.Instance)
            {
                Manager_GameState.Instance.objectsSpawnedDuringRuntime.Remove(spawnedMonster);
                Manager_GameState.Instance.ResetDataSinceMonsterSpawn();
            }

            // spawn pocki if we havent collected it yet
            if (!playerUnlockedPockiBox)
                SpawnOrMovePockiBox();
        }
    }
    #endregion MONSTERS

    #region MESSAGES/TUTORIALS
    private void CheckForTutorial() // TODO: we can remove this, or revamp it since this type of text will be relayed globally
    {
        if (Time.time > blockedTimeStamp + blockedTimeUntilControlsPopUp && Manager_TutorialUI.Instance)
        {
            print("Originally we show the character a text/message about jumping here");
            //Manager_TutorialUI.Instance.QueueMessage("Controls_Jump");
            blockedTimeStamp = Time.time;
        }

        if (Time.time > timePressedJumpOrMove + timeUntilShowTutorial && !pressedMove)
        {
            if (Manager_TutorialUI.Instance)
            {
                print("Originally we show text that helps people move / jump / dash at this moment");
                //Manager_TutorialUI.Instance.QueueMessage("Controls_Move");
                //Manager_TutorialUI.Instance.QueueMessage("Controls_Dash");
                timePressedJumpOrMove = Time.time;
            }
        }
    }

    public void CheckLevelMessages()
    {
        if (!CurrentLoadedLevel || CurrentLoadedLevel.listOfTextToShow.Count == 0)
        { Debug.Log("Manager_Platform: CheckMessages - Missing Data For TextToShow/CurrentLoadedLevel"); return; }
        if (!Manager_GameState.Instance || !Manager_TutorialUI.Instance) { Debug.Log("Manager_Platform: CheckMessages - Missing Important Manager References"); return; }

        for (int i = 0; i < CurrentLoadedLevel.listOfTextToShow.Count; i++)
        {
            if (Manager_GameState.Instance.CheckMetConditions( // if we met the conditions for the text to display
                Manager_GameState.Instance.dataSinceLevelStarted,
                CurrentLoadedLevel.listOfTextToShow[i].requiredConditions)
                )
            {
                if (CurrentLoadedLevel.listOfTextToShow[i].hasPlayed && !CurrentLoadedLevel.listOfTextToShow[i].messageCanRepeat) return;// non repeatables already played, stop here
                if (CurrentLoadedLevel.listOfTextToShow[i].requiredTextBefore != null) // if there is a text we want to show first, we check it here
                    for (int j = 0; j < CurrentLoadedLevel.listOfTextToShow.Count; j++) // loop the text again & if our match hasn't been played
                        if (CurrentLoadedLevel.listOfTextToShow[j] == CurrentLoadedLevel.listOfTextToShow[i].requiredTextBefore && !CurrentLoadedLevel.listOfTextToShow[j].hasPlayed)
                            return; // stop

                Manager_TutorialUI.Instance.QueueMessageToShow(CurrentLoadedLevel.listOfTextToShow[i]);
                CurrentLoadedLevel.listOfTextToShow[i].hasPlayed = true;
                break;
            }
        }
    }
    #endregion end messages/tutorials



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
