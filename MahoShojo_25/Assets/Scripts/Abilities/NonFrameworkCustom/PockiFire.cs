using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PockiFire : MonoBehaviour
{

    public ChargingPocki ref_ChargingPocki;

    public Transform prefabToSpawn;
    public Transform pointToSpawnFrom;
    public bool tryToPool = true;
    [Range(0, 300)]
    public int destroyTimer; // if > ZERO (and not pooling) it will auto destroy 

    [Space]
    public Transform pockiSticksInBoxParent;

    private Transform prefabParent;


    public void FirePocki()
    {
        if (!prefabToSpawn || !pointToSpawnFrom || !ref_ChargingPocki)
        { Debug.Log($"WARNING: Missing important references for PockiFire.cs on - {transform.name}"); return; }

        if (!prefabParent)
        { prefabParent = new GameObject($"FirePrefabParent_{Time.time}").transform; prefabParent.tag = "SpawnParent"; }

        Transform cloneObj = null;
        if (tryToPool)
            foreach (Transform child in prefabParent)
                if (child.gameObject.activeSelf == false)
                { cloneObj = child; break; } // we found a poolable object in our list

        if (!cloneObj)
            cloneObj = Instantiate(prefabToSpawn, prefabParent); // create a clone, either there wasnt a free one OR we are destroying them

        cloneObj.position = pointToSpawnFrom.position;
        cloneObj.gameObject.SetActive(true);

        if (!tryToPool)
            Destroy(cloneObj, destroyTimer); // if we arent pooling then we can destroy it on the timer
    }

    public void UpdatePockiBox()
    {

        if (ref_ChargingPocki && pockiSticksInBoxParent)
        {
            // update box art
            for(int i = 0; i <= pockiSticksInBoxParent.childCount; i++)
            {
                if (i <= ref_ChargingPocki.pockiCollected)
                    pockiSticksInBoxParent.GetChild(i).gameObject.SetActive(true);
                else
                    pockiSticksInBoxParent.GetChild(i).gameObject.SetActive(false);
            }
            
        }



    }
}
