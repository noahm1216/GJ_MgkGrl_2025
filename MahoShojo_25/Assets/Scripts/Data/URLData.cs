using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class URLData : MonoBehaviour
{
    private string url_Ninostudios = "https://www.ninostudios.com/";

    public void OpenWebsite_NinoStudios()
    {
        Application.OpenURL(url_Ninostudios);
    }
}
