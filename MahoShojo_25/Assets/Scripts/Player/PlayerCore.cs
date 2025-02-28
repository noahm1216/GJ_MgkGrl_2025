using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCore : MonoBehaviour
{
    [Header("Input Keys\n______________")]
    [Tooltip("Move our character up")]
    public KeyCode key_MoveUp = KeyCode.W;
    [Tooltip("Move our character down")]
    public KeyCode key_MoveDown = KeyCode.S;


    // jump ability
    public float maximumJumpPower = 500;
    public Rigidbody rb3D;
    // can add private variables to itterate jumping up as a translate if we dont have a rigidbody (but dont need to right now)
    [Range(0.1f, 10)]
    public float speedToMaxJumpPower = 3;
    [Range(0,10)]
    public int numberOfJumps = 2;
    public LayerMask layersThatResetJumps;

    private float inputXYTime; // the time we press down any of the keys
    private int dir = 1;
    private int timesJumpedSinceLastGround;

    // slam down ability


    // dash right/ left ability


    public bool CanJump()
    {
        return timesJumpedSinceLastGround < numberOfJumps;
    }

    private void Update()
    {
        if(Input.GetKey(key_MoveUp))
        {
            inputXYTime += Time.deltaTime * speedToMaxJumpPower;
            if (inputXYTime > 1)
                inputXYTime = 1;
        }

        if (Input.GetKeyUp(key_MoveUp) && CanJump())
        {
            dir = 1;
            if (rb3D)
                rb3D.AddForce((Vector3.up * dir) * (maximumJumpPower * inputXYTime));
            inputXYTime = 0;
            timesJumpedSinceLastGround++;
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if ((layersThatResetJumps.value & (1 << col.transform.gameObject.layer)) > 0) // collide with object within our specified layers
        {
            timesJumpedSinceLastGround = 0;
        }
    }
}
