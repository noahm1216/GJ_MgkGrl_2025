using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class KeyInputData : MonoBehaviour
{
    public bool changingNow { get; private set; }
    public KeyCode theKeycode { get; private set; }
    public TextMeshProUGUI keycodeText, keycodeActionText;
    public UnityEvent changedKeycodeEvent;


    public void FillOutTheData(KeyCode _newKeycode, string _keyPurpose)
    {
        theKeycode = _newKeycode;

        //if (keycodeText)
        //    keycodeText.text = _newKeycode.ToString(); // this isnt working for some reason

        if (keycodeText != null)
        {
            string keycodeToString = _newKeycode.ToString();
            keycodeText.text = keycodeToString;
        }

        if (keycodeActionText)
            keycodeActionText.text = _keyPurpose;

        changedKeycodeEvent.Invoke();
    }

    public void TryingToChange(bool _isChanging)
    {
        changingNow = _isChanging;

        if (changingNow && keycodeActionText)
        {
            keycodeText.text = "...";
            if (Manager_UI.Instance)
                Manager_UI.Instance.TryingToChangeKeyInput();
        }       
    }

}
