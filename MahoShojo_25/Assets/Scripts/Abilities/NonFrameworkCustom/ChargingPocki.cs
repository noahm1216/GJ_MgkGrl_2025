using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System;

public class ChargingPocki : MonoBehaviour
{
    public KeyCode keyToCharge;
    // -C
    public bool showPockiWhileCharging;
    public Transform pockiPrefabArtOnly;
    public Transform pointToSpawnFrom;
    // -/C
    public bool alwaysShowPockiOnceUnlocked = true;
    public Transform pockiBoxObj, visualObjMeter;
    public float requiredChargeTime = 1;
    public bool chargeTimeEqualsPockiCollected; 
    [Range(0.0f, 10)]
    public float chargeTimeMultiplier = 2;
    public bool followPlayer;
    public Vector3 pockiBoxOffset = new Vector3(-1, 1, 0);
    public int pockiCollected = 1;

    private float timeStampPressedKey;
    private float chargePercent = 0;
    private float pockiFollowSpeed = 4.5f;
    // -C
    private List<Vector3> chargingPockiPositions = new List<Vector3>();
    private List<Transform> chargingPockiArtObjs = new List<Transform>();
    private float showPockiWhileChargingTimeStamp;
    private int pockiShownWhileCharging;
    // -/C

    public UnityEvent onChargeStartEvent, onChargeCompleteEvent, onReleaseSuccessEvent, onReleaseFailEvent;


    // Start is called before the first frame update
    void Start()
    {

        if (pockiBoxObj)
            pockiBoxObj.gameObject.SetActive(false);
        if (!pockiPrefabArtOnly)
        { pockiPrefabArtOnly = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform; pockiPrefabArtOnly.gameObject.SetActive(false); }
    }

    // Update is called once per frame
    void Update()
    {

        if (keyToCharge == KeyCode.None)
        { Debug.Log("WARNING: Unable to run charge code due to no key specified"); return; }

        if (alwaysShowPockiOnceUnlocked && Manager_Platforms.Instance)
        {
            pockiCollected = Manager_Platforms.Instance.pockiBoxSticks;
            if (Manager_Platforms.Instance.playerUnlockedPockiBox)
                pockiBoxObj.gameObject.SetActive(true);
            else
            {
                pockiBoxObj.gameObject.SetActive(false);
                return;
            }
        }

        if (pockiCollected == 0) // no pocki to fire
            return;


        if (Input.GetKeyUp(keyToCharge)) // let go of key (no longer charging)
        {
            if (Time.time > timeStampPressedKey + requiredChargeTime / chargeTimeMultiplier)  // finished charging ability            
                onReleaseSuccessEvent.Invoke();
            else
                onReleaseFailEvent.Invoke();

            if (pockiBoxObj && followPlayer)
                pockiBoxObj.SetParent(null);

            if (pockiBoxObj && visualObjMeter)
                visualObjMeter.localScale = new Vector3(visualObjMeter.localScale.x, 0.1f, visualObjMeter.localScale.z);

            if (showPockiWhileCharging)
            {  ShowPockiArtObjs(Vector3.zero, -1); } // spawn and disable pocki art when 
        }

        if (Input.GetKeyDown(keyToCharge)) // Pressing key (start charging)
        {
            chargePercent = 0;
            if (showPockiWhileCharging)
            { pockiShownWhileCharging = 0; SpawnAllPockiArtRefs(); }
            timeStampPressedKey = Time.time;
            UpdateTotalCirclePositions(pockiCollected);
            if (pockiBoxObj && followPlayer)
            { pockiBoxObj.position = transform.position + pockiBoxOffset; pockiBoxObj.SetParent(transform); }
            onChargeStartEvent.Invoke();
        }

        if (Input.GetKey(keyToCharge)) // Holding key (sustain charging)
        {
            if (chargePercent < 1)
                chargePercent = ((Time.time - timeStampPressedKey) / requiredChargeTime) * chargeTimeMultiplier;
            else
                chargePercent = 1;

            if (pockiBoxObj && visualObjMeter)
            { visualObjMeter.localScale = new Vector3(visualObjMeter.localScale.x, chargePercent, visualObjMeter.localScale.z); }

            if (showPockiWhileCharging) // display the pocki at the same time as we prepare to launch it
            {
                for(int i = 0; i < chargingPockiPositions.Count; i++)
                {
                    //if(chargingPockiArtObjs.Count == 0 ||)

                    //if(chargingPockiArtObjs[])
                }

                float pockiChargeTime = requiredChargeTime / chargeTimeMultiplier;

                if(Time.time > showPockiWhileChargingTimeStamp + pockiChargeTime)
                {                   
                    pockiShownWhileCharging++;
                    showPockiWhileChargingTimeStamp = Time.time;
                    ShowPockiArtObjs(chargingPockiPositions[pockiShownWhileCharging], pockiShownWhileCharging);                                    
                }
            }
        }
    }


    private void UpdateTotalCirclePositions(int _numberCollected)
    {
        chargingPockiPositions.Clear(); // clear our list (TODO: could replace for better effeciency)

        if(!pointToSpawnFrom) pointToSpawnFrom = transform;
        float radiusForMultiplePocki = 2;

        for (int i = 0; i < _numberCollected; i++)
        {        
            /* Distance around the circle */
            var radians = 2 * MathF.PI / _numberCollected * i;

            /* Get the vector direction */
            var vertical = MathF.Sin(radians);
            var horizontal = MathF.Cos(radians);

            var spawnDir = new Vector3(horizontal, vertical, 0);

            /* Get the spawn position */            
            var spawnPos = pointToSpawnFrom.position + spawnDir * radiusForMultiplePocki; // Radius is just the distance away from the point

            /* Now spawn */
            if (chargingPockiPositions.Count == 0 || i > chargingPockiPositions.Count)
                chargingPockiPositions.Add(spawnPos);
            else
                chargingPockiPositions[i-1] = spawnPos;
        }

    }

    private bool SpawnAllPockiArtRefs()
    {
        for(int i = chargingPockiArtObjs.Count; i < chargingPockiPositions.Count; i++)
        {
            if (i < chargingPockiPositions.Count)
                chargingPockiArtObjs.Add(Instantiate(pockiPrefabArtOnly, transform));
        }

        return true;
    }

    private void ShowPockiArtObjs(Vector3 _pos, int _id)
    {
        if (_id < 0)
        {
            for (int i = 0; i < chargingPockiArtObjs.Count; i++)
                chargingPockiArtObjs[i].gameObject.SetActive(false);
            return;
        }

        chargingPockiArtObjs[_id].position = _pos;
        chargingPockiArtObjs[_id].gameObject.SetActive(true);

    }

    public void AddPocki()
    {
        pockiCollected++;

        if (chargeTimeEqualsPockiCollected)
            requiredChargeTime = pockiCollected;

        if (Manager_Platforms.Instance)
            pockiCollected += 1;        
    }


}
