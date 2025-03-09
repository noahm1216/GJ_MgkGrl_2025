using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SimpleMusicSlider : MonoBehaviour
{

    public Slider musicSlider;
    public AudioSource aSource;

    // Start is called before the first frame update
    void Start()
    {
        UpdateVolume();
    }

    public void UpdateVolume()
    {
        aSource.volume = musicSlider.value;
    }
}
