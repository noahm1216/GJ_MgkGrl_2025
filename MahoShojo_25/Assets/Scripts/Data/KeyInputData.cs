using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class KeyInputData : MonoBehaviour
{
    public bool changingNow { get; private set; }
    public KeyCode theKeycode { get; private set; }
    public KeyCode lastKeycode { get; private set; }
    public TextMeshProUGUI keycodeText, keycodeActionText;
    public UnityEvent changedKeycodeEvent;


    public void FillOutTheData(KeyCode _newKeycode, string _keyPurpose, bool _invokeChangeFx)
    {
        if (theKeycode != KeyCode.None)
            lastKeycode = theKeycode;
        theKeycode = _newKeycode;

        //if (keycodeText)
        //    keycodeText.text = _newKeycode.ToString(); // this isnt working for some reason

        if (keycodeText)
        {
            print($"updating keycode text {_newKeycode}");
            //keycodeText.gameObject.SetActive(false);
            keycodeText.text = $"{_newKeycode}";
            //keycodeText.gameObject.SetActive(true);
            //keycodeText.ForceMeshUpdate();
        }

        if (keycodeActionText)
            keycodeActionText.text = _keyPurpose;

        if (_invokeChangeFx)
            changedKeycodeEvent.Invoke();
    }

    public void TryingToChange(bool _isChanging)
    {     
        changingNow = _isChanging;

        if (changingNow && keycodeActionText)
        {
            if (keycodeText)
                keycodeText.text = "PRESS ANY KEY";
            if (Manager_UI.Instance)
                Manager_UI.Instance.TryingToChangeKeyInput();
        }       
    }

}
