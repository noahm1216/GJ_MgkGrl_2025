using UnityEngine;

[CreateAssetMenu]
public class Ability_ChargeEvent : Ability
{
    public Transform chargeVisualPrefab;
    private AbilityHolder ref_AbilityHolder;
    private KeyCode holdingKey;

    public override void Activate(Transform _playerObj)
    {
        //base.Activate(); // if we also want to activate things in the original function

        if (!_playerObj)
        { Debug.Log("WARNING: Missing Player Reference for ability"); return; }

        if (!ref_AbilityHolder)
            _playerObj.TryGetComponent(out ref_AbilityHolder);

        if(!ref_AbilityHolder)
        { Debug.Log("WARNING: Missing Ability Reference for ability"); return; }

        foreach (CustomAbilityOptions cao in ref_AbilityHolder.abilitiesList)
            if (cao.abilityNickname == abilityName)
            { holdingKey = cao.KeyHold; break; }


        Debug.Log($"GOT KEY PRESS: {holdingKey}");

        //while (Input.GetKey(holdingKey))    
        //{
        //    Debug.Log($"HOLDING Key: {holdingKey}");
        //}

        // get the keycode to this ability
        // count a timer 
        // show visuals to reflect that timer
        // when we release that key
        // play an event of things

    }

    private void Update()
    {
        if (holdingKey != KeyCode.None && Input.GetKey(holdingKey))
            Debug.Log($"HOLDING Key: {holdingKey}");
    }
}
