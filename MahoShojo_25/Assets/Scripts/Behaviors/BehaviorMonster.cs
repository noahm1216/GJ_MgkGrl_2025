using UnityEngine;
using UnityEngine.Events;

public abstract class BehaviorMonster : MonoBehaviour
{
    // REFERENCES
    public enum MONSTERSTATE
    { //    Spawning, Looking, Prepare Attk, Charging, Waiting,    Beaten 
        None, Waiting, Hunting, TargetLocked, Attacking, Recovering, Captured
    }

    private BehaviorCameraFollower _BehaviorCameraFollower;
    private PlayerCore _PlayerCore;


    // GENERAL   
    [Header("General Variables\n______________")]
    public MONSTERSTATE currentState = MONSTERSTATE.Waiting; // BASE // { get; protected set; }     // enable when testing monsters is done
    public float timeUntilDespawn = 30f;
    public LayerMask groundLayers;
    public Transform playerObj;
    public SphereCollider colliderSphere;
    public bool runIndependantly = false;

    protected string tag_ToHunt = "Player";
    protected float currentStateTimeStamp; // BASE
    protected Quaternion startRotation;
    protected Vector3 spawnPosition;

    // WAITING
    [Header("Waiting Variables\n______________")]
    public float stateWaitTime = 10;
    public float betweenWaitActionsTime = 3;
    public UnityEvent onWaitEndOne;

    protected bool alreadySpawned; // so the monster doesnt wait everytime it spawns. If it despawns, it will skip waiting again when enabled.
    protected float betweenWaitActionsStamp;    


    // HUNTING
    [Header("Hunting Variables\n______________")]
    public float stateHuntingTime = 10;
    public float getIntoPositionSpeed = 15;
    public Vector2 accetpablePlayerOffsetX = new Vector2(-4, 9); // offsets from where the player is
    public Vector2 accetpablePlayerOffsetY = new Vector2(-2, 4); // these ranges are still within camera frames, but may need tweaking
    public float forceZOffset = 0; // if we want the monster to be more forward or behind
    public UnityEvent onHuntEndOne;

    protected Vector3 huntingTargPos;

    // TARGET LOCKED
    [Header("TargetLocked Variables\n______________")]
    public float stateTargLockTime = 10;
    public float shakeSpeed = 1;
    [Range(0, 1)]
    public float shakeAmount = 0.05f;
    public UnityEvent onTargetEndOne;
    public Transform targetGraphic;

    protected Vector3 startPos;


    // ATTACKING
    [Header("Attacking Variables\n______________")]
    public UnityEvent onAttackEndOne;



    // RECOVERING
    [Header("Recovering Variables\n______________")]
    public float stateRecoverTime = 10;
    public UnityEvent onRecoverEndOne;


    // RECOVERING
    [Header("Capturing Variables\n______________")]
    public int pointsUntilCaptured = 10;
    public int pointsForCapturing = 100;
    public Transform uiHolderOfHeartPoints;
    public float sizeToShrinkTo = 0.15f;
    public float sizePercentChangeEveryFrame = 0.999f;
    public float capturedMoveSpeed = 1;
    public bool spinWhileCaptured;
    public Vector3 spinSpeedDir = new Vector3(0, 90, 0);
    [Space]
    [Space]
    public bool flyTowardsPlayer;
    public float distanceToCollect = 0.5f;
    [Space]
    [Space]
    public float stateCaptureFlyTime = 10;
    [Space]
    public UnityEvent onHit, onHitAudio, onCapture;

    protected float percentHP;
    protected int currentCapturePointsLeft;
    protected Vector3 startScale;
    protected float audioPlayedTimeWait = 0.3f;
    protected float[] audioPlayedTimeStamps = new float[6];

    protected void OnEnable()
    {   // initialize
        if (BehaviorCameraFollower.Instance) _BehaviorCameraFollower = BehaviorCameraFollower.Instance;

        ChangeState(MONSTERSTATE.Waiting);
        currentStateTimeStamp = Time.time;
        currentCapturePointsLeft = pointsUntilCaptured;
        spawnPosition = transform.position;
        startRotation = transform.rotation;

        if (!colliderSphere) TryGetComponent(out colliderSphere);
        if (colliderSphere) colliderSphere.enabled = false;
        if (startScale == Vector3.zero) startScale = transform.localScale;
        transform.localScale = startScale;
        UpdateUserInterface();

        if (!playerObj) playerObj = GameObject.FindGameObjectWithTag(tag_ToHunt).transform;
        if (playerObj && !_PlayerCore) playerObj.TryGetComponent<PlayerCore>(out _PlayerCore);

    }

    protected void OnDisable()
    {
        DisableMonster();
    }

#if UNITY_EDITOR
    protected void Update() // we'll run this from manager_platforms in the build
    {
        if (runIndependantly) RunMonsterBehavior(true);
    }
#endif //unity_editor

    public void SendStartPosition(Vector3 _pos) // needed to ensure any code referencing start position of creature considers where the player "experiences" the creature spawned - (which usually should not be vector3.zero because of how the endless runner is coded)
    {
        spawnPosition = _pos;
    }

    public virtual void RunMonsterBehavior(bool _runSelf)
    {
        if (!playerObj)
        { Debug.Log($"ERROR: Cant find player obj for this monster ({transform.name}) to hunt"); playerObj = GameObject.FindGameObjectWithTag(tag_ToHunt).transform; return; }

        if (!_runSelf) runIndependantly = false;

        if (currentState != MONSTERSTATE.Waiting && Manager_GameState.Instance && Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing)
        { currentState = MONSTERSTATE.Waiting; return; }

        StateChecker();
    }


    #region HELPER FUNCTIONS
    public void ChangeCapturePoints(int _changeAmount)
    {
        print($"My HP Is Changing by 1");
        currentCapturePointsLeft += _changeAmount;
        if (currentCapturePointsLeft > pointsUntilCaptured)
            currentCapturePointsLeft = pointsUntilCaptured;
        if (currentCapturePointsLeft <= 0)
            ChangeState(MONSTERSTATE.Captured);

        UpdateUserInterface();
        CheckOnHitAudio();
        onHit?.Invoke();
    }

    public virtual void DisableMonster()
    {
       // whatever we want when the monster is disabled
    }

    protected void CheckOnHitAudio() // this is done to avoid audio glitches when we fire a LOT of attacks at our enemy
    {
        for (int i = 0; i < audioPlayedTimeStamps.Length; i++)
        {
            if (Time.time > audioPlayedTimeStamps[i] + audioPlayedTimeWait)
            {
                audioPlayedTimeStamps[i] = Time.time;
                onHitAudio?.Invoke();
                break;
            }
        }
    }

    protected void UpdateUserInterface()
    {
        if (!uiHolderOfHeartPoints || uiHolderOfHeartPoints.childCount == 0)
            return;

        percentHP = ((float)currentCapturePointsLeft / (float)pointsUntilCaptured); // get the decimal      
        percentHP = (Mathf.RoundToInt(percentHP * 10)); // we have 10 hp bars to show or hide     

        for (int i = 0; i < uiHolderOfHeartPoints.childCount; i++)
        {
            if (uiHolderOfHeartPoints.GetChild(i))
                if (i < percentHP)
                    uiHolderOfHeartPoints.GetChild(i).gameObject.SetActive(true);
                else
                    uiHolderOfHeartPoints.GetChild(i).gameObject.SetActive(false);
        }
    }

    public virtual void ChangeState(MONSTERSTATE _newState)
    {
        if (BehaviorCameraFollower.Instance) print("WE SHOULD HAVE CAM REF");
        else print("WE CANNOT FIND CAM REF");

        currentStateTimeStamp = Time.time;
        ResetVariables();

        if (currentState != MONSTERSTATE.Captured && _newState == MONSTERSTATE.Captured)
            onCapture?.Invoke();

        if (_newState != currentState) // ON CHANGE ONLY
        {
            switch (_newState) // setting variables and calling functions on new switch events
            {
                case MONSTERSTATE.Waiting:
                    //if (BehaviorCameraFollower.Instance) BehaviorCameraFollower.Instance.DollyTargetMonster(transform);  
                    print($"CALL CAMERA - WAITING - ref is available = {_BehaviorCameraFollower != null}"); // NOT CALLING CORRECTLY !!!!!!!!!!!!!!!!!!!!!!!!!!!
                    if (_BehaviorCameraFollower) { _BehaviorCameraFollower.LimitCamera(false); _BehaviorCameraFollower.MoveCamTargetSmooth(5f, transform.position, 1, 5); }
                    break;
                case MONSTERSTATE.Hunting:
                    print($"CALL MONSTER - HUNTING - ref is available = {_BehaviorCameraFollower != null}");
                    //if (BehaviorCameraFollower.Instance && BehaviorCameraFollower.Instance.currentState == BehaviorCameraFollower.CameraFocusState.Dolly) { print("HUNTING TIME"); BehaviorCameraFollower.Instance.DollyTargetMonster(null); }
                    if (_BehaviorCameraFollower) { _BehaviorCameraFollower.LimitCamera(true); _BehaviorCameraFollower.MoveCamTargetSmooth(5f, playerObj.position, 0.33f, 5); }
                    if (targetGraphic) { targetGraphic.SetParent(null); targetGraphic.position = transform.position; targetGraphic.gameObject.SetActive(true); }
                    break;
                case MONSTERSTATE.TargetLocked:
                    // nothing on change
                    break;
                case MONSTERSTATE.Attacking:
                    // nothing on change
                    break;
                case MONSTERSTATE.Recovering:
                    // nothing on change
                    break;
                case MONSTERSTATE.Captured:
                    // nothing on change
                    break;
                default:
                    Debug.Log($"WARNING: Case for Monster state '{currentState}' - not found");
                    ChangeState(MONSTERSTATE.Waiting);
                    break;
            }
        }

        //print($"MONSTERSTATE Change To: {_newState}\nFrom: {currentState}");
        currentState = _newState;
    }

    protected virtual void ResetVariables()
    {
        startPos = Vector3.zero;
    }

    protected Vector3 MoveWithBackground()
    {
        if (Manager_Platforms.Instance)
            return new Vector3(Manager_Platforms.Instance.CurrentSpeed(), 0, 0);
        else
            return Vector3.zero;
    }

    protected void StateChecker()
    {
        switch (currentState)
        {
            case MONSTERSTATE.Waiting:
                StateWaiting();
                break;
            case MONSTERSTATE.Hunting:
                StateHunting();
                break;
            case MONSTERSTATE.TargetLocked:
                StateTargetLocked();
                break;
            case MONSTERSTATE.Attacking:
                StateAttacking();
                break;
            case MONSTERSTATE.Recovering:
                StateRecovering();
                break;
            case MONSTERSTATE.Captured:
                StateCaptured();
                break;
            default:
                Debug.Log($"WARNING: Case for Monster state '{currentState}' - not found");
                break;
        }
    }

    protected void ForcePosInFront()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -3);
    }

    protected Vector3 ReturnRaycastPosition(float _xOffset, LayerMask _rayCastable)
    {
        // move it
        Vector3 position = Vector3.zero + new Vector3(_xOffset, 20, 0); // add offset
        // raycast from the position down onto a platform and place it there
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, _rayCastable))
        {
            position = hit.point + new Vector3(0, 0, 0);
            //print($"New Hunting Point: {position}");
        }
        else
        {
            Debug.Log("WARNING: MonsterBehvaior Raycast wasnt able to find surface to land on");
            position = Vector3.zero;
        }

        return position;
    }

    protected static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)  // NOTE: may be able to RELOCATE to Jelly if only user
    { // i don't know how this works, found it on the internet and it works... 
        float u = 1 - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // (1-t)^3 * P0
        p += 3 * uu * t * p1; // 3 * (1-t)^2 * t * P1
        p += 3 * u * tt * p2; // 3 * (1-t) * t^2 * P2
        p += ttt * p3; // t^3 * P3

        return p;
    }

    protected static Vector3 MoveTowardsHelper(Vector3 _yourPos, Vector3 _targetPos, float _step) // NOTE: may be able to remove
    {
        return Vector3.MoveTowards(_yourPos, _targetPos, _step);
    }
    #endregion helper functions


    /// <summary>
    ///  MARKED FOR DELETION once no longer needed
    /// </summary>

    #region STATE: WAITING
    protected virtual void StateWaiting()
    {
        //print("MONSTER WAITING DEBUG");
        transform.position += MoveWithBackground(); // keep moving with the background
        timeUntilDespawn += Time.deltaTime; // doesnt count waiting time when considering despawn time
        if (Time.time > currentStateTimeStamp + stateWaitTime || alreadySpawned)
        {
            alreadySpawned = true;
            onWaitEndOne?.Invoke();
            ChangeState(MONSTERSTATE.Hunting);// change state
        }
    }
    #endregion state: waiting 

    #region STATE: HUNTING
    protected virtual void StateHunting()
    {
        if (Time.time > currentStateTimeStamp + stateHuntingTime) //if (Time.time > betweenWaitActionsStamp + betweenWaitActionsTime)\
        {
            onHuntEndOne?.Invoke();
            ChangeState(MONSTERSTATE.TargetLocked); // change state
        }      
    }
    #endregion state: hunting

    #region STATE: TARGET LOCKED
    protected virtual void StateTargetLocked()
    {
        //print("MONSTER TARGET LOCKED DEBUG");
        transform.position += MoveWithBackground();       
        if (Time.time > currentStateTimeStamp + stateTargLockTime) { onTargetEndOne?.Invoke(); ChangeState(MONSTERSTATE.Attacking); } // change state forward           
    }
    #endregion state: target locked

    #region STATE: ATTACKING
    protected virtual void StateAttacking()
    {
        transform.position += MoveWithBackground();
        //print("MONSTER ATTACKING DEBUG");
        if (Time.time > currentStateTimeStamp + stateTargLockTime) ChangeState(MONSTERSTATE.Recovering); // change state      
    }
    #endregion state: attacking

    #region STATE: RECOVERING
    protected virtual void StateRecovering()
    {
        //print("MONSTER RECOVERING DEBUG");
        transform.position += MoveWithBackground();

        if (Time.time > currentStateTimeStamp + stateRecoverTime)
        {
            onRecoverEndOne?.Invoke();
            ChangeState(MONSTERSTATE.Hunting);
        }
    }
    #endregion state: recovering

    #region STATE: CAPTURED
    protected virtual void StateCaptured()
    {
        //print("MONSTER CAPTURED DEBUG");
        if (Manager_Platforms.Instance) Manager_Platforms.Instance.monsterSignaledCapture = true;


        if (transform.localScale.x > sizeToShrinkTo)
        {
            transform.localScale *= sizePercentChangeEveryFrame;
            capturedMoveSpeed *= 1.1f;
            if (transform.localScale.x < 0.01f)
                transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        }

        if (spinWhileCaptured)
        {
            transform.Rotate(spinSpeedDir.x * Time.deltaTime, spinSpeedDir.y * Time.deltaTime, spinSpeedDir.z * Time.deltaTime);
        }

        if (flyTowardsPlayer)
        {
            float dist = Vector3.Distance(transform.position, playerObj.position);
            if (dist > distanceToCollect)
            {
                var step = capturedMoveSpeed * Time.deltaTime; // calculate distance to move
                transform.position = Vector3.MoveTowards(transform.position, playerObj.position, step);
                ForcePosInFront();
            }
            else // DONE
            {
                CaptureEvents();
                gameObject.SetActive(false);
            }
        }
        else
        {
            if (capturedMoveSpeed == 0 || capturedMoveSpeed > -0.01f && capturedMoveSpeed < 0.01f) // was getting NaN error
                transform.Translate(Vector3.up * Time.deltaTime * capturedMoveSpeed);

            if (transform.position.y > 99) // to help with point floating errors
                transform.position = new Vector3(0, 99, 0);

            if (Time.time > currentStateTimeStamp + stateCaptureFlyTime) // DONE
            {
                if (CaptureEvents())
                    gameObject.SetActive(false);
            }
        }
    }

    protected bool CaptureEvents() // turned this into a bool so we can ensure its done before turning the object off
    {
        transform.rotation = startRotation;

        if (Manager_GameState.Instance)
        { Manager_GameState.Instance.CaptureChange(1, pointsForCapturing); }

        if (Manager_Platforms.Instance)
        { print("BehaviorMonster: CaptureEvents() - ChangeMonsterVariable HERE"); }// Manager_Platforms.Instance.ChangeMonsterVariables(true, false, true); }

        return true;
    }
    #endregion state: captured

}

