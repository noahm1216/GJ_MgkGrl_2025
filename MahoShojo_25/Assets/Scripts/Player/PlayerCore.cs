using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCore : MonoBehaviour
{
    [Header("Input Keys\n______________")]
    [Tooltip("Move our character up")]
    public KeyCode key_MoveUp = KeyCode.W;
    [Tooltip("Move our character down")]
    public KeyCode key_MoveDown = KeyCode.S;
    [Tooltip("When true this makes sure our character's always in the 2.5D position we expect them to be")]
    public bool lockYPositionAtZero = true;


    [Space]
    [Header("Jump Ability\n______________")]
    public bool jumpBasedOnKeyPressTime;
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


    [Range(0.00f, 10)]
    public float jumpAcceleration = 9f;
    [Range(0.00f, 100)]
    public float fallAcceleration = 15f;

    private float inputXYTime; // the time we press down any of the keys
    public int dir { get; private set; } = 1;
    private int timesJumpedSinceLastGround;
    private float jumpLeftToAchieve;
    private float lagInputTime;

    // slam down ability (optional if time)

    [Space]
    [Header("Camera\n______________")]
    public BehaviorCameraFollower ref_BehaviorCameraFollower;


    [Space]
    [Header("Animations\n______________")]
    public PlayerAnimations ref_PlayerAnimations;
    public int activeModel = 0;
    public Transform[] modelsToPickFrom;

    private int activeModelReference;


    [Space]
    [Header("Input Keys Events UP\n______________")]
    public UnityEvent onPress_MoveUp;
    public UnityEvent onRelease_MoveUp;
    public UnityEvent onSuccess_MoveUp;

    [Space]
    [Header("Input Keys Events DOWN\n______________")]
    public UnityEvent onPress_MoveDown;
    public UnityEvent onRelease_MoveDown;
    public UnityEvent onSuccess_MoveDown;

    [Space]
    [Header("Reaction Events\n______________")]
    public UnityEvent onHit, onDeath, onModelChange, onPockiBoxCollect, onPockiStickCollect;

    public bool CanJump()
    {
        return timesJumpedSinceLastGround < numberOfJumps;
    }

    private void Start()
    {
        jumpLeftToAchieve = maximumJumpPower;

        if (!ref_BehaviorCameraFollower && Camera.main)
            Camera.main.TryGetComponent(out ref_BehaviorCameraFollower);
    }

    private void Update() // TODO: remove inputXY from affecting jump (always affect at full speed)
    {
        if (activeModelReference != activeModel && modelsToPickFrom.Length > 0)
        {
            if (activeModel >= modelsToPickFrom.Length)
                activeModel = 0;
            if (activeModel < 0)
                activeModel = modelsToPickFrom.Length - 1;

            for (int i = 0; i < modelsToPickFrom.Length; i++)
                if (i == activeModel)
                    modelsToPickFrom[i].GetChild(0).gameObject.SetActive(true);
                else
                    modelsToPickFrom[i].GetChild(0).gameObject.SetActive(false);

            activeModelReference = activeModel;
            // onModelChange.Invoke();
        }

        if (Manager_GameState.Instance) // if we have the game manager then we want things to look a specific way
        {
            ReactToGameManager();
            if (Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing)
                return;
        }

        if (lockYPositionAtZero)
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        CheckIfBlocked();

        if (Input.GetKey(key_MoveUp))
        {
            inputXYTime += Time.deltaTime * speedToMaxJumpPower;
            if (!jumpBasedOnKeyPressTime)
                inputXYTime = 1;

            if (inputXYTime > 1)
                inputXYTime = 1;           

            onPress_MoveUp.Invoke();
        }

        if (Input.GetKeyDown(key_MoveUp))
        {
            onRelease_MoveUp.Invoke(); // changed to pres down instead of press up
            if (CanJump())
            {
                if (inputXYTime <= inputTimeForQuickJumps) // jumpTime for tapping inputs
                    inputXYTime = quickJumpPercentOfMaxJump;

                dir = 1;
                //if (rb3D)
                //    rb3D.AddForce((Vector3.up * dir) * (maximumJumpPower * inputXYTime)); // we want to do an arc (and have it in fixed update)
                lagInputTime = inputXYTime;
                inputXYTime = 0;
                timesJumpedSinceLastGround++;
                if (ref_PlayerAnimations) // jump animation & // falling animation
                { ref_PlayerAnimations.SetAnyTrigger("Jumped"); } // ref_PlayerAnimations.SetAnyBool("isFalling", true);
                if (Manager_Platforms.Instance)
                    Manager_Platforms.Instance.ChangePlayerInAir(true);
                jumpLeftToAchieve = 0;// maximumJumpPower;

                if (ref_BehaviorCameraFollower)
                    ref_BehaviorCameraFollower.StoreChangeState(BehaviorCameraFollower.CameraFocusState.InTheAir, false);

                onSuccess_MoveUp.Invoke();
            }
        }
    }

    private void FixedUpdate()
    {
        if (jumpLeftToAchieve < maximumJumpPower)
        {
            if (rb3D)
                rb3D.AddForce((Vector3.up * dir) * jumpLeftToAchieve * lagInputTime);
            jumpLeftToAchieve += 1 * jumpAcceleration;
        }
        if (rb3D.velocity.y < 0) // faling / moving down
            rb3D.AddForce(Vector3.down * (1 * fallAcceleration));

        if (Manager_GameState.Instance)
        {
            if (Manager_GameState.Instance.capturedCreatues_Unique >= 6 && transform.position.y < 25) // falling off the buildings
            {
                //print($"transform.position = {transform.position}");
                Manager_GameState.Instance.WonTheGame();
            }
        }
    }

    private void ReactToGameManager()
    {

    }

    public void ChangeModel_TransformMaho()
    {
        StartCoroutine(AnimateModelChange());
    }

    private IEnumerator AnimateModelChange()
    {
        if (ref_PlayerAnimations)
            ref_PlayerAnimations.SetAnyTrigger("Transform");

        yield return new WaitForSeconds(0.5f);
        onModelChange.Invoke();
        yield return new WaitForSeconds(0.5f);
        activeModel = 0;
    }

    private void CheckIfBlocked() // if moving in a direction but cant keep going
    {
        if (Manager_Platforms.Instance)
        {
            Vector3 raycastDirection = transform.right;

            if (Manager_Platforms.Instance.dir < 0) // forward or idle
                raycastDirection = -transform.right;

            //Manager_Platforms.Instance.ChangeIsBlocked(Physics.Raycast(transform.position + offset, transform.TransformDirection(raycastDirection), out hit, raycastDistance, layersThatResetJumps));
            if (CheckBlockerRaycasts((transform.right), Manager_Platforms.Instance.dir, new Vector3(0, 0.25f, 0), 0.15f) ||
                CheckBlockerRaycasts(transform.right, Manager_Platforms.Instance.dir, new Vector3(0, 0.65f, 0), 0.5f) ||
                CheckBlockerRaycasts(transform.right, Manager_Platforms.Instance.dir, new Vector3(0, 1f, 0), 1.05f))
                Manager_Platforms.Instance.ChangeIsBlocked(true);
            else
                Manager_Platforms.Instance.ChangeIsBlocked(false);
        }
    }

    private bool CheckBlockerRaycasts(Vector3 _raycastDirection, int _dir, Vector3 _offset, float _dist)
    {
        if (_dir < 0) // forward or idle
            _raycastDirection = -_raycastDirection;

        RaycastHit hit; // raycast to the nearest wall within X (raycastDistance) meters and if there is a wall our speed is zero
        Debug.DrawLine(transform.position + _offset, transform.position + _offset + (new Vector3(_dist * -_dir, 0, 0)), Color.red);
        return Physics.Raycast(transform.position + _offset, transform.TransformDirection(_raycastDirection), out hit, _dist, layersThatResetJumps);
    }


    private void OnCollisionEnter(Collision col)
    {
        if ((layersThatResetJumps.value & (1 << col.transform.gameObject.layer)) > 0) // collide with object within our specified layers
        {
            timesJumpedSinceLastGround = 0;
            if (ref_PlayerAnimations) // falling animation
                ref_PlayerAnimations.SetAnyBool("isFalling", false);
            if (Manager_Platforms.Instance)
                Manager_Platforms.Instance.ChangePlayerInAir(false);
        }
    }

    private void OnTriggerEnter(Collider trig)
    {
        //print($"trig: with: {trig.tag}");
        if (trig.tag == "Monster") // we were hit
        {
            if (Manager_GameState.Instance)
                if (Manager_GameState.Instance.ChangeHitPoints(-1))
                    onDeath.Invoke();
                else
                    onHit.Invoke();
        }

        if (trig.tag == "PockiBox") // we were hit
        {
            if (Manager_Platforms.Instance)
                if (!Manager_Platforms.Instance.playerUnlockedPockiBox)
                    Manager_Platforms.Instance.playerUnlockedPockiBox = true;
            if (trig.GetComponent<Collectible>())
                trig.GetComponent<Collectible>().Interacted();

            onPockiBoxCollect.Invoke();
        }

        if (trig.tag == "PockiStick") // we were hit
        {
            if (Manager_Platforms.Instance)
                Manager_Platforms.Instance.pockiBoxSticks += 1;
            if (trig.GetComponent<Collectible>())
                trig.GetComponent<Collectible>().Interacted();

            onPockiStickCollect.Invoke();
        }
    }
}
