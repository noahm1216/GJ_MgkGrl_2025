using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntoCinematicAnimationTrigger : MonoBehaviour
{
    public Animator[] acMahos;

    public void ChangeAnimatorIdEvent(int _id)
    {
        print("Animation Event Called");
        //Debug.Log("PrintEvent called at " + Time.time + " with a value of " + s);
        if(acMahos.Length > 0)
        {
            for (int i = 0; i < acMahos.Length; i++)
                acMahos[i].SetInteger("CinematicId", _id);
        }
    }
}
