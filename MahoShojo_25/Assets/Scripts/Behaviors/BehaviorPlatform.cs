using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorPlatform : MonoBehaviour
{
    //public bool isMainPlatform { get; private set; } // when this platform is the core one on screen
    public Vector2 scale = new Vector2(80, 30); // how big this platform is  
    public bool isVisible { get; private set; }
    public GameObject artToShowHide;
    private Vector3 startPos;
    [HideInInspector] public string nickname;

    private void Start()
    {
        startPos = transform.position;
    }

    public void ShowHideArt(bool _show)
    {
        if (artToShowHide)
            artToShowHide.SetActive(_show);
        isVisible = _show;
    }

    public void ResetToStart()
    {
        transform.position = startPos;
        ShowHideArt(true);
    }

}
