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
