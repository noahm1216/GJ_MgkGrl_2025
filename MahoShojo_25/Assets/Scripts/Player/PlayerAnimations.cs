using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{

    public Animator[] animPlayer;
    public string[] triggersToReset;

    

    public void SetAnyBool(string _name, bool _canMove)
    {
        for (int i = 0; i < animPlayer.Length; i++)
            if (animPlayer[i] != null && animPlayer[i].gameObject.activeSelf == true)
                animPlayer[i].SetBool(_name, _canMove);
    }

    public void SetAnyTrigger(string _name)
    {
        ResetAllTriggers();
        for (int i = 0; i < animPlayer.Length; i++)
            if (animPlayer[i] != null && animPlayer[i].gameObject.activeSelf == true)
                animPlayer[i].SetTrigger(_name);
    }

    private void ResetAllTriggers()
    {
        foreach (string trigger in triggersToReset)
            for (int i = 0; i < animPlayer.Length; i++)
                if (animPlayer[i] != null && animPlayer[i].gameObject.activeSelf == true)
                    animPlayer[i].ResetTrigger(trigger);
    }

    public void EventTriggerCastLayer(float _layerWeight)
    {
        ChangeAnimLayerMask("TopHalf", _layerWeight);
    }

    public void EventTriggerCastBool(bool _isAttacking)
    {
        SetAnyBool("ChargingAttack", _isAttacking);
    }

    public void ChangeAnimLayerMask(string _layerName, float _layerWeight) // "TopHalf" = maho upper body
    {
        foreach (string trigger in triggersToReset)
            for (int i = 0; i < animPlayer.Length; i++)
                if (animPlayer[i] != null && animPlayer[i].gameObject.activeSelf == true)
                {
                    int layerIndex = animPlayer[i].GetLayerIndex(_layerName);
                    animPlayer[i].SetLayerWeight(layerIndex, _layerWeight);
                }
    }


    private void LateUpdate()
    {
        if (Manager_GameState.Instance) // if we have the game manager then we want things to look a specific way
        {
            ReactToGameManager();
            if (Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing)
                return;
        }


        if (animPlayer.Length > 0 && Manager_Platforms.Instance)
        {
            //if(Manager_Platforms.Instance.CurrentSpeed() > 0)
            //    print("running: backwards");
            //if (Manager_Platforms.Instance.CurrentSpeed() < 0)
            //    print("running: forward");

            SetAnyBool("isRunningBackwards", Manager_Platforms.Instance.CurrentSpeed() > 0); // moving left
            SetAnyBool("isMoving", Mathf.Abs(Manager_Platforms.Instance.CurrentSpeed()) > 0 && !Manager_Platforms.Instance.isBlocked); // moving at all
            SetAnyBool("isDashing", Manager_Platforms.Instance.isDashing); // dashing process
            SetAnyBool("isFalling", Manager_Platforms.Instance.playerInAir && !Manager_Platforms.Instance.isDashing); // in air
            //if (Manager_Platforms.Instance.isDashing) // setting it through events
            //    SetAnyTrigger("Dashed");
        }
    }

    private void ReactToGameManager()
    {

    }
}
