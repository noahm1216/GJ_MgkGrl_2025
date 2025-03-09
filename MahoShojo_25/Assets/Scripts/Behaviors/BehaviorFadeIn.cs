using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BehaviorFadeIn : MonoBehaviour
{

    public Image imgToFade;
    public RawImage rawImgToFade;
    private Color storedColor;

    private void OnEnable()
    {
        if (imgToFade)
            storedColor = imgToFade.color;
        if (rawImgToFade)
            storedColor = rawImgToFade.color;

        storedColor.a = 0;

        if (imgToFade)
            imgToFade.color = storedColor;
        if (rawImgToFade)
            rawImgToFade.color = storedColor;

        StartCoroutine(FadeIn());
    }

    private void OnDisable()
    {
        storedColor.a = 1.5f;

        if (imgToFade)
            imgToFade.color = storedColor;
        if (rawImgToFade)
            rawImgToFade.color = storedColor;
    }

    private IEnumerator FadeIn()
    {
        for (float i = 0; i <= 1.5f; i+= 0.1f)
        {
            storedColor.a = i;

            if (imgToFade)
                imgToFade.color = storedColor;
            if (rawImgToFade)
                rawImgToFade.color = storedColor;

                yield return new WaitForSeconds(0.05f);
        }
    }
}
