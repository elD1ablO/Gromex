using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static Action OnCoinCatch;
    public static Action OnFailCatch;   

    [Header("Player sprites")]
    [SerializeField] private Sprite _bottomPlayerSprite;
    [SerializeField] private Sprite _topPlayerSprite;
    [SerializeField] private SpriteRenderer _playerImage;

    [Header("Catch colliders (1-4)")]
    [SerializeField] private BoxCollider2D[] _catchColliders;

    private int _currentPosition = 1;

    private void Start()
    {
        UpdateVisualsAndColliders();
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

        // Спрайт + напрямок
        switch (_currentPosition)
        {
            case 1:
                _playerImage.sprite = _topPlayerSprite;
                _playerImage.flipX = false;
                break;

            case 2:
                _playerImage.sprite = _topPlayerSprite;
                _playerImage.flipX = true;
                break;

            case 3:
                _playerImage.sprite = _bottomPlayerSprite;
                _playerImage.flipX = false;
                break;

            case 4:
                _playerImage.sprite = _bottomPlayerSprite;
                _playerImage.flipX = true;
                break;
        }

        // Активуємо тільки один catch-collider
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

    private void OnTriggerEnter2D(Collider2D other)
    {       
        OnCoinCatch?.Invoke();
    }
}
