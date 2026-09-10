using UnityEngine;
using System.Collections.Generic;

public class Manager_Audio : MonoBehaviour
{
    public static Manager_Audio Instance { get; private set; }

    public SimpleAudioSlider ref_SimpleAudioSlider;

    public AudioSource aSourceMusic, aSourceSFX, aSourceAtmosphere; // generic audio sources to play things through

    public AudioClip clipMusic_Gameplay, clipMusic_Win;

    public AudioClip[] clipMusic_GameplayList;

    public float[] pitchRanges = { 1, 1.26f, 1.5f, 2, 2.52f, 3 };

    private void Awake()
    {
        if (Instance != null && Instance != this) // If there is an instance, and it's not me, delete myself.
            Destroy(this);
        else
            Instance = this;
    }

    public void PlaySfxFromListRandom(AudioClip[] _clipsList)
    {
        if (_clipsList == null || _clipsList.Length == 0 || aSourceSFX == null) return;

    }

    public void PlayOneShotPitched(AudioClip _clip, AudioSource _aSource, float _pitch = 1)
    {
        if (!_clip || !_aSource) return;       
        
        //_pitch = Mathf.Clamp(_pitch, pitchRanges[0], pitchRanges[pitchRanges.Length-1]); // clamp the pitch within the range
        _aSource.pitch = _pitch;
        _aSource.PlayOneShot(_clip);
    }

    public void PickRandomGameplaySong()
    {
        if (!aSourceMusic) return;

        if(clipMusic_GameplayList.Length > 1)
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
        else
        {
            if (clipMusic_GameplayList.Length == 1)
            {
                aSourceMusic.clip = clipMusic_GameplayList[0];
                aSourceMusic.Play();
            }
            else
            {
                Debug.LogError($"MISSING MUSIC TO PLAY: on {transform.name}");
                aSourceMusic.Stop();
            }
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
