using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_UI : MonoBehaviour
{
    public static Manager_UI Instance { get; private set; }

    public GameObject[] objectsToHideOnStart, objectsToShowOnPlay;
    public GameObject pauseMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this) // If there is an instance, and it's not me, delete myself.
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (objectsToHideOnStart.Length > 0)
            for (int i = 0; i < objectsToHideOnStart.Length; i++)
                objectsToHideOnStart[i].SetActive(false);
    }

    public void EnableStartObjects()
    {
        if (objectsToShowOnPlay.Length > 0)
            for (int i = 0; i < objectsToShowOnPlay.Length; i++)
                objectsToShowOnPlay[i].SetActive(true);
    }

    public void ClickedHomeButton()
    {
        print("didnt set up the code for this yet"); // need to account for if game mode is Main Menu or if it is paused
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
}
