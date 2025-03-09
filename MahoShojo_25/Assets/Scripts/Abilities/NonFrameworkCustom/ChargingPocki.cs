using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

public class ChargingPocki : MonoBehaviour
{
    public KeyCode keyToCharge;
    public Transform pockiBoxObj, visualObjMeter;
    public float requiredChargeTime = 1;
    public bool chargeTimeEqualsPockiCollected; 
    [Range(0.0f, 10)]
    public float chargeTimeMultiplier = 2;
    public bool followPlayer;
    public Vector3 pockiBoxOffset = new Vector3(-1, 1, 0);
    public float pockiCollected = 1;

    private float timeStampPressedKey;
    private float chargePercent = 0;
    private float pockiFollowSpeed = 4.5f;

    public UnityEvent onChargeStartEvent, onChargeCompleteEvent, onReleaseSuccessEvent, onReleaseFailEvent;


    // Start is called before the first frame update
    void Start()
    {

        if (pockiBoxObj)
            pockiBoxObj.gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {

        if (keyToCharge == KeyCode.None)
        { Debug.Log("WARNING: Unable to run charge code due to no key specified"); return; }

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
        }

        if (Input.GetKeyDown(keyToCharge)) // Pressing key (start charging)
        {
            chargePercent = 0;
            timeStampPressedKey = Time.time;
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
        }

    }


    public void AddPocki()
    {
        pockiCollected++;

        if (chargeTimeEqualsPockiCollected)
            requiredChargeTime = pockiCollected;
    }


}
