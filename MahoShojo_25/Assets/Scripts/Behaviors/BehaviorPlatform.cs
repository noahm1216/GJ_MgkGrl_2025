using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorPlatform : MonoBehaviour
{
    public bool isMainPlatform { get; private set; } // when this platform is the core one on screen
    public Vector2 scale = new Vector2(80, 30); // how big this platform is    


}
