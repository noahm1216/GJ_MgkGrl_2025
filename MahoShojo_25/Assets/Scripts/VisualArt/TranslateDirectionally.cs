using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslateDirectionally : MonoBehaviour
{

    public Vector3 directionalSpeedToMove;

    

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(directionalSpeedToMove);
    }
}
