// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class Parallax : MonoBehaviour
// {
//     private float length, startPos, startY, startZ;
//     public GameObject cam;
//     public float parallaxEffect;
//
//     private Manager_Platforms managerInstance;
//     
//     private GameObject currentFirstPlatform;
//     private GameObject lastFirstPlatform;
//     
//     // Start is called before the first frame update
//     void Start()
//     {
//         managerInstance = Manager_Platforms.Instance;
//         startPos = transform.position.x;
//         startY = transform.position.y;
//         startZ = transform.position.z;
//         length = GetComponent<SpriteRenderer>().bounds.size.x;
//         currentFirstPlatform = managerInstance.spawnedPlatformsInPlay[0].gameObject;
//         lastFirstPlatform = managerInstance.spawnedPlatformsInPlay[0].gameObject;
//     }
//
//     // Update is called once per frame
//     void FixedUpdate()
//     {
//         float temp = (cam.transform.position.x * (1 - parallaxEffect));
//         float dist = (cam.transform.position.x * parallaxEffect);
//
//         transform.position = new Vector3(startPos + dist, startY, startZ);
//         Debug.Log(managerInstance.CurrentSpeed());
//         transform.localPosition = new Vector3(transform.localPosition.x, 0, 0);
//
//         if (temp > startPos + length) startPos += length;
//         else if (temp < startPos - length) startPos -= length;
//     }
// }

// using UnityEngine;
//
// public class Parallax : MonoBehaviour
// {
//     [SerializeField] private Transform referenceObject;
//     [SerializeField] private Transform[] backgroundLayers; // Assign the background layers in the inspector
//     [SerializeField] private float parallaxMultiplier = 0.5f; // Adjust the speed of parallax effect
//     [SerializeField] private float speed = 1f; // Speed at which the background moves
//     [SerializeField] private float resetDistance; // The distance at which the layers reset
//
//     private float spriteWidth; // Width of a single background sprite
//     
//     private Manager_Platforms managerInstance;
//
//     private void Start()
//     {
//         if (Manager_Platforms.Instance)
//             managerInstance = Manager_Platforms.Instance;
//         
//         if (backgroundLayers.Length == 0)
//         {
//             Debug.LogError("No background layers assigned!");
//             return;
//         }
//
//         // Assuming all backgrounds have the same width, get the first one's width
//         SpriteRenderer spriteRenderer = backgroundLayers[0].GetComponent<SpriteRenderer>();
//         if (spriteRenderer)
//         {
//             spriteWidth = spriteRenderer.bounds.size.x;
//         }
//         else
//         {
//             Debug.LogError("Background layers must have SpriteRenderer components.");
//         }
//
//         // Set resetDistance to match the full span of all three background images
//         resetDistance = spriteWidth * backgroundLayers.Length / 3f;
//     }
//
//     private void Update()
//     {
//         speed = managerInstance.CurrentSpeed();
//         MoveBackground();
//         CheckAndResetBackgrounds();
//     }
//
//     private void MoveBackground()
//     {
//         float movement = speed * parallaxMultiplier * Time.deltaTime;
//         foreach (Transform layer in backgroundLayers)
//         {
//             layer.position += Vector3.left * movement;
//             layer.transform.localPosition = new Vector3(layer.transform.localPosition.x, 0, 0);
//         }
//     }
//
//     private void CheckAndResetBackgrounds()
//     {
//         float referenceX = referenceObject.position.x; // Get the reference object's X position
//
//         foreach (Transform layer in backgroundLayers)
//         {
//             if (layer.position.x <= referenceX - resetDistance)
//             {
//                 float newX = layer.position.x + resetDistance * 3; // Move to the rightmost position
//                 layer.position = new Vector3(newX, layer.position.y, layer.position.z);
//                 layer.transform.localPosition = new Vector3(layer.transform.localPosition.x, 0, 0);
//             }
//         }
//     }
// }

using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform referenceObject;
    [SerializeField] private Transform[] backgroundLayers; // assign your repeated tiles

    [Header("Parallax")]
    [SerializeField, Range(0f, 2f)] private float parallaxMultiplier = 0.5f; // strength of effect
    [SerializeField] private int tilesPerCycle = 3; // how many contiguous tiles form one seamless strip

    [Header("Smoothing")]
    [SerializeField] private float smoothingTime = 0.18f;   // higher = smoother, more lag
    [SerializeField] private float deltaDeadzone = 0.0005f; // ignore tiny deltas (units)

    // Internals
    private float spriteWidth;
    private float cycleSpan;

    private float smoothedRefX;
    private float smoothedRefXVel;
    private float prevSmoothedRefX;

    private Vector3[] initialLocalPos;

    private void Awake()
    {
        if (!referenceObject)
        {
            Debug.LogError("[Parallax] Missing referenceObject.");
            enabled = false;
            return;
        }

        if (backgroundLayers == null || backgroundLayers.Length == 0)
        {
            Debug.LogError("[Parallax] No background layers assigned!");
            enabled = false;
            return;
        }

        initialLocalPos = new Vector3[backgroundLayers.Length];
        for (int i = 0; i < backgroundLayers.Length; i++)
            initialLocalPos[i] = backgroundLayers[i].localPosition;
    }

    private void Start()
    {
        // Measure width from the first tile
        var sr = backgroundLayers[0].GetComponent<SpriteRenderer>();
        if (!sr)
        {
            Debug.LogError("[Parallax] Background layers must have SpriteRenderer components.");
            enabled = false;
            return;
        }

        spriteWidth = sr.bounds.size.x;
        tilesPerCycle = Mathf.Max(1, tilesPerCycle);
        cycleSpan = spriteWidth * tilesPerCycle;

        // Initialize smoothed position to avoid a big first-frame jump
        smoothedRefX = referenceObject.position.x;
        prevSmoothedRefX = smoothedRefX;
    }

    private void Update()
    {
        // Smooth the reference X position (position-driven, not speed-driven)
        smoothedRefX = Mathf.SmoothDamp(
            smoothedRefX,
            referenceObject.position.x,
            ref smoothedRefXVel,
            smoothingTime
        );

        float deltaX = smoothedRefX - prevSmoothedRefX;

        // Kill micro-jitters
        if (Mathf.Abs(deltaX) < deltaDeadzone)
            deltaX = 0f;

        if (deltaX != 0f)
        {
            // Move opposite the player: if player moves +X, backgrounds move -X.
            float movementX = -parallaxMultiplier * deltaX; // NOTE: deltaX already accounts for dt
            MoveBackgrounds(movementX);
            WrapTilesAroundReference();
        }
        else
        {
            // Even when not moving, keep Y/Z locked
            RelockLocalYZ();
        }

        prevSmoothedRefX = smoothedRefX;
    }

    private void MoveBackgrounds(float movementX)
    {
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            var t = backgroundLayers[i];

            // Move in world space along X only
            t.position += new Vector3(movementX, 0f, 0f);

            // Re-lock Y/Z to initial local values
            var lp = t.localPosition;
            t.localPosition = new Vector3(lp.x, initialLocalPos[i].y, initialLocalPos[i].z);
        }
    }

    private void WrapTilesAroundReference()
    {
        float refX = referenceObject.position.x;

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            var t = backgroundLayers[i];
            float dx = t.position.x - refX;

            if (dx <= -cycleSpan)
            {
                // too far left -> bring right by one full strip
                t.position = new Vector3(t.position.x + cycleSpan, t.position.y, t.position.z);
            }
            else if (dx >= cycleSpan)
            {
                // too far right -> bring left by one full strip
                t.position = new Vector3(t.position.x - cycleSpan, t.position.y, t.position.z);
            }

            // Re-lock local Y/Z after wrapping
            var lp = t.localPosition;
            t.localPosition = new Vector3(lp.x, initialLocalPos[i].y, initialLocalPos[i].z);
        }
    }

    private void RelockLocalYZ()
    {
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            var t = backgroundLayers[i];
            var lp = t.localPosition;
            t.localPosition = new Vector3(lp.x, initialLocalPos[i].y, initialLocalPos[i].z);
        }
    }
}