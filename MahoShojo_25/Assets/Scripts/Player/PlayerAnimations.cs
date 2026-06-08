using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{

    public Animator[] animPlayer;
    public string[] triggersToReset;
    public bool updateAnimSpeed = true;

    private bool delayMaskDisable;
    private float maskDisableTimeStamp, maskDisableTime = 1.5f;
    

    public void SetAnyBool(string _name, bool _canMove)
    {
        Debug.Log($"SetBool: {_name} = {_canMove}");

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
        if (_layerWeight == 0)
        { maskDisableTimeStamp = Time.time; delayMaskDisable = true; }
        else
        { delayMaskDisable = false;  ChangeAnimLayerMask("TopHalf", _layerWeight); }
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


    private void LateUpdate() // TODO: this should not be update calls, but called from playercore / manager platforms 
    {
        if (Manager_GameState.Instance) // if we have the game manager then we want things to look a specific way
        {
            ReactToGameManager();
            if (Manager_GameState.Instance.currentState != Manager_GameState.GAMESTATE.Playing)
                return;
        }

        if (delayMaskDisable && Time.time > maskDisableTimeStamp + maskDisableTime)
            ChangeAnimLayerMask("TopHalf", 0);

        if (animPlayer.Length > 0 && Manager_Platforms.Instance)
        {
            //if(Manager_Platforms.Instance.CurrentSpeed() > 0)
            //    print("running: backwards");
            //if (Manager_Platforms.Instance.CurrentSpeed() < 0)
            //    print("running: forward");

            SetAnyBool("isRunningBackwards", Manager_Platforms.Instance.CurrentSpeed() > 0); // moving left
            SetAnyBool("isMoving", Mathf.Abs(Manager_Platforms.Instance.CurrentSpeed()) > 0 && !Manager_Platforms.Instance.isBlocked); // moving at all
            //SetAnyBool("isDashing", Manager_Platforms.Instance.isDashing); // dashing process
            //SetAnyBool("isFalling", Manager_Platforms.Instance.playerInAir && !Manager_Platforms.Instance.isDashing); // in air
                                                                                                                      //if (Manager_Platforms.Instance.isDashing) // setting it through events
                                                                                                                      //    SetAnyTrigger("Dashed");

            
                
        }
    }

    public void UpdateAnimationSpeed(float _curSpd)
    {
        if (updateAnimSpeed)
            for (int i = 0; i < animPlayer.Length; i++)
                if (animPlayer[i] != null && animPlayer[i].gameObject.activeSelf == true)
                    animPlayer[i].speed = 0.5f + Mathf.Abs(_curSpd * 8.0f);
    }

    private void ReactToGameManager()
    {

    }
}
