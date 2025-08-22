using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class URLData : MonoBehaviour
{
    private string url_Ninostudios = "https://www.ninostudios.com/";
    private string url_MahoSurvey = "https://docs.google.com/forms/d/e/1FAIpQLSdF19dAjtBQMn1alMDyhhqd9fhULk3Q-Lg-zjUAD9K-JuRDMA/viewform?usp=dialog";

    public void OpenWebsite_NinoStudios()
    {
        Application.OpenURL(url_Ninostudios);
    }

    public void OpenWebsite_MahoSurvey()
    {
        Application.OpenURL(url_MahoSurvey);
    }

}
