using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorCameraFollower : MonoBehaviour
{
    // the camera should be parented to the player object and the camer will just move around to help

    public enum CameraFocusState {None, Idle, MovingForward, MovingBackwards, InTheAir, FightingMonster,  }
    public CameraFocusState currentState;
    public float stateChangeTime = 3;

    public PlayerCore ref_PlayerCore;
    public Camera camMain;

    public bool useLegacyBehavior;
    public Transform camObj_Min, camObj_MedRight, camObj_MedLeft, camObj_Max;

    public float speedOfCameraTranslate = 2;
    public float speedOfCameraZoom = 1.25f;


    private Transform currentTarget;
    private float currentPlatformSpeed, platformSpeedMax;
    private float speedForZoomMed, speedForZoomMax;

    private float targetZoom;
    private Vector3 targetLookAtPoint;
    private float stateChangeTimeStamp;
    private CameraFocusState storedStateToChange;

    public Transform starrySkyObj;

    // Start is called before the first frame update
    void Start()
    {
        if (!camMain)
            camMain = Camera.main;
        currentState = CameraFocusState.Idle;
    }

    public void StoreChangeState(CameraFocusState _newState, bool forceChange)
    {    
        if (forceChange)
            ChangeState(_newState);
        else
        {
            if (_newState == storedStateToChange)
                return; // if we stored this state already then dont continue
            if (_newState != CameraFocusState.InTheAir && ref_PlayerCore && !ref_PlayerCore.CanJump())
                return; // dont change from jumping if we havent landed yet

            stateChangeTimeStamp = Time.time;
            storedStateToChange = _newState;
        }
    }

    private void ChangeState(CameraFocusState _newState)
    {
        stateChangeTimeStamp = Time.time;
        currentState = _newState;
        storedStateToChange = _newState;
        //print($"state change: {_newState}");
    }

    // Update is called once per frame
    void Update()
    {
        if (storedStateToChange != currentState && Time.time > stateChangeTimeStamp + stateChangeTime)
            ChangeState(storedStateToChange);

        if (starrySkyObj && camMain)
            starrySkyObj.localScale = new Vector3(camMain.orthographicSize*0.2f, camMain.orthographicSize * 0.2f, camMain.orthographicSize * 0.2f);


        if (Manager_Platforms.Instance && camMain)
        {
            if (useLegacyBehavior)
                LegacyGameJamMotion();
            else
                ImprovedGameJamMotion();
        }      
    }

    private void ImprovedGameJamMotion()
    {
        // create a position offset we'll track. 
        // have the camera lerp to that offset
        // if we have an enemy
        // then make the offset to be inbetween us and our enemy (and zoom out)
        // if we are running backwards long enough, then we can also offset in the opposite direction

        // X sets the Z (but is still left and right) || Y sets the Y (up and down) || Z sets the X (which is depth)
        Vector3 cameraOffset = new Vector3(0, 0, 0);  // Z, Y, X global space (i mention this because locally it feels different/off in inspector)
        

        switch (currentState)
        {
            case CameraFocusState.Idle:
                targetZoom = 2.75f;
                cameraOffset = new Vector3(2, 1f, -10); // TODO: Move this back a little (was (2,1,-5) )
                targetLookAtPoint = ref_PlayerCore.transform.position + cameraOffset;
                break;
            case CameraFocusState.MovingForward:
                targetZoom = 6;
                cameraOffset = new Vector3(8, 5f - Mathf.Abs(ref_PlayerCore.transform.position.y), -5);
                //cameraOffset = new Vector3(8, 4, -5); // if camera is too full of motion we can tweek this || if( Mathf.Abs(player.y) > Mathf.Abs(cam.y) + 3) ... then move based on Y ... else ... hard set 
                //if (ref_PlayerCore.transform.position.y > transform.position.y + 3)
                //    cameraOffset.y = 6;
                //if (ref_PlayerCore.transform.position.y < transform.position.y - 3)
                //    cameraOffset.y = 2;          
                targetLookAtPoint = ref_PlayerCore.transform.position + cameraOffset;
                break;
            case CameraFocusState.MovingBackwards:
                targetZoom = 6;
                cameraOffset = new Vector3(-5, 5 - Mathf.Abs(ref_PlayerCore.transform.position.y), -5);
                targetLookAtPoint = ref_PlayerCore.transform.position + cameraOffset;
                break;
            case CameraFocusState.InTheAir:
                targetZoom = 8;
                cameraOffset = new Vector3(4, 0, -5);
                targetLookAtPoint = ref_PlayerCore.transform.position + cameraOffset;
                break;
            case CameraFocusState.FightingMonster:
                if (Manager_Platforms.Instance)
                {
                    if (Manager_Platforms.Instance.spawnedMonster == null)
                        stateChangeTimeStamp = Time.time - stateChangeTime;
                    else
                    {
                        float dist = Vector3.Distance(transform.position, Manager_Platforms.Instance.spawnedMonster.position);
                        targetZoom = dist;  // calculate by the distance between our player and the enemy monster
                        //print("dist: " + (int)dist);
                        if (dist <= 15)
                            targetLookAtPoint = (ref_PlayerCore.transform.position + Manager_Platforms.Instance.spawnedMonster.position) / 2 + new Vector3(0, 0, -5);
                        stateChangeTimeStamp = Time.time; // makes sure we dont change the camera until the monster is gone
                    }
                }
                break;
            default:
                Debug.Log($"Camera Has No Behavior For: {currentState}");
                ChangeState(CameraFocusState.Idle);
                break;
        }

        var step = speedOfCameraTranslate * Time.deltaTime; // calculate distance to move
        camMain.transform.position = Vector3.MoveTowards(transform.position, targetLookAtPoint, step);

        if (targetZoom != camMain.orthographicSize)
        {
            if (camMain.orthographicSize < targetZoom)
            {
                camMain.orthographicSize += Time.deltaTime * speedOfCameraZoom;
                if (camMain.orthographicSize >= targetZoom)
                    camMain.orthographicSize = targetZoom;
            }
            else
            {
                camMain.orthographicSize -= Time.deltaTime * speedOfCameraZoom;
                if (camMain.orthographicSize <= targetZoom)
                    camMain.orthographicSize = targetZoom;
            }
        }

        if (Manager_Platforms.Instance && Manager_Platforms.Instance.isBlocked)
            StoreChangeState(CameraFocusState.Idle, false);
    }

    private void LegacyGameJamMotion()
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
