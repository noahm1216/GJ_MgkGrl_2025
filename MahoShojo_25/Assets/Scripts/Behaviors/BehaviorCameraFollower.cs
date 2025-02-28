using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorCameraFollower : MonoBehaviour
{
    // the camera should be parented to the player object and the camer will just move around to help

    public PlayerCore ref_PlayerCore;
    public Camera camMain;
    public Transform camObj_Min, camObj_MedRight, camObj_MedLeft, camObj_Max;

    public float speedOfCameraTranslate = 2;
   

    private Transform currentTarget;
    private float currentPlatformSpeed, platformSpeedMax;
    private float speedForZoomMed, speedForZoomMax;

    // Start is called before the first frame update
    void Start()
    {
        if (!camMain)
            camMain = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Manager_Platforms.Instance && camMain)
        {
            // set up the paremters for the camera
            currentPlatformSpeed = Mathf.Abs(Manager_Platforms.Instance.CurrentSpeed());
            speedForZoomMax = Mathf.Abs(Manager_Platforms.Instance.speedBase * Manager_Platforms.Instance.speedLimiting);
            speedForZoomMed = platformSpeedMax * 0.5f;            

            // follow those camera paremters
            if (currentPlatformSpeed <= speedForZoomMed)
                currentTarget = camObj_Min;
            else
            if (currentPlatformSpeed > speedForZoomMed && currentPlatformSpeed < speedForZoomMax)
            {
                if (ref_PlayerCore && ref_PlayerCore.dir > 0) // if platforms are going right or left
                    currentTarget = camObj_MedRight;
                else
                    currentTarget = camObj_MedLeft;             
            }
            else
            if (currentPlatformSpeed >= speedForZoomMax)
                currentTarget = camObj_Max;

            var step = speedOfCameraTranslate * Time.deltaTime; // calculate distance to move
            camMain.transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, step);
        }
    }
}
