using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastCheck : MonoBehaviour
{
    public Vector3 directionToRaycast;
    public LayerMask layerMaskToAcceptRaycast;

    //void Update()
    //{
    //    RaycastWorking(transform.position, directionToRaycast, layerMaskToAcceptRaycast);
    //}

    public RaycastHit RaycastWorking(Vector3 _origin, Vector3 _direction, LayerMask _layerMask)
    {
        RaycastHit hit;
        // Vector3 fwd = transform.TransformDirection(Vector3.forward);

        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(_origin, _direction, out hit, Mathf.Infinity, _layerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            Debug.Log("Did Hit");
        }
        else
        {
            Debug.DrawRay(transform.position, _direction * 1000, Color.white);
            Debug.Log("Did not Hit");            
        }

        return hit;
    }
}
