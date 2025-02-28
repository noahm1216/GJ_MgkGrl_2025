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

    private float inputXYTime;
    private int dir = 1;

    [Space]
    [Header("Platform Objects\n______________")]   
    [Tooltip("a transform of the object holding all of our children")]
    public Transform parentOfMapModelsToMove;

    [Range(1, 10)] public int platformsToSpawnOnStart = 3;
    [Range(1,20)] public int platformsToKeepOnScreen = 5;

    [Tooltip("A list of platforms we can spawn during runtime. Stats within each platform should allow for some customization")]
    public List<CustomPlatformData> listOfSpawnablePlatforms = new List<CustomPlatformData>();
    public List<BehaviorPlatform> spawnedPlatformsInPlay = new List<BehaviorPlatform>();


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
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        if (FoundErrors())
            return;

        for(int i = 0; i < platformsToKeepOnScreen-1; i++)
           SpawnOrPoolPlatform(null, true);             
    }

    // Update is called once per frame
    private void Update()
    {
        if (FoundErrors())
            return;

        CheckForInputs();
        MoveMaps();
    }

    public float CurrentSpeed()
    {
        float speed = ((speedBase * dir * inputXYTime) *speedLimiting);        
        speed = Mathf.Round(speed * 100f) / 100f; // rounding 2 Decimals so for other reliable calculations
        return speed; // if we dont round we dont need to create a new variable
    }

    public void SpawnNewPlatformFromEdge()
    {
        SpawnOrPoolPlatform(null, true);
    }

    private void CheckForInputs()
    {
        if (Input.GetKey(key_MovePlatformsLeft) || automaicallyMoveRight) // going right || TODO: unable to run left after adding this (need to add && conditions)
        {
            if (dir > 0)
            { inputXYTime *= speedChangeOnDirectionChange; dir = -1; }
            inputXYTime += 1 * Time.deltaTime * speedAcceleration;           
            if (inputXYTime > 1) { inputXYTime = 1; }
        }
        else if (Input.GetKey(key_MovePlatformsRight)) // going left
        {
            if(dir < 0)
            { inputXYTime *= speedChangeOnDirectionChange; dir = 1; }
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
                    if (parentOfMapModelsToMove.GetChild(p).gameObject.activeSelf == false)
                        returnPlatform = parentOfMapModelsToMove.GetChild(p);
            }
            else // try to pool something specific
            {
                for (int p = 0; p < parentOfMapModelsToMove.childCount; p++)
                    if (parentOfMapModelsToMove.GetChild(p).gameObject.activeSelf == false && parentOfMapModelsToMove.GetChild(p).GetComponent<CustomPlatformData>() != null && parentOfMapModelsToMove.GetChild(p).GetComponent<CustomPlatformData>().platformNickname == _forceByNickname)
                        returnPlatform = parentOfMapModelsToMove.GetChild(p);
            }
        }

        CustomPlatformData newPlatform = null;
        if (!returnPlatform) // if we still need to create a platform because we havent yet
        {
            while (newPlatform == null)
                newPlatform = PickOurNextPlatform(_forceByNickname);
            returnPlatform = Instantiate(newPlatform.prefabToSpawn, parentOfMapModelsToMove);
        }
        else
        returnPlatform.TryGetComponent(out newPlatform);  

        // find where it goes & move it over
        BehaviorPlatform newPlatformBehavior = null;
        returnPlatform.TryGetComponent(out newPlatformBehavior);
        if (newPlatformBehavior == null)
        { Debug.Log("ERROR: Platform missing script resources"); return null; }


        if (spawnedPlatformsInPlay.Count > 0 && returnPlatform != null) // move it if we can
        returnPlatform.position = spawnedPlatformsInPlay[spawnedPlatformsInPlay.Count - 1].transform.transform.position + new Vector3((spawnedPlatformsInPlay[spawnedPlatformsInPlay.Count - 1].scale.x / 2) + (newPlatformBehavior.scale.x / 2), 0, 0); 

        //add the platform to the list
        spawnedPlatformsInPlay.Add(newPlatformBehavior);
        // make sure it's visible
        returnPlatform.gameObject.SetActive(true);
        if (spawnedPlatformsInPlay.Count > platformsToKeepOnScreen) // check if we need to remove any
            RemovePlatform(0);

        return returnPlatform;
    }

    public void RemovePlatform(int _removeId)
    {
        if (_removeId >= 0 && _removeId < spawnedPlatformsInPlay.Count)
        {
            spawnedPlatformsInPlay[_removeId].gameObject.SetActive(false);
            spawnedPlatformsInPlay.RemoveAt(_removeId);
        }
        else
            Debug.Log("WARNING: Tried to remove a platform that wasnt in the list");
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

}



// the custom data for abilities
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

}//end of data for abilities
