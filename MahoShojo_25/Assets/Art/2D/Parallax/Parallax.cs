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

using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private Transform referenceObject;
    [SerializeField] private Transform[] backgroundLayers; // Assign the background layers in the inspector
    [SerializeField] private float parallaxMultiplier = 0.5f; // Adjust the speed of parallax effect
    [SerializeField] private float speed = 1f; // Speed at which the background moves
    [SerializeField] private float resetDistance; // The distance at which the layers reset

    private float spriteWidth; // Width of a single background sprite
    
    private Manager_Platforms managerInstance;

    private void Start()
    {
        if (Manager_Platforms.Instance)
            managerInstance = Manager_Platforms.Instance;
        
        if (backgroundLayers.Length == 0)
        {
            Debug.LogError("No background layers assigned!");
            return;
        }

        // Assuming all backgrounds have the same width, get the first one's width
        SpriteRenderer spriteRenderer = backgroundLayers[0].GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            spriteWidth = spriteRenderer.bounds.size.x;
        }
        else
        {
            Debug.LogError("Background layers must have SpriteRenderer components.");
        }

        // Set resetDistance to match the full span of all three background images
        resetDistance = spriteWidth * backgroundLayers.Length / 3f;
    }

    private void Update()
    {
        speed = managerInstance.CurrentSpeed();
        MoveBackground();
        CheckAndResetBackgrounds();
    }

    private void MoveBackground()
    {
        float movement = speed * parallaxMultiplier * Time.deltaTime;
        foreach (Transform layer in backgroundLayers)
        {
            layer.position += Vector3.left * movement;
            layer.transform.localPosition = new Vector3(layer.transform.localPosition.x, 0, 0);
        }
    }

    private void CheckAndResetBackgrounds()
    {
        float referenceX = referenceObject.position.x; // Get the reference object's X position

        foreach (Transform layer in backgroundLayers)
        {
            if (layer.position.x <= referenceX - resetDistance)
            {
                float newX = layer.position.x + resetDistance * 3; // Move to the rightmost position
                layer.position = new Vector3(newX, layer.position.y, layer.position.z);
                layer.transform.localPosition = new Vector3(layer.transform.localPosition.x, 0, 0);
            }
        }
    }
}



