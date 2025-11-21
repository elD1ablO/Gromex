using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private const string BEST_SCORE_KEY = "BestScore";

    [SerializeField] private GameObject _startScreen;

    [SerializeField] private TMP_Text _bestScoreText;
    [SerializeField] private TMP_Text _scoreText;

    [Header("Start buttons")]
    [SerializeField] private Button _startLivesGameButton;
    [SerializeField] private Button _startTimeGameButton;

    [Header("Timer (for time mode)")]
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private float _timeRemaining = 60f;

    [Header("Player / Lives")]
    [SerializeField] private int initialPlayerPosition = 1;
    [SerializeField] private int _initialLives = 3;

    [SerializeField] private GameObject _livesContainer;
    [SerializeField] private GameObject _lifePrefab;

    private PlayerController _playerController;
    private CoinSpawner _coinSpawner;
    private GameManager _gameManager;

    private void Awake()
    {
        _playerController = FindFirstObjectByType<PlayerController>();
        _coinSpawner = FindFirstObjectByType<CoinSpawner>();
        _gameManager = FindFirstObjectByType<GameManager>();

        if (_startLivesGameButton != null)
            _startLivesGameButton.onClick.AddListener(OnStartLivesButton);

        if (_startTimeGameButton != null)
            _startTimeGameButton.onClick.AddListener(OnStartTimeButton);

        BestScoreUpdate();

        // ensure UI initial visibility
        ShowTimer(false);
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

    private void OnStartLivesButton()
    {
        _startScreen.SetActive(false);

        if (_playerController != null)
            _playerController.PlayerPosition(initialPlayerPosition);

        // call GameManager to start lives mode
        _gameManager?.StartLivesGame();
    }

    private void OnStartTimeButton()
    {
        _startScreen.SetActive(false);

        if (_playerController != null)
            _playerController.PlayerPosition(initialPlayerPosition);

        // call GameManager to start time mode with configured time
        _gameManager?.StartTimeGame(_timeRemaining);
    }

    // Called by GameManager every frame when time-mode runs
    public void UpdateTimerDisplay(float timeLeft)
    {
        if (_timerText == null)
            return;

        // format mm:ss or s
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);

        if (minutes > 0)
            _timerText.text = $"{minutes:00}:{seconds:00}";
        else
            _timerText.text = $"{seconds:00}";
    }

    // UI helpers called by GameManager when switching modes
    public void EnterTimeMode(float timeSeconds)
    {
        // hide lives UI
        ShowLives(false);

        // show timer and set initial value
        ShowTimer(true);
        UpdateTimerDisplay(timeSeconds);

        // ensure score shown
        if (_scoreText != null)
            _scoreText.text = "0";
    }

    public void EnterLivesMode()
    {
        // show lives UI
        ShowLives(true);

        // hide timer
        ShowTimer(false);

        // ensure score shown
        if (_scoreText != null)
            _scoreText.text = "0";
    }

    // General UI update for score and lives (GameManager calls this)
    public void UIUpdate(int currentScore, int currentLives)
    {
        if (_scoreText != null)
            _scoreText.text = currentScore.ToString();

        if (_livesContainer != null && _lifePrefab != null)
        {
            // Clear previous
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
        // stop spawner (defensive)
        _coinSpawner?.StopCoinSpawning();

        // show start screen
        _startScreen.SetActive(true);

        // reset UI: show lives by default and hide timer
        EnterLivesMode();

        // update best score
        BestScoreUpdate();
    }

    private void ShowLives(bool show)
    {
        if (_livesContainer != null)
        {
            _livesContainer.SetActive(show);
        }
    }

    private void ShowTimer(bool show)
    {
        if (_timerText != null)
        {
            _timerText.gameObject.SetActive(show);
        }
    }
}


