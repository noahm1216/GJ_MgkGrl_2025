using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BehaviorFadeIn : MonoBehaviour
{

    public Image imgToFade;
    private Color storedColor;

    private void OnEnable()
    {
        print("Enabled");
        storedColor = imgToFade.color;
        storedColor.a = 0;
        imgToFade.color = storedColor;
        StartCoroutine(FadeIn());
    }

    private void OnDisable()
    {
        storedColor.a = 1.5f;
        imgToFade.color = storedColor;
    }

    private IEnumerator FadeIn()
    {
        for (float i = 0; i <= 1.5f; i+= 0.1f)
        {
            storedColor.a = i;
            imgToFade.color = storedColor;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
