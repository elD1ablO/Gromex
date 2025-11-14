using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioMixerGroup _sfxMixer;

    [Header("Clips")]
    [SerializeField] private AudioClip _coinCatch;
    [SerializeField] private AudioClip _coinFail;

    private void Start()
    {
        PlayerController.OnCoinCatch += PlayCatchCoinSound;
        PlayerController.OnFailCatch += PlayCoinFailSound;
    }

    private void PlayCoinFailSound()
    {
        _audioSource.outputAudioMixerGroup = _sfxMixer;
        _audioSource.PlayOneShot(_coinFail);
    }

    private void PlayCatchCoinSound()
    {
        _audioSource.outputAudioMixerGroup = _sfxMixer;
        _audioSource.PlayOneShot(_coinCatch);
    }
}

