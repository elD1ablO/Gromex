using System;
using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class PlayerController : MonoBehaviour
{
    public static Action OnCoinCatch;
    public static Action OnFailCatch;

    [Header("Player sprites")]
    [SerializeField] private Sprite _topLeftPlayerSprite;
    [SerializeField] private Sprite _topRightPlayerSprite;
    [SerializeField] private Sprite _bottomLeftPlayerSprite;
    [SerializeField] private Sprite _bottomRightPlayerSprite;
    [SerializeField] private SpriteRenderer _playerImage;

    [Header("Catch colliders (1-4)")]
    [SerializeField] private BoxCollider2D[] _catchColliders;
    // order: 1,2,3,4 as positions

    private int _currentPosition = 1;

    private void Start()
    {
        UpdateVisualsAndColliders();
    }

    private void Update()
    {
        bool flowControl = UseKeyboardControls();
        if (!flowControl)
        {
            return;
        }
    }

    private bool UseKeyboardControls()
    {
        var kb = Keyboard.current;
        if (kb == null)
            return false;

        if (kb.qKey.wasPressedThisFrame)
        {
            PlayerPosition(1);
        }
        else if (kb.pKey.wasPressedThisFrame)
        {
            PlayerPosition(2);
        }
        else if (kb.aKey.wasPressedThisFrame)
        {
            PlayerPosition(3);
        }
        else if (kb.lKey.wasPressedThisFrame)
        {
            PlayerPosition(4);
        }

        return true;
    }

    public void PlayerPosition(int pos)
    {
        _currentPosition = Mathf.Clamp(pos, 1, 4);
        UpdateVisualsAndColliders();
    }

    private void UpdateVisualsAndColliders()
    {
        if (_playerImage == null)
            return;

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
        }

        if (_catchColliders != null)
        {
            for (int i = 0; i < _catchColliders.Length; i++)
            {
                if (_catchColliders[i] == null)
                    continue;

                bool shouldBeActive = (i == _currentPosition - 1);
                _catchColliders[i].gameObject.SetActive(shouldBeActive);
            }
        }
    }
}
