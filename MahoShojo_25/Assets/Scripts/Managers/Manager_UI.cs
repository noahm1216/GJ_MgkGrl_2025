using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_UI : MonoBehaviour
{
    public static Manager_UI Instance { get; private set; }

    public GameObject[] objectsToHideOnStart, objectsToShowOnPlay;
    public GameObject pauseMenu;

    public KeyInputData[] keybindings;
    public PlayerCore ref_PlayerCore;
    public ChargingPocki ref_ChargingPocki;
    private bool detectingKeyInput;


    private void Awake()
    {
        if (Instance != null && Instance != this) // If there is an instance, and it's not me, delete myself.
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        // set references to start data || TODO: Eventually we want to pull this data from a list or save file
        if (ref_PlayerCore)
            EstablishInitialKeys(ref_PlayerCore.key_MoveUp, "jump");
        if (Manager_GameState.Instance)
        {
            EstablishInitialKeys(Manager_GameState.Instance.key_Pause1, "pause 1");
            EstablishInitialKeys(Manager_GameState.Instance.key_Pause2, "pause 2");
            EstablishInitialKeys(Manager_Platforms.Instance.key_MovePlatformsLeft, "dash");
            EstablishInitialKeys(Manager_Platforms.Instance.key_MovePlatformsRight, "retract");
        }

        if (ref_ChargingPocki)
            EstablishInitialKeys(ref_ChargingPocki.keyToCharge, "shoot");

        detectingKeyInput = false;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (objectsToHideOnStart.Length > 0)
            for (int i = 0; i < objectsToHideOnStart.Length; i++)
                if (objectsToHideOnStart[i] != null)
                    objectsToHideOnStart[i].SetActive(false);

        detectingKeyInput = false;
    }

    private void LateUpdate()
    {
        if (detectingKeyInput)
        {
            //print("waiting for new key input");
            var newKey = DetectInput();
            if (newKey != KeyCode.None)
            {
                for (int i = 0; i < keybindings.Length; i++)
                {
                    if (keybindings[i].changingNow)
                    {
                        //print($"Found key that needs changing: {keybindings[i].keycodeActionText.text} - changing key from: {keybindings[i].theKeycode} (previously {keybindings[i].lastKeycode}) - to: {newKey}");
                        keybindings[i].FillOutTheData(newKey, keybindings[i].keycodeActionText.text, false);
                        keybindings[i].TryingToChange(false);
                        UpdateGameControls(newKey, keybindings[i].keycodeActionText.text.ToLower());
                        detectingKeyInput = false;
                        break;
                    }
                }
            }
        }
    }

    private void EstablishInitialKeys(KeyCode _newKey, string _keyDescription)
    {
        for (int i = 0; i < keybindings.Length; i++)
        {
            if (keybindings[i].keycodeActionText.text.ToLower() == _keyDescription.ToLower())
            {
                //print($"Found key that needs establishing: {_newKey} - {_keyDescription}");
                keybindings[i].FillOutTheData(_newKey, _keyDescription, false);
                keybindings[i].TryingToChange(false);
                UpdateGameControls(_newKey, keybindings[i].keycodeActionText.text.ToLower());             
                break;
            }
        }
    }

    private void UpdateGameControls(KeyCode _newKey, string _keyDescription) // updates unique objects to use the new controls
    {
        if (string.IsNullOrEmpty(_keyDescription))
            return;

        switch (_keyDescription)
        {
            case "jump":
                if (ref_PlayerCore)
                    ref_PlayerCore.key_MoveUp = _newKey;
                break;
            case "pause 1":
                if (Manager_GameState.Instance)
                {
                    if (_newKey == KeyCode.Mouse0)
                    { Debug.Log("Please Dont Set Pause Key To: 'Mouse0'"); EstablishInitialKeys(Manager_GameState.Instance.key_Pause1, "pause 1"); }
                    else
                        Manager_GameState.Instance.key_Pause1 = _newKey;
                }
                break;
            case "pause 2":
                if (Manager_GameState.Instance)
                {
                    if (_newKey == KeyCode.Mouse0)
                    { Debug.Log("Please Dont Set Pause Key To: 'Mouse0'"); EstablishInitialKeys(Manager_GameState.Instance.key_Pause2, "pause 2"); }
                    else
                        Manager_GameState.Instance.key_Pause2 = _newKey;
                }
                break;
            case "dash":
                if (Manager_Platforms.Instance)
                    Manager_Platforms.Instance.key_MovePlatformsLeft = _newKey;
                break;
            case "retract":
                if (Manager_Platforms.Instance)
                    Manager_Platforms.Instance.key_MovePlatformsRight = _newKey;
                break;
            case "shoot":
                if (ref_ChargingPocki)
                    ref_ChargingPocki.keyToCharge = _newKey;
                break;
            default:
                Debug.Log($"ERROR: Missing keybinding text for: {_keyDescription}");
                break;
        }
    }


    public void EnableStartObjects()
    {
        if (objectsToShowOnPlay.Length > 0)
            for (int i = 0; i < objectsToShowOnPlay.Length; i++)
                objectsToShowOnPlay[i].SetActive(true);
    }

    public void ClickedHomeButton()
    {
        //print("didnt set up the code for this yet"); // need to account for if game mode is Main Menu or if it is paused
        if (Manager_GameState.Instance && Manager_GameState.Instance.currentState == Manager_GameState.GAMESTATE.Paused)
        {
            Manager_GameState.Instance.RestartGame(true);
        }
    }

    public void ClickedResumeButton()
    {
        if (Manager_GameState.Instance)
            Manager_GameState.Instance.PauseToggle();
    }

    public void PauseToggle()
    {
        if (pauseMenu)
            pauseMenu.SetActive(!pauseMenu.activeSelf);
    }

    public void TryingToChangeKeyInput()
    {
        if (!detectingKeyInput)
            detectingKeyInput = true;
    }

    public KeyCode DetectInput()
    {
        foreach (KeyCode vkey in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(vkey))
                return vkey; //print($"Changed Key To: {vkey}");
        }
        return KeyCode.None;
    }
}
