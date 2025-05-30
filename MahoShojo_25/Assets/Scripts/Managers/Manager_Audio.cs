using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Manager_Audio : MonoBehaviour
{
    public static Manager_Audio Instance { get; private set; }

    public SimpleAudioSlider ref_SimpleAudioSlider;

    public AudioSource aSourceMusic, aSourceSFX, aSourceAtmosphere; // generic audio sources to play things through

    public AudioClip clipMusic_Gameplay, clipMusic_Win;

    private void Awake()
    {
        if (Instance != null && Instance != this) // If there is an instance, and it's not me, delete myself.
            Destroy(this);
        else
            Instance = this;
    }

    public void SwitchClip(AudioSource _chosenSource, AudioClip _newSong)
    {
        print("Switgch Audio");

        if (_newSong && _chosenSource)
        { _chosenSource.clip = _newSong; _chosenSource.Play(); }
    }

}
