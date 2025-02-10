using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Ability_FirePrefab : Ability
{
    public Transform prefabToSpawn;    
    public bool tryToPool = true;
    [Range(0,300)]
    public int destroyTimer; // if > ZERO (and not pooling) it will auto destroy 

    private Transform prefabParent;


    private bool AllPooledObjsAreOff(List<Transform> pooledList)
    {
        foreach (Transform poolObj in pooledList)
            if (poolObj.gameObject.activeSelf == true)
                return false;

        return true;
    }

    public override void Activate(Transform _playerObj)
    {
        //base.Activate(); // if we also want to activate things in the original function

        if (!prefabToSpawn)
        { Debug.Log("WARNING: Missing prefab for ability"); return; }

        if (!prefabParent)
        { prefabParent = new GameObject($"FirePrefabParent_{Time.time}").transform; prefabParent.tag = "SpawnParent";}

        //if (AllPooledObjsAreOff(prefabParent)) // to reduce chance of floating point error on great distances /// THIS is pretty inneficient || ONLY come back to this if our maps are HUGE 
        //    prefabParent.transform.position = _playerObj.position;

        Transform cloneObj = null;
        if (tryToPool)
            foreach (Transform child in prefabParent)
                if (child.gameObject.activeSelf == false)
                { cloneObj = child; break; } // we found a poolable object in our list

        if (!cloneObj)
            cloneObj = Instantiate(prefabToSpawn, prefabParent); // create a clone, either there wasnt a free one OR we are destroying them

        cloneObj.position = _playerObj.position;
        cloneObj.gameObject.SetActive(true);

        if (!tryToPool)
            Destroy(cloneObj, destroyTimer); // if we arent pooling then we can destroy it on the timer
    }
}
