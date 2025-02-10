using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static AbilityHolder;

public class AbilityHolder : MonoBehaviour
{
    public enum AbilityState {Ready, Active, OnCooldown, Locked }

    public List<CustomAbilityOptions> abilitiesList = new List<CustomAbilityOptions>();
    private Transform playerObj;
    public KeyCode lastKeyPressed;

    private void Update()
    {
        if (!playerObj)
            playerObj = transform;

        CheckAbilityStats();
    }

    public void CheckAbilityStats()
    {
        if (abilitiesList.Count == 0) return;

        for (int i = 0; i < abilitiesList.Count; i++)
        {
            // check the state 
            switch (abilitiesList[i].currentState)
            {
                case AbilityState.Active: // Ability currently in use or being used
                    if (abilitiesList[i].activeTime > 0)
                        abilitiesList[i].activeTime -= Time.deltaTime;
                    else
                    {
                        abilitiesList[i].currentState = AbilityState.OnCooldown;
                        abilitiesList[i].onCooldownEvents.Invoke();
                        for (int j = 0; j < abilitiesList[i].abilitiesPlayingOnPress.Count; j++)
                             abilitiesList[i].cooldownTime = abilitiesList[i].abilitiesPlayingOnPress[j].cooldownTime;
                    }
                    break;
                case AbilityState.OnCooldown: // Ability is not currently usable
                    if (abilitiesList[i].cooldownTime > 0)
                    { abilitiesList[i].cooldownTime -= Time.deltaTime;  }
                    else
                    { abilitiesList[i].currentState = AbilityState.Ready; abilitiesList[i].onReadyEvents.Invoke(); }
                    break;
                case AbilityState.Ready: // Ability is ready to be used
                    if (Input.GetKeyDown(abilitiesList[i].keyPress) || Input.GetKeyUp(abilitiesList[i].keyRelease) || Input.GetKey(abilitiesList[i].KeyHold))
                    {
                        if (!RequiresAnotherTap(abilitiesList[i])) // check if our ability requires doubleTap
                            ActivateAbility(abilitiesList[i]); // activate ability
                    }
                    break;
                case AbilityState.Locked:
                    Debug.Log($"Ability: {abilitiesList[i].abilityNickname} - is marked as LOCKED and cannot be used until unlocked");
                    break;
                default:
                    Debug.Log($"ERROR: Ability State Issue for ability {abilitiesList[i].abilityNickname} - {transform.name} obj");
                    break;
            }           
        }
    }

    public bool RequiresAnotherTap(CustomAbilityOptions _abilityToActivate)
    {
        if (_abilityToActivate.doubleTapTimeTolerance == 0) // it does not require double tap
            return false;
        else // it does require double tap
        {
            // if our last key is the same as the key we need
            if (lastKeyPressed == _abilityToActivate.keyPress || lastKeyPressed == _abilityToActivate.keyRelease || lastKeyPressed == _abilityToActivate.KeyHold)
                if (Time.time < _abilityToActivate.doubleTapTime + _abilityToActivate.doubleTapTimeTolerance)// and the timer is close enough
                { return false; } // we can use our ability               

            if (_abilityToActivate.keyPress != KeyCode.None)
                lastKeyPressed = _abilityToActivate.keyPress;
            else if (_abilityToActivate.keyRelease != KeyCode.None)
                lastKeyPressed = _abilityToActivate.keyRelease;
            else if (_abilityToActivate.KeyHold != KeyCode.None)
                lastKeyPressed = _abilityToActivate.KeyHold;

            _abilityToActivate.doubleTapTime = Time.time;
            return true;
        }
    }

    public void ActivateAbility(CustomAbilityOptions _abilityToActivate) // TODO: Add a charge option
    {
        _abilityToActivate.currentState = AbilityState.Active;
        _abilityToActivate.onActivateEvents.Invoke();
        for (int j = 0; j < _abilityToActivate.abilitiesPlayingOnPress.Count; j++)
        {
            _abilityToActivate.abilitiesPlayingOnPress[j].Activate(playerObj);
            _abilityToActivate.activeTime = _abilityToActivate.abilitiesPlayingOnPress[j].activeTime;
        }
    }


}


// the custom data for abilities
[System.Serializable]
public class CustomAbilityOptions
{
    public string abilityNickname;

    [Range(0, 10)]
    public float doubleTapTimeTolerance; // if set to ZERO then we do NOT require double tap || 0.25f is a could roughh time
    [HideInInspector] public float doubleTapTime;

    public KeyCode keyPress, keyRelease, KeyHold;
    [HideInInspector] public float cooldownTime;
    [HideInInspector] public float activeTime;
    public AbilityState currentState = AbilityState.Ready;

    public List<Ability> abilitiesPlayingOnPress = new List<Ability>();

    public UnityEvent onActivateEvents, onCooldownEvents, onReadyEvents;

    //public CustomAbilityOptions(string _newName,)
    //{
    //    //abilityNickname = _newName;
    //}

}//end of data for abilities
