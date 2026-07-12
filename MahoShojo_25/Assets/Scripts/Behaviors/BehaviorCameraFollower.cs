using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorCameraFollower : MonoBehaviour
{
    // the camera should be parented to the player object and the camer will just move around to help
    public static BehaviorCameraFollower Instance { get; private set; }
    public PlayerCore ref_PlayerCore;    

    public Camera camMain;
    public Transform starrySkyObj; // for visuals on background

    [SerializeField] private Transform playerObj, poiObj, midPointVisualizer; // the player and the target position we'll find the middle ground for
    private Vector3 camMidPos, camEndPos;
    private Vector3 camVelocity;
    private bool camIsEnumorating;


    [Header("Look Ahead")]
    [SerializeField] [Range(0, 1)] private float camPoiWeight = 0.5f;
    [SerializeField] private float cameraSmoothSpeed = 0.2f;//6f;

    [Header("Viewport Safe Zone")]
    [SerializeField] private bool useViewportLimiter = true;

    // percentages of the screen
    [SerializeField] private float leftLimit = 0.35f;
    [SerializeField] private float rightLimit = 0.65f;
    [SerializeField] private float bottomLimit = 0.35f;
    [SerializeField] private float topLimit = 0.65f;
    [Space]
    [SerializeField] private float verticalMinLimit = 0.35f;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);  // If there is an instance, and it's not me, delete myself.
        else Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!camMain)  camMain = Camera.main;
        if (!ref_PlayerCore) transform.parent.TryGetComponent<PlayerCore>(out PlayerCore playerCore);
        if (ref_PlayerCore && !playerObj) playerObj = ref_PlayerCore.transform;
        if (!poiObj) poiObj = new GameObject("Point Of Interest Object").transform;

    }

    // Update is called once per frame
    void Update()
    {
        if (starrySkyObj && camMain)
            starrySkyObj.localScale = new Vector3(camMain.orthographicSize * 0.2f, camMain.orthographicSize * 0.2f, camMain.orthographicSize * 0.2f);

        if (camMain) LookAheadBehavior(); // the camera should follow 2 points ( the player and a point of interest) then grab the middle * weighted preference
    }

    #region CAMERA MAIN BEHAVIOR

    private void LookAheadBehavior()
    {
        if (!playerObj) { Debug.LogWarning("No Player Assigned"); return; }

        if (transform.parent == playerObj) transform.SetParent(playerObj.parent);

        // Calculate desired camera position
        if (poiObj) camEndPos = poiObj.position;
        else camEndPos = playerObj.position;

        Vector3 desiredPos = Vector3.Lerp(playerObj.position, camEndPos, camPoiWeight);
        desiredPos.z = camMain.transform.position.z;
        if (midPointVisualizer) midPointVisualizer.position = desiredPos;

        // figure vertical sectioning
        float yDif = Mathf.Abs(desiredPos.y - camMain.transform.position.y);
        if (yDif < verticalMinLimit) desiredPos.y = camMain.transform.position.y;
        else desiredPos.y -= Mathf.Sign(yDif) * verticalMinLimit;

        // Smooth follow
        camMain.transform.position = Vector3.SmoothDamp(camMain.transform.position, desiredPos, ref camVelocity, cameraSmoothSpeed); // camMain.transform.position = Vector3.Lerp(transform.position, desiredPos, cameraSmoothSpeed * Time.deltaTime);

        // Keeps player inside viewport safe zone
        if (useViewportLimiter) ApplyViewportLimiter();
    }

    private void ApplyViewportLimiter()
    {
        Vector3 viewport = camMain.WorldToViewportPoint(playerObj.position);

        Vector3 correction = Vector3.zero;

        float worldHeight = camMain.orthographicSize * 2f;
        float worldWidth = worldHeight * camMain.aspect;

        // Horizontal
        if (viewport.x < leftLimit)
        {
            float percent = leftLimit - viewport.x;
            correction.x -= percent * worldWidth;
        }
        else if (viewport.x > rightLimit)
        {
            float percent = viewport.x - rightLimit;
            correction.x += percent * worldWidth;
        }

        // Vertical
        if (viewport.y < bottomLimit)
        {
            float percent = bottomLimit - viewport.y;
            correction.y -= percent * worldHeight;
        }
        else if (viewport.y > topLimit)
        {
            float percent = viewport.y - topLimit;
            correction.y += percent * worldHeight;
        }

       camMain.transform.position += correction;
    }

    #endregion cam main behavior


    #region PUBLIC CALLERS
    /// Can call these functions to set different scenarios
    /// 
    /// Final version could have preset functions to call like "EnemyMonster" which might focus on a target
    /// or
    /// "Boss Battle" which locks the camera in a single spot on the screen

    /// <summary>
    /// <para> Set the camera free from forcefully following the player </para>
    /// </summary>
    public void LimitCamera(bool _followLimits)
    {
        // set follow to false for free camera
        useViewportLimiter = _followLimits;
    }

    /// <summary>
    /// <para> Set the camera target to a position instantly </para>
    /// </summary>
    public void MoveCamTargetInstant(Vector3 _targetPos, float _weight = -1, float _orthoZoom = -1)
    {
        if (_weight != -1) camPoiWeight = _weight;
        if (_targetPos != Vector3.zero) camMain.transform.position = _targetPos;
        if (_orthoZoom != -1) camMain.orthographicSize = _orthoZoom;
    }


    /// <summary>
    /// <para> Set the camera target to a position over a period of time </para>
    /// </summary>
    public IEnumerator MoveCamTargetSmooth(float _seconds, Vector3 _targetPos, float _weight = -1, float _orthoZoom = -1)
    {
        if (!camIsEnumorating)
        {
            camIsEnumorating = true;
            // calculate time to move to position 
            // calculate time to move to weight
            yield return new WaitForSeconds(0);
            camIsEnumorating = false;
        }
    }

    #endregion public callers
}
