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
                    if (Input.GetKeyDown(abilitiesList[i].key))
                    {
                        //activate ability
                        abilitiesList[i].currentState = AbilityState.Active;
                        abilitiesList[i].onActivateEvents.Invoke();
                        for (int j = 0; j < abilitiesList[i].abilitiesPlayingOnPress.Count; j++)
                        {
                            abilitiesList[i].abilitiesPlayingOnPress[j].Activate(playerObj);
                            abilitiesList[i].activeTime = abilitiesList[i].abilitiesPlayingOnPress[j].activeTime;
                        }
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


}


// the custom data for abilities
[System.Serializable]
public class CustomAbilityOptions
{
    public string abilityNickname;
    public KeyCode key;
    [HideInInspector] public float cooldownTime;
    [HideInInspector] public float activeTime;
    public AbilityState currentState = AbilityState.Ready;
    public List<Ability> abilitiesPlayingOnPress = new List<Ability>();
    public UnityEvent onActivateEvents, onCooldownEvents, onReadyEvents;

    //public CustomAbilityOptions(int _newOrderNum,)
    //{
    //    //orderNumber = _newOrderNum;
    //}

}//end of data for abilities
