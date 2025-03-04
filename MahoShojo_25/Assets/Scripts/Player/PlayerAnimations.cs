using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    
    public Animator animPlayer;
    public string[] triggersToReset;

    public void SetAnyBool(string _name, bool _canMove)
    {
        animPlayer.SetBool(_name, _canMove);
    }

    public void SetAnyTrigger(string _name)
    {
        ResetAllTriggers();
        animPlayer.SetTrigger(_name);
    }

    private void ResetAllTriggers()
    {
        foreach(string trigger in triggersToReset)
            animPlayer.ResetTrigger(trigger);
    }


    private void LateUpdate()
    {
        if (animPlayer && Manager_Platforms.Instance)
        {
            //if(Manager_Platforms.Instance.CurrentSpeed() > 0)
            //    print("running: backwards");
            //if (Manager_Platforms.Instance.CurrentSpeed() < 0)
            //    print("running: forward");

            SetAnyBool("isRunningBackwards", Manager_Platforms.Instance.CurrentSpeed() > 0); // moving left
            SetAnyBool("canMove", Mathf.Abs(Manager_Platforms.Instance.CurrentSpeed()) > 0); // moving at all
        }
    }
}
