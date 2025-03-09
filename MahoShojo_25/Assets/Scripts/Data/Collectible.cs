using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{

    public bool reactivateOnTimer;
    public bool tryMoveWithPlatforms = true;
    public Collider colliderToToggle;
    public GameObject artObjToToggle;

    public float timeUntilReactivate = 30f;
    private float timeCollided;

    public void Interacted()
    {
        if (reactivateOnTimer)
        {
            timeCollided = Time.time;
            colliderToToggle.enabled = false;
            artObjToToggle.SetActive(false);
        }
        else
            gameObject.SetActive(false);
    }

    private void LateUpdate()
    {

        if (tryMoveWithPlatforms && Manager_Platforms.Instance)
            transform.position += new Vector3(Manager_Platforms.Instance.CurrentSpeed(), 0, 0);


        if (!reactivateOnTimer)
            return;


        if(artObjToToggle.activeSelf == false && Time.time > timeCollided + timeUntilReactivate)
        {
            colliderToToggle.enabled = true;
            artObjToToggle.SetActive(true);
        }

        
        
    }
}
