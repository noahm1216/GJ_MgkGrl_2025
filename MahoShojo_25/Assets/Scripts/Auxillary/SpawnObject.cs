using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    [Tooltip("The Object Reference We Will Be Spawning")]
    public Transform prefabSpawnable;
    [Tooltip("If enabled, this script will manage the data of spawned objects and reuse them")]
    public bool poolObjects = true;
    [Tooltip("The max amount of poolable objects we are willing to use. When the max is reached, the engine will take the oldest if none are disabled/unused")]
    [Range(0,999)]
    public int poolMax = 20;

    private Transform spawnParent;
    private List<Transform> spawnedObjects = new List<Transform>();
    private List<float> enabledTimeStamps = new List<float>();

    private void Initialize()
    {
        if (!prefabSpawnable) { Debug.LogWarning($"Missing Spawnable Object on: {transform.name}"); return; }
        if (!spawnParent) { spawnParent = new GameObject($"{transform.name}_SpawnParent").transform; spawnParent.transform.position = Vector3.zero; }
        if (poolMax <= 0) poolObjects = false;
    }

    private Transform InstantiateToList(int i)
    {
        spawnedObjects.Add(Instantiate(prefabSpawnable));
        spawnedObjects[i].transform.SetParent(spawnParent);
        spawnedObjects[i].gameObject.SetActive(false); // objects should have onenable / ondisable functions
        enabledTimeStamps.Add(Time.time);
        return spawnedObjects[i];
    }

    private void Start()
    {
        Initialize();

        for (int i = 0; i < poolMax; i++) // spawn our pool of objects
        {
            InstantiateToList(i);
        }
    }

    public void SpawnOrPoolObj()
    {
        Initialize();

        Transform objToPool = null;

        if (poolObjects) {
            if (spawnedObjects.Count > 0)
            {
                for (int i = 0; i < spawnedObjects.Count; i++)
                    if (spawnedObjects[i].gameObject.activeSelf == false) { objToPool = spawnedObjects[i]; enabledTimeStamps[i] = Time.time; break; }

                if (!objToPool)
                {
                    float oldestTime = 0;
                    int oldestId = 0;

                    for (int i = 0; i < spawnedObjects.Count; i++)
                    {
                        if (i == 0) oldestTime = enabledTimeStamps[i];
                        if (enabledTimeStamps[i] > oldestTime) { oldestTime = enabledTimeStamps[i]; oldestId = i; }
                    }

                    objToPool = spawnedObjects[oldestId];
                }
            }
        }

        if (!objToPool) objToPool = InstantiateToList(spawnedObjects.Count + 1);
        objToPool.transform.position = transform.position;
        objToPool.transform.Rotate(0, Random.Range(-90,90), 0);
        objToPool.gameObject.SetActive(true);
    }
}
