using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class Countdown : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private GameObject _countdownGO;

    private bool _isRunning = false;

    /// <summary>
    /// Start countdown with no callback.
    /// </summary>
    public void StartCountdown()
    {
        StartCountdown(null);
    }

    /// <summary>
    /// Start countdown and call onComplete when finished.
    /// </summary>
    public void StartCountdown(Action onComplete)
    {
        if (!_isRunning)
            StartCoroutine(CountdownRoutine(onComplete));
    }

    private IEnumerator CountdownRoutine(Action onComplete)
    {
        _isRunning = true;

        if (_countdownGO != null)
            _countdownGO.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            if (_text != null)
                _text.text = i.ToString();

            // Set color depending on number
            if (_text != null)
            {
                if (i == 3) _text.color = Color.red;
                if (i == 2) _text.color = Color.yellow;
                if (i == 1) _text.color = Color.green;
            }

            // Reset scale + alpha
            if (_countdownGO != null)
                _countdownGO.transform.localScale = Vector3.one;

            if (_text != null)
            {
                Color cc = _text.color;
                cc.a = 1f;
                _text.color = cc;
            }

            // Animate scale 1 → 2 over 1 second
            float timer = 0f;
            float duration = 1f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);

                // Scale animation
                float scale = Mathf.Lerp(1f, 2f, t);
                if (_countdownGO != null)
                    _countdownGO.transform.localScale = new Vector3(scale, scale, scale);

                // Start fade-out after scale reaches 1.5 (complete fade by 1.95)
                if (_text != null && scale >= 1.5f)
                {
                    float fadeProgress = Mathf.InverseLerp(1.5f, 1.95f, scale);
                    float alpha = Mathf.Lerp(1f, 0f, fadeProgress);
                    Color fadeColor = _text.color;
                    fadeColor.a = alpha;
                    _text.color = fadeColor;
                }

                yield return null;
            }
        }

        if (_countdownGO != null)
            _countdownGO.SetActive(false);

        _isRunning = false;

        // invoke completion callback
        try
        {
            onComplete?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError("Countdown onComplete threw exception: " + e);
        }
    }
}
