using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// <para> handles the movement and spawning indefinitely of platforms</para>
/// </summary>
public class Manager_Platforms : Manager_Singleton
{

    [Header("Input Keys\n______________")]
    [Tooltip("This keycode will move the maps to the Left (as if we are going right)")]
    public KeyCode key_MovePlatformsLeft = KeyCode.D;
    [Tooltip("This keycode will move the maps to the Right (as if we are going left)")]
    public KeyCode key_MovePlatformsRight = KeyCode.A;

    [Space]
    [Header("Platform Objects\n______________")]
    [Tooltip("Change how fast / slow we can go at top speed")]
    [Range(0.1f, 1.0f)] public float speedLimiting = 0.25f;
    [Tooltip("Change how fast / slow we ramp up to full speed")]
    [Range(0.1f, 6f)] public float speedAcceleration = 1;
    [Tooltip("Change how fast / slow we ramp down when we let go of the controls")]
    [Range(0.1f, 6f)] public float speedDeceleration = 1;

    private float inputXYTime;
    private int dir = 1;
    private bool pressing_MovePlatformsLeft;

    [Space]
    [Header("Platform Objects\n______________")]   
    [Tooltip("a transform of the object holding all of our children")]
    public Transform parentOfMapModelsToMove;

    [Range(1,10)] public int platformsToKeepOnScreen = 3;

    [Tooltip("A list of platforms we can spawn during runtime. Stats within each platform should allow for some customization")]
    public List<CustomPlatformData> listOfSpawnablePlatforms = new List<CustomPlatformData>();


    private bool FoundErrors()
    {
        if (listOfSpawnablePlatforms.Count == 0)
        { Debug.Log("ERROR: Missing Platforms To Spawn"); return true; }

        if (!parentOfMapModelsToMove)
        { Debug.Log("ERROR: Missing Platforms To Spawn"); return true; }

        return false;
    }


    private void Start()
    {
        if (FoundErrors())
            return;

        for(int i = 0; i < platformsToKeepOnScreen; i++)
        {
           // Transform clone = SpawnOrPoolPlatform();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (FoundErrors())
            return;

        CheckForInputs();
        MoveMaps();


    }

    private void CheckForInputs()
    {
        if (Input.GetKey(key_MovePlatformsLeft)) // going right
        {
            inputXYTime += 1 * Time.deltaTime * speedAcceleration;
            dir = -1;
            if (inputXYTime > 1) { inputXYTime = 1; }
        }
        else if (Input.GetKey(key_MovePlatformsRight)) // going left
        {
            inputXYTime += 1 * Time.deltaTime * speedAcceleration;
            dir = 1;
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
                    parentOfMapModelsToMove.GetChild(p).position += new Vector3(dir * inputXYTime * speedLimiting, 0, 0);
        }
    }

    private Transform SpawnOrPoolPlatform()
    {
        Transform returnPlatform = null;
        if (listOfSpawnablePlatforms.Count > 0)
        {
            for (int p = 0; p < parentOfMapModelsToMove.childCount; p++)
                if (parentOfMapModelsToMove.GetChild(p).gameObject.activeSelf == false)
                    returnPlatform = parentOfMapModelsToMove.GetChild(p);

        }


        return returnPlatform;
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
