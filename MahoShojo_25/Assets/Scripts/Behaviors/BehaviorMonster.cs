using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BehaviorMonster : MonoBehaviour
{

    public enum MONSTERSTATE
    { //Spawning, Looking, Prepare Attk, Charging, Waiting,    Beaten 
        Waiting, Hunting, TargetLocked, Attacking, Recovering, Captured
    }

    // GENERAL   
    [Header("General Variables\n______________")]
    public MONSTERSTATE currentState = MONSTERSTATE.Waiting; // { get; private set; } 
    public Transform playerObj;

    private string tag_ToHunt = "Player";
    private float currentStateTimeStamp;

    // WAITING
    [Header("Waiting Variables\n______________")]
    public bool activatesOnWait;
    public float waitTime = 10;
    public bool activatesOnDistance;
    public float distanceToActivate = 4;


    // HUNTING
    [Header("Hunting Variables\n______________")]
    public float huntingTime = 10;
    public float blendSpeed = 1;
    public Vector2 accetpablePlayerOffsetX = new Vector2(-4, 9); // offsets from where the player is
    public Vector2 accetpablePlayerOffsetY = new Vector2(-2, 4); // these ranges are still within camera frames, but may need tweaking
    public float forceZOffset = 0; // if we want the monster to be more forward or behind

    private Vector3 lerpPosA, lerpPosB;
    private float blend;
    private int dir = 1;
    private float getIntoPositionSpeed = 15;
    private bool gettingIntoPosition;


    // TARGET LOCKED
    [Header("TargetLocked Variables\n______________")]
    public float targetLockedTime = 10;
    public float shakeSpeed = 1;
    [Range(0, 1)]
    public float shakeAmount = 0.05f;

    private Vector3 startPos;


    // ATTACKING
    [Header("Attacking Variables\n______________")]
    public float attackSpeed = 5;
    public float targetTolerance = 0.25f;
    public Vector3 positionalTargetOffset = new Vector3(0, 0, 0);

    private Vector3 storedAttackPos;
    private bool didStoreAttack;


    // RECOVERING
    [Header("Recovering Variables\n______________")]
    public float recoveringTime = 10;


    // RECOVERING
    [Header("Capturing Variables\n______________")]
    public int pointsUntilCaptured = 10;
    public int pointsForCapturing = 100;
    public Transform uiHolderOfHeartPoints;
    public float sizeToShrinkTo = 0.15f;
    public float sizePercentChangeEveryFrame = 0.999f;
    public float capturedMoveSpeed = 1;
    [Space]
    [Space]
    public bool flyTowardsPlayer;     
    public float distanceToCollect = 0.5f;    
    [Space]
    [Space]
    public float captureFlyAwayTime = 10;      

   
    private float percentHP;
    private int currentCapturePointsLeft;
    private Vector3 startScale;

    private void OnEnable()
    {   // initialize
        currentStateTimeStamp = Time.time;
        currentCapturePointsLeft = pointsUntilCaptured;
        if (startScale == Vector3.zero)
            startScale = transform.localScale;
        transform.localScale = startScale;
        UpdateUserInterface();
        if (!playerObj)
            playerObj = GameObject.FindGameObjectWithTag(tag_ToHunt).transform;
    }

    private void Update()
    {
        if (!playerObj)
        { Debug.Log($"ERROR: Cant find player obj for this monster ({transform.name}) to hunt"); playerObj = GameObject.FindGameObjectWithTag(tag_ToHunt).transform; return; }

        StateChecker();
    }

    public void ChangeCapturePoints(int _changeAmount)
    {       
        currentCapturePointsLeft += _changeAmount;
        if (currentCapturePointsLeft > pointsUntilCaptured)
            currentCapturePointsLeft = pointsUntilCaptured;
        if (currentCapturePointsLeft <= 0)
            ChangeState(MONSTERSTATE.Captured);

        UpdateUserInterface();
    }

    private void UpdateUserInterface()
    {
        if (!uiHolderOfHeartPoints || uiHolderOfHeartPoints.childCount == 0)
            return;
       
        percentHP = ((float)currentCapturePointsLeft / (float)pointsUntilCaptured); // get the decimal      
        percentHP = (Mathf.RoundToInt(percentHP * 10)); // we have 10 hp bars to show or hide     

        for (int i = 0; i < uiHolderOfHeartPoints.childCount; i++)
        {
            Image heartPiece = null;
            uiHolderOfHeartPoints.GetChild(i).TryGetComponent(out heartPiece);
            if (heartPiece)
                if (i < percentHP)
                    heartPiece.enabled = true;
                else
                    heartPiece.enabled = true;
        }
    }

    public void ChangeState(MONSTERSTATE _newState)
    {
        currentStateTimeStamp = Time.time;
        ResetVariables();
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

    private void StateWaiting()
    {
        transform.position += MoveWithBackground();

        if (activatesOnDistance)
        {
            float dist = Vector3.Distance(transform.position, playerObj.position);
            if (dist <= distanceToActivate)
                ChangeState(MONSTERSTATE.Hunting);
        }

        if (activatesOnWait && Time.time > currentStateTimeStamp + huntingTime)
            ChangeState(MONSTERSTATE.Hunting);
    }

    private void StateHunting()
    {
        if (gettingIntoPosition)
        {
            lerpPosA = playerObj.transform.position + new Vector3(accetpablePlayerOffsetX.x, accetpablePlayerOffsetY.y, forceZOffset); // update target position   
            float distToHuntingSpot = Vector3.Distance(transform.position, lerpPosA);

            if (distToHuntingSpot > targetTolerance * 2)
            {
                var step = getIntoPositionSpeed * Time.deltaTime; // calculate distance to move
                transform.position = Vector3.MoveTowards(transform.position, lerpPosA, step);
                currentStateTimeStamp = Time.time; // gives us extra time to get into position
            }
            else
                gettingIntoPosition = false;
        }
        else
        {
            if (dir > 0 && blend < 1) // go forward
            {
                blend += (Time.deltaTime * blendSpeed) * dir;
                transform.position = Vector3.Lerp(lerpPosA, lerpPosB, blend);
            }
            else if (dir < 0 && blend > 0) // go backwards
            {
                blend += (Time.deltaTime * blendSpeed) * dir;
                transform.position = Vector3.Lerp(lerpPosA, lerpPosB, blend);
            }
            else
            {
                dir *= -1;
                lerpPosA = playerObj.transform.position + new Vector3(accetpablePlayerOffsetX.x, accetpablePlayerOffsetY.y, forceZOffset);
                lerpPosB = playerObj.transform.position + new Vector3(accetpablePlayerOffsetX.y, accetpablePlayerOffsetY.y, forceZOffset);
            }
            if (Time.time > currentStateTimeStamp + huntingTime)
                ChangeState(MONSTERSTATE.TargetLocked);
        }
    }


    private void StateTargetLocked()
    {
        if (startPos == Vector3.zero)
            startPos = transform.position;

        startPos += MoveWithBackground();

        float randomX = startPos.x + shakeSpeed * shakeAmount * Random.Range(0.95f, 1.05f);
        float randomY = startPos.y + shakeSpeed * shakeAmount * Random.Range(0.85f, 1.15f);
        transform.position = new Vector3(randomX, randomY, startPos.z + forceZOffset);

        // place a target obj where the player is to show this creatures intentions

        if (Time.time > currentStateTimeStamp + targetLockedTime)
            ChangeState(MONSTERSTATE.Attacking); // attackings
    }


    private void StateAttacking()
    {
        if (!didStoreAttack)
        { storedAttackPos = (playerObj.transform.position + positionalTargetOffset); didStoreAttack = true; }

        var step = attackSpeed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, storedAttackPos, step);

        float dist = Vector3.Distance(transform.position, storedAttackPos);
        transform.position += MoveWithBackground();
        storedAttackPos += MoveWithBackground();

        if (dist <= targetTolerance)
            ChangeState(MONSTERSTATE.Recovering);
    }

    private void StateRecovering()
    {
        transform.position += MoveWithBackground();

        if (Time.time > currentStateTimeStamp + recoveringTime)
            ChangeState(MONSTERSTATE.Hunting);
    }

    private void StateCaptured()
    {      

        if (transform.localScale.x > sizeToShrinkTo)
        { transform.localScale *= sizePercentChangeEveryFrame; capturedMoveSpeed *= 1.1f; }


        if (flyTowardsPlayer)
        {
            float dist = Vector3.Distance(transform.position, playerObj.position);
            if (dist > distanceToCollect)
            {
                var step = capturedMoveSpeed * Time.deltaTime; // calculate distance to move
                transform.position = Vector3.MoveTowards(transform.position, playerObj.position, step);
            }
            else // DONE
            {
                ChangeState(MONSTERSTATE.Waiting);  //TODO : add points to our score
                gameObject.SetActive(false);
            }
        }
        else
        {
            transform.Translate(Vector3.up * Time.deltaTime * capturedMoveSpeed);
            if (Time.time > currentStateTimeStamp + captureFlyAwayTime) // done //TODO : add points to our score
            { ChangeState(MONSTERSTATE.Waiting); gameObject.SetActive(false); }

        }
    }

}

