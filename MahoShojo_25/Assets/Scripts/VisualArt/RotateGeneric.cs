using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGeneric : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAmount;
    [SerializeField] private Space rotSpace;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationAmount * Time.deltaTime, rotSpace);
    }
}
