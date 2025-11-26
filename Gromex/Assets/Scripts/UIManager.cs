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

    [Header("Token Used State")]
    [SerializeField] private TMP_Text _tokenUsedText;

    [Header("Timer (for time mode)")]
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private float _timeRemaining = 60f;

    [Header("Player / Lives")]
    [SerializeField] private int _initialLives = 3;

    [SerializeField] private GameObject _livesContainer;
    [SerializeField] private GameObject _lifePrefab;

    [Header("Countdown Canvas (activated at game start)")]
    [SerializeField] private GameObject _countdownCanvas;

    [Header("Player turn buttons container (kept active even during countdown)")]
    [SerializeField] private GameObject _playerTurnButtonsContainer;

    [Header("In-game menu elements")]
    [SerializeField] private Button _menuButton;
    [SerializeField] private GameObject _inGameMenu;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _quitButton;

    private Countdown _countdown;
    private PlayerController _playerController;
    private CoinSpawner _coinSpawner;
    private GameManager _gameManager;

    private bool _isPaused = false;

    private void Awake()
    {
        _playerController = FindFirstObjectByType<PlayerController>();
        _coinSpawner = FindFirstObjectByType<CoinSpawner>();
        _gameManager = FindFirstObjectByType<GameManager>();

        // Ensure countdown component reference (may be null if not assigned)
        if (_countdownCanvas != null)
        {
            _countdown = _countdownCanvas.GetComponent<Countdown>();
            // Ensure countdown canvas initially hidden
            _countdownCanvas.SetActive(false);
        }

        if (_startLivesGameButton != null)
            _startLivesGameButton.onClick.AddListener(OnStartLivesButton);

        if (_startTimeGameButton != null)
            _startTimeGameButton.onClick.AddListener(OnStartTimeButton);

        // In-game menu buttons
        if (_menuButton != null)
            _menuButton.onClick.AddListener(OpenPauseMenu);

        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(ResumeGame);

        if (_quitButton != null)
            _quitButton.onClick.AddListener(QuitGame);

        BestScoreUpdate();

        // ensure UI initial visibility
        ShowTimer(false);
        ShowTokenUsedMessage(false);

        // make sure in-game menu is hidden at start
        if (_inGameMenu != null)
            _inGameMenu.SetActive(false);
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

        // Player stays in idle until user chooses a position (keyboard / UI button)

        // Show player turn buttons immediately and keep them active during countdown
        if (_playerTurnButtonsContainer != null)
            _playerTurnButtonsContainer.SetActive(true);

        // Spawn one static (non-moving) coin for countdown preview
        _coinSpawner?.SpawnPreviewCoin();

        // Activate countdown canvas and start countdown — when finished, start the actual game
        if (_countdownCanvas != null && _countdown != null)
        {
            _countdownCanvas.SetActive(true);
            _countdown.StartCountdown(() =>
            {
                // start lives game in GameManager (this will also start normal coin spawning and release preview coin)
                _gameManager?.StartLivesGame();

                // hide countdown canvas
                _countdownCanvas.SetActive(false);
            });
        }
        else
        {
            // fallback — if no countdown configured, start immediately
            _gameManager?.StartLivesGame();
        }
    }

    private void OnStartTimeButton()
    {
        _startScreen.SetActive(false);

        // Player stays in idle until user chooses a position (keyboard / UI button)

        // Immediately switch UI to time-mode layout:
        // - hide lives
        // - show timer with full time value
        EnterTimeMode(_timeRemaining);

        // Show player turn buttons immediately
        if (_playerTurnButtonsContainer != null)
            _playerTurnButtonsContainer.SetActive(true);

        // Spawn one static (non-moving) coin for countdown preview
        _coinSpawner?.SpawnPreviewCoin();

        if (_countdownCanvas != null && _countdown != null)
        {
            _countdownCanvas.SetActive(true);
            _countdown.StartCountdown(() =>
            {
                // start time game in GameManager (this will actually start time ticking)
                _gameManager?.StartTimeGame(_timeRemaining);

                // hide countdown canvas
                _countdownCanvas.SetActive(false);
            });
        }
        else
        {
            // fallback
            _gameManager?.StartTimeGame(_timeRemaining);
        }
    }

    public void UpdateTimerDisplay(float timeLeft)
    {
        if (_timerText == null)
            return;

        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);

        if (minutes > 0)
            _timerText.text = $"{minutes:00}:{seconds:00}";
        else
            _timerText.text = $"{seconds:00}";
    }

    public void EnterTimeMode(float timeSeconds)
    {
        // Lives are not needed in time mode
        ShowLives(false);

        // Timer should be visible in time mode
        ShowTimer(true);
        UpdateTimerDisplay(timeSeconds);

        if (_scoreText != null)
            _scoreText.text = "0";
    }

    public void EnterLivesMode()
    {
        ShowLives(true);
        ShowTimer(false);

        if (_scoreText != null)
            _scoreText.text = "0";
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

    /// <summary>
    /// Called when game ends and token was used - hides start buttons
    /// </summary>
    public void GoToMenuTokenUsed()
    {
        _coinSpawner?.StopCoinSpawning();

        _startScreen.SetActive(true);

        // hide start buttons since token is used
        SetStartButtonsVisible(false);

        // show message that token was used
        ShowTokenUsedMessage(true);

        EnterLivesMode();
        BestScoreUpdate();

        // Reset player visuals back to idle in menu
        _playerController?.ResetToIdle();
    }

    /// <summary>
    /// Standard menu return (token still valid or for other cases)
    /// </summary>
    public void GoToMenu()
    {
        _coinSpawner?.StopCoinSpawning();

        _startScreen.SetActive(true);

        // show start buttons
        SetStartButtonsVisible(true);
        ShowTokenUsedMessage(false);

        EnterLivesMode();
        BestScoreUpdate();

        // Reset player visuals back to idle in menu
        _playerController?.ResetToIdle();
    }

    private void SetStartButtonsVisible(bool visible)
    {
        if (_startLivesGameButton != null)
            _startLivesGameButton.gameObject.SetActive(visible);

        if (_startTimeGameButton != null)
            _startTimeGameButton.gameObject.SetActive(visible);
    }

    private void ShowTokenUsedMessage(bool show)
    {
        if (_tokenUsedText != null)
            _tokenUsedText.gameObject.SetActive(show);
    }

    private void ShowLives(bool show)
    {
        if (_livesContainer != null)
            _livesContainer.SetActive(show);
    }

    private void ShowTimer(bool show)
    {
        if (_timerText != null)
            _timerText.gameObject.SetActive(show);
    }

    // ==========================
    //      PAUSE / RESUME
    // ==========================

    private void OpenPauseMenu()
    {
        if (_isPaused)
            return;

        _isPaused = true;

        // Freeze everything, including countdown
        Time.timeScale = 0f;

        // Stop spawning new coins (existing ones will be frozen by timeScale = 0)
        _coinSpawner?.StopCoinSpawning();

        if (_inGameMenu != null)
            _inGameMenu.SetActive(true);
    }

    private void ResumeGame()
    {
        if (!_isPaused)
            return;

        _isPaused = false;

        // Resume time
        Time.timeScale = 1f;

        // If game is running, resume spawning.
        // If we are still on countdown screen, IsGameRunning == false
        // and preview coin will stay static.
        if (_gameManager != null && _gameManager.IsGameRunning)
        {
            _coinSpawner?.StartCoinSpawning();
        }

        if (_inGameMenu != null)
            _inGameMenu.SetActive(false);
    }

    private void QuitGame()
    {
        // Restore time just in case
        Time.timeScale = 1f;
        _isPaused = false;

        if (_inGameMenu != null)
            _inGameMenu.SetActive(false);

        // Properly end game session
        _gameManager?.EndGameSession();
    }
}
