using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{

    public bool reactivateOnTimer;
    public bool tryMoveWithPlatforms = true;
    public bool startDisabled;
    public bool cannotEnableIfPockiBoxNotCollected;
    public Collider colliderToToggle;
    public GameObject artObjToToggle;

    public float timeUntilReactivate = 30f;
    private float timeCollided;

    private void OnEnable()
    {
        if (startDisabled)
            ChangeCollectibleCondition(false);
    }

    public bool MeetsAllConditionsToEnable()
    {
        if (Manager_Platforms.Instance)
            if (cannotEnableIfPockiBoxNotCollected && Manager_Platforms.Instance.playerUnlockedPockiBox == false)
                return false;

        if (reactivateOnTimer && Time.time < timeCollided + timeUntilReactivate)
            return false;


        return true;
    }

    public void Interacted()
    {
        if (reactivateOnTimer)
        {
            timeCollided = Time.time;
            ChangeCollectibleCondition(false);
        }
        else
            gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (tryMoveWithPlatforms && Manager_Platforms.Instance)
            transform.position += new Vector3(Manager_Platforms.Instance.CurrentSpeed(), 0, 0);

        if (!reactivateOnTimer)
            return;

        if (artObjToToggle.activeSelf == false)
        {
            if (MeetsAllConditionsToEnable() == true)
                ChangeCollectibleCondition(true);
            else
                ChangeCollectibleCondition(false);
        }
    }

    public void ChangeCollectibleCondition(bool _usable)
    {
        colliderToToggle.enabled = _usable;
        artObjToToggle.SetActive(_usable);
    }
}
