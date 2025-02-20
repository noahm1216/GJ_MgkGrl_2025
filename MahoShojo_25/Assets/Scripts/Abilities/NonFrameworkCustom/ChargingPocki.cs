using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

[ExecuteInEditMode]
public class ChargingPocki : MonoBehaviour
{
    public KeyCode keyToCharge;
    public Transform pockiBoxObj, visualObjMeter;
    public float requiredChargeTime = 1;
    public bool chargeTimeEqualsPockiCollected;
    public Vector3 pockiBoxOffset = new Vector3(-1, 1, 0);
    public float pockiCollected = 1;

    private float timeStampPressedKey;
    private float chargePercent = 0;
    private float pockiFollowSpeed = 4.5f;

    public UnityEvent onChargeStartEvent, onChargeCompleteEvent, onReleaseSuccessEvent, onReleaseFailEvent;


    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (pockiBoxObj)
                pockiBoxObj.gameObject.SetActive(true);
        }
        else
        {
#endif
            if (pockiBoxObj)
                pockiBoxObj.gameObject.SetActive(false);

#if UNITY_EDITOR
        }
#endif
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (pockiBoxObj)
                pockiBoxObj.position = transform.position + pockiBoxOffset;
        }
        else
        {
#endif
            if (keyToCharge == KeyCode.None)
            { Debug.Log("WARNING: Unable to run charge code due to no key specified"); return; }

            if (pockiCollected == 0) // no pocki to fire
                return;

          

            //print("Yoh"); // running regularly

            if (Input.GetKeyUp(keyToCharge)) // let go of key (no longer charging)
            {
                if (Time.time > timeStampPressedKey + requiredChargeTime)  // finished charging ability            
                    onReleaseSuccessEvent.Invoke();
                else
                    onReleaseFailEvent.Invoke();

                if (pockiBoxObj)
                    pockiBoxObj.SetParent(null);
            }

            if (Input.GetKeyDown(keyToCharge)) // Pressing key (start charging)
            {
                chargePercent = 0;
                timeStampPressedKey = Time.time;
                if (pockiBoxObj)
                { pockiBoxObj.position = transform.position + pockiBoxOffset; pockiBoxObj.SetParent(transform); }
                onChargeStartEvent.Invoke();
            }

            if (Input.GetKey(keyToCharge)) // Holding key (sustain charging)
            {
                if (chargePercent < 1)
                    chargePercent = ((Time.time - timeStampPressedKey) / requiredChargeTime);
                else
                    chargePercent = 1;

                if (pockiBoxObj && visualObjMeter)
                { visualObjMeter.localScale = new Vector3(visualObjMeter.localScale.x, chargePercent, visualObjMeter.localScale.z); }
            }

#if UNITY_EDITOR
        }
#endif

    }


    public void AddPocki()
    {
        pockiCollected++;

        if (chargeTimeEqualsPockiCollected)
            requiredChargeTime = pockiCollected;
    }

}
