using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class PlayerCore : MonoBehaviour
{
    #region INPUT_KEYS

    [Header("Input Keys\n______________")]

    [Tooltip("Move our character up")]
    public KeyCode key_MoveUp = KeyCode.W;

    [Tooltip("Move our character down")]
    public KeyCode key_MoveDown = KeyCode.S;

    [Tooltip("When true this makes sure our character's always in the 2.5D position we expect them to be")]
    public bool lockYPositionAtZero = true;

    private KeyCode key_MoveUp2 = KeyCode.UpArrow;

    #endregion

    #region JUMP_ABILITY

    [Space]
    [Header("Jump Ability\n______________")]

    [Tooltip("When true the strength of our jump is based on how long the player holds the jump key")]
    public bool jumpBasedOnKeyPressTime;

    [Tooltip("The maximum jump force our character can achieve")]
    public float maximumJumpPower = 20;

    [Tooltip("The Rigidbody we use for all movement and jump physics")]
    public Rigidbody rb3D;

    [Tooltip("If the player taps jump quickly, this percent of the maximum jump power is used instead")]
    [Range(0.00f, 1)]
    public float quickJumpPercentOfMaxJump = 0.66f;

    [Tooltip("How long the player can press jump before it is no longer considered a quick jump")]
    [Range(0.00f, 1)]
    public float inputTimeForQuickJumps = 0.25f;

    [Tooltip("How quickly our jump reaches its maximum jump power")]
    [Range(0.1f, 10)]
    public float speedToMaxJumpPower = 3;

    [Tooltip("The number of jumps our character can perform before touching the ground again")]
    [Range(0, 10)]
    public int numberOfJumps = 2;

    [Tooltip("Any layers that reset our jumps when touched")]
    public LayerMask layersThatResetJumps;

    [Tooltip("When the player releases jump while rising, this reduces the current upward velocity")]
    [Range(0.1f, 1f)]
    public float jumpReleaseVelocityMultiplier = 0.5f;

    [Tooltip("How quickly additional jump force is added during a jump")]
    [Range(0.00f, 10)]
    public float jumpAcceleration = 9f;

    [Tooltip("How aggressively we shorten upward movement when the player releases jump early")]
    [Range(0.00f, 100)]
    public float fallAcceleration = 15f;

    [Tooltip("Extra gravity applied while falling to make the game feel less floaty overall")]
    [Range(1f, 10f)]
    public float gravityMultiplier = 2.5f;

    [Header("Fall Tuning\n______________")]

    [Tooltip("While holding jump during a fall, this slows how quickly the player falls. Lower values feel floatier")]
    [Range(0.1f, 3f)]
    public float heldJumpFallGravityMultiplier = 0.75f;

    [Tooltip("While NOT holding jump during a fall, this increases gravity for faster more precise falling")]
    [Range(1f, 10f)]
    public float releasedJumpFallGravityMultiplier = 3.5f;

    [Tooltip("Additional downward force applied while fast-falling after releasing jump")]
    [Range(0f, 50f)]
    public float releasedJumpFallAcceleration = 18f;

    private bool isJumpHeld;

    private float inputXYTime;

    public int dir    {        get;        private set;    } = 1;

    private int timesJumpedSinceLastGround;

    private float jumpLeftToAchieve;

    private float lagInputTime;

    private bool holdingJumpSinceJump;

    // Prevents repeated release logic.
    private bool jumpReleasedThisFrame;

    #endregion

    #region VISUALS

    [Space]
    [Header("Visuals\n______________")]

    [Tooltip("The visual effect shown on the player's right shoe during jumps")]
    public GameObject jumpShoeVfx_R;

    [Tooltip("The visual effect shown on the player's left shoe during jumps")]
    public GameObject jumpShoeVfx_L;

    #endregion

    #region CAMERA

    [Space]
    [Header("Camera\n______________")]

    [Tooltip("Reference to the camera behavior controller used to react to player movement states")]
    public BehaviorCameraFollower ref_BehaviorCameraFollower;

    #endregion

    #region PLATFORMS

    [Space]
    [Header("Platforms\n______________")]

    [Tooltip("Used for raycasts that help position the player onto platforms")]
    public RaycastCheck ref_RaycastCheck;

    #endregion

    #region ANIMATIONS

    [Space]
    [Header("Animations\n______________")]

    [Tooltip("Reference to the animation controller for our player")]
    public PlayerAnimations ref_PlayerAnimations;

    [Tooltip("The currently active player model index")]
    public int activeModel = 0;

    [Tooltip("A list of all player models that can be activated")]
    public Transform[] modelsToPickFrom;

    private int activeModelReference;

    #endregion

    #region EVENTS_UP

    [Space]
    [Header("Input Keys Events UP\n______________")]

    [Tooltip("Called every frame while the jump key is being held")]
    public UnityEvent onPress_MoveUp;

    [Tooltip("Called when the player releases the jump key")]
    public UnityEvent onRelease_MoveUp;

    [Tooltip("Called after a jump is successfully performed")]
    public UnityEvent onSuccess_MoveUp;

    #endregion

    #region EVENTS_DOWN

    [Space]
    [Header("Input Keys Events DOWN\n______________")]

    [Tooltip("Called every frame while the down key is being held")]
    public UnityEvent onPress_MoveDown;

    [Tooltip("Called when the player releases the down key")]
    public UnityEvent onRelease_MoveDown;

    [Tooltip("Called after a successful down input action")]
    public UnityEvent onSuccess_MoveDown;

    #endregion

    #region REACTION_EVENTS

    [Space]
    [Header("Reaction Events\n______________")]

    [Tooltip("Called when the player takes damage but survives")]
    public UnityEvent onHit;

    [Tooltip("Called when the player dies")]
    public UnityEvent onDeath;

    [Tooltip("Called when the player changes models or transforms")]
    public UnityEvent onModelChange;

    [Tooltip("Called when the player collects a Pocki Box")]
    public UnityEvent onPockiBoxCollect;

    [Tooltip("Called when the player collects a Pocki Stick")]
    public UnityEvent onPockiStickCollect;

    #endregion

    #region CACHED_REFERENCES

    private Manager_Platforms _platforms;

    private Manager_GameState _gameState;

    private Manager_TutorialUI _tutorialUI;

    private Transform _cachedTransform;

    #endregion

    #region OPTIMIZATION_TIMERS

    // Reduces expensive raycasts.
    private float nextBlockCheckTime;

    [Header("Optimization\n______________")]

    [Tooltip("How often we check for nearby blockers using raycasts. Lower values are more responsive but more expensive")]
    [SerializeField]
    private float blockCheckInterval = 0.05f;

    #endregion



    #region UNITY_LIFECYCLE

    private void Start()
    {
        _cachedTransform = transform;

        _platforms = Manager_Platforms.Instance;

        _gameState = Manager_GameState.Instance;

        _tutorialUI = Manager_TutorialUI.Instance;

        if (_platforms)
            _platforms.PopulatePlayerCoreRef(this);

        if (!ref_BehaviorCameraFollower && Camera.main)
            Camera.main.TryGetComponent(
                out ref_BehaviorCameraFollower);

        jumpLeftToAchieve = maximumJumpPower;

        // IMPORTANT:
        // Helps prevent floaty startup states.
        ReleaseJump();
    }

    private void Update()
    {
        HandleModelChanges();

        if (_gameState)
        {
            ReactToGameManager();

            if (_gameState.currentState
                != Manager_GameState.GAMESTATE.Playing)
            {
                return;
            }
        }

        if (lockYPositionAtZero)
        {
            Vector3 pos = _cachedTransform.position;

            if (pos.z != 0)
            {
                pos.z = 0;
                _cachedTransform.position = pos;
            }
        }

        CheckForInputs();

        HandleBlockChecks();
    }

    private void FixedUpdate()
    {
        HandleJumpPhysics();

        HandleFallPhysics();

        HandleOutOfBounds();

        HandleXCorrection();
    }

    #endregion


    #region INPUT

    private void CheckForInputs()
    {
        bool jumpPressed =
            Input.GetKey(key_MoveUp)
            || Input.GetKey(key_MoveUp2);

        bool jumpDown =
            Input.GetKeyDown(key_MoveUp)
            || Input.GetKeyDown(key_MoveUp2);

        bool jumpReleased =
            Input.GetKeyUp(key_MoveUp)
            || Input.GetKeyUp(key_MoveUp2);

        if (jumpPressed)
        {
            HandleJumpHeld();
        }

        if (jumpDown)
        {
            StartJump();
        }

        if (jumpReleased)
        {
            ReleaseJump();
        }
    }

    private void HandleJumpHeld()
    {
        isJumpHeld = true;

        inputXYTime +=
            Time.deltaTime
            * speedToMaxJumpPower;

        if (!jumpBasedOnKeyPressTime)
        {
            inputXYTime = 1;
        }

        inputXYTime =
            Mathf.Clamp01(inputXYTime);

        onPress_MoveUp?.Invoke();
    }

    private void StartJump()
    {
        onRelease_MoveUp?.Invoke();

        if (!CanJump())
            return;

        if (_platforms && _platforms.isDashing)
            return;

        if (inputXYTime <= inputTimeForQuickJumps)
        {
            inputXYTime =
                quickJumpPercentOfMaxJump;
        }

        holdingJumpSinceJump = false;

        jumpReleasedThisFrame = false;

        dir = 1;

        lagInputTime = inputXYTime;

        inputXYTime = 0;

        timesJumpedSinceLastGround++;

        if (ref_PlayerAnimations)
        {
            ref_PlayerAnimations.SetAnyTrigger(
                "Jumped");
        }

        if (_platforms)
        {
            _platforms.ChangePlayerInAir(true);
        }

        jumpLeftToAchieve = 0;

        if (ref_BehaviorCameraFollower)
        {
            ref_BehaviorCameraFollower
                .StoreChangeState(
                    BehaviorCameraFollower
                    .CameraFocusState
                    .InTheAir,
                    false);
        }

        UpdateVfx_Jump(
            timesJumpedSinceLastGround);

        onSuccess_MoveUp?.Invoke();
    }

    private void ReleaseJump()
    {
        if (jumpReleasedThisFrame)
            return;

        jumpReleasedThisFrame = true;

        isJumpHeld = false;

        holdingJumpSinceJump = true;

        if (rb3D && rb3D.velocity.y > 0)
        {
            rb3D.velocity =
                new Vector3(
                    rb3D.velocity.x,
                    rb3D.velocity.y
                    * jumpReleaseVelocityMultiplier,
                    rb3D.velocity.z);
        }
    }

    #endregion


    #region JUMP_PHYSICS

    public bool CanJump()
    {
        return timesJumpedSinceLastGround
            < numberOfJumps;
    }

    private void HandleJumpPhysics()
    {
        if (!rb3D)
            return;

        if (jumpLeftToAchieve >= maximumJumpPower)
            return;

        if (!isJumpHeld)
            return;

        rb3D.AddForce(
            (
                (Vector3.up * dir)
                * jumpLeftToAchieve
                * lagInputTime
                - rb3D.velocity
            ),
            ForceMode.VelocityChange);

        jumpLeftToAchieve +=
            jumpAcceleration;
    }

    private void HandleFallPhysics()
    {
        if (!rb3D)
            return;


        float gravity =
            Physics.gravity.y
            * Time.fixedDeltaTime;

        // FALLING

        if (rb3D.velocity.y < 0)
        {
            // FLOATY FALL
            // Holding jump slows descent.
            if (isJumpHeld)
            {
                rb3D.velocity +=
                    Vector3.up
                    * gravity
                    * (heldJumpFallGravityMultiplier - 1);
            }

            // FAST FALL
            // Letting go increases gravity heavily.
            else
            {
                rb3D.velocity +=
                    Vector3.up
                    * gravity
                    * (releasedJumpFallGravityMultiplier - 1);

                // Extra acceleration for sharper control.
                rb3D.velocity +=
                    Vector3.down
                    * releasedJumpFallAcceleration
                    * Time.fixedDeltaTime;
            }
        }

        // SHORT HOP CONTROL

        else if (
            rb3D.velocity.y > 0
            &&
            !isJumpHeld)
        {
            rb3D.velocity +=
                Vector3.up
                * gravity
                * (fallAcceleration - 1);
        }


    }


    #endregion


    #region MODEL_HANDLING

    private void HandleModelChanges()
    {
        if (activeModelReference == activeModel)
            return;

        if (modelsToPickFrom.Length <= 0)
            return;

        if (activeModel >= modelsToPickFrom.Length)
            activeModel = 0;

        if (activeModel < 0)
            activeModel =
                modelsToPickFrom.Length - 1;

        for (int i = 0;
            i < modelsToPickFrom.Length;
            i++)
        {
            bool active = i == activeModel;

            modelsToPickFrom[i]
                .GetChild(0)
                .gameObject
                .SetActive(active);
        }

        activeModelReference = activeModel;
    }

    #endregion


    #region BLOCK_CHECKS

    private void HandleBlockChecks()
    {
        // PERFORMANCE:
        // Only raycast periodically.

        if (Time.time < nextBlockCheckTime)
            return;

        nextBlockCheckTime =
            Time.time + blockCheckInterval;

        // PERFORMANCE:
        // Skip while airborne.

        if (timesJumpedSinceLastGround > 0)
            return;

        CheckIfBlocked();
    }

    private void CheckIfBlocked()
    {
        if (!_platforms)
            return;

        Vector3 raycastDirection =
            transform.right;

        if (_platforms.dir < 0)
            raycastDirection =
                -transform.right;

        if (rb3D &&
            Mathf.Abs(rb3D.velocity.x) > 0.1f)
        {
            _platforms.ChangeIsBlocked(
                true,
                null);
        }

        bool blocked =
            CheckBlockerRaycasts(
                transform.right,
                _platforms.dir,
                new Vector3(0, 0.25f, 0),
                0.15f)

            ||

            CheckBlockerRaycasts(
                transform.right,
                _platforms.dir,
                new Vector3(0, 0.65f, 0),
                0.375f)

            ||

            CheckBlockerRaycasts(
                transform.right,
                _platforms.dir,
                new Vector3(0, 1f, 0),
                0.5f);

        if (blocked)
        {
            _platforms.ChangeIsBlocked(
                true,
                RaycastHitObj());
        }
        else
        {
            _platforms.ChangeIsBlocked(
                false,
                null);
        }
    }

    #endregion


    #region PLAYER_CORRECTIONS

    private void HandleOutOfBounds()
    {
        if (!_gameState)
            return;

        if (_gameState.currentState
            != Manager_GameState.GAMESTATE.Playing)
        {
            return;
        }

        if (_cachedTransform.position.y < -20)
        {
            _gameState.GameOver();
        }
    }

    private void HandleXCorrection()
    {
        if (_cachedTransform.position.x > 1
            || _cachedTransform.position.x < -1)
        {
            AdjustPlayerToZeroX();
        }
    }

    private void AdjustPlayerToZeroX()
    {
        Vector3 pos =
            _cachedTransform.position;

        if (pos.x > 1)
        {
            pos -= new Vector3(
                Time.deltaTime
                * Mathf.Abs(pos.x),
                0,
                0);
        }

        if (pos.x < -1)
        {
            pos += new Vector3(
                Time.deltaTime
                * Mathf.Abs(pos.x),
                0,
                0);
        }

        _cachedTransform.position = pos;
    }

    #endregion


    #region VFX

    private void UpdateVfx_Jump(
        int _timesJumped)
    {
        bool rightEnabled = false;
        bool leftEnabled = false;

        switch (_timesJumped)
        {
            case 0:
                rightEnabled = true;
                leftEnabled = true;
                break;

            case 1:
                leftEnabled = true;
                break;
        }

        if (jumpShoeVfx_R)
            jumpShoeVfx_R.SetActive(
                rightEnabled);

        if (jumpShoeVfx_L)
            jumpShoeVfx_L.SetActive(
                leftEnabled);
    }

    #endregion


    #region EXISTING_BEHAVIOR

    private void ReactToGameManager()
    {

    }

    public void ChangeModel_TransformMaho()
    {
        StartCoroutine(
            AnimateModelChange());
    }

    public void SpawnPlayerOnPlatforms()
    {
        if (ref_RaycastCheck)
        {
            int attempts = 0;

            RaycastHit hit =
                new RaycastHit();

            while (
                hit.collider == null
                || attempts < 100)
            {
                hit =
                    ref_RaycastCheck
                    .RaycastWorking(
                        new Vector3(
                            0,
                            2 + (attempts * 0.75f),
                            0),
                        Vector3.down,
                        layersThatResetJumps);

                attempts++;
            }

            if (hit.collider)
            {
                transform.position =
                    new Vector3(
                        transform.position.x,
                        hit.collider.bounds.max.y,
                        transform.position.z);

                return;
            }
        }

        print( "Missing ref_RaycastToCheck |or| Unable to place character on a platform as designed");

        transform.position =
            new Vector3(0, 2, 0);
    }

    private IEnumerator AnimateModelChange()
    {
        yield return
            new WaitForSeconds(0.15f);

        SpawnPlayerOnPlatforms();

        if (ref_PlayerAnimations)
        {
            ref_PlayerAnimations
                .SetAnyTrigger(
                    "Transform");
        }

        yield return
            new WaitForSeconds(0.5f);

        onModelChange?.Invoke();

        yield return
            new WaitForSeconds(0.5f);

        activeModel = 0;
    }

    #endregion


    #region RAYCASTS

    private Transform RaycastHitObj()
    {
        int _dir = _platforms.dir;

        Vector3 _raycastDirection =
            transform.right;

        if (_dir < 0)
            _raycastDirection =
                -_raycastDirection;

        float _dist = 0.51f;

        Vector3 _offset =
            new Vector3(0, 0.65f, 0);

        RaycastHit hit;

        Debug.DrawLine(
            transform.position + _offset,
            transform.position + _offset
            + (new Vector3(
                _dist * -_dir,
                0,
                0)),
            Color.red);

        if (
            Physics.Raycast(
                transform.position + _offset,
                transform.TransformDirection(
                    _raycastDirection),
                out hit,
                _dist,
                layersThatResetJumps))
        {
            return hit.transform;
        }

        return null;
    }

    private bool CheckBlockerRaycasts(
        Vector3 _raycastDirection,
        int _dir,
        Vector3 _offset,
        float _dist)
    {
        if (_dir < 0)
            _raycastDirection =
                -_raycastDirection;

        RaycastHit hit;

        Debug.DrawLine(
            transform.position + _offset,
            transform.position + _offset
            + (new Vector3(
                _dist * -_dir,
                0,
                0)),
            Color.red);

        return Physics.Raycast(
            transform.position + _offset,
            transform.TransformDirection(
                _raycastDirection),
            out hit,
            _dist,
            layersThatResetJumps);
    }

    #endregion


    #region COLLISIONS

    private void OnCollisionEnter(
        Collision col)
    {
        if (
            (layersThatResetJumps.value
            &
            (1 << col.transform.gameObject.layer))
            > 0)
        {
            timesJumpedSinceLastGround = 0;

            UpdateVfx_Jump(
                timesJumpedSinceLastGround);

            if (ref_PlayerAnimations)
            {
                ref_PlayerAnimations
                    .SetAnyBool(
                        "isFalling",
                        false);
            }

            if (_platforms)
            {
                _platforms
                    .ChangePlayerInAir(false);
            }
        }

        if (col.transform.CompareTag(
            "Obstacle"))
        {
            HandleObstacleInteraction(
                col.transform);
        }
    }

    private void OnTriggerEnter(
        Collider trig)
    {
        if (trig.CompareTag("Monster"))
        {
            if (_gameState)
            {
                if (_gameState
                    .ChangeHitPoints(-1))
                {
                    onDeath?.Invoke();
                }
                else
                {
                    onHit?.Invoke();
                }
            }
        }

        if (trig.CompareTag("PockiBox"))
        {
            HandlePockiBox(trig);
        }

        if (trig.CompareTag("PockiStick"))
        {
            HandlePockiStick(trig);
        }

        if (trig.CompareTag("Obstacle"))
        {
            HandleObstacleInteraction(
                trig.transform);
        }
    }

    private void OnTriggerStay(
        Collider trig)
    {
        if (
            trig.CompareTag("Obstacle")
            &&
            _platforms
            &&
            _platforms.isDashing)
        {
            BehaviorObstacles ref_BehObs =
                null;

            trig.TryGetComponent(
                out ref_BehObs);

            if (ref_BehObs)
            {
                ref_BehObs.Interacted(
                    transform,
                    this,
                    BehaviorObstacles
                    .signalType
                    .Dash);
            }
        }
    }

    #endregion


    #region INTERACTIONS

    private void HandleObstacleInteraction(
        Transform obstacle)
    {
        BehaviorObstacles ref_BehObs =
            null;

        obstacle.TryGetComponent(
            out ref_BehObs);

        if (!ref_BehObs)
        {
            print(
                "Unable To Handle Interaction with this Obstacle");

            return;
        }

        if (_platforms &&
            _platforms.isDashing)
        {
            ref_BehObs.Interacted(
                transform,
                this,
                BehaviorObstacles
                .signalType
                .Dash);
        }
        else
        {
            ref_BehObs.Interacted(
                transform,
                this,
                BehaviorObstacles
                .signalType
                .Bump);
        }
    }

    private void HandlePockiBox(
        Collider trig)
    {
        if (_platforms)
        {
            if (!_platforms.playerUnlockedPockiBox)
            {
                _platforms
                    .playerUnlockedPockiBox = true;

                if (_tutorialUI)
                {
                    _tutorialUI
                        .DisplayPockiCollection(
                            true);
                }
            }
        }

        Collectible collectible =
            trig.GetComponent<Collectible>();

        if (collectible)
        {
            collectible.Interacted();
        }

        onPockiBoxCollect?.Invoke();
    }

    private void HandlePockiStick(
        Collider trig)
    {
        if (_platforms)
        {
            _platforms.pockiBoxSticks += 1;

            if (_tutorialUI)
            {
                _tutorialUI
                    .DisplayPockiCollection(
                        true);
            }
        }

        Collectible collectible =
            trig.GetComponent<Collectible>();

        if (collectible)
        {
            collectible.Interacted();
        }

        onPockiStickCollect?.Invoke();
    }

#endregion

}
