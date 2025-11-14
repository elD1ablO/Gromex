using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string BEST_SCORE_KEY = "BestScore";
    [SerializeField] private int _maxLives = 3;

    private UIManager _uiManager;
    private PlayerController _playerController;

    private int _currentLives;
    private int _currentScore;

    private int _bestScore;

    private void Awake()
    {
        _uiManager = FindFirstObjectByType<UIManager>();
        _playerController = FindFirstObjectByType<PlayerController>();
    }

    private void Start()
    {
        PlayerController.OnFailCatch += HandleFailCatch;
        PlayerController.OnCoinCatch += HandleCoinCatch;

        _currentLives = _maxLives;
        _currentScore = 0;

        _bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        if (_bestScore < 0)
            _bestScore = 0;

        if (_uiManager != null)
        {
            _uiManager.UIUpdate(_currentScore, _currentLives);
        }
    }

    private void OnDestroy()
    {
        PlayerController.OnFailCatch -= HandleFailCatch;
        PlayerController.OnCoinCatch -= HandleCoinCatch;
    }

    private void HandleCoinCatch()
    {        
        _currentScore++;

        if (_uiManager != null)
        {
            _uiManager.UIUpdate(_currentScore, _currentLives);
        }
    }

    private void HandleFailCatch()
    {
        _currentLives--;

        if (_uiManager != null)
        {
            _uiManager.UIUpdate(_currentScore, _currentLives);
        }

        
        if (_currentLives <= 0)
        {
            EndGameSession();
        }
    }

    private void EndGameSession()
    {        
        if (_currentScore > _bestScore)
        {
            _bestScore = _currentScore;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, _bestScore);
            PlayerPrefs.Save();
        }

        if (_uiManager != null)
        {
            _uiManager.GoToMenu();
        }

        _currentLives = _maxLives;
        _currentScore = 0;
    }
}
