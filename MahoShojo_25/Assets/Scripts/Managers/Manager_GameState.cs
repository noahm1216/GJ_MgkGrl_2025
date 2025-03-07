using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_GameState : MonoBehaviour
{
    public static Manager_GameState Instance { get; private set; }

    public enum GAMESTATE {Menu, Playing, Paused, Lost, Won }
    public GAMESTATE currentState { get; private set; }

    public int scoreTotal; // the score we get from capturing

    public KeyCode keyToPause1 = KeyCode.P, keyToPause2 = KeyCode.Escape;


    private void Awake()
    {
        if (Instance != null && Instance != this) // If there is an instance, and it's not me, delete myself.
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    public void ChangeState(GAMESTATE _newState)
    {     
        switch (_newState)
        {
            case GAMESTATE.Menu:
                // load main menu canvas and scene
                break;
            case GAMESTATE.Playing:
                // update any code so we can begin early tutorial
                break;
            case GAMESTATE.Paused:
                // pause the game and open the menu for pause
                break;
            case GAMESTATE.Lost:
                // show the screen when we lose and offer restart || or go after X time
                break;
            case GAMESTATE.Won:
                // show end game cutscene
                break;
            default:
                Debug.Log($"WARNING: Case for Gamestate '{currentState}' - not found");
                break;
        }
        currentState = _newState;
    }

    // Update is called once per frame
    private void Update()
    {
        StateChecker();
    }


    private void StateChecker()
    {
        switch (currentState)
        {
            case GAMESTATE.Menu:
                break;
            case GAMESTATE.Playing:
                break;
            case GAMESTATE.Paused:
                break;
            case GAMESTATE.Lost:
                break;
            case GAMESTATE.Won:
                break;
            default:
                Debug.Log($"WARNING: Case for Gamestate '{currentState}' - not found");
                break;
        }
    }
}
