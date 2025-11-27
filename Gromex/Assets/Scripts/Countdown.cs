using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class Countdown : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private GameObject _countdownGO;

    private bool _isRunning = false;

    public void StartCountdown()
    {
        StartCountdown(null);
    }

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

        Color32 mainColor = new Color32(0x6C, 0xCF, 0xE2, 255); // #6CCFE2

        for (int i = 3; i > 0; i--)
        {
            if (_text != null)
            {
                _text.text = i.ToString();
                _text.color = mainColor;
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

            float timer = 0f;
            float duration = 1f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);

                float scale = Mathf.Lerp(1f, 2f, t);
                if (_countdownGO != null)
                    _countdownGO.transform.localScale = new Vector3(scale, scale, scale);

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
