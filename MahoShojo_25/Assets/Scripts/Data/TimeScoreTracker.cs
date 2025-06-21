using UnityEngine;
using System;
using TMPro;

public class TimeScoreTracker : MonoBehaviour
{
    public Camera worldSpaceCameraForPostProcessing;
    public Canvas progressCanvas;

    public TextMeshProUGUI scoreText, timeText, distanceText;

    private void OnEnable()
    {
        if (!progressCanvas)
            TryGetComponent(out progressCanvas);
        if (!worldSpaceCameraForPostProcessing)
            worldSpaceCameraForPostProcessing = Camera.main;
        if (worldSpaceCameraForPostProcessing && progressCanvas && progressCanvas.renderMode != RenderMode.ScreenSpaceCamera && !progressCanvas.worldCamera)
            progressCanvas.worldCamera = worldSpaceCameraForPostProcessing; // if found references, renderMode is good, and no camera already assigned to it
    }

    // Update is called once per frame
    void Update()
    {
        if (Manager_GameState.Instance)
        {
            if (scoreText)
                scoreText.text = $"Score: {Manager_GameState.Instance.scoreTotal}";

            if (timeText)
                timeText.text = $"{ConvertTimeToTimer(Manager_GameState.Instance.timeOfCurrentGameRun)} min";
            //timeText.text = $"{Manager_GameState.Instance.timeOfCurrentGameRun.ToString("#.00")} seconds";

            if (distanceText)
                distanceText.text = $"{Manager_GameState.Instance.distanceOfCurrentGameRun.ToString("#.00")} m";
        }
    }

    public string ConvertTimeToTimer(float _time)
    {
        float intTime = _time;
        float minutes = intTime / 60;
        float seconds = intTime % 60;
        float fraction= _time * 1000;
        fraction = fraction % 1000; 
        return String.Format("{0:00}:{1:00}:{2:0}", minutes, seconds, fraction);
        // var result = (Mathf.Round(TimeTaken * 100)) / 100.0 || roundValue = Math.Round( notRoundValue, 2, MidpointRounding.AwayFromZero);
    }
}
