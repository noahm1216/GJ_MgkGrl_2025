using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SimpleAudioSlider : MonoBehaviour
{
    public Slider musicSlider, sfxSlider, atmosphereSlider, masterSlider;
    public AudioMixer aMixerMaster;
    

    // Start is called before the first frame update
    void Start()
    {
        UpdateVolume();
    }

    public void UpdateVolume()
    {   

        if (aMixerMaster)
        {
            if (musicSlider)
                aMixerMaster.SetFloat("Volume_Music", musicSlider.value);

            if (sfxSlider)
                aMixerMaster.SetFloat("Volume_SFX", sfxSlider.value);

            if (atmosphereSlider)
                aMixerMaster.SetFloat("Volume_Atmosphere", atmosphereSlider.value);

            if (masterSlider)
                aMixerMaster.SetFloat("Volume_Master", masterSlider.value);
        }

    }

 
}
