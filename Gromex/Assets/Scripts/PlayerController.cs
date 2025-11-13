using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static Action OnCoinCatch;
    public static Action OnFailCatch;

    [SerializeField] private BoxCollider2D[] _failColliders;

    [SerializeField] private Sprite _bottomPlayerSprite;
    [SerializeField] private Sprite _topPlayerSprite;

    [SerializeField] private SpriteRenderer _playerImage;

    private BoxCollider2D _catchCollider;
    private Vector3 _defaultScale;

    private void Start()
    {
        _catchCollider = GetComponent<BoxCollider2D>();
            
    }

    public void PlayerPosition(int pos)
    {
        if (_playerImage == null)
            return;


        switch (pos)
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

            default:
                _playerImage.sprite = _bottomPlayerSprite;
                _playerImage.flipX = false;
                break;
        }

    }

    private void Update()
    {
        ProcessFail();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {      
        OnCoinCatch?.Invoke();
    }

    private void ProcessFail()
    {
        if (_catchCollider == null || _failColliders == null)
            return;

        for (int i = 0; i < _failColliders.Length; i++)
        {
            if (_failColliders[i] == null)
                continue;

            if (_catchCollider.IsTouching(_failColliders[i]))
            {
                OnFailCatch?.Invoke();
                break;
            }
        }
    }
}
