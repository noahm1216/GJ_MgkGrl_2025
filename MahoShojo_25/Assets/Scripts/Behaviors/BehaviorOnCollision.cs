using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BehaviorOnCollision : MonoBehaviour
{

    public bool onTrigger = true;

    public LayerMask allowedCollisionLayers;
    public int maxCollisionsAllowed = 1;
    public bool spawnNewPlatform;
    public bool resetsOnEnable;
    public UnityEvent onCollisionEnterEvents;

    private int numberOfActivations = 0;


    public void OnEnable()
    {
        if (resetsOnEnable)
            numberOfActivations = 0;        
    }

    public void ActivateBehavior()
    {
        if (numberOfActivations >= maxCollisionsAllowed)
            return;
        if (spawnNewPlatform && Manager_Platforms.Instance)
            Manager_Platforms.Instance.SpawnNewPlatformFromEdge();
        numberOfActivations++;
        onCollisionEnterEvents.Invoke();
    }

    public void OnTriggerEnter(Collider trig)
    {       
        if (!onTrigger)
            return;
        if ((allowedCollisionLayers.value & (1 << trig.transform.gameObject.layer)) > 0)
            ActivateBehavior(); //Debug.Log("Hit with Layermask");        
    }

    public void OnCollisionEnter(Collision col)
    {
        if (onTrigger)
            return;
        if ((allowedCollisionLayers.value & (1 << col.transform.gameObject.layer)) > 0)
            ActivateBehavior(); //Debug.Log("Hit with Layermask");
    }
}
