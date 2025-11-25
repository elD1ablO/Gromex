using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPlay : MonoBehaviour
{
    public static AudioPlay Instance;

    [Range(0f, 2f)]
    [SerializeField] private float _masterVolume = 1f;

    [SerializeField] private SoundsCollectionSO _soundsCollectionSO;

    [Header("AudioMixers")]
    [SerializeField] private AudioMixerGroup _sfxMixer;
    [SerializeField] private AudioMixerGroup _musicMixer;

    private AudioSource _musicSource;

    #region Unity Methods
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        //PlayerController.OnJetpack += PlayerController_OnJetpack;
        //Gun.OnShoot += Gun_OnShoot;
        //Gun.OnGrenade += Gun_OnGrenade;
        //PlayerController.OnJump += PlayerController_OnJump;
        //Health.OnDeath += Health_OnDeath;
        //DiscoBallManager.OnDiscoBallHitEvent += DiscoBallMusic;
    }


    private void OnDisable()
    {
        //PlayerController.OnJetpack -= PlayerController_OnJetpack;
        //Gun.OnShoot -= Gun_OnShoot;
        //Gun.OnGrenade -= Gun_OnGrenade;

        //PlayerController.OnJump -= PlayerController_OnJump;
        //Health.OnDeath -= Health_OnDeath;
        //DiscoBallManager.OnDiscoBallHitEvent -= DiscoBallMusic;
    }
    private void Start()
    {
        PlayRandomSound(_soundsCollectionSO.Music);
    }
    #endregion

    #region SoundMethods
    private void PlayRandomSound(SoundSO[] sounds)
    {
        if (sounds != null && sounds.Length > 0)
        {
            SoundSO soundSO = sounds[UnityEngine.Random.Range(0, sounds.Length)];
            SoundToPlay(soundSO);
        }
    }
    private void SoundToPlay(SoundSO soundSO)
    {
        AudioClip clip = soundSO.audioClip;
        float pitch = soundSO.Pitch;
        float volume = soundSO.Volume * _masterVolume;
        bool loop = soundSO.Loop;

        AudioMixerGroup audioMixerGroup = soundSO.audioType == SoundSO.AutioTypes.SFX ? _sfxMixer : _musicMixer;

        pitch = RandomizePitch(soundSO, pitch);

        PlaySound(clip, pitch, volume, loop, audioMixerGroup);
    }

    private static float RandomizePitch(SoundSO soundSO, float pitch)
    {
        if (soundSO.RandomizePitch)
        {
            float randomModifier = UnityEngine.Random.Range(-soundSO.RandomPitchModifier, soundSO.RandomPitchModifier);
            pitch = soundSO.Pitch + randomModifier;
        }

        return pitch;
    }

    private void PlaySound(AudioClip clip, float pitch, float volume, bool loop, AudioMixerGroup audioMixerGroup)
    {
        GameObject soundObject = new GameObject("TempSource");
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.outputAudioMixerGroup = audioMixerGroup;
        audioSource.Play();

        if (!loop)
        {
            Destroy(soundObject, clip.length);
        }

        if(audioMixerGroup == _musicMixer)
        {
            if(_musicSource != null)
            {
                _musicSource.Stop();               
            }
            _musicSource = audioSource;
        }
    }
    #endregion

    #region SFX
    private void Gun_OnShoot()
    {
        PlayRandomSound(_soundsCollectionSO.GunShot);
    }
    private void PlayerController_OnJump()
    {
        PlayRandomSound(_soundsCollectionSO.Jump);
    }
    //private void Health_OnDeath(Health health)
    //{
    //    PlayRandomSound(_soundsCollectionSO.Splat);
    //}
    private void PlayerController_OnJetpack()
    {
        PlayRandomSound(_soundsCollectionSO.Jetpack);
    }
    public void Grenade_OnBeep()
    {
        PlayRandomSound(_soundsCollectionSO.GrenadeBeep);
    }
    public void Grenade_OnExplode()
    {
        PlayRandomSound(_soundsCollectionSO.GrenadeExplode);
    }
    private void Gun_OnGrenade()
    {
        PlayRandomSound(_soundsCollectionSO.GrenadeShoot);
    }

    public void Enemy_OnPlayerHit()
    {
        PlayRandomSound(_soundsCollectionSO.PlayerHit);
    }
    #endregion

    #region Music
    private void FightMusic()
    {
        PlayRandomSound(_soundsCollectionSO.Music);
    }
    private void DiscoBallMusic()
    {
        PlayRandomSound(_soundsCollectionSO.DiscoBall);
        Invoke("FightMusic", 3f);
    }
    
    #endregion
}
