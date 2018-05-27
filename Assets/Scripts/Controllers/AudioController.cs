using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public enum SoundtrackType
{
    Menu,
    Level,
    Minigame,
    Victory
}
public class AudioController : Singleton<AudioController>
{
    const string SFX_PARAM = "sfxVol",
        ST_PARAM = "stVol";
    const int MIN_VOLUME = -80;

    public AudioMixer mixer;
    public AudioClip[] tracks;
    public AnimationCurve fadeCurve;
    public AudioClip popup,
        book;
    public AudioClip[] tileSounds;
    public AudioSource loopingSource;
    [HideInInspector]
    public AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayLooping(AudioClip clip)
    {
        if (loopingSource.isPlaying)
            loopingSource.Stop();
        loopingSource.clip = clip;
        loopingSource.Play();
    }

    public void StopLooping()
    {
        loopingSource.Stop();
    }

    #region SoundtrackControl
    public void ChangeSFXVolume(Slider slider)
    {
        mixer.SetFloat(SFX_PARAM, MIN_VOLUME - slider.value * MIN_VOLUME);
    }

    public void ChangeSoundtrackVolume(Slider slider)
    {
        mixer.SetFloat(ST_PARAM, MIN_VOLUME - slider.value * MIN_VOLUME);
    }

    public void TransitionToSoundtrack(SoundtrackType track, float time)
    {
        StartCoroutine(TransitionRoutine(tracks[(int)track], time));
    }

    IEnumerator TransitionRoutine(AudioClip track, float totalTime)
    {
        float time = 0,
            max = totalTime / 2;
        // Lower the volume
        while (time < max)
        {
            time += Time.deltaTime;
            mixer.SetFloat(ST_PARAM, (time / max) * MIN_VOLUME);
            yield return null;
        }
        // Stop and switch track
        source.Stop();
        source.clip = track;
        source.Play();
        time = 0;
        // Raise the volume
        while (time < max)
        {
            time += Time.deltaTime;
            mixer.SetFloat(ST_PARAM, MIN_VOLUME - (time / max) * MIN_VOLUME);
            yield return null;
        }
    }
    #endregion
}