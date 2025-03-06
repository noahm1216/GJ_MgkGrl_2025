using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public Vector2 accetpablePlayerOffsetX = new Vector2(-4, 9); // offsets from where the player is
    public Vector2 accetpablePlayerOffsetY = new Vector2(-2, 5); // these ranges are still within camera frames, but may need tweaking     

    private float currentStateTimeStamp;

    // HUNTING
    [Header("Waiting Variables\n______________")]
    public bool activatesOnWait;
    public float waitTime = 10;
    public bool activatesOnDistance;
    public float distanceToActivate = 4;


    // HUNTING
    [Header("Hunting Variables\n______________")]
    public float huntingTime = 10;
    public float blendSpeed = 1;
    private Vector3 lerpPosA, lerpPosB;
    private float blend;
    private int dir = 1;


    // TARGET LOCKED
    [Header("TargetLocked Variables\n______________")]
    public float targetLockedTime = 10;
    public float shakeSpeed = 1;
    [Range(0, 1)]
    public float shakeAmount = 0.05f;

    private Vector3 startPos;


    // ATTACKING
    [Header("Attacking Variables\n______________")]
    public float attackingTime = 10;

    private void Start()
    {
        // initialize
        currentStateTimeStamp = Time.time;
    }

    private void Update()
    {
        StateChecker();
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
                // state
                break;
            case MONSTERSTATE.Captured:
                // state
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
            if(dist <= distanceToActivate)
                ChangeState(MONSTERSTATE.TargetLocked);
        }

        if (activatesOnWait && Time.time > currentStateTimeStamp + huntingTime)
            ChangeState(MONSTERSTATE.Hunting);
    }

    private void StateHunting()
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
            lerpPosA = playerObj.transform.position + new Vector3(accetpablePlayerOffsetX.x, accetpablePlayerOffsetY.y, 0);
            lerpPosB = playerObj.transform.position + new Vector3(accetpablePlayerOffsetX.y, accetpablePlayerOffsetY.y, 0);
        }
        if (Time.time > currentStateTimeStamp + huntingTime)
            ChangeState(MONSTERSTATE.TargetLocked);
    }


    private void StateTargetLocked()
    {
        if (startPos == Vector3.zero)
            startPos = transform.position;

        startPos += MoveWithBackground();

        float randomX = startPos.x + (Time.time * shakeSpeed) * shakeAmount * Random.Range(0.9f, 1.1f);
        float randomY = startPos.y + (Time.time * shakeSpeed) * shakeAmount * Random.Range(0.9f, 1.1f);
        transform.position = new Vector3(randomX, randomY, startPos.z);

        if (Time.time > currentStateTimeStamp + targetLockedTime)
            ChangeState(MONSTERSTATE.Hunting); // attackings
    }


    private void StateAttacking()
    {


        if (Time.time > currentStateTimeStamp + attackingTime)
            ChangeState(MONSTERSTATE.Recovering);
    }


}


// the custom data for monsters
[System.Serializable]
public class MonsterCustomData
{
    public string monsterNickname;

    [Tooltip("The chance our monster can spawn. 1 (100%) means there is ALWAYS a chance it will spawn")]
    [Range(0, 1)] public float spawnChance; // 100% means it will always have a chance to spawn
    [Tooltip("The Model we will spawn when this is selected")]
    public Transform prefabToSpawn;

    public UnityEvent onSpawnEvents, onMainPlatformEvents, onFinishedEvents;

    //public CustomAbilityOptions(string _newName,)
    //{
    //    //abilityNickname = _newName;
    //}

}//end of data for monsters
