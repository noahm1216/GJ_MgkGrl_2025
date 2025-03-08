using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager_GameState : MonoBehaviour
{
    public static Manager_GameState Instance { get; private set; }

    public enum GAMESTATE {Menu, Playing, Paused, Lost, Won }
    public GAMESTATE currentState;// { get; private set; }
    private GAMESTATE stateWhenPaused;

    public int scoreTotal; // the score we get from capturing

    public KeyCode key_Pause1 = KeyCode.P, key_Pause2 = KeyCode.Escape;

    public Transform[] objectsToResetPositions;
    private Vector3[] startPositions, startScales;
    private Quaternion[] startRotations;
    private bool[] wasEnabled;
   [HideInInspector] public List<Transform> objectsSpawnedDuringRuntime = new List<Transform>();


    public int capturedCreatues_Unique { get; private set; }
    public int capturedPoints { get; private set; }

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
        SaveObjectDataForRestart();
    }

    public void StartGameButton()
    {
        ChangeState(GAMESTATE.Playing);
    }

    public void RestartGameButton()
    {
        RestartGame(false);
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

    public void RestartGame(bool _totalRestart)
    {
        PauseToggle();
        ChangeState(GAMESTATE.Menu);
        CaptureChange(-capturedCreatues_Unique, -capturedPoints);

        if (_totalRestart)
        {          
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
        else
        {           
            LoadbjectDataForRestart();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        StateChecker();
        CheckForInputs();
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

    public void CheckForInputs()
    {
        if(Input.GetKeyDown(key_Pause1) || Input.GetKeyDown(key_Pause2)) // pause toggle
        {
            PauseToggle();
        }
    }

    public void PauseToggle()
    {
        if (currentState != GAMESTATE.Paused)
        {
            stateWhenPaused = currentState;
            currentState = GAMESTATE.Paused;
            if (Manager_UI.Instance)
                Manager_UI.Instance.PauseToggle();

            Time.timeScale = 0;
        }
        else
        {
            currentState = stateWhenPaused;
            if (Manager_UI.Instance)
                Manager_UI.Instance.PauseToggle();

            Time.timeScale = 1;
        }
    }

    public void CaptureChange(int _amountChange, int _changePoints)
    {
        capturedCreatues_Unique += _amountChange;
        capturedPoints += _changePoints;

        if (Manager_TutorialUI.Instance)
        { Manager_TutorialUI.Instance.SetCaptureShowcase(capturedCreatues_Unique); }
    }








    public void SaveObjectDataForRestart()
    {
        if (objectsToResetPositions.Length > 0) // store references for fast restarts
        {
            startPositions = new Vector3[objectsToResetPositions.Length];
            startScales = new Vector3[objectsToResetPositions.Length];
            startRotations = new Quaternion[objectsToResetPositions.Length];
            wasEnabled = new bool[objectsToResetPositions.Length];

            for (int i = 0; i < objectsToResetPositions.Length; i++)
            {
                startPositions[i] = objectsToResetPositions[i].position;
                startScales[i] = objectsToResetPositions[i].localScale;
                startRotations[i] = objectsToResetPositions[i].rotation;
                wasEnabled[i] = objectsToResetPositions[i].gameObject.activeSelf;
            }
        }
    }

    public void LoadbjectDataForRestart()
    {
        if (objectsSpawnedDuringRuntime.Count > 0)
        {
            Transform deleteRef = null;
            for (int i = 0; i < objectsSpawnedDuringRuntime.Count; i++)
            {
                if (objectsSpawnedDuringRuntime[i] != null)
                {
                    deleteRef = objectsSpawnedDuringRuntime[i];
                    objectsSpawnedDuringRuntime.RemoveAt(i);
                    Destroy(deleteRef.gameObject);
                }               
            }

        }     

        if (objectsToResetPositions.Length > 0) // retrieve references for fast restarts
        {           
            for (int i = 0; i < objectsToResetPositions.Length; i++)
            {
                objectsToResetPositions[i].position = startPositions[i];
                objectsToResetPositions[i].localScale = startScales[i];
                objectsToResetPositions[i].rotation = startRotations[i];
                objectsToResetPositions[i].gameObject.SetActive(wasEnabled[i]);
            }
        }

        StartCoroutine(ClearReferences());
    }

    private IEnumerator ClearReferences()
    {
        yield return new WaitForSeconds(0.5f);

        if (Manager_Platforms.Instance)
            Manager_Platforms.Instance.RemoveAnyNullPlatforms();
    }
}
