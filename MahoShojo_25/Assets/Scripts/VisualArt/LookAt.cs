using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    public Transform lookAtObj;

    void Start()
    {
    }

    void Update()
    {
     if(lookAtObj) transform.LookAt(lookAtObj);
    }
}
