using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// REFERENCE
public enum JELLYSTATES
{ //    Spawning, Looking, Prepare Attk, Charging, Waiting,    Beaten 
    None, Waiting, Hunting, TargetLocked, Attacking, Recovering, Captured
}

public class MonsterOJelly : MonsterOrigin
{

    [Header("General Variables\n______________")]
    public float timeUntilDespawn = 30f;
    public JELLYSTATES currentState = JELLYSTATES.Waiting; // BASE // { get; protected set; }     // uncomment/switch when testing monsters is done
    public LayerMask groundLayers;
    public SphereCollider colliderSphere;

    private float currentStateTimeStamp; // BASE
    private Vector3 waitVarVector3One, waitVarVector3Two;
    private float stepArc;


    [Header("Waiting-Intro Variables\n______________")]
    public float stateWaitTime = 10;
    public float betweenWaitActionsTime = 3;

    private bool playedIntroduction; // a reference to stop introductions if we played one
    private float betweenWaitActionsStamp;


    [Header("Hunting Variables\n______________")]
    public float stateHuntingTime = 10;
    public float getIntoPositionSpeed = 15;
    public Vector2 accetpablePlayerOffsetX = new Vector2(-4, 9); // offsets from where the player is
    public Vector2 accetpablePlayerOffsetY = new Vector2(-2, 4); // these ranges are still within camera frames, but may need tweaking
    public float forceZOffset = 0; // if we want the monster to be more forward or behind
    public UnityEvent onHuntEndOne;

    private Vector3 huntingTargPos;
    private Vector3 targetGraphicStartPos;


    [Header("TargetLocked Variables\n______________")]
    public float stateTargLockTime = 10;
    public float shakeSpeed = 1;
    [Range(0, 1)]
    public float shakeAmount = 0.05f;
    public UnityEvent onTargetEndOne;
    public Transform targetGraphic;

    private Vector3 startPos;

    // ATTACKING
    [Header("Attacking Variables\n______________")]
    public UnityEvent onAttackEndOne;



    // RECOVERING
    [Header("Recovering Variables\n______________")]
    public float stateRecoverTime = 10;
    public UnityEvent onRecoverEndOne;


    // CAPTURING
    //[Header("Capturing Variables\n______________")]
    private float percentHP;
    private int currentCapturePointsLeft;
    private Vector3 startScale;
    


    protected void OnEnable()
    {   // initialize
        ChangeState(JELLYSTATES.Waiting);
        currentStateTimeStamp = Time.time;
        healthCurrent = healthMax;
        spawnPosition = transform.position;
        startRotation = transform.rotation;

        if (!colliderSphere) TryGetComponent(out colliderSphere);
        if (colliderSphere) colliderSphere.enabled = false;
        if (startScale == Vector3.zero) startScale = transform.localScale;
        transform.localScale = startScale;
        //UpdateUserInterface();
    }

    protected override void MonsterSpawn()
    {
        base.MonsterSpawn();
    }

    protected override void MonsterIntroduction()
    {
        base.MonsterIntroduction();
        StateWaiting();
    }

    protected override void MonsterActive()
    {
        base.MonsterActive();
        // switch between the different states
    }

    protected override void MonsterCaptured()
    {
        base.MonsterCaptured();
    }

    protected override void MonsterHit()
    {
        base.MonsterHit();
    }

    public override void Tick()
    {
        base.Tick();

        RunMonsterBehavior();
    }


    #region HELPERS
    private void RunMonsterBehavior()
    {   
        if (currentState != JELLYSTATES.Waiting && Manager_GameState.Instance && Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing)
        { currentState = JELLYSTATES.Waiting; return; }

        StateChecker();
    }


    protected virtual void ResetVariables()
    {
        //startPos = Vector3.zero;
    }

    public virtual void ChangeState(JELLYSTATES _newState)
    {
        if (BehaviorCameraFollower.Instance) print("WE SHOULD HAVE CAM REF");
        else print("WE CANNOT FIND CAM REF");

        currentStateTimeStamp = Time.time;
        ResetVariables();

        if (currentState != JELLYSTATES.Captured && _newState == JELLYSTATES.Captured)
            print("CAPTURED THIS MONSTER");//onCapture?.Invoke(); // CAPTURED

        if (_newState != currentState) // ON CHANGE ONLY
        {
            switch (_newState) // setting variables and calling functions on new switch events
            {
                case JELLYSTATES.Waiting:
                    //if (BehaviorCameraFollower.Instance) BehaviorCameraFollower.Instance.DollyTargetMonster(transform);  
                    print($"CALL CAMERA - WAITING - ref is available = {_BehaviorCameraFollower != null}"); // NOT CALLING CORRECTLY !!!!!!!!!!!!!!!!!!!!!!!!!!!
                    if (_BehaviorCameraFollower) { _BehaviorCameraFollower.LimitCamera(false); _BehaviorCameraFollower.MoveCamTargetSmooth(5f, transform.position, 1, 5); }
                    break;
                case JELLYSTATES.Hunting:
                    print($"CALL MONSTER - HUNTING - ref is available = {_BehaviorCameraFollower != null}");
                    //if (BehaviorCameraFollower.Instance && BehaviorCameraFollower.Instance.currentState == BehaviorCameraFollower.CameraFocusState.Dolly) { print("HUNTING TIME"); BehaviorCameraFollower.Instance.DollyTargetMonster(null); }
                    if (_BehaviorCameraFollower) { _BehaviorCameraFollower.LimitCamera(true); _BehaviorCameraFollower.MoveCamTargetSmooth(5f, _PlayerCore.transform.position, 0.33f, 5); }
                    if (targetGraphic) { targetGraphic.SetParent(null); targetGraphic.position = transform.position; targetGraphic.gameObject.SetActive(true); }
                    break;
                case JELLYSTATES.TargetLocked:
                    // nothing on change
                    break;
                case JELLYSTATES.Attacking:
                    // nothing on change
                    break;
                case JELLYSTATES.Recovering:
                    // nothing on change
                    break;
                case JELLYSTATES.Captured:
                    // nothing on change
                    break;
                default:
                    Debug.Log($"WARNING: Case for Monster state '{currentState}' - not found");
                    ChangeState(JELLYSTATES.Waiting);
                    break;
            }
        }

        //print($"JELLYSTATES Change To: {_newState}\nFrom: {currentState}");
        currentState = _newState;
    }

    private void StateChecker()
    {
        switch (currentState)
        {
            case JELLYSTATES.Waiting:
                StateWaiting();
                break;
            case JELLYSTATES.Hunting:
                StateHunting();
                break;
            case JELLYSTATES.TargetLocked:
                StateTargetLocked();
                break;
            case JELLYSTATES.Attacking:
                StateAttacking();
                break;
            case JELLYSTATES.Recovering:
                StateRecovering();
                break;
            case JELLYSTATES.Captured:
                StateCaptured();
                break;
            default:
                Debug.Log($"WARNING: Case for Monster state '{currentState}' - not found");
                break;
        }
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

    protected void ForcePosInFront()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -3);
    }
    #endregion helpers


    #region STATE: WAITING
    private void StateWaiting()
    {
        //print("MONSTER WAITING DEBUG");
        transform.position += MoveWithBackground(); // keep moving with the background
        timeUntilDespawn += Time.deltaTime; // doesnt count waiting time when considering despawn time


        if (Time.time > betweenWaitActionsStamp + betweenWaitActionsTime)
        {
            waitVarVector3Two = ReturnRaycastPosition(spawnPosition.x + Random.Range(-1, 8), groundLayers); // locate and set target area on the ground
            waitVarVector3Two.y += 0.05f;

            if (waitVarVector3Two != Vector3.zero)
            {
                betweenWaitActionsStamp = Time.time; // reset Timer                                                             
                waitVarVector3One = transform.position; // store current points for path
                stepArc = 0; // ready the arc path to move again
            }

            if (Time.time > currentStateTimeStamp + stateWaitTime || playedIntroduction) { playedIntroduction = true; ChangeState(JELLYSTATES.Hunting); }// change state
        }
        if (waitVarVector3Two != Vector3.zero)  // step across said path
        {
            if (stepArc < 1f) // calculate path and move along it
            {
                //print("Move Along Path");
                stepArc += Time.deltaTime * getIntoPositionSpeed * 0.1f;
                Vector3 controlPoint1 = (waitVarVector3One + new Vector3(waitVarVector3One.x * 1.5f, waitVarVector3One.y + 3f, forceZOffset)); // TODO: change X (on both) to be a between percent (x2-x1 / to keep consistent)
                Vector3 controlPoint2 = (waitVarVector3Two + new Vector3(waitVarVector3Two.x * 0.5f, waitVarVector3Two.y + 1.5f, forceZOffset));
                transform.position = CalculateBezierPoint(stepArc, waitVarVector3One, controlPoint1, controlPoint2, waitVarVector3Two);
                //if (stepArc >= 1) onWaitEndOne?.Invoke(); // reached end-point
            }
        }

        //if(playedIntroduction) ChangeState(JELLYSTATES.Hunting); // if the monster already spawned, then we dont need to wait anymore

    }
    #endregion state: waiting

    #region STATE: HUNTING
    private void StateHunting()
    {
        //print("MONSTER HUNTING DEBUG");

        if (Time.time > currentStateTimeStamp + stateHuntingTime) //if (Time.time > betweenWaitActionsStamp + betweenWaitActionsTime)
        {
            if (!_PlayerCore) { Debug.LogError("MISSING PLAYER REFERENCE NO TARGET TO HUNT"); ChangeState(JELLYSTATES.TargetLocked); return; }
            waitVarVector3Two = ReturnRaycastPosition(_PlayerCore.transform.position.x + Random.Range(1, 4), groundLayers); // locate and set target area on the ground
            waitVarVector3Two.y += 0.05f;

            if (stepArc != 0)
            {
                betweenWaitActionsStamp = Time.time; // reset Timer                                                            
                stepArc = 0; // ready the arc path to move again
            }
            onHuntEndOne?.Invoke();
            ChangeState(JELLYSTATES.TargetLocked); // change state
        }
        else
        {
            if (targetGraphic)
            {
                float totalTime = currentStateTimeStamp + stateHuntingTime;
                targetGraphic.position = Vector3.Lerp(transform.position, _PlayerCore.transform.position, Time.time / totalTime);
            }
        }
    }
    #endregion state: hunting

    #region STATE: TARGET LOCKED
    private void StateTargetLocked()
    {
        //print("MONSTER TARGET LOCKED DEBUG");
        transform.position += MoveWithBackground();
        if (targetGraphic) targetGraphic.transform.position += MoveWithBackground();
        huntingTargPos += MoveWithBackground();
        waitVarVector3Two += MoveWithBackground();

        if (targetGraphic) { targetGraphic.SetParent(null); targetGraphic.position = waitVarVector3Two; targetGraphic.gameObject.SetActive(true); }
        waitVarVector3One = transform.position; // store current points for path                
        if (Time.time > currentStateTimeStamp + stateTargLockTime) { onTargetEndOne?.Invoke(); ChangeState(JELLYSTATES.Attacking); } // change state forward         
    }
    #endregion state: target locked

    #region STATE: ATTACKING
    private void StateAttacking()
    {
        if (targetGraphic) targetGraphic.transform.position += MoveWithBackground();
        huntingTargPos += MoveWithBackground();
        waitVarVector3Two += MoveWithBackground();

        //print("MONSTER ATTACKING DEBUG");

        if (waitVarVector3Two != Vector3.zero)  // step across said path
        {
            if (stepArc < 1f) // calculate path and move along it
            {
                //print("Move Along Path");
                stepArc += Time.deltaTime * getIntoPositionSpeed * 0.1f;
                Vector3 controlPoint1 = (waitVarVector3One + new Vector3(waitVarVector3One.x * 1.5f, waitVarVector3One.y + 3f, forceZOffset)); // TODO: change X (on both) to be a between percent (x2-x1 / to keep consistent)
                Vector3 controlPoint2 = (waitVarVector3Two + new Vector3(waitVarVector3Two.x * 0.5f, waitVarVector3Two.y + 1.5f, forceZOffset));
                transform.position = CalculateBezierPoint(stepArc, waitVarVector3One, controlPoint1, controlPoint2, waitVarVector3Two);
                if (stepArc >= 1) { onAttackEndOne?.Invoke(); ChangeState(JELLYSTATES.Recovering); }
            }
        }
        //if (Time.time > currentStateTimeStamp + stateTargLockTime) ChangeState(JELLYSTATES.Recovering); // change state
    }
    #endregion state: attacking

    #region STATE: RECOVERING
    private void StateRecovering()
    {
        //print("MONSTER RECOVERING DEBUG");
        transform.position += MoveWithBackground();
        if (targetGraphic) { targetGraphic.SetParent(transform); targetGraphic.gameObject.SetActive(false); }

        if (Time.time > currentStateTimeStamp + stateRecoverTime)
        { onRecoverEndOne?.Invoke(); waitVarVector3Two = Vector3.zero; stepArc = 0; ChangeState(JELLYSTATES.Hunting); }
    }
    #endregion state: recovering

    #region STATE: CAPTURED
    private void StateCaptured()
    {
        //print("MONSTER CAPTURED DEBUG");
        if (Manager_Platforms.Instance)
            print("OLD - SIGNAL MONSTER CAPTURE");// Manager_Platforms.Instance.monsterSignaledCapture = true;

        if (targetGraphic) { targetGraphic.SetParent(transform); targetGraphic.gameObject.SetActive(false); }

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
            if (!_PlayerCore) { Debug.LogError("MISSING PLAYER REFERENCE NO TARGET TO FLY TOWARDS"); gameObject.SetActive(false); return; }
            float dist = Vector3.Distance(transform.position, _PlayerCore.transform.position);
            if (dist > distanceToCollect)
            {
                var step = capturedMoveSpeed * Time.deltaTime; // calculate distance to move
                transform.position = Vector3.MoveTowards(transform.position, _PlayerCore.transform.position, step);
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
    #endregion state: captured

}
