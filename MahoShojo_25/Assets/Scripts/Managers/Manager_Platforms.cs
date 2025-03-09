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
    [Range(0.1f, 10)]
    public float dashingSpeedMultiplier = 2f;
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
    [Range(0.1f, 1)]
    public float speedBase = 10;
    [Tooltip("Change how fast / slow we can go at top speed")]
    [Range(0.1f, 1.0f)] public float speedLimiting = 0.25f;
    [Tooltip("Change how fast / slow we ramp up to full speed")]
    [Range(0.1f, 6f)] public float speedAcceleration = 1;
    [Tooltip("Change how fast / slow we ramp down when we let go of the controls")]
    [Range(0.1f, 6f)] public float speedDeceleration = 1;
    [Tooltip("When changing direction, what percent of speed should we cut? (0.5 means we cut half of our speed)")]
    [Range(0.1f, 1f)] public float speedChangeOnDirectionChange = 0.33f;
    [Tooltip("When not touching anything (in air). our speed will be adjusted by this amound (0.5 means we cut half of our speed)")]
    [Range(0.00f, 2f)] public float speedChangeWhileInAir = 0.66f;

    private float inputXYTime;
    public int dir { get; private set; } = 1;
    public bool isBlocked { get; private set; }
    public bool playerInAir { get; private set; }
   

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

    public float waitTimeAfterCapture = 10;

    public bool readyToSpawn { get; private set; } // decidor when we can spawn
    public bool monsterIsInPlay { get; private set; } // so we dont over spawn
    public int monstersSpawned { get; private set; } // tracking how manywe've spawned
    public Transform spawnedMonster { get; private set; } // the monster we want to track
    //private List<Transform> allSpawnedMonsters = new List<Transform>(); // to pool the monsters, but this should come in later versions
    private float lastCaptureTimeStamp;


    [Space]
    [Space]
    [Header("Pocki Objects\n______________")]
    public bool playerUnlockedPockiBox;
    public int pockiBoxSticks = 1;


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

        for (int i = 0; i < platformsToKeepOnScreen - 1; i++)
            SpawnOrPoolPlatform(null, true);

        lastCaptureTimeStamp = Time.time + (waitTimeAfterCapture * 4); // the first monster shouldnt spawn right away, but the normal pacing is good for most
        ChangeMonsterVariables(true, false);
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
        CheckForInputs();
        CheckDashing();
        MoveMaps();
        CheckForMonsterSpawn();
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
    }

    private void ReactToGameManager()
    {
        // can put code for animations or other objects to enable / disable
        // also code for checking if we are done or finished with the game
    }

    public void ChangeSpawningPlatforms(bool _stop)
    {
        stopSpawningPlatforms = _stop;
    }

    public void ChangeIsBlocked(bool _isBlocked)
    {
        print($"Changin isBlocked to: {_isBlocked}");
        isBlocked = _isBlocked;
    }

    public void ChangePlayerInAir(bool _inAir)
    {
        playerInAir = _inAir;
    }

    public float CurrentSpeed()
    {
        if (isBlocked)
            return 0;

        float speed = ((speedBase * dir * inputXYTime) * speedLimiting);
        speed = Mathf.Round(speed * 100f) / 100f; // rounding 2 Decimals so for other reliable calculations
        if (playerInAir && !isDashing)
            speed *= speedChangeWhileInAir;
        if (isDashing)
            speed *= dashingSpeedMultiplier;
        return speed; // if we dont round we dont need to create a new variable
    }

    public void SpawnNewPlatformFromEdge()
    {
        if (stopSpawningPlatforms)
        {            
            if(spawnedPlatformsInPlay.Count > 1)
            {
                for (int i = 1; i < spawnedPlatformsInPlay.Count; i++)
                    RemoveSpecificPlatform(i, false);
            }
            return;
        }
            

        if (Manager_GameState.Instance)
            Manager_GameState.Instance.objectsSpawnedDuringRuntime.Add(SpawnOrPoolPlatform(null, false));
        else
            SpawnOrPoolPlatform(null, false);
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

    private void CheckForInputs()
    {
        if (Input.GetKey(key_MovePlatformsRight)) // going left  
        {
            if (dir < 0) // check and set directions to be LEFT (platforms going right)
            { inputXYTime *= speedChangeOnDirectionChange; dir = 1; }
            inputXYTime += 1 * Time.deltaTime * speedAcceleration;
            if (inputXYTime > 1) { inputXYTime = 1; }
        }
        else if (Input.GetKey(key_MovePlatformsLeft) || automaicallyMoveRight ) // going right
        {
            if (Input.GetKeyDown(key_MovePlatformsLeft)) // pres forward to dash
            {
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
        }      
        else // not pressing left or rights
        {
            inputXYTime -= 1 * Time.deltaTime;
            if (inputXYTime < 0) { inputXYTime = 0; }
        }
    }
    

    private void MoveMaps()
    {
        if (parentOfMapModelsToMove.childCount > 0) // the code to move the environment
        {
            for (int p = 0; p < parentOfMapModelsToMove.childCount; p++)
                if (parentOfMapModelsToMove.GetChild(p).gameObject.activeSelf == true)
                    parentOfMapModelsToMove.GetChild(p).position += new Vector3(CurrentSpeed(), 0, 0);
        }
    }

    public Transform SpawnOrPoolPlatform(string _forceByNickname, bool _forceNewPlatformInstance) // can try to get a platform by name and/or force the code to spawn a new instance
    {
        if (FoundErrors())
            return null;

        Transform returnPlatform = null;
        if (listOfSpawnablePlatforms.Count > 0 && !_forceNewPlatformInstance) // for pooling when possible
        {
            if (string.IsNullOrEmpty(_forceByNickname)) // just pool anything available
            {
                for (int p = 0; p < parentOfMapModelsToMove.childCount; p++)
                    if (parentOfMapModelsToMove.GetChild(p).gameObject.activeSelf == false || parentOfMapModelsToMove.GetChild(p).GetComponent<BehaviorPlatform>().isVisible == false) // not optimized
                        returnPlatform = parentOfMapModelsToMove.GetChild(p);
            }
            else // try to pool something specific
            {
                for (int p = 0; p < parentOfMapModelsToMove.childCount; p++)
                    if (parentOfMapModelsToMove.GetChild(p).gameObject.activeSelf == false && parentOfMapModelsToMove.GetChild(p).GetComponent<BehaviorPlatform>().nickname == _forceByNickname ||
                        parentOfMapModelsToMove.GetChild(p).GetComponent<BehaviorPlatform>().isVisible == false && parentOfMapModelsToMove.GetChild(p).GetComponent<BehaviorPlatform>().nickname == _forceByNickname)
                        returnPlatform = parentOfMapModelsToMove.GetChild(p);
            }
        }

        string nicknameRef = "";
        if (!returnPlatform) // if we still need to create a platform because we havent yet
        {
            CustomPlatformData newPlatform = null;
            int attempts = 0; // ensure we dont freeze the engine due to inspector errors
            while (newPlatform == null || attempts < 100)
            { newPlatform = PickOurNextPlatform(_forceByNickname); attempts++; }
            returnPlatform = Instantiate(newPlatform.prefabToSpawn, parentOfMapModelsToMove);
            if (newPlatform != null)
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
        ChangeMonsterVariables(true, false);

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

        float diceRoller = Random.Range(0.1f, 100);

        List<CustomPlatformData> listPlatformsInRange = new List<CustomPlatformData>();
        for (int cpd = 0; cpd < listOfSpawnablePlatforms.Count; cpd++)
            if (listOfSpawnablePlatforms[cpd].spawnChance >= diceRoller)
                listPlatformsInRange.Add(listOfSpawnablePlatforms[cpd]);
               

        if (listPlatformsInRange.Count > 0) // pick a random one from the options allowed
            return listPlatformsInRange[Random.Range(0, listPlatformsInRange.Count - 1)];


        return null;
    } 

    public void ChangeMonsterVariables(bool _readyToSpawn, bool _monsterInPlay)
    {
        if (monsterIsInPlay && !_monsterInPlay)
            lastCaptureTimeStamp = Time.time;

        readyToSpawn = _readyToSpawn;
        monsterIsInPlay = _monsterInPlay;
    }

    private void CheckForMonsterSpawn()
    {
        if (Manager_GameState.Instance && Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing)
        { print($"no spawning monsters during { Manager_GameState.Instance.currentState} mode");  return; }

        if (readyToSpawn)
        {
            if (monsterIsInPlay)
                return;

            //if (isBlocked)
            //    lastCaptureTimeStamp += Time.time;

            if (Time.time > lastCaptureTimeStamp + waitTimeAfterCapture)
                SpawnMonster();
            //else
            //    print("waiting to spawn a new monster");
        }
    }

   public void SpawnMonster()
    {
        if (monstersSpawned < monstersToSpawnInOrder.Length)
        {
            // spawn monster
            spawnedMonster = Instantiate(monstersToSpawnInOrder[monstersSpawned]);
            spawnedMonster.transform.position = distanceOkayToSpawnFromPlayer[monstersSpawned];
            monstersSpawned++;
            if (Manager_GameState.Instance)
                Manager_GameState.Instance.objectsSpawnedDuringRuntime.Add(spawnedMonster);
            ChangeMonsterVariables(false, true);
        }
        else // spawn a random one
        {
            ChangeSpawningPlatforms(true);
            Debug.Log("WARNING: We should be finished with the demo game and dont want to spawn more");
        }

        //readyToSpawn = false;
        //monsterIsInPlay = true;
    }

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
