using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume")]
    [Range(0f, 2f)]
    [SerializeField] private float _masterVolume = 1f;

    [Header("Collections")]
    [SerializeField] private SoundsCollectionSO _soundsCollection;

    [Header("Audio Mixers")]
    [SerializeField] private AudioMixerGroup _sfxMixer;
    [SerializeField] private AudioMixerGroup _musicMixer;

    [Header("Autoplay menu music on start")]
    [SerializeField] private bool _playMenuMusicOnStart = true;

    // Single music source – all background music goes here
    private AudioSource _musicSource;
    private Coroutine _musicFadeRoutine;

    // Default fade times (seconds)
    private const float FADE_OUT_TIME = 0.2f;
    private const float FADE_IN_TIME = 0.2f;

    #region Unity lifecycle

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Uncomment if you want this to persist between scenes:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Subscribe to coin events
        PlayerController.OnCoinCatch += OnGoodCoin;
        PlayerController.OnFailCatch += OnBadCoin;
    }

    private void OnDisable()
    {
        PlayerController.OnCoinCatch -= OnGoodCoin;
        PlayerController.OnFailCatch -= OnBadCoin;
    }

    private void Start()
    {
        if (_playMenuMusicOnStart)
            PlayMenuMusic();
    }

    #endregion

    #region Public API (used by UI / GameManager)

    /// <summary>
    /// Background music for main menu (1.ogg) with fade.
    /// </summary>
    public void PlayMenuMusic()
    {
        FadeToRandomMusic(_soundsCollection != null ? _soundsCollection.Menu : null);
    }

    /// <summary>
    /// Called when Time Mode game was chosen (button click).
    /// </summary>
    public void PlayTimeModeStart()
    {
        FadeToRandomMusic(_soundsCollection != null ? _soundsCollection.TimeMode : null);
    }

    /// <summary>
    /// Called when Lives Mode game was chosen (button click).
    /// </summary>
    public void PlayLivesModeStart()
    {
        FadeToRandomMusic(_soundsCollection != null ? _soundsCollection.LivesMode : null);
    }

    /// <summary>
    /// SFX for countdown 3-2-1 (321_go.ogg).
    /// </summary>
    public void PlayCountdown()
    {
        PlayRandomSfx(_soundsCollection != null ? _soundsCollection.Countdown321 : null);
    }

    /// <summary>
    /// Immediate stop for any playing background music.
    /// </summary>
    public void StopMusic()
    {
        if (_musicFadeRoutine != null)
        {
            StopCoroutine(_musicFadeRoutine);
            _musicFadeRoutine = null;
        }

        if (_musicSource != null)
            _musicSource.Stop();
    }

    #endregion

    #region Coin SFX

    private void OnGoodCoin()
    {
        PlayRandomSfx(_soundsCollection != null ? _soundsCollection.GoodCoin : null);
    }

    private void OnBadCoin()
    {
        PlayRandomSfx(_soundsCollection != null ? _soundsCollection.BadCoin : null);
    }

    #endregion

    #region Music logic (single source in music mixer + fade)

    private void FadeToRandomMusic(SoundSO[] sounds)
    {
        if (sounds == null || sounds.Length == 0)
            return;

        SoundSO sound = sounds[Random.Range(0, sounds.Length)];
        FadeToMusicSound(sound, FADE_OUT_TIME, FADE_IN_TIME);
    }

    private void FadeToMusicSound(SoundSO soundSO, float fadeOutDuration, float fadeInDuration)
    {
        if (soundSO == null || soundSO.audioClip == null)
            return;

        if (_musicFadeRoutine != null)
            StopCoroutine(_musicFadeRoutine);

        _musicFadeRoutine = StartCoroutine(FadeMusicRoutine(soundSO, fadeOutDuration, fadeInDuration));
    }

    private IEnumerator FadeMusicRoutine(SoundSO soundSO, float fadeOutDuration, float fadeInDuration)
    {
        // Create music source once
        if (_musicSource == null)
        {
            GameObject go = new GameObject("MusicSource");
            go.transform.SetParent(transform);
            _musicSource = go.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.outputAudioMixerGroup = _musicMixer;
            _musicSource.volume = 0f;
        }

        float startVolume = _musicSource.isPlaying ? _musicSource.volume : 0f;

        // Fade out current music
        if (_musicSource.isPlaying && fadeOutDuration > 0f && startVolume > 0f)
        {
            float t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.unscaledDeltaTime; // unscaled, so it works even if Time.timeScale = 0
                float lerp = Mathf.Clamp01(t / fadeOutDuration);
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, lerp);
                yield return null;
            }
        }

        _musicSource.Stop();
        _musicSource.volume = 0f;

        // Configure new clip
        _musicSource.clip = soundSO.audioClip;
        _musicSource.loop = soundSO.Loop;

        float pitch = soundSO.Pitch;
        if (soundSO.RandomizePitch)
        {
            float randomModifier =
                Random.Range(-soundSO.RandomPitchModifier, soundSO.RandomPitchModifier);
            pitch = soundSO.Pitch + randomModifier;
        }

        _musicSource.pitch = pitch;

        float targetVolume = soundSO.Volume * _masterVolume;

        _musicSource.Play();

        // Fade in new music
        if (fadeInDuration > 0f && targetVolume > 0f)
        {
            float t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.unscaledDeltaTime;
                float lerp = Mathf.Clamp01(t / fadeInDuration);
                _musicSource.volume = Mathf.Lerp(0f, targetVolume, lerp);
                yield return null;
            }
        }

        _musicSource.volume = targetVolume;
        _musicFadeRoutine = null;
    }

    #endregion

    #region SFX logic (short-lived sources in sfx mixer)

    private void PlayRandomSfx(SoundSO[] sounds)
    {
        if (sounds == null || sounds.Length == 0)
            return;

        SoundSO sound = sounds[Random.Range(0, sounds.Length)];
        PlaySfxSound(sound);
    }

    private void PlaySfxSound(SoundSO soundSO)
    {
        if (soundSO == null || soundSO.audioClip == null)
            return;

        GameObject go = new GameObject($"SFX_{soundSO.name}");
        go.transform.SetParent(transform);

        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;

        float pitch = soundSO.Pitch;
        if (soundSO.RandomizePitch)
        {
            float randomModifier =
                Random.Range(-soundSO.RandomPitchModifier, soundSO.RandomPitchModifier);
            pitch = soundSO.Pitch + randomModifier;
        }

        src.clip = soundSO.audioClip;
        src.loop = soundSO.Loop;
        src.pitch = pitch;
        src.volume = soundSO.Volume * _masterVolume;
        src.outputAudioMixerGroup = _sfxMixer;

        src.Play();

        if (!src.loop)
        {
            Destroy(go, src.clip.length + 0.1f);
        }
    }

    #endregion
}
