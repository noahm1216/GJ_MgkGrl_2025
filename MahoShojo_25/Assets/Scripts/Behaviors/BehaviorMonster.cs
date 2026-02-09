using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BehaviorMonster : MonoBehaviour
{

    public enum MONSTERSTATE
    { //Spawning, Looking, Prepare Attk, Charging, Waiting,    Beaten 
        Waiting, Hunting, TargetLocked, Attacking, Recovering, Captured
    }

    public enum MONSTERPLUSHIE
    { //The Cases offered based on each creature's unique behavior
        Unnamed, Jello, Mushroom, Frog, Hamster, Cat, Shark, Wolf, Bunny, Bear
    }

    // GENERAL   
    [Header("General Variables\n______________")]
    public MONSTERPLUSHIE monsterPlushy;
    public MONSTERSTATE currentState = MONSTERSTATE.Waiting; // { get; private set; }     // enable when testing monsters is done
    public float timeUntilDespawn = 15f;
    public LayerMask groundLayers;
    public Transform playerObj;
    public SphereCollider collider;
    public bool runIndependantly = false;

    private string tag_ToHunt = "Player";
    private float currentStateTimeStamp;
    private Quaternion startRotation;
    private Vector3 spawnPosition;

    // WAITING
    [Header("Waiting Variables\n______________")]
    public bool activatesOnWait;
    public float stateWaitTime = 10;
    public float betweenWaitActionsTime = 3;
    public bool activatesOnDistance;
    public float distanceToActivate = 4;
    public UnityEvent onWaitEndOne;

    private float betweenWaitActionsStamp;
    private Vector3 waitVarVector3One, waitVarVector3Two;
    private float stepArc;


    // HUNTING
    [Header("Hunting Variables\n______________")]
    public float stateHuntingTime = 10;
    public float getIntoPositionSpeed = 15;
    public Vector2 accetpablePlayerOffsetX = new Vector2(-4, 9); // offsets from where the player is
    public Vector2 accetpablePlayerOffsetY = new Vector2(-2, 4); // these ranges are still within camera frames, but may need tweaking
    public float forceZOffset = 0; // if we want the monster to be more forward or behind
    public UnityEvent onHuntEndOne;

    private Vector3 huntingTargPos;
    private float huntOffX, huntOffY;
    private int dir = 1;
    private bool gettingIntoPosition;


    // TARGET LOCKED
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
    public float attackSpeed = 5;
    public float targetTolerance = 0.25f;
    public Vector3 positionalTargetOffset = new Vector3(0, 0, 0);
    public UnityEvent onAttackEndOne;

    private Vector3 storedAttackPos;
    private bool didStoreAttack;


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
    public UnityEvent onHit, onHitAudio, onCapture, onRushAttacking;

    private float percentHP;
    private int currentCapturePointsLeft;
    private Vector3 startScale;
    private float audioPlayedTimeWait = 0.3f;
    private float[] audioPlayedTimeStamps = new float[6];

    private void OnEnable()
    {   // initialize
        ChangeState(MONSTERSTATE.Waiting);
        currentStateTimeStamp = Time.time;
        currentCapturePointsLeft = pointsUntilCaptured;
        spawnPosition = transform.position;
        startRotation = transform.rotation;
        collider.enabled = false;
        if (startScale == Vector3.zero)
            startScale = transform.localScale;
        transform.localScale = startScale;
        UpdateUserInterface();
        if (!playerObj)
            playerObj = GameObject.FindGameObjectWithTag(tag_ToHunt).transform;

    }

#if UNITY_EDITOR
    private void LateUpdate() // we'll run this from manager_platforms in the build
    {
        if (runIndependantly) RunMonsterBehavior();
    }
#endif //unity_editor

    public void RunMonsterBehavior()
    {
        if (!playerObj)
        { Debug.Log($"ERROR: Cant find player obj for this monster ({transform.name}) to hunt"); playerObj = GameObject.FindGameObjectWithTag(tag_ToHunt).transform; return; }

        if (runIndependantly) runIndependantly = false;

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

    private void CheckOnHitAudio() // this is done to avoid audio glitches when we fire a LOT of attacks at our enemy
    {
        for (int i = 0; i < audioPlayedTimeStamps.Length; i++)
        {
            if (audioPlayedTimeStamps[i] > Time.time + audioPlayedTimeWait)
            {
                audioPlayedTimeStamps[i] = Time.time;
                onHitAudio?.Invoke();
                break;
            }
        }
    }

    private void UpdateUserInterface()
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

    public void ChangeState(MONSTERSTATE _newState)
    {
        currentStateTimeStamp = Time.time;
        ResetVariables();

        if (currentState != MONSTERSTATE.Captured && _newState == MONSTERSTATE.Captured)
            onCapture?.Invoke();
        print($"MONSTERSTATE Change To: {_newState}\nFrom: {currentState}");
        currentState = _newState;
    }

    private void ResetVariables()
    {
        startPos = Vector3.zero;
        didStoreAttack = false;
        gettingIntoPosition = true;
    }

    private Vector3 MoveWithBackground()
    {
        if (Manager_Platforms.Instance)
            return new Vector3(Manager_Platforms.Instance.CurrentSpeed(), 0, 0);
        else
            return Vector3.zero;
    }

    private void StateChecker()
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

    private void ForcePosInFront()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -3);
    }

    private Vector3 ReturnRaycastPosition(float _xOffset, LayerMask _rayCastable)
    {
        // move it
        Vector3 position = Vector3.zero + new Vector3(_xOffset, 20, 0); // add offset
        // raycast from the position down onto a platform and place it there
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, _rayCastable))
        {
            position = hit.point + new Vector3(0, 0, 0);
            print($"New Hunting Point: {position}");
        }
        else
        { Debug.Log("WARNING: MonsterBehvaior Raycast wasnt able to find surface to land on"); position = Vector3.zero; }

        return position;
    }

    public static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
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

    public static Vector3 MoveTowardsHelper(Vector3 _yourPos, Vector3 _targetPos, float _step)
    {
        return Vector3.MoveTowards(_yourPos, _targetPos, _step);
    }
    #endregion helper functions



    #region STATE: WAITING
    private void StateWaiting()
    {
        //print("MONSTER WAITING DEBUG");
        transform.position += MoveWithBackground(); // keep moving with the background

        switch (monsterPlushy)
        {
            case MONSTERPLUSHIE.Jello:
                if (Time.time > betweenWaitActionsStamp + betweenWaitActionsTime)
                {
                    waitVarVector3Two = ReturnRaycastPosition(spawnPosition.x + Random.Range(-5, 8), groundLayers); // locate and set target area on the ground
                    waitVarVector3Two.y += 0.05f;

                    if (waitVarVector3Two != Vector3.zero)
                    {
                        betweenWaitActionsStamp = Time.time; // reset Timer                                                             
                        waitVarVector3One = transform.position; // store current points for path
                        stepArc = 0; // ready the arc path to move again
                    }

                    if (Time.time > currentStateTimeStamp + stateWaitTime) { ChangeState(MONSTERSTATE.Hunting); }// change state
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
                        if (stepArc >= 1) onWaitEndOne?.Invoke();
                    }
                }
                break;
            default:
                print($"STATE-WAITING: Plushie '{monsterPlushy}' not handled yet...\nReturning to Demo Idle Behavior");
                //--------------------------------------------ORIGINAL STATE WAITING()
                transform.position += MoveWithBackground();
                ForcePosInFront();

                if (activatesOnDistance)
                {
                    float dist = Vector3.Distance(transform.position, playerObj.position);
                    if (dist <= distanceToActivate)
                    { onWaitEndOne?.Invoke(); ChangeState(MONSTERSTATE.Hunting); }
                }

                if (activatesOnWait && Time.time > currentStateTimeStamp + stateWaitTime)
                { onWaitEndOne?.Invoke(); ChangeState(MONSTERSTATE.Hunting); }
                break;
        }

    }
    #endregion state: waiting

    #region STATE: HUNTING
    private void StateHunting()
    {
        //print("MONSTER HUNTING DEBUG");
        switch (monsterPlushy)
        {
            case MONSTERPLUSHIE.Jello:
                if (Time.time > currentStateTimeStamp + stateHuntingTime) //if (Time.time > betweenWaitActionsStamp + betweenWaitActionsTime)
                {
                    waitVarVector3Two = ReturnRaycastPosition(playerObj.position.x + Random.Range(-2, 4), groundLayers); // locate and set target area on the ground
                    waitVarVector3Two.y += 0.05f;

                    if (stepArc != 0)
                    {
                        betweenWaitActionsStamp = Time.time; // reset Timer                                                            
                        stepArc = 0; // ready the arc path to move again
                    }
                    onHuntEndOne?.Invoke();
                    ChangeState(MONSTERSTATE.TargetLocked); // change state
                }
                else { if (targetGraphic) { targetGraphic.SetParent(null); float targetSpeed = 2.5f; targetGraphic.position = MoveTowardsHelper(targetGraphic.position, waitVarVector3Two, targetSpeed*Time.deltaTime); targetGraphic.gameObject.SetActive(true); } }
                break;
            default:
                print($"STATE-HUNTING: Plushie '{monsterPlushy}' not handled yet...\nReturning to Demo Idle Behavior");
                //--------------------------------------------ORIGINAL STATE WAITING()
                if (gettingIntoPosition) // move to the lerp position
                {
                    huntingTargPos = playerObj.transform.position + new Vector3(accetpablePlayerOffsetX.x, accetpablePlayerOffsetY.y, forceZOffset); // update target position   
                    float distToHuntingSpot = Vector3.Distance(transform.position, huntingTargPos);

                    if (distToHuntingSpot > targetTolerance * 2)
                    {
                        var step = getIntoPositionSpeed * Time.deltaTime; // calculate distance to move
                        transform.position = Vector3.MoveTowards(transform.position, huntingTargPos, step);
                        currentStateTimeStamp = Time.time; // gives us extra time to get into position
                    }
                    else
                    {
                        huntOffX = Random.Range(accetpablePlayerOffsetX.x, accetpablePlayerOffsetX.y);
                        huntOffY = Random.Range(accetpablePlayerOffsetY.x, accetpablePlayerOffsetY.y);
                        gettingIntoPosition = false;
                    }
                }
                else // lerping back and forth
                {
                    float distToHuntingSpot = Vector3.Distance(transform.position, huntingTargPos);
                    if (dir > 0)
                    {
                        distToHuntingSpot = Vector3.Distance(transform.position, huntingTargPos);
                        if (distToHuntingSpot <= 0.025f) { dir *= -1; huntingTargPos = playerObj.transform.position + new Vector3(huntOffX, huntOffY, forceZOffset); }
                    }
                    else
                    {
                        distToHuntingSpot = Vector3.Distance(transform.position, huntingTargPos);
                        if (distToHuntingSpot <= 0.025f) { dir *= -1; huntingTargPos = playerObj.transform.position + new Vector3(-huntOffX, huntOffY, forceZOffset); }
                    }

                    var step = getIntoPositionSpeed * Time.deltaTime; // calculate distance to move
                    transform.position = Vector3.MoveTowards(transform.position, huntingTargPos, step);

                    if (Time.time > currentStateTimeStamp + stateHuntingTime)
                    { onHuntEndOne?.Invoke(); ChangeState(MONSTERSTATE.TargetLocked); gettingIntoPosition = true; }
                }
                break;
        }

    }
    #endregion state: hunting

    #region STATE: TARGET LOCKED
    private void StateTargetLocked()
    {
        //print("MONSTER TARGET LOCKED DEBUG");
        transform.position += MoveWithBackground();      

        switch (monsterPlushy)
        {
            case MONSTERPLUSHIE.Jello:
                if (targetGraphic) { targetGraphic.SetParent(null); targetGraphic.position = waitVarVector3Two; targetGraphic.gameObject.SetActive(true); }
                waitVarVector3One = transform.position; // store current points for path                
                if (Time.time > currentStateTimeStamp + stateTargLockTime) { onTargetEndOne?.Invoke(); ChangeState(MONSTERSTATE.Attacking); } // change state forward         
                break;
            default:
                print($"STATE-TARGETLOCKED: Plushie '{monsterPlushy}' not handled yet...\nReturning to Demo Idle Behavior");
                //--------------------------------------------ORIGINAL STATE WAITING()
                if (startPos == Vector3.zero)
                    startPos = transform.position;

                //startPos += MoveWithBackground();

                float randomX = startPos.x + shakeSpeed * shakeAmount * Random.Range(0.95f, 1.05f);
                float randomY = startPos.y + shakeSpeed * shakeAmount * Random.Range(0.85f, 1.15f);
                transform.position = new Vector3(randomX, randomY, startPos.z + forceZOffset);
                ForcePosInFront();

                // place a target obj where the player is to show this creatures intentions

                if (Time.time > currentStateTimeStamp + stateTargLockTime)
                { onTargetEndOne?.Invoke(); ChangeState(MONSTERSTATE.Attacking); } // attackings
                break;
        }
    }
    #endregion state: target locked

    #region STATE: ATTACKING
    private void StateAttacking()
    {
        //print("MONSTER ATTACKING DEBUG");
        switch (monsterPlushy)
        {
            case MONSTERPLUSHIE.Jello:
                if (waitVarVector3Two != Vector3.zero)  // step across said path
                {
                    if (stepArc < 1f) // calculate path and move along it
                    {
                        //print("Move Along Path");
                        stepArc += Time.deltaTime * getIntoPositionSpeed * 0.1f;
                        Vector3 controlPoint1 = (waitVarVector3One + new Vector3(waitVarVector3One.x * 1.5f, waitVarVector3One.y + 3f, forceZOffset)); // TODO: change X (on both) to be a between percent (x2-x1 / to keep consistent)
                        Vector3 controlPoint2 = (waitVarVector3Two + new Vector3(waitVarVector3Two.x * 0.5f, waitVarVector3Two.y + 1.5f, forceZOffset));
                        transform.position = CalculateBezierPoint(stepArc, waitVarVector3One, controlPoint1, controlPoint2, waitVarVector3Two);
                        if (stepArc >= 1){ onAttackEndOne?.Invoke(); ChangeState(MONSTERSTATE.Recovering); }
                    }
                }
                //if (Time.time > currentStateTimeStamp + stateTargLockTime) ChangeState(MONSTERSTATE.Recovering); // change state
                break;
            default:
                if (!didStoreAttack)
                { storedAttackPos = (playerObj.transform.position + positionalTargetOffset); didStoreAttack = true; collider.enabled = true; onRushAttacking.Invoke(); }

                var step = attackSpeed * Time.deltaTime; // calculate distance to move
                transform.position = Vector3.MoveTowards(transform.position, storedAttackPos, step);

                float dist = Vector3.Distance(transform.position, storedAttackPos);
                transform.position += MoveWithBackground();
                storedAttackPos += MoveWithBackground();

                if (dist <= targetTolerance)
                { ChangeState(MONSTERSTATE.Recovering); collider.enabled = false; }
                break;
        }

    }
    #endregion state: attacking

    #region STATE: RECOVERING
    private void StateRecovering()
    {
        //print("MONSTER RECOVERING DEBUG");
        transform.position += MoveWithBackground();
        if (targetGraphic) { targetGraphic.SetParent(transform); targetGraphic.gameObject.SetActive(false); }

        if (Time.time > currentStateTimeStamp + stateRecoverTime)
        { onRecoverEndOne?.Invoke(); waitVarVector3Two = Vector3.zero; stepArc = 0; ChangeState(MONSTERSTATE.Hunting); }
    }
    #endregion state: recovering

    #region STATE: CAPTURED
    private void StateCaptured()
    {
        //print("MONSTER CAPTURED DEBUG");
        if (Manager_Platforms.Instance)
            Manager_Platforms.Instance.monsterSignaledCapture = true;

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

    private bool CaptureEvents() // turned this into a bool so we can ensure its done before turning the object off
    {
        transform.rotation = startRotation;

        if (Manager_GameState.Instance)
        { Manager_GameState.Instance.CaptureChange(1, pointsForCapturing); }

        if (Manager_Platforms.Instance)
        { Manager_Platforms.Instance.ChangeMonsterVariables(true, false, true); }

        return true;
    }
    #endregion state: captured

}

