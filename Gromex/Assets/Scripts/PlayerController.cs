using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // New Input System
using UnityEngine.UI;          // For Button
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public static Action OnCoinCatch;
    public static Action OnFailCatch;

    [Header("Player sprites")]
    [SerializeField] private Sprite _idlePlayerSprite;
    [SerializeField] private Sprite _topLeftPlayerSprite;
    [SerializeField] private Sprite _topRightPlayerSprite;
    [SerializeField] private Sprite _bottomLeftPlayerSprite;
    [SerializeField] private Sprite _bottomRightPlayerSprite;

    [SerializeField] private SpriteRenderer _playerImage;

    [Header("Catch colliders (1-4)")]
    [SerializeField] private BoxCollider2D[] _catchColliders;
    // order: 1,2,3,4 as positions

    [Header("UI buttons for positions (1-4)")]
    [Tooltip("Order: 1,2,3,4 (same as positions)")]
    [SerializeField] private Button[] _positionButtons;

    [Tooltip("How long keyboard 'press' tint stays active.")]
    [SerializeField] private float _keyboardPressFlashDuration = 0.12f;

    // 0 = idle, 1..4 = active positions
    private int _currentPosition = 0;

    private void Awake()
    {
        // Auto-wire UI buttons to change position
        if (_positionButtons != null)
        {
            for (int i = 0; i < _positionButtons.Length; i++)
            {
                int pos = i + 1; // 1..4
                Button btn = _positionButtons[i];
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnPositionButtonClicked(pos));
                }
            }
        }
    }

    private void Start()
    {
        ResetToIdle();
    }

    private void Update()
    {
        UseKeyboardControls();
    }

    /// <summary>
    /// Resets player to idle sprite and disables all catch colliders.
    /// </summary>
    public void ResetToIdle()
    {
        _currentPosition = 0;

        if (_playerImage != null)
            _playerImage.sprite = _idlePlayerSprite;

        if (_catchColliders != null)
        {
            foreach (var col in _catchColliders)
            {
                if (col != null)
                    col.gameObject.SetActive(false);
            }
        }
    }

    // =======================
    //   UI BUTTON HANDLER
    // =======================
    private void OnPositionButtonClicked(int pos)
    {
        PlayerPosition(pos);
        TriggerKeyboardButtonFlash(pos - 1); // reuse same flash effect
    }

    // =======================
    //     KEYBOARD INPUT
    // =======================
    private void UseKeyboardControls()
    {
        var kb = Keyboard.current;
        if (kb == null)
            return;

        if (kb.qKey.wasPressedThisFrame)
        {
            SetPositionFromInput(1);
        }
        else if (kb.pKey.wasPressedThisFrame)
        {
            SetPositionFromInput(2);
        }
        else if (kb.aKey.wasPressedThisFrame)
        {
            SetPositionFromInput(3);
        }
        else if (kb.lKey.wasPressedThisFrame)
        {
            SetPositionFromInput(4);
        }
    }

    private void SetPositionFromInput(int pos)
    {
        PlayerPosition(pos);
        TriggerKeyboardButtonFlash(pos - 1);
    }

    /// <summary>
    /// Sets active position (1..4). 0 is reserved for idle and is not used here.
    /// </summary>
    public void PlayerPosition(int pos)
    {
        _currentPosition = Mathf.Clamp(pos, 1, 4);
        UpdateVisualsAndColliders();
    }

    private void UpdateVisualsAndColliders()
    {
        if (_playerImage == null)
            return;

        // sprite
        switch (_currentPosition)
        {
            case 1:
                _playerImage.sprite = _topLeftPlayerSprite;
                break;
            case 2:
                _playerImage.sprite = _topRightPlayerSprite;
                break;
            case 3:
                _playerImage.sprite = _bottomLeftPlayerSprite;
                break;
            case 4:
                _playerImage.sprite = _bottomRightPlayerSprite;
                break;
            default:
                _playerImage.sprite = _idlePlayerSprite;
                break;
        }

        // colliders
        if (_catchColliders != null)
        {
            for (int i = 0; i < _catchColliders.Length; i++)
            {
                if (_catchColliders[i] == null)
                    continue;

                // i == _currentPosition - 1  only one collider enabled, for 1..4
                bool shouldEnable = (i == _currentPosition - 1);
                _catchColliders[i].gameObject.SetActive(shouldEnable);
            }
        }
    }

    // =======================
    //  BUTTON FLASH EFFECT
    // =======================
    private void TriggerKeyboardButtonFlash(int buttonIndex)
    {
        if (_positionButtons == null || buttonIndex < 0 || buttonIndex >= _positionButtons.Length)
            return;

        Button btn = _positionButtons[buttonIndex];
        if (btn == null || btn.targetGraphic == null)
            return;

        StartCoroutine(FlashButtonColor(btn));
    }

    private IEnumerator FlashButtonColor(Button btn)
    {
        var colors = btn.colors;
        var originalColor = btn.targetGraphic.color;
        var pressedColor = colors.pressedColor;

        btn.targetGraphic.color = pressedColor;

        yield return new WaitForSeconds(_keyboardPressFlashDuration);

        btn.targetGraphic.color = originalColor;
    }
}

