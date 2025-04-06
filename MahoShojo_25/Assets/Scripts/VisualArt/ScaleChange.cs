using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleChange : MonoBehaviour
{
    public Transform objToScale;

    private float sizePercentChange = 1.005f;
    private float timeUntilScaleReturns = .01f;

    private Vector3 startScale;    
    private float scaleChangeStamp;

    private void Start()
    {
        if (!objToScale)
            objToScale = transform;

        startScale = objToScale.transform.localScale;
        scaleChangeStamp = Time.time - timeUntilScaleReturns;
    }


    public void ChangeScale()
    {
        scaleChangeStamp = Time.time;
        objToScale.localScale = startScale;
    }

    private void Update()
    {    
        if(Time.time > scaleChangeStamp + timeUntilScaleReturns)
        {
            objToScale.localScale = startScale;
        }
        else
        {
            objToScale.localScale *= (sizePercentChange);
        }
    }
}
