using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorCameraFollower : MonoBehaviour
{
    // the camera should be parented to the player object and the camer will just move around to help
    public static BehaviorCameraFollower Instance { get; private set; }

    public PlayerCore ref_PlayerCore;
    [SerializeField] private Transform playerObj, poiObj, midPointVisualizer; // the player and the target position we'll find the middle ground for
    private Vector3 camMidPos, camEndPos;
    public Camera camMain;
    public Transform starrySkyObj; // for visuals on background


    [Header("Look Ahead")]
    [SerializeField] [Range(0, 1)] private float camPoiWeight = 0.5f;
    [SerializeField] private float cameraFollowSpeed = 6f;

    [Header("Viewport Safe Zone")]
    [SerializeField] private bool useViewportLimiter = true;

    // percentages of the screen
    [SerializeField] private float leftLimit = 0.35f;
    [SerializeField] private float rightLimit = 0.65f;
    [SerializeField] private float bottomLimit = 0.35f;
    [SerializeField] private float topLimit = 0.65f;

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

    private void LookAheadBehavior()
    {
        if (!playerObj) { Debug.LogWarning("No Player Assigned"); return; }

        if (transform.parent == playerObj) transform.SetParent(playerObj.parent);

        // Calculate desired camera position
        if (poiObj) camEndPos = poiObj.position;
        else camEndPos = playerObj.position;

        Vector3 desiredPos = Vector3.Lerp(playerObj.position, camEndPos, camPoiWeight);
        desiredPos.z =camMain.transform.position.z;
        if (midPointVisualizer) midPointVisualizer.position = desiredPos;

        // Smooth follow
       camMain.transform.position = Vector3.Lerp(transform.position, desiredPos, cameraFollowSpeed * Time.deltaTime);

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
}
