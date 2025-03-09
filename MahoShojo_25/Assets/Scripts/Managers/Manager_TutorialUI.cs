using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Manager_TutorialUI : MonoBehaviour
{
    public static Manager_TutorialUI Instance { get; private set; }

    public Image imgBearHolder;
    public TextMeshProUGUI speechText;
    public Sprite imgBearyNormal, imgBearySmirky;
    public string[] textToSayInOrder;

    private int textId;
    private float timeToKeepTextUp;
    private float timeSinceNotStuck;
    private float stuckWaitTime = 7;
    private bool playedJellyMonologue, playedBearMonologue;
    private float timeSinceCreatureSpawn;
    private float stuckCombatTime = 25;
    private int trackingSpawnedCreatures = 0;

    [Space]
    [Space]
    public Image[] capturedImages;

    [Space]
    [Space]
    public GameObject gameOverScreenObj;

    private void Awake()
    {
        if (Instance != null && Instance != this) // If there is an instance, and it's not me, delete myself.
            Destroy(this);
        else
            Instance = this;
    }


    private void Start()
    {
        ResetTutorial();
    }

    private void LateUpdate()
    {
        if (Manager_Platforms.Instance)
        {
            if (Manager_Platforms.Instance.isBlocked == false)
                timeSinceNotStuck = Time.time;

            if (Time.time > timeSinceNotStuck + stuckWaitTime && imgBearHolder.gameObject.activeSelf == false) // stuck on movement
            {
                ShowText(false, 2, false, 4);
                StartCoroutine(DelayMessage(false, 3, false, 4));
                StartCoroutine(DelayMessage(false, 3, false, 8));
                timeSinceNotStuck = Time.time;
            }


            if (trackingSpawnedCreatures != Manager_Platforms.Instance.monstersSpawned)
            { timeSinceCreatureSpawn = Time.time; trackingSpawnedCreatures = Manager_Platforms.Instance.monstersSpawned; }

            if(Time.time > timeSinceCreatureSpawn + stuckCombatTime && imgBearHolder.gameObject.activeSelf == false)  // in combat for a long time
            {
                ShowText(false, 8, false, 4);
                StartCoroutine(DelayMessage(false, 9, false, 4));
                timeSinceCreatureSpawn = Time.time;
            }


        }
        if (Manager_GameState.Instance)
        {
            if (Manager_GameState.Instance.capturedCreatues_Unique == 0 && Manager_Platforms.Instance && Manager_Platforms.Instance.monstersSpawned == 1 && !playedJellyMonologue && imgBearHolder.gameObject.activeSelf == false) // fighting mushroom lines
            {
                ShowText(false, 6, false, 8);
                StartCoroutine(DelayMessage(false, 7, false, 8));
                playedJellyMonologue = true;
            }

            if (Manager_GameState.Instance.capturedCreatues_Unique == 5 && !playedBearMonologue && imgBearHolder.gameObject.activeSelf == false) // fighting bear lines
            {
                ShowText(false, 10, true, 8);
                StartCoroutine(DelayMessage(false, 11, true, 8));
                playedBearMonologue = true;
            }

            
        }
    }

    public void SetCaptureShowcase(int _howManyCaptured)
    {
        for (int i = 0; i < capturedImages.Length; i++)
            if (i < _howManyCaptured)
                capturedImages[i].color = Color.white;
            else
                capturedImages[i].color = Color.black;
    }

    public void ResetTutorial()
    {
        textId = -1;
        HideText();
        SetCaptureShowcase(0);
    }

    public void HitStartTutorial()
    {
        StartCoroutine(DelayMessage(true, 0, false, 4));
        StartCoroutine(DelayMessage(true, 0, false, 8));        
    }

    public void ShowText(bool _nextOne, int _forcedTextLine, bool _showSmirkyBeary, float _timeToShowText)
    {
        if (_nextOne)
            textId++;
        else
            textId = _forcedTextLine;

        if (textToSayInOrder.Length == 0 || textId < 0 || textId > textToSayInOrder.Length) // error
            speechText.text = "Whoops ... I was just about to say something but I forgot... Sorry Maho somethings wrong with me lately.";
        else
            speechText.text = textToSayInOrder[textId];

        if (_showSmirkyBeary)
            imgBearHolder.sprite = imgBearySmirky;
        else
            imgBearHolder.sprite = imgBearyNormal;

        if (_timeToShowText != 0)
            timeToKeepTextUp = _timeToShowText;
        else
            timeToKeepTextUp = 4;
        imgBearHolder.gameObject.SetActive(true);
        StartCoroutine(DelayTextDisable());
    }

    public void HideText()
    {
        imgBearHolder.gameObject.SetActive(false);
        speechText.text = "";
    }

    private IEnumerator DelayTextDisable()
    {
        for(float i = timeToKeepTextUp; i >0; i--)
        {
            yield return new WaitForSeconds(1);
        }
        HideText();
    }

    private IEnumerator DelayMessage(bool _nextOne, int _forcedTextLine, bool _showSmirkyBeary, float _timeToShowText)
    {
        yield return new WaitForSeconds(_timeToShowText);
        ShowText(_nextOne, _forcedTextLine, _showSmirkyBeary, _timeToShowText);
    }


    public void ShowGameOverScreen()
    {
        gameOverScreenObj.SetActive(true);
    }

    public void SendRestartGame()
    {
        if (Manager_GameState.Instance)
        { gameOverScreenObj.SetActive(false); Manager_GameState.Instance.RestartGame(true); }
    }
}
