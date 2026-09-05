using UnityEngine;
using UnityEngine.Events;

public class MonsterJelly : BehaviorMonster
{

    protected Vector3 targetGraphicStartPos;
    protected Vector3 waitVarVector3One, waitVarVector3Two;
    protected float stepArc;

    public UnityEvent eJellyLand;

    public override void DisableMonster()
    {
        if (targetGraphic) targetGraphic.gameObject.SetActive(false);
    }


    #region STATE: WAITING
    protected override void StateWaiting()
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

            if (Time.time > currentStateTimeStamp + stateWaitTime || alreadySpawned) { alreadySpawned = true; ChangeState(MONSTERSTATE.Hunting); }// change state
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

        //if(alreadySpawned) ChangeState(MONSTERSTATE.Hunting); // if the monster already spawned, then we dont need to wait anymore

    }
    #endregion state: waiting

    #region STATE: HUNTING
    protected override void StateHunting()
    {
        //print("MONSTER HUNTING DEBUG");

        if (Time.time > currentStateTimeStamp + stateHuntingTime) //if (Time.time > betweenWaitActionsStamp + betweenWaitActionsTime)
        {
            waitVarVector3Two = ReturnRaycastPosition(playerObj.position.x + Random.Range(1, 4), groundLayers); // locate and set target area on the ground
            waitVarVector3Two.y += 0.05f;

            if (stepArc != 0)
            {
                betweenWaitActionsStamp = Time.time; // reset Timer                                                            
                stepArc = 0; // ready the arc path to move again
            }
            onHuntEndOne?.Invoke();
            ChangeState(MONSTERSTATE.TargetLocked); // change state
        }
        else
        {
            if (targetGraphic)
            {
                float totalTime = currentStateTimeStamp + stateHuntingTime;
                targetGraphic.position = Vector3.Lerp(transform.position, playerObj.position, Time.time / totalTime);
            }
        }
    }
    #endregion state: hunting

    #region STATE: TARGET LOCKED
    protected override void StateTargetLocked()
    {
        //print("MONSTER TARGET LOCKED DEBUG");
        transform.position += MoveWithBackground();
        if (targetGraphic) targetGraphic.transform.position += MoveWithBackground();
        huntingTargPos += MoveWithBackground();
        waitVarVector3Two += MoveWithBackground();

        if (targetGraphic) { targetGraphic.SetParent(null); targetGraphic.position = waitVarVector3Two; targetGraphic.gameObject.SetActive(true); }
        waitVarVector3One = transform.position; // store current points for path                
        if (Time.time > currentStateTimeStamp + stateTargLockTime) { onTargetEndOne?.Invoke(); ChangeState(MONSTERSTATE.Attacking); } // change state forward         

    }
    #endregion state: target locked

    #region STATE: ATTACKING
    protected override void StateAttacking()
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
                if (stepArc >= 1) { onAttackEndOne?.Invoke(); ChangeState(MONSTERSTATE.Recovering); }
            }
        }
        //if (Time.time > currentStateTimeStamp + stateTargLockTime) ChangeState(MONSTERSTATE.Recovering); // change state
    }
    #endregion state: attacking

    #region STATE: RECOVERING
    protected override void StateRecovering()
    {
        //print("MONSTER RECOVERING DEBUG");
        transform.position += MoveWithBackground();
        if (targetGraphic) { targetGraphic.SetParent(transform); targetGraphic.gameObject.SetActive(false); }

        if (Time.time > currentStateTimeStamp + stateRecoverTime)
        { onRecoverEndOne?.Invoke(); waitVarVector3Two = Vector3.zero; stepArc = 0; ChangeState(MONSTERSTATE.Hunting); }
    }
    #endregion state: recovering

    #region STATE: CAPTURED
    protected override void StateCaptured()
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
    #endregion state: captured

}