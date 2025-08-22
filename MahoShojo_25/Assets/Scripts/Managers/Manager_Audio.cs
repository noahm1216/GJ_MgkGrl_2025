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

    public AudioClip[] clipMusic_GameplayList;

    private void Awake()
    {
        if (Instance != null && Instance != this) // If there is an instance, and it's not me, delete myself.
            Destroy(this);
        else
            Instance = this;
    }

    public void PickRandomGameplaySong()
    {
        if(clipMusic_GameplayList.Length > 0)
        {
            int ranId = Random.Range(0, clipMusic_GameplayList.Length);            
            if (clipMusic_GameplayList[ranId] && aSourceMusic.clip != clipMusic_GameplayList[ranId])
            {
                aSourceMusic.clip = clipMusic_GameplayList[ranId];
                aSourceMusic.Play();
            }
            else
                PickRandomGameplaySong();
        }
    }

    public void SwitchClip(AudioSource _chosenSource, AudioClip _newSong)
    {
        print("Switgch Audio");

        if (_newSong && _chosenSource)
        { _chosenSource.clip = _newSong; _chosenSource.Play(); }
    }

    public void LateUpdate()
    {
        if(aSourceMusic && !aSourceMusic.loop &&  !aSourceMusic.isPlaying)
        {
            PickRandomGameplaySong();
        }
    }

}
