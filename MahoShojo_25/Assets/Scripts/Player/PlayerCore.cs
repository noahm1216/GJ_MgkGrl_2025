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
    [Tooltip("When true this makes sure our character's always in the 2.5D position we expect them to be")]
    public bool lockYPositionAtZero = true;

    // jump ability
    public float maximumJumpPower = 500;
    public Rigidbody rb3D;
    // can add private variables to itterate jumping up as a translate if we dont have a rigidbody (but dont need to right now)
    [Range(0.00f, 1)]
    public float quickJumpPercentOfMaxJump = 0.66f;
    [Range(0.00f, 1)]
    public float inputTimeForQuickJumps = 0.25f;
    [Range(0.1f, 10)]
    public float speedToMaxJumpPower = 3;
    [Range(0, 10)]
    public int numberOfJumps = 2;
    public LayerMask layersThatResetJumps;

    private float inputXYTime; // the time we press down any of the keys
    public int dir { get; private set; } = 1;
    private int timesJumpedSinceLastGround;

    // slam down ability


    // dash right/ left ability


    // animation
    public PlayerAnimations ref_PlayerAnimations;

    public bool CanJump()
    {
        return timesJumpedSinceLastGround < numberOfJumps;
    }

    private void Update()
    {
        if (lockYPositionAtZero)
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        CheckIfBlocked();

        if (Input.GetKey(key_MoveUp))
        {
            inputXYTime += Time.deltaTime * speedToMaxJumpPower;
            if (inputXYTime > 1)
                inputXYTime = 1;
        }

        if (Input.GetKeyUp(key_MoveUp) && CanJump())
        {
            if (inputXYTime <= inputTimeForQuickJumps) // jumpTime for tapping inputs
                inputXYTime = quickJumpPercentOfMaxJump;

            dir = 1;
            if (rb3D)
                rb3D.AddForce((Vector3.up * dir) * (maximumJumpPower * inputXYTime));
            inputXYTime = 0;
            timesJumpedSinceLastGround++;
            if (ref_PlayerAnimations) // jump animation & // falling animation
            { ref_PlayerAnimations.SetAnyTrigger("Jumped"); ref_PlayerAnimations.SetAnyBool("isFalling", true); }
        }       
    }

    private void CheckIfBlocked() // if moving in a direction but cant keep going
    {
        if (Manager_Platforms.Instance)
        {
            Vector3 raycastDirection = transform.right;

            if (Manager_Platforms.Instance.dir < 0) // forward or idle
                raycastDirection = -transform.right;

            float raycastDistance = 1.05f;
            Vector3 offset = new Vector3(0, 0.65f, 0);
            RaycastHit hit; // raycast to the nearest wall within X (raycastDistance) meters and if there is a wall our speed is zero
            Debug.DrawLine(transform.position + offset, transform.position + offset + (new Vector3(raycastDistance * -Manager_Platforms.Instance.dir, 0,0)), Color.red);

            if(Physics.Raycast(transform.position + offset, transform.TransformDirection(raycastDirection), out hit, raycastDistance, layersThatResetJumps))
                { print("something in front of our move dir"); }

            Manager_Platforms.Instance.ChangeIsBlocked(Physics.Raycast(transform.position + offset, transform.TransformDirection(raycastDirection), out hit, raycastDistance, layersThatResetJumps));
           
        }

    }


    private void OnCollisionEnter(Collision col)
    {
        if ((layersThatResetJumps.value & (1 << col.transform.gameObject.layer)) > 0) // collide with object within our specified layers
        {
            timesJumpedSinceLastGround = 0;
            if (ref_PlayerAnimations) // falling animation
                ref_PlayerAnimations.SetAnyBool("isFalling", false);
        }
    }
}
