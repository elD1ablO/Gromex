using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private const string BEST_SCORE_KEY = "BestScore";

    [SerializeField] private GameObject _startScreen;

    [SerializeField] private TMP_Text _bestScoreText;
    [SerializeField] private TMP_Text _scoreText;      

    [SerializeField] private Button _startGameButton;

    [SerializeField] private int initialPlayerPosition = 1;
    [SerializeField] private int _initialLives = 3;

    [SerializeField] private GameObject _livesContainer;
    [SerializeField] private GameObject _lifePrefab;

    private PlayerController _playerController;
    private CoinSpawner _coinSpawner;
    
    private void Awake()
    {
        _playerController = FindFirstObjectByType<PlayerController>();
        _coinSpawner = FindFirstObjectByType<CoinSpawner>();

        if (_startGameButton != null)
            _startGameButton.onClick.AddListener(StartGame);

        BestScoreUpdate();
    }

    private void BestScoreUpdate()
    {
        int bestScore = 0;

        if (PlayerPrefs.HasKey(BEST_SCORE_KEY))
        {
            bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);

            if (bestScore < 0)
                bestScore = 0;
        }

        if (_bestScoreText != null)
            _bestScoreText.text = bestScore.ToString();
    }

    private void StartGame()
    {
        _startScreen.SetActive(false);

        if (_playerController != null)
            _playerController.PlayerPosition(initialPlayerPosition);

        if (_coinSpawner != null)
            _coinSpawner.StartCoinSpawning();

        
        UIUpdate(0, _initialLives);
    }

    public void UIUpdate(int currentScore, int currentLives)
    {
        
        if (_scoreText != null)
            _scoreText.text = currentScore.ToString();

       
        if (_livesContainer != null && _lifePrefab != null)
        {
            
            foreach (Transform child in _livesContainer.transform)
            {
                Destroy(child.gameObject);
            }
                        
            for (int i = 0; i < currentLives; i++)
            {
                Instantiate(_lifePrefab, _livesContainer.transform);
            }
        }
    }

    public void GoToMenu()
    {        
        if (_coinSpawner != null)
            _coinSpawner.StopCoinSpawning();

        _startScreen.SetActive(true);

        BestScoreUpdate();
    }
}

