using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// does not handle errors and will break game if an index greater than the number of songs is called
/// </summary>
public class AudioController : MonoBehaviour
{
    [SerializeField] AudioSource[] musicArray;

    [SerializeField] AudioSource[] sFXArray;

    [SerializeField] bool playOnStart;

    // Start is called before the first frame update
    void Start()
    {
        if(musicArray.Length>0 && playOnStart) PlayMusic(0);
    }

    #region music functions
    public void PlayMusic(string name)
    {
        foreach (AudioSource audioSource in musicArray)
        {
            if (audioSource.name.Equals(name))
            {
                audioSource.Play();
            }
        }
    }

    public void PlayMusic(string name, bool fromStart)
    {
        foreach (AudioSource audioSource in musicArray)
        {
            if (audioSource.name.Equals(name))
            {
                if(fromStart) audioSource.Stop();
                audioSource.Play();
            }
        }
    }

    public void PlayMusic(int index)
    {
        musicArray[index].Play();
    }

    public void PlayMusic(int index, bool fromStart)
    {
        if (fromStart) musicArray[index].Stop();
        musicArray[index].Play();
    }

    public void PauseMusic(int index)
    {
        musicArray[index].Pause();
    }

    public void PauseAllMusic()
    {
        foreach(AudioSource audioSource in musicArray)
        {
            audioSource.Pause();
        }
    }

    public void StopMusic(int index)
    {
        musicArray[index].Stop();
    }

    public void StopAllMusic()
    {
        foreach (AudioSource audioSource in musicArray)
        {
            audioSource.Stop();
        }
    }

    public void RestartMusic(int index)
    {
        musicArray[index].Stop();
        musicArray[index].Play();
    }

    public void SetAllMusicVolume(float volume)
    {
        foreach (AudioSource audioSource in musicArray)
        {
            audioSource.volume = volume;
        }
    }

    public void SetMusicVolume(string name, float volume)
    {
        foreach (AudioSource audioSource in musicArray)
        {
            if (audioSource.name.Equals(name))
            {
                audioSource.volume = volume;
            }
        }
    }

    public void SetMusicVolume(int index, float volume)
    {
        musicArray[index].volume = volume;
    }
    #endregion

    #region SFX functions
    public void PlaySFX(string name)
    {
        foreach(AudioSource audioSource in sFXArray)
        {
            if (audioSource.name.Equals(name))
            {
                audioSource.Play();
            }
        }
    }

    public void PlaySFX(string name, bool fromStart)
    {
        foreach (AudioSource audioSource in sFXArray)
        {
            if (audioSource.name.Equals(name))
            {
                if(fromStart) audioSource.Stop();
                audioSource.Play();
            }
        }
    }

    public void PlaySFX(int index)
    {
        sFXArray[index].Play();
    }

    public void PlaySFX(int index, bool fromStart)
    {
        if (fromStart) sFXArray[index].Stop();
        sFXArray[index].Play();
    }

    public void PauseSFX(int index)
    {
        sFXArray[index].Pause();
    }

    public void PauseAllSFX()
    {
        foreach (AudioSource audioSource in sFXArray)
        {
            audioSource.Pause();
        }
    }

    public void StopSFX(int index)
    {
        sFXArray[index].Stop();
    }

    public void StopAllSFX()
    {
        foreach (AudioSource audioSource in sFXArray)
        {
            audioSource.Stop();
        }
    }

    public void RestartSFX(int index)
    {
        sFXArray[index].Stop();
        sFXArray[index].Play();
    }

    public void SetAllSfxVolume(float volume)
    {
        foreach (AudioSource audioSource in sFXArray)
        {
            audioSource.volume = volume;
        }
    }

    public void SetSFXVolume(string name, float volume)
    {
        foreach (AudioSource audioSource in sFXArray)
        {
            if (audioSource.name.Equals(name))
            {
                audioSource.volume = volume;
            }
        }
    }

    public void SetSFXVolume(int index, float volume)
    {
        sFXArray[index].volume = volume;
    }
    #endregion
}
