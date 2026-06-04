using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// PlayerCore
///
/// REFACTOR GOALS:
/// - Reduce Update overhead
/// - Reduce repeated singleton lookups
/// - Prepare for modular controllers
/// - Maintain compatibility with existing systems
///
/// FUTURE SPLIT TARGETS:
/// - PlayerInputController
/// - PlayerJumpController
/// - PlayerCollisionController
/// - PlayerVisualController
/// - PlayerSpawnController
/// - PlayerBlockerDetector
/// </summary>
public class PlayerCore : MonoBehaviour
{
    #region EVENTS

    // EVENT-DRIVEN REFACTOR NOTES
    // ------------------------------------------------------
    // Replace direct manager manipulation later with events.
    //
    // EXAMPLE:
    // OLD:
    // Manager_Platforms.Instance.ChangePlayerInAir(true);
    //
    // NEW:
    // OnAirStateChanged?.Invoke(true);

    public event System.Action OnJumpStarted;
    public event System.Action OnJumpReleased;
    public event System.Action OnPlayerLanded;
    public event System.Action<bool> OnAirStateChanged;
    public event System.Action<int> OnJumpCountChanged;
    public event System.Action<Transform> OnObstacleHit;

    #endregion


    #region REFERENCES

    [Header("References\n______________")]

    public Rigidbody rb3D;

    public RaycastCheck ref_RaycastCheck;

    public PlayerAnimations ref_PlayerAnimations;

    public BehaviorCameraFollower ref_BehaviorCameraFollower;

    // Cached references
    private Manager_Platforms _platformManager;
    private Manager_GameState _gameState;
    private Transform _cachedTransform;

    #endregion


    #region INPUT_KEYS

    [Header("Input Keys\n______________")]

    [Tooltip("Move our character up")]
    public KeyCode key_MoveUp = KeyCode.W;

    [Tooltip("Move our character down")]
    public KeyCode key_MoveDown = KeyCode.S;

    private KeyCode key_MoveUp2 = KeyCode.UpArrow;

    #endregion


    #region INPUT_RUNTIME

    /// <summary>
    /// FUTURE SCRIPT:
    /// PlayerInputController
    /// </summary>

    private bool _jumpHeld;
    private bool _jumpPressed;
    private bool _jumpReleased;

    private void ReadInputs()
    {
        _jumpHeld =
            Input.GetKey(key_MoveUp) ||
            Input.GetKey(key_MoveUp2);

        _jumpPressed =
            Input.GetKeyDown(key_MoveUp) ||
            Input.GetKeyDown(key_MoveUp2);

        _jumpReleased =
            Input.GetKeyUp(key_MoveUp) ||
            Input.GetKeyUp(key_MoveUp2);
    }

    #endregion


    #region PLAYER_STATE

    [Header("Player State\n______________")]

    [Tooltip("When true this makes sure our character's always in the 2.5D position we expect them to be")]
    public bool lockYPositionAtZero = true;

    public int dir
    {
        get;
        private set;
    } = 1;

    private int timesJumpedSinceLastGround;

    private bool isJumpHeld;

    private bool holdingJumpSinceJump;

    private float inputXYTime;

    private float lagInputTime;

    private float jumpLeftToAchieve;

    private bool triggeredWin;

    #endregion


    #region PLAYER_JUMP_CONTROLLER

    /// <summary>
    /// FUTURE SCRIPT:
    /// PlayerJumpController
    /// </summary>

    [Header("Jump Ability\n______________")]

    public bool jumpBasedOnKeyPressTime;

    public float maximumJumpPower = 20;

    [Range(0.00f, 1)]
    public float quickJumpPercentOfMaxJump = 0.66f;

    [Range(0.00f, 1)]
    public float inputTimeForQuickJumps = 0.25f;

    [Range(0.1f, 10)]
    public float speedToMaxJumpPower = 3;

    [Range(0, 10)]
    public int numberOfJumps = 2;

    public LayerMask layersThatResetJumps;

    [Range(0.1f, 1f)]
    public float jumpReleaseVelocityMultiplier = 0.5f;

    [Range(0.00f, 10)]
    public float jumpAcceleration = 9f;

    [Range(0.00f, 100)]
    public float fallAcceleration = 15f;

    public bool CanJump()
    {
        return timesJumpedSinceLastGround < numberOfJumps;
    }

    private void HandleJumpInput()
    {
        if (_jumpHeld)
        {
            isJumpHeld = true;

            inputXYTime +=
                Time.deltaTime *
                speedToMaxJumpPower;

            if (!jumpBasedOnKeyPressTime)
                inputXYTime = 1;

            inputXYTime = Mathf.Clamp01(inputXYTime);
        }

        if (_jumpPressed)
        { StartJump(); }

        if (_jumpReleased)
        { ReleaseJump();  }
    }

    private void StartJump()
    {

        onPress_MoveUp.Invoke();

        if (!CanJump())
            return;

        if (_platformManager != null &&
            _platformManager.isDashing)
            return;

        if (inputXYTime <= inputTimeForQuickJumps)
            inputXYTime = quickJumpPercentOfMaxJump;

        holdingJumpSinceJump = false;

        dir = 1;

        lagInputTime = inputXYTime;

        inputXYTime = 0;

        timesJumpedSinceLastGround++;

        jumpLeftToAchieve = 0;

        // ------------------------------------------------------
        // EVENT REFACTOR NOTES
        // ------------------------------------------------------

        onSuccess_MoveUp?.Invoke();
        OnJumpStarted?.Invoke();

        // Replace:
        // Manager_Platforms.Instance.ChangePlayerInAir(true);

        OnAirStateChanged?.Invoke(true);

        // Replace:
        // ref_BehaviorCameraFollower.StoreChangeState(
        // BehaviorCameraFollower.CameraFocusState.InTheAir,
        // false);

        OnJumpCountChanged?.Invoke(
            timesJumpedSinceLastGround);

        if (ref_PlayerAnimations)
            ref_PlayerAnimations.SetAnyTrigger("Jumped");

        if (ref_BehaviorCameraFollower)
        {
            ref_BehaviorCameraFollower.StoreChangeState(
                BehaviorCameraFollower.CameraFocusState.InTheAir,
                false);
        }

        if (_platformManager)
            _platformManager.ChangePlayerInAir(true);

        UpdateVfx_Jump(timesJumpedSinceLastGround);

        onSuccess_MoveUp.Invoke();
    }

    private void ReleaseJump()
    {
        isJumpHeld = false;

        holdingJumpSinceJump = true;

        onRelease_MoveUp.Invoke();
        OnJumpReleased?.Invoke();

        if (rb3D != null &&
            rb3D.velocity.y > 0)
        {
            rb3D.velocity = new Vector3(
                rb3D.velocity.x,
                rb3D.velocity.y *
                jumpReleaseVelocityMultiplier,
                rb3D.velocity.z);
        }
    }

    private void ApplyJumpPhysics()
    {
        if (jumpLeftToAchieve < maximumJumpPower &&
            isJumpHeld)
        {
            if (rb3D != null)
            {
                rb3D.AddForce(
                    (
                        (Vector3.up * dir)
                        * jumpLeftToAchieve
                        * lagInputTime
                        - rb3D.velocity
                    ),
                    ForceMode.VelocityChange);
            }

            jumpLeftToAchieve += jumpAcceleration;
        }

        if (!holdingJumpSinceJump &&
            rb3D != null &&
            rb3D.velocity.y < 0)
        {
            rb3D.AddForce(
                Vector3.down *
                (1 + rb3D.velocity.y * fallAcceleration));
        }
    }

    #endregion


    #region PLAYER_BLOCKER_DETECTOR

    /// <summary>
    /// FUTURE SCRIPT:
    /// PlayerBlockerDetector
    /// 
    /// PERFORMANCE IMPROVEMENTS:
    /// - Delayed raycast checks
    /// - Cached transform
    /// - Reduced singleton lookups
    /// 
    /// FUTURE:
    /// Replace 3 raycasts with BoxCast
    /// </summary>

    [Header("Blocker Detection\n______________")]

    [SerializeField]
    private float blockerCheckInterval = 0.05f;

    private float nextBlockerCheckTime;

    private void HandleBlockerChecks()
    {
        if (_platformManager == null)
            return;

        if (Time.time < nextBlockerCheckTime)
            return;

        nextBlockerCheckTime =
            Time.time + blockerCheckInterval;

        CheckIfBlocked();
    }

    private void CheckIfBlocked()
    {
        Vector3 direction = _cachedTransform.right;

        if (_platformManager.dir < 0)
            direction = -direction;

        bool blocked =
            CheckBlockerRaycasts(
                direction,
                _platformManager.dir,
                new Vector3(0, 0.25f, 0),
                0.15f)

            ||

            CheckBlockerRaycasts(
                direction,
                _platformManager.dir,
                new Vector3(0, 0.65f, 0),
                0.375f)

            ||

            CheckBlockerRaycasts(
                direction,
                _platformManager.dir,
                new Vector3(0, 1f, 0),
                0.5f);

        _platformManager.ChangeIsBlocked(
            blocked,
            blocked ? RaycastHitObj() : null);
    }

    private Transform RaycastHitObj()
    {
        int moveDir = _platformManager.dir;

        Vector3 direction = _cachedTransform.right;

        if (moveDir < 0)
            direction = -direction;

        float dist = 0.51f;

        Vector3 offset =
            new Vector3(0, 0.65f, 0);

        RaycastHit hit;

        Debug.DrawLine(
            _cachedTransform.position + offset,
            _cachedTransform.position + offset +
            (new Vector3(dist * -moveDir, 0, 0)),
            Color.red);

        if (Physics.Raycast(
            _cachedTransform.position + offset,
            _cachedTransform.TransformDirection(direction),
            out hit,
            dist,
            layersThatResetJumps))
        {
            return hit.transform;
        }

        return null;
    }

    private bool CheckBlockerRaycasts(
        Vector3 direction,
        int moveDir,
        Vector3 offset,
        float dist)
    {
        if (moveDir < 0)
            direction = -direction;

        RaycastHit hit;

        Debug.DrawLine(
            _cachedTransform.position + offset,
            _cachedTransform.position + offset +
            (new Vector3(dist * -moveDir, 0, 0)),
            Color.red);

        return Physics.Raycast(
            _cachedTransform.position + offset,
            _cachedTransform.TransformDirection(direction),
            out hit,
            dist,
            layersThatResetJumps);
    }

    #endregion


    #region PLAYER_VISUAL_CONTROLLER



    /// <summary>
    /// FUTURE SCRIPT:
    /// PlayerVisualController
    /// </summary>

    [Header("Visuals\n______________")]

    public GameObject jumpShoeVfx_R;

    public GameObject jumpShoeVfx_L;

    [Space]

    public int activeModel = 0;

    public Transform[] modelsToPickFrom;

    private int activeModelReference;

    private void HandleModelUpdates()
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
            bool enable =
                i == activeModel;

            modelsToPickFrom[i]
                .GetChild(0)
                .gameObject
                .SetActive(enable);
        }

        activeModelReference = activeModel;
    }

    private void UpdateVfx_Jump(int jumpsUsed)
    {
        bool rightEnabled = false;
        bool leftEnabled = false;

        switch (jumpsUsed)
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
            jumpShoeVfx_R.SetActive(rightEnabled);

        if (jumpShoeVfx_L)
            jumpShoeVfx_L.SetActive(leftEnabled);
    }

    #endregion


    #region PLAYER_POSITION_CONSTRAINTS

    /// <summary>
    /// FUTURE SCRIPT:
    /// PlayerPositionController
    /// </summary>

    private void HandlePositionConstraints()
    {
        if (lockYPositionAtZero)
        {
            Vector3 pos =
                _cachedTransform.position;

            pos.y = 0;

            _cachedTransform.position = pos;
        }

        if (_cachedTransform.position.x > 1 ||
            _cachedTransform.position.x < -1)
        {
            AdjustPlayerToZeroX();
        }
    }

    private void AdjustPlayerToZeroX()
    {
        Vector3 pos = _cachedTransform.position;

        if (pos.x > 1)
        {
            pos -= new Vector3(
                Time.deltaTime * Mathf.Abs(pos.x),
                0,
                0);
        }

        if (pos.x < -1)
        {
            pos += new Vector3(
                Time.deltaTime * Mathf.Abs(pos.x),
                0,
                0);
        }

        _cachedTransform.position = pos;
    }

    #endregion


    #region PLAYER_SPAWNING

    /// <summary>
    /// FUTURE SCRIPT:
    /// PlayerSpawnController
    /// </summary>

    public void SpawnPlayerOnPlatforms()
    {
        if (ref_RaycastCheck)
        {
            int attempts = 0;

            RaycastHit hit =
                new RaycastHit();

            while (
                hit.collider == null &&
                attempts < 100)
            {
                hit =
                    ref_RaycastCheck.RaycastWorking(
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
                _cachedTransform.position =
                    new Vector3(
                        _cachedTransform.position.x,
                        hit.collider.bounds.max.y,
                        _cachedTransform.position.z);

                return;
            }
        }

        Debug.LogWarning(
            "Missing RaycastCheck reference or unable to place player.");

        _cachedTransform.position =
            new Vector3(0, 2, 0);
    }

    #endregion


    #region PLAYER_COLLISION_CONTROLLER

    /// <summary>
    /// FUTURE SCRIPT:
    /// PlayerCollisionController
    /// </summary>

    private void OnCollisionEnter(Collision col)
    {
        if ((layersThatResetJumps.value &
            (1 << col.transform.gameObject.layer)) > 0)
        {
            timesJumpedSinceLastGround = 0;

            UpdateVfx_Jump(0);

            OnPlayerLanded?.Invoke();

            OnAirStateChanged?.Invoke(false);

            if (_platformManager)
                _platformManager.ChangePlayerInAir(false);

            if (ref_PlayerAnimations)
                ref_PlayerAnimations.SetAnyBool(
                    "isFalling",
                    false);
        }

        if (col.transform.CompareTag("Obstacle"))
        {
            HandleObstacleInteraction(col.transform);
        }
    }

    private void OnTriggerEnter(Collider trig)
    {
        if (trig.CompareTag("Monster"))
        {
            HandleMonster(trig);
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
            HandleObstacleInteraction(trig.transform);
        }
    }

    private void HandleObstacleInteraction(
        Transform obstacleTransform)
    {
        BehaviorObstacles obstacle = null;

        obstacleTransform.TryGetComponent(
            out obstacle);

        if (!obstacle)
        {
            Debug.LogWarning(
                "Unable To Handle Interaction with this Obstacle");

            return;
        }

        OnObstacleHit?.Invoke(obstacleTransform);

        if (_platformManager &&
            _platformManager.isDashing)
        {
            obstacle.Interacted(
                transform,
                this,
                BehaviorObstacles.signalType.Dash);
        }
        else
        {
            obstacle.Interacted(
                transform,
                this,
                BehaviorObstacles.signalType.Bump);
        }
    }


    private void HandleMonster(Collider trig)
    {
        // anything for colliding with monsters here (like jumping on the mushrooms)
        if (_gameState != null)
        {
            if (_gameState.ChangeHitPoints(-1))
                onDeath.Invoke();
            else
                onHit.Invoke();
        }
    }

    private void HandlePockiBox(Collider trig)
    {
        if (_platformManager)
        {
            if (!_platformManager.playerUnlockedPockiBox)
            {
                _platformManager.playerUnlockedPockiBox = true;

                if (Manager_TutorialUI.Instance)
                    Manager_TutorialUI.Instance.DisplayPockiCollection(true);
            }
        }

        Collectible collectible;

        if (trig.TryGetComponent(out collectible))
            collectible.Interacted();

        onPockiBoxCollect.Invoke();
    }

    private void HandlePockiStick(Collider trig)
    {
        if (_platformManager)
        {
            _platformManager.pockiBoxSticks++;

            if (Manager_TutorialUI.Instance)
                Manager_TutorialUI.Instance.DisplayPockiCollection(true);
        }

        Collectible collectible;

        if (trig.TryGetComponent(out collectible))
            collectible.Interacted();

        onPockiStickCollect.Invoke();
    }

    #endregion


    #region UNITY_EVENTS

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

    public UnityEvent onHit;
    public UnityEvent onDeath;
    public UnityEvent onModelChange;
    public UnityEvent onPockiBoxCollect;
    public UnityEvent onPockiStickCollect;

    #endregion


    #region UNITY_LIFECYCLE

    private void Awake()
    {
        _cachedTransform = transform;

        _platformManager =
            Manager_Platforms.Instance;

        _gameState =
            Manager_GameState.Instance;

        if (!ref_BehaviorCameraFollower &&
            Camera.main)
        {
            Camera.main.TryGetComponent(
                out ref_BehaviorCameraFollower);
        }
    }

    private void Start()
    {
        jumpLeftToAchieve =
            maximumJumpPower;

        if (_platformManager)
            _platformManager.PopulatePlayerCoreRef(this);
    }

    private void Update()
    {
        HandleModelUpdates();

        if (_gameState != null)
        {
            if (_gameState.currentState !=
                Manager_GameState.GAMESTATE.Playing)
            {
                return;
            }
        }

        ReadInputs();

        HandleJumpInput();

        HandlePositionConstraints();

        HandleBlockerChecks();
    }

    private void FixedUpdate()
    {
        ApplyJumpPhysics();

        if (_gameState != null &&
            _gameState.currentState ==
            Manager_GameState.GAMESTATE.Playing)
        {
            if (_cachedTransform.position.y < -20)
            {
                _gameState.GameOver();
            }
        }
    }

    #endregion


    #region MODEL_TRANSFORMATION

    public void ChangeModel_TransformMaho()
    {
        StartCoroutine(
            AnimateModelChange());
    }

    private IEnumerator AnimateModelChange()
    {
        yield return new WaitForSeconds(0.15f);

        SpawnPlayerOnPlatforms();

        if (ref_PlayerAnimations)
            ref_PlayerAnimations
                .SetAnyTrigger("Transform");

        yield return new WaitForSeconds(0.5f);

        onModelChange.Invoke();

        yield return new WaitForSeconds(0.5f);

        activeModel = 0;
    }

    #endregion

}
