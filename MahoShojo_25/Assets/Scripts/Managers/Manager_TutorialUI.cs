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
    public List<CustomMessageData> messageLibrary = new List<CustomMessageData>();

    private float textDisplayTime = 8; // show time
    private float waitUntilNewTextTime = 2; // time until next one can show next one
    private float textDisplayTimeStamp;
    private List<CustomMessageData> queuedMessages = new List<CustomMessageData>();

    [Space]
    [Space]
    public Image[] capturedImages;

    [Space]
    [Space]
    public GameObject gameOverScreenObj;
    public GameObject gameWinScreenObj;

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
        // our condition to be able to play a message 
        if(queuedMessages.Count > 0 && imgBearHolder && imgBearHolder.gameObject.activeSelf == false && Time.time > textDisplayTimeStamp + waitUntilNewTextTime)
            PlayFromQueue();
    }

    public void QueueMessage(string _lookUpName)
    {
        CustomMessageData messageToAdd = null;

        messageToAdd = MessageLookup(_lookUpName);

        if (messageToAdd == null)
            return;
        else // add the message to the queue
            queuedMessages.Add(messageToAdd);
    }

    private void PlayFromQueue()
    {
        if (queuedMessages[0] != null)
            StartCoroutine(PlayQueueMessage(queuedMessages[0]));
        else
            Debug.Log("WARNING: Unable to play message from queue due to being null");
    }

    private void RemoveQueuedMessage(int _removeAtId)
    {
        if (queuedMessages.Count > _removeAtId)
            queuedMessages.RemoveAt(_removeAtId);
    }
    
    private CustomMessageData MessageLookup(string _lookUpName)
    {
        if (string.IsNullOrEmpty(_lookUpName) || messageLibrary.Count == 0)
        { Debug.Log($"WARNING: Empty/Null LookUp - {_lookUpName}"); return null; }

        for (int i = 0; i < messageLibrary.Count; i++) { // check our libray for the lookup code and additional data
            if (messageLibrary[i].lookUpName.ToLower() == _lookUpName.ToLower()) {
                if (messageLibrary[i].messageType == CustomMessageData.MessageType.OneTime)
                {
                    if (messageLibrary[i].timesPlayed < 1)
                    {
                        messageLibrary[i].timesPlayed++;
                        return messageLibrary[i];
                    }
                    else
                        return null;
                }
                else
                {
                    messageLibrary[i].timesPlayed++;
                    return messageLibrary[i];
                }
            }
        }


        Debug.Log($"WARNING: Unable To Lookup - {_lookUpName}");
        return null;

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
        queuedMessages.Clear();
        HideText();
        SetCaptureShowcase(0);
    }

    public void HitStartTutorial()
    {
        QueueMessage("Story_1");
        QueueMessage("Story_2");  
    }

    public void ShowText(CustomMessageData _queuedMessage)
    {

        if (string.IsNullOrEmpty(_queuedMessage.messageContent)) // error
            speechText.text = "Whoops ... I was just about to say something but I forgot... Sorry Maho somethings wrong with me lately.";
        else
            speechText.text = _queuedMessage.messageContent;
        
        if (_queuedMessage.useSneakyBearIcon)
            imgBearHolder.sprite = imgBearySmirky;
        else
            imgBearHolder.sprite = imgBearyNormal;

        if (Manager_GameState.Instance && Manager_GameState.Instance.capturedCreatues_Unique == 5) // NOTE: right now changing the bear to smirky once we are about to fight him or are fighting him
            imgBearHolder.sprite = imgBearySmirky;

        imgBearHolder.gameObject.SetActive(true);
    }


    public void HideText()
    {
        textDisplayTimeStamp = Time.time;
        imgBearHolder.gameObject.SetActive(false);
        speechText.text = "";
        RemoveQueuedMessage(0);
    }

    private IEnumerator PlayQueueMessage(CustomMessageData _queuedMessage)
    {
        ShowText(_queuedMessage);
        yield return new WaitForSeconds(textDisplayTime);
        HideText();
    }

    public void ShowGameOverScreen()
    {
        if(gameOverScreenObj)
        gameOverScreenObj.SetActive(true);
    }

    public void ShowWinGameScreen()
    {
        if(gameWinScreenObj)
        gameWinScreenObj.SetActive(true);
    }

    public void SendRestartGame()
    {
        if (Manager_GameState.Instance)
        { gameOverScreenObj.SetActive(false); Manager_GameState.Instance.RestartGame(true); }
    }
}




// the custom data for platforms
[System.Serializable]
public class CustomMessageData
{
    public enum MessageType { OneTime, Repeatable, DebugText, }


    public string lookUpName = "";    
    public MessageType messageType;
    public int timesPlayed;
    public bool useSneakyBearIcon;
    [TextArea]
    public string messageContent;


    //public CustomMessageData(string _newName,)
    //{
    //    //abilityNickname = _newName;
    //}

}//end of data for platforms
