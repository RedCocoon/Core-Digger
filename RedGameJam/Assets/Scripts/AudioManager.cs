using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource sfxAudioSource;
    [SerializeField] AudioSource bgmAudioSource;
    [SerializeField] AudioSource omnidrillAudioSource;
    [SerializeField] float sfxPitchMin, sfxPitchMax;

    static AudioManager instance;

    public static AudioManager Instance => instance;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        
        PlayBgm();
    }

    public static void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            instance.sfxAudioSource.pitch = Random.Range(instance.sfxPitchMin, instance.sfxPitchMax);
            instance.sfxAudioSource.PlayOneShot(clip);
        }
    }
    public static void PlaySfxs(List<AudioClip> clips)
    {
        if (clips != null)
        {
            var chosenClip = clips[Random.Range(0, clips.Count)];
            instance.sfxAudioSource.pitch = Random.Range(instance.sfxPitchMin, instance.sfxPitchMax);
            instance.sfxAudioSource.PlayOneShot(chosenClip);
        }
    }

    public void PlayBgm()
    {
        bgmAudioSource.Play();
    }
    public void PlayDrillBgm()
    {
        bgmAudioSource.DOFade(0, 0.25f);
        omnidrillAudioSource.Play();
    }
    public void StopDrillBgm()
    {
        bgmAudioSource.DOFade(1, 0.25f);
        omnidrillAudioSource.Stop();
    }
}
