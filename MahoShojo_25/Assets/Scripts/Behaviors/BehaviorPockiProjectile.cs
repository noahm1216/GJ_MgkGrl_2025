using UnityEngine;
using UnityEngine.Events;

public class BehaviorPockiProjectile : MonoBehaviour
{
    public string tag_ToHunt = "Monster";
    [Range(0, 100)]
    public float targetSpeedMove = 90;
    public float acceleration = 1;
    private float speedMove = 1;

    public float distanceTolerance = 0.1f; // can instead do a collision based encounter if desired
    public float limpRotation = 0.5f; // when no target the pocki will fire and fall
    public UnityEvent onEnableEvent, onDisableEvent;

    private Transform targetToChase;
    private GameObject[] allMonstersEnabled;
    private float timeEnabled;
    private float noTargetLifeSpan = 3;
    private bool storedStartRotation;
    private Quaternion startRot;

    private BehaviorMonster ref_BehaviorMonster; // TODO: Get reference to this during runtime... then if we have it CALL changeHP on the monster

    // Start is called before the first frame update
    void OnEnable()
    {
        if (!storedStartRotation)
        { startRot = transform.rotation; storedStartRotation = true; }

        ref_BehaviorMonster = null;
        speedMove = 1;
        transform.rotation = startRot;
        transform.Rotate(-45, 90, 0); // sets it straight up
        onEnableEvent.Invoke();
        allMonstersEnabled = GameObject.FindGameObjectsWithTag(tag_ToHunt);
        targetToChase = FindClosestMonster();
        timeEnabled = Time.time;
    }

    private Transform FindClosestMonster()
    {
        if (allMonstersEnabled.Length == 0)
            return null;

        float closestDistance = 0;
        int closestID = 0;

        for(int i = 0; i < allMonstersEnabled.Length; i++)
        {
            float dist3D = Vector3.Distance(allMonstersEnabled[i].transform.position, transform.position);
            if( i == 0 || dist3D < closestDistance)
            { closestDistance = dist3D; closestID = i; }
        }

        return allMonstersEnabled[closestID].transform;
    }

    private float DistanceToOther(Transform _otherObj)
    {
        return Vector3.Distance(_otherObj.position, transform.position);
    }

    private void OnDisable()
    {
        
        onDisableEvent.Invoke();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (targetToChase == null) // with no target we'll just shoot forward and turn off
        { transform.Translate(Vector3.forward * (speedMove /2) * Time.deltaTime); if (Time.time > timeEnabled + noTargetLifeSpan)  gameObject.SetActive(false); }

        if (targetToChase)
        {
            if (DistanceToOther(targetToChase) <= distanceTolerance)
            {
                // we got to it -> run code on the object we want (monster code)
                targetToChase = null;
                return;
            }

            transform.LookAt(targetToChase);
            var step = speedMove * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, targetToChase.position, step);
        }
        else
            transform.Rotate(1 * limpRotation, 0, 0);

        if (speedMove < targetSpeedMove)
            speedMove += acceleration * Time.deltaTime;
        else
            speedMove = targetSpeedMove;
    }
}
