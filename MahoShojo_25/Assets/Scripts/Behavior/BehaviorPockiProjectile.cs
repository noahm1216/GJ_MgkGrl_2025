using UnityEngine;
using UnityEngine.Events;

public class BehaviorPockiProjectile : MonoBehaviour
{
    public string tag_ToHunt = "Monster";
    [Range(0, 100)]
    public float speedMove = 1;
    public float distanceTolerance = 0.1f; // can instead do a collision based encounter if desired
    public UnityEvent onEnableEvent, onDisableEvent;

    private Transform targetToChase;
    private GameObject[] allMonstersEnabled;

    // Start is called before the first frame update
    void OnEnable()
    {
        onEnableEvent.Invoke();
        allMonstersEnabled = GameObject.FindGameObjectsWithTag(tag_ToHunt);
        targetToChase = FindClosestMonster();
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
    void Update()
    {
        if (!targetToChase)
        { gameObject.SetActive(false); return; }

        if(DistanceToOther(targetToChase) <= distanceTolerance)
        {
            // we got to it -> run code on the object we want (monster code)
            targetToChase = null;
            return;
        }

        transform.LookAt(targetToChase);
        var step = speedMove * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, targetToChase.position, step);

    }
}
