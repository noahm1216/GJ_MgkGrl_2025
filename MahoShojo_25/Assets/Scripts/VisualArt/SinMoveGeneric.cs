using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinMoveGeneric : MonoBehaviour
{
    private Vector3 startPos;
    [SerializeField] Vector3 amount;
    [SerializeField] float speedOffset = 1;
    [SerializeField] Transform objectToFollow;
    [SerializeField] Vector3 offsetFromObject;

    void Start()
    {
        if (objectToFollow == null)
            startPos = transform.position;        
    }

    void Update()
    {
        if (objectToFollow != null)
            startPos = objectToFollow.position + offsetFromObject;

        transform.position = startPos + new Vector3(Mathf.Sin(Time.time* speedOffset) * amount.x, Mathf.Sin(Time.time* speedOffset) * amount.y, Mathf.Sin(Time.time* speedOffset) * amount.z);
    }
}