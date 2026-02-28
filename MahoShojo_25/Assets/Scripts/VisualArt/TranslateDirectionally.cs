using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslateDirectionally : MonoBehaviour
{
    public bool useDeltaTime;
    public Vector3 directionalSpeedToMove;



    // Update is called once per frame
    void FixedUpdate()
    {
        if (useDeltaTime)
            transform.Translate(directionalSpeedToMove * Time.deltaTime);
        else
            transform.Translate(directionalSpeedToMove);
    }
}
