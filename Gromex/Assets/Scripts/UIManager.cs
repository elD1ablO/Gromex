using System.Collections;
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
    [SerializeField] private TMP_Text _minusTimerText;
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

    // minus-timer FX
    private Coroutine _minusTimerRoutine;
    private Vector3 _minusTimerBaseScale = Vector3.one;

    private void Awake()
    {
        _playerController = FindFirstObjectByType<PlayerController>();
        _coinSpawner = FindFirstObjectByType<CoinSpawner>();
        _gameManager = FindFirstObjectByType<GameManager>();

        // Ensure countdown reference
        if (_countdownCanvas != null)
        {
            _countdown = _countdownCanvas.GetComponent<Countdown>();
            _countdownCanvas.SetActive(false);
        }

        if (_startLivesGameButton != null)
            _startLivesGameButton.onClick.AddListener(OnStartLivesButton);

        if (_startTimeGameButton != null)
            _startTimeGameButton.onClick.AddListener(OnStartTimeButton);

        if (_menuButton != null)
            _menuButton.onClick.AddListener(OpenPauseMenu);

        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(ResumeGame);

        if (_quitButton != null)
            _quitButton.onClick.AddListener(QuitGame);

        BestScoreUpdate();

        ShowTimer(false);
        ShowTokenUsedMessage(false);

        if (_inGameMenu != null)
            _inGameMenu.SetActive(false);

        // Apply UI colors
        if (_scoreText != null)
            _scoreText.color = new Color32(0x00, 0x25, 0x35, 255); // #002535

        if (_timerText != null)
            _timerText.color = new Color32(0x6C, 0xCF, 0xE2, 255); // #6CCFE2

        if (_minusTimerText != null)
        {
            _minusTimerBaseScale = _minusTimerText.rectTransform.localScale;
            _minusTimerText.gameObject.SetActive(false);
            _minusTimerText.color = new Color32(0xFF, 0x4F, 0x1A, 255); // #FF4F1A
        }
    }

    private void BestScoreUpdate()
    {
        int bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);

        if (_bestScoreText != null)
            _bestScoreText.text = bestScore.ToString();
    }

    private void OnStartLivesButton()
    {
        _startScreen.SetActive(false);

        if (_playerTurnButtonsContainer != null)
            _playerTurnButtonsContainer.SetActive(true);

        _coinSpawner?.SpawnPreviewCoin();

        if (_countdownCanvas != null && _countdown != null)
        {
            _countdownCanvas.SetActive(true);
            _countdown.StartCountdown(() =>
            {
                _gameManager?.StartLivesGame();
                _countdownCanvas.SetActive(false);
            });
        }
        else
        {
            _gameManager?.StartLivesGame();
        }
    }

    private void OnStartTimeButton()
    {
        _startScreen.SetActive(false);

        EnterTimeMode(_timeRemaining);

        if (_playerTurnButtonsContainer != null)
            _playerTurnButtonsContainer.SetActive(true);

        _coinSpawner?.SpawnPreviewCoin();

        if (_countdownCanvas != null && _countdown != null)
        {
            _countdownCanvas.SetActive(true);
            _countdown.StartCountdown(() =>
            {
                _gameManager?.StartTimeGame(_timeRemaining);
                _countdownCanvas.SetActive(false);
            });
        }
        else
        {
            _gameManager?.StartTimeGame(_timeRemaining);
        }
    }

    public void UpdateTimerDisplay(float timeLeft)
    {
        if (_timerText == null)
            return;

        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);

        // Special color for last 5 seconds
        if (timeLeft <= 5f)
            _timerText.color = new Color32(0xFF, 0x4F, 0x1A, 255); // #FF4F1A
        else
            _timerText.color = new Color32(0x6C, 0xCF, 0xE2, 255); // #6CCFE2

        if (minutes > 0)
            _timerText.text = $"{minutes:00}:{seconds:00}";
        else
            _timerText.text = $"{seconds:00}";
    }

    public void EnterTimeMode(float timeSeconds)
    {
        ShowLives(false);
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
                Object.Destroy(child.gameObject);

            for (int i = 0; i < currentLives; i++)
                Object.Instantiate(_lifePrefab, _livesContainer.transform);
        }
    }

    public void GoToMenuTokenUsed()
    {
        _coinSpawner?.StopCoinSpawning();

        _startScreen.SetActive(true);

        SetStartButtonsVisible(false);
        ShowTokenUsedMessage(true);

        EnterLivesMode();
        BestScoreUpdate();

        _playerController?.ResetToIdle();
    }

    public void GoToMenu()
    {
        _coinSpawner?.StopCoinSpawning();

        _startScreen.SetActive(true);

        SetStartButtonsVisible(true);
        ShowTokenUsedMessage(false);

        EnterLivesMode();
        BestScoreUpdate();

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
    //   MINUS TIMER FX (-5s)
    // ==========================

    public void PlayMinusTimeEffect()
    {
        if (_minusTimerText == null)
            return;

        if (_minusTimerRoutine != null)
            StopCoroutine(_minusTimerRoutine);

        _minusTimerRoutine = StartCoroutine(MinusTimeRoutine());
    }

    private IEnumerator MinusTimeRoutine()
    {
        RectTransform rt = _minusTimerText.rectTransform;

        _minusTimerText.gameObject.SetActive(true);

        // reset color to orange
        _minusTimerText.color = new Color32(0xFF, 0x4F, 0x1A, 255);

        float punchScale = 1.5f;
        float punchDuration = 0.15f;
        float holdDuration = 1.0f;
        float fadeDuration = 0.5f;

        Vector3 startScale = _minusTimerBaseScale * punchScale;
        Vector3 endScale = _minusTimerBaseScale;

        rt.localScale = startScale;

        float t = 0f;
        while (t < punchDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / punchDuration);
            float eased = 1f - Mathf.Pow(1f - lerp, 2f);

            rt.localScale = Vector3.Lerp(startScale, endScale, eased);
            yield return null;
        }

        rt.localScale = endScale;

        yield return new WaitForSeconds(holdDuration);

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeDuration);
            float alpha = Mathf.Lerp(1f, 0f, lerp);

            Color cc = _minusTimerText.color;
            cc.a = alpha;
            _minusTimerText.color = cc;

            yield return null;
        }

        _minusTimerText.gameObject.SetActive(false);

        Color final = _minusTimerText.color;
        final.a = 1f;
        _minusTimerText.color = final;

        rt.localScale = _minusTimerBaseScale;

        _minusTimerRoutine = null;
    }

    // ==========================
    //      PAUSE / RESUME
    // ==========================

    private void OpenPauseMenu()
    {
        if (_isPaused)
            return;

        _isPaused = true;

        Time.timeScale = 0f;
        _coinSpawner?.StopCoinSpawning();

        if (_inGameMenu != null)
            _inGameMenu.SetActive(true);
    }

    private void ResumeGame()
    {
        if (!_isPaused)
            return;

        _isPaused = false;

        Time.timeScale = 1f;

        if (_gameManager != null && _gameManager.IsGameRunning)
            _coinSpawner?.StartCoinSpawning();

        if (_inGameMenu != null)
            _inGameMenu.SetActive(false);
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
        _isPaused = false;

        if (_inGameMenu != null)
            _inGameMenu.SetActive(false);

        _gameManager?.EndGameSession();
    }
}
