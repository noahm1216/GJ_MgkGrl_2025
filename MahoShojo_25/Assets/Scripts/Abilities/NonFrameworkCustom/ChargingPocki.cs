using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System;

public class ChargingPocki : MonoBehaviour // TODO: Add a reference to player animations that accounts for holding this button ... then set the bool until we release it
{
    public KeyCode keyToCharge;
    public string axisFire { get; private set; } = "Fire0";
    private float axisInputDeadzone = 0.15f;
    private bool holdingKey = false;
 
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

    private void FixedUpdate()
    {
        if (alwaysShowPockiOnceUnlocked && pockiBoxObj && pockiBoxObj.gameObject.activeSelf == false)
        {
            if (Manager_Platforms.Instance)
                pockiBoxObj.gameObject.SetActive(Manager_Platforms.Instance.playerUnlockedPockiBox);
        }
    }

    public void UpdatePokkiCollected()
    {
        if (Manager_Platforms.Instance)
            pockiCollected = Manager_Platforms.Instance.pockiBoxSticks;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForInput();
    }

    private void CheckForInput()
    {
        if (keyToCharge == KeyCode.None)
        { Debug.Log("WARNING: Unable to run charge code due to no key specified"); return; }

        if (pockiCollected == 0) // no pocki to fire
            return;

        CheckKeyRelease();        
        CheckKeyPress();
        CheckKeyHold();
    }

    private void CheckKeyPress()
    {
        if (Input.GetKeyDown(keyToCharge) || Input.GetButtonDown(axisFire) || Input.GetAxis(axisFire) > axisInputDeadzone && !holdingKey) // Pressing key (start charging)
        {
            print("0- PRESSING FIRE KEY");
            holdingKey = false;
            chargePercent = 0;

            if (showPockiWhileCharging)
            {
                //pockiShownWhileCharging = 0;
                //SpawnAllPockiArtRefs();
            }

            timeStampPressedKey = Time.time;
            UpdateTotalCirclePositions(pockiCollected);

            if (pockiBoxObj && followPlayer)
            { pockiBoxObj.position = transform.position + pockiBoxOffset; pockiBoxObj.SetParent(transform); }

            onChargeStartEvent.Invoke();
        }
    }

    private void CheckKeyHold()
    {
        if (Input.GetKey(keyToCharge) || Input.GetButton(axisFire) || Input.GetAxis(axisFire) > axisInputDeadzone) // Holding key (sustain charging)
        {
            print("1- HOLDING FIRE KEY");
            if (Input.GetAxis(axisFire) > axisInputDeadzone)
                holdingKey = true;

            if (chargePercent < 1)
                chargePercent = ((Time.time - timeStampPressedKey) / requiredChargeTime) * chargeTimeMultiplier;
            else
                chargePercent = 1;

            if (pockiBoxObj && visualObjMeter)
            { visualObjMeter.localScale = new Vector3(visualObjMeter.localScale.x, chargePercent, visualObjMeter.localScale.z); }

            if (showPockiWhileCharging) // display the pocki at the same time as we prepare to launch it
            {
                //float pockiChargeTime = requiredChargeTime / chargeTimeMultiplier;

                //if (Time.time > showPockiWhileChargingTimeStamp + pockiChargeTime)
                //{
                //    pockiShownWhileCharging++;
                //    showPockiWhileChargingTimeStamp = Time.time;
                //    ShowPockiArtObjs(chargingPockiPositions[pockiShownWhileCharging], pockiShownWhileCharging);
                //}
            }
        }
    }

    private void CheckKeyRelease()
    {
        if (Input.GetKeyUp(keyToCharge) || Input.GetButtonUp(axisFire) || Input.GetAxis(axisFire) < axisInputDeadzone && holdingKey) // let go of key (no longer charging)
        {
            print("2- RELEASING FIRE KEY");
            holdingKey = false;

            if (Time.time > timeStampPressedKey + requiredChargeTime / chargeTimeMultiplier)  // finished charging ability            
                onReleaseSuccessEvent.Invoke();
            else
                onReleaseFailEvent.Invoke();

            if (pockiBoxObj && followPlayer)
                pockiBoxObj.SetParent(null);

            if (pockiBoxObj && visualObjMeter)
                visualObjMeter.localScale = new Vector3(visualObjMeter.localScale.x, 0.1f, visualObjMeter.localScale.z);

            //if (showPockiWhileCharging)
            //{ ShowPockiArtObjs(Vector3.zero, -1); } // spawn and disable pocki art when 
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
