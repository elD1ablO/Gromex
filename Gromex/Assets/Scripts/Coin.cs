using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin type")]
    [Tooltip("If true, this is a fail (bad/red) coin. Otherwise this is a normal coin.")]
    [SerializeField] private bool _isFailCoin = false;
    public bool IsFailCoin => _isFailCoin;

    [Header("Normal coin FX (good coin)")]
    [Tooltip("Total duration of catch / miss effect in seconds for normal coins.")]
    [SerializeField] private float _normalEffectDuration = 0.25f;

    [Tooltip("How high the normal coin jumps when successfully caught.")]
    [SerializeField] private float _normalJumpHeight = 0.4f;

    [Tooltip("Shine location property (AllIn1 Sprite Shader → 24.Shine → Shine Location).")]
    [SerializeField] private string _shineLocationProp = "_ShineLocation";

    [SerializeField] private float _shineStart = 0f;
    [SerializeField] private float _shineEnd = 1f;
    [SerializeField] private float _shineSpeed = 2.0f;

    [Tooltip("Blur property for normal-coin miss (AllIn1 → 16.Blur → Blur Intensity).")]
    [SerializeField] private string _blurProp = "_BlurIntensity";

    [SerializeField] private float _blurStart = 0f;
    [SerializeField] private float _blurEnd = 1f;

    [Header("Fail coin FX (bad coin)")]
    [Tooltip("Total duration of effects for FAIL (bad) coins.")]
    [SerializeField] private float _failEffectDuration = 0.25f;

    [Tooltip("Fade property for FAIL coin miss (AllIn1 → 2.Fade → Fade Amount).")]
    [SerializeField] private string _failFadeProp = "_FadeAmount";

    [SerializeField] private float _failFadeStart = -0.1f;
    [SerializeField] private float _failFadeEnd = 1f;

    [Tooltip("Glitch property for FAIL coin catch (AllIn1 → 21.Glitch → Glitch Amount).")]
    [SerializeField] private string _failGlitchProp = "_GlitchAmount";

    [SerializeField] private float _failGlitchStart = 17f;
    [SerializeField] private float _failGlitchEnd = 17f;

    private SpriteRenderer _renderer;
    private Material _mat;
    private Collider2D[] _colliders;
    private Rigidbody2D _rb;

    private bool _effectPlaying;

    private void OnEnable()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer == null)
                _renderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (_renderer != null)
        {
            _mat = _renderer.material; // instance material

            var c = _renderer.color;
            _renderer.color = new Color(c.r, c.g, c.b, 1f); // reset alpha
        }

        if (_colliders == null || _colliders.Length == 0)
            _colliders = GetComponentsInChildren<Collider2D>();

        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        _effectPlaying = false;
    }

    // ---------- Public API for zones ----------

    // Normal coin: caught
    public void PlayCatchSuccessEffect()
    {
        if (_effectPlaying || _isFailCoin)
            return;

        _effectPlaying = true;
        DisablePhysics();
        StartCoroutine(NormalCatchRoutine());
    }

    // Normal coin: missed
    public void PlayMissEffect()
    {
        if (_effectPlaying || _isFailCoin)
            return;

        _effectPlaying = true;
        DisablePhysics();
        StartCoroutine(NormalMissRoutine());
    }

    // Fail coin: caught (bad catch)
    public void PlayFailCoinCaughtEffect()
    {
        if (_effectPlaying || !_isFailCoin)
            return;

        _effectPlaying = true;
        DisablePhysics();
        StartCoroutine(FailCoinGlitchRoutine());
    }

    // Fail coin: missed (let it pass)
    public void PlayFailCoinMissEffect()
    {
        if (_effectPlaying || !_isFailCoin)
            return;

        _effectPlaying = true;
        DisablePhysics();
        StartCoroutine(FailCoinFadeRoutine());
    }

    // ---------- Normal coin routines ----------

    private IEnumerator NormalCatchRoutine()
    {
        if (_renderer == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * (_normalJumpHeight * 2f);

        float t = 0f;

        if (_mat != null && _mat.HasProperty(_shineLocationProp))
            _mat.SetFloat(_shineLocationProp, _shineStart);

        Color startColor = _renderer.color;

        while (t < _normalEffectDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / _normalEffectDuration);

            // Jump with ease-out
            float eased = EaseOutQuad(k);
            transform.position = Vector3.LerpUnclamped(startPos, endPos, eased);

            // Shine sweep (faster due to _shineSpeed)
            if (_mat != null && _mat.HasProperty(_shineLocationProp))
            {
                float shineT = Mathf.Clamp01(k * _shineSpeed);
                float shineValue = Mathf.Lerp(_shineStart, _shineEnd, shineT);
                _mat.SetFloat(_shineLocationProp, shineValue);
            }

            // Fade-out on second half
            if (k >= 0.5f)
            {
                float fadeK = (k - 0.5f) / 0.5f;
                float alpha = Mathf.Lerp(1f, 0f, fadeK);
                _renderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator NormalMissRoutine()
    {
        if (_renderer == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float t = 0f;

        if (_mat != null && _mat.HasProperty(_blurProp))
            _mat.SetFloat(_blurProp, _blurStart);

        Color startColor = _renderer.color;

        while (t < _normalEffectDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / _normalEffectDuration);

            if (_mat != null && _mat.HasProperty(_blurProp))
            {
                float blur = Mathf.Lerp(_blurStart, _blurEnd, k);
                _mat.SetFloat(_blurProp, blur);
            }

            if (k >= 0.5f)
            {
                float fadeK = (k - 0.5f) / 0.5f;
                float alpha = Mathf.Lerp(1f, 0f, fadeK);
                _renderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    // ---------- Fail coin routines ----------

    private IEnumerator FailCoinGlitchRoutine()
    {
        if (_renderer == null)
        {
            Destroy(gameObject);
            yield break;
        }
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * (_normalJumpHeight * 1f);
        float t = 0f;
        Color startColor = _renderer.color;

        if (_mat != null && _mat.HasProperty(_failGlitchProp))
            _mat.SetFloat(_failGlitchProp, _failGlitchStart);

        while (t < _failEffectDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / _failEffectDuration);

            float eased = EaseOutQuad(k);
            transform.position = Vector3.LerpUnclamped(startPos, endPos, eased);

            if (_mat != null && _mat.HasProperty(_failGlitchProp))
            {                
                float amount = _failGlitchEnd;
                _mat.SetFloat(_failGlitchProp, amount);
            }

            if (k >= 0.5f)
            {
                float fadeK = (k - 0.5f) / 0.5f;
                float alpha = Mathf.Lerp(1f, 0f, fadeK);
                _renderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator FailCoinFadeRoutine()
    {
        if (_renderer == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float t = 0f;
        Color startColor = _renderer.color;

        if (_mat != null && _mat.HasProperty(_failFadeProp))
            _mat.SetFloat(_failFadeProp, _failFadeStart);

        while (t < _failEffectDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / _failEffectDuration);

            if (_mat != null && _mat.HasProperty(_failFadeProp))
            {
                float fade = Mathf.Lerp(_failFadeStart, _failFadeEnd, k);
                _mat.SetFloat(_failFadeProp, fade);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    // ---------- Helpers ----------

    private void DisablePhysics()
    {
        if (_colliders != null)
        {
            foreach (var col in _colliders)
            {
                if (col != null)
                    col.enabled = false;
            }
        }

        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.simulated = false;
        }
    }

    private float EaseOutQuad(float x)
    {
        return 1f - (1f - x) * (1f - x);
    }
}
