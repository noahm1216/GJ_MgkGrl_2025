using System;
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
    public float radiusForMultiplePocki = 2;
    private Transform prefabParent;


    private bool FoundErrors()
    {
        if (!prefabToSpawn || !pointToSpawnFrom || !ref_ChargingPocki)
        { Debug.Log($"WARNING: Missing important references for PockiFire.cs on - {transform.name}"); return true; }

        if (!prefabParent)
        { prefabParent = new GameObject($"FirePrefabParent_{Time.time}").transform; prefabParent.tag = "SpawnParent"; }

        return false;
    }


    public void FirePocki()
    {
        if (FoundErrors())
            return;

        Transform cloneObj = CreateOrPoolPocki();

        if (!tryToPool && cloneObj)
            Destroy(cloneObj, destroyTimer); // if we arent pooling then we can destroy it on the timer
    }

    private Transform CreateOrPoolPocki()
    {
        Transform cloneObj = null;
        if (tryToPool)
            foreach (Transform child in prefabParent)
                if (child.gameObject.activeSelf == false)
                { cloneObj = child; break; } // we found a poolable object in our list

        if (!cloneObj)
            cloneObj = Instantiate(prefabToSpawn, prefabParent); // create a clone, either there wasnt a free one OR we are destroying them

        cloneObj.position = pointToSpawnFrom.position;
        cloneObj.gameObject.SetActive(true);
        return cloneObj;
    }

    public void UpdatePockiBox()
    {
        if (FoundErrors())
            return;

        if (pockiSticksInBoxParent)
        {
            // update box art
            for (int i = 0; i < pockiSticksInBoxParent.childCount; i++)
            {
                if (i <= ref_ChargingPocki.pockiCollected)
                    pockiSticksInBoxParent.GetChild(i).gameObject.SetActive(true);
                else
                    pockiSticksInBoxParent.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public void RingOfPocki()
    {
        if (FoundErrors())
            return;

        // calculate how many 
        // grab them
        // disable their scripts
        // (maybe) parent them to an object we'll rotate whill spawning them
        // form the ring
        // enable the scripts       

        if (pockiSticksInBoxParent)
        {
            List<Transform> pockiChargeItems = new List<Transform>();

            for (int i = 0; i < ref_ChargingPocki.pockiCollected; i++)
            {
                Transform pockiItem = CreateOrPoolPocki();
                BehaviorPockiProjectile ref_BehaviorPockiProjectile = null;
                pockiItem.TryGetComponent(out ref_BehaviorPockiProjectile);
                if(ref_BehaviorPockiProjectile)
                    ref_BehaviorPockiProjectile.enabled = false;
                pockiChargeItems.Add(pockiItem);

                /* Distance around the circle */
                var radians = 2 * MathF.PI / ref_ChargingPocki.pockiCollected * i;

                /* Get the vector direction */
                var vertical = MathF.Sin(radians);
                var horizontal = MathF.Cos(radians);

                var spawnDir = new Vector3(horizontal, vertical, 0);

                /* Get the spawn position */
                var spawnPos = pointToSpawnFrom.position + spawnDir * radiusForMultiplePocki; // Radius is just the distance away from the point

                /* Now spawn */
                pockiItem.transform.position = spawnPos;

                /* Rotate the enemy to face towards player */
                pockiItem.LookAt(pointToSpawnFrom);
                pockiItem.LookAt(null);
                pockiItem.Rotate(0, 180, 0);

                if (ref_BehaviorPockiProjectile)
                    ref_BehaviorPockiProjectile.enabled = true;
                pockiChargeItems.Add(pockiItem);
            }
        }
    }


}
