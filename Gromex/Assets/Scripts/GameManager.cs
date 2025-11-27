using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    private const string BEST_SCORE_KEY = "BestScore";

    [SerializeField] private int _maxLives = 3;
    [SerializeField] private float _timeRemaining = 60f;

    private UIManager _uiManager;
    private PlayerController _playerController;
    private CoinSpawner _coinSpawner;
    private SupabaseLeaderboard _supabaseLeaderboard;

    private int _currentLives;
    private int _currentScore;
    private int _bestScore;

    private bool _isTimeMode = false;
    private float _timeLeft = 0f;
    private bool _isGameRunning = false;
    private bool _tokenUsed = false;

    // Supabase session tracking
    private DateTime _sessionStartTimeUtc;
    private bool _hasSessionStartTime = false;

    private void Awake()
    {
        _uiManager = FindFirstObjectByType<UIManager>();
        _playerController = FindFirstObjectByType<PlayerController>();
        _coinSpawner = FindFirstObjectByType<CoinSpawner>();
        _supabaseLeaderboard = FindFirstObjectByType<SupabaseLeaderboard>();
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
            _uiManager.UIUpdate(_currentScore, _currentLives);
    }

    private void OnDestroy()
    {
        PlayerController.OnFailCatch -= HandleFailCatch;
        PlayerController.OnCoinCatch -= HandleCoinCatch;
    }

    private void Update()
    {
        if (!_isGameRunning)
            return;

        if (_isTimeMode)
        {
            _timeLeft -= Time.deltaTime;
            if (_timeLeft < 0f)
                _timeLeft = 0f;

            _uiManager?.UpdateTimerDisplay(_timeLeft);

            if (_timeLeft <= 0f)
                EndGameSession();
        }
    }

    #region Start Modes

    public void StartLivesGame()
    {
        _isTimeMode = false;
        _isGameRunning = true;

        _currentLives = _maxLives;
        _currentScore = 0;

        // mark session start time for Supabase log
        _sessionStartTimeUtc = DateTime.UtcNow;
        _hasSessionStartTime = true;

        _coinSpawner?.StartCoinSpawning();

        _uiManager?.EnterLivesMode();
        _uiManager?.UIUpdate(_currentScore, _currentLives);
    }

    public void StartTimeGame(float timeSeconds)
    {
        _isTimeMode = true;
        _isGameRunning = true;

        _timeLeft = Mathf.Max(0f, timeSeconds);
        _currentScore = 0;

        // mark session start time for Supabase log
        _sessionStartTimeUtc = DateTime.UtcNow;
        _hasSessionStartTime = true;

        _coinSpawner?.StartCoinSpawning();

        _uiManager?.EnterTimeMode(_timeLeft);
        _uiManager?.UIUpdate(_currentScore, _currentLives);
    }

    #endregion

    private void HandleCoinCatch()
    {
        // Normal coin catch in both modes: always +1 score.
        _currentScore++;

        if (_uiManager != null)
            _uiManager.UIUpdate(_currentScore, _currentLives);
    }

    private void HandleFailCatch()
    {
        if (_isTimeMode)
        {
            _timeLeft -= 5f;
            if (_timeLeft < 0f)
                _timeLeft = 0f;

            _uiManager?.UpdateTimerDisplay(_timeLeft);

            _uiManager?.PlayMinusTimeEffect();

            if (_timeLeft <= 0f)
                EndGameSession();

            return;
        }

        // LIVES MODE
        _currentLives--;
        _uiManager?.UIUpdate(_currentScore, _currentLives);

        if (_currentLives <= 0)
            EndGameSession();
    }

    public void EndGameSession()
    {
        _isGameRunning = false;

        // Stop spawning and immediately clear all existing coins
        _coinSpawner?.StopCoinSpawning();
        _coinSpawner?.ClearAllCoins();

        // Update best score
        if (_currentScore > _bestScore)
        {
            _bestScore = _currentScore;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, _bestScore);
            PlayerPrefs.Save();
        }

        // --- Supabase log (always, even without ticket / token) ---
        DateTime endTimeUtc = DateTime.UtcNow;
        DateTime startTimeUtc = _hasSessionStartTime ? _sessionStartTimeUtc : endTimeUtc;

        int? ticketIdNullable = (SessionData.TicketId > 0) ? SessionData.TicketId : (int?)null;

        _supabaseLeaderboard?.LogGameSession(
            startTimeUtc,
            endTimeUtc,
            _currentScore,
            _isTimeMode,
            ticketIdNullable
        );
        // ------------------------------------------------------------

        // SAFETY: if we are playing without a ticket, just go to menu and show score
        if (SessionData.TicketId == 0 || string.IsNullOrEmpty(SessionData.GameToken))
        {
            Debug.Log("<color=yellow>Offline mode – no ticket, returning to menu without server call.</color>");

            _uiManager?.GoToMenu();

            ResetGameState();
            return;
        }

        // Online mode: send finish request to server and then go to menu with token-used state
        StartCoroutine(SendFinishRequestAndReturnToMenu(_currentScore));
    }

    private IEnumerator SendFinishRequestAndReturnToMenu(int finalScore)
    {
        yield return StartCoroutine(SendFinishRequest(finalScore));

        _tokenUsed = true;

        SessionData.TicketId = 0;
        SessionData.GameToken = "";

        _uiManager?.GoToMenuTokenUsed();

        ResetGameState();
    }

    private IEnumerator SendFinishRequest(int finalScore)
    {
        if (SessionData.TicketId == 0 || string.IsNullOrEmpty(SessionData.GameToken))
            yield break;

        string url = $"https://uni.gromex.io/28fk2/api/game/ticket/{SessionData.TicketId}/finish";

        var body = new FinishRequest
        {
            game_token = SessionData.GameToken,
            outcome = "win",
            payout = finalScore,
            score = finalScore
        };

        string json = JsonUtility.ToJson(body);
        var request = new UnityWebRequest(url, "POST");
        byte[] data = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(data);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(SessionData.BearerToken))
            request.SetRequestHeader("Authorization", "Bearer " + SessionData.BearerToken);

        request.timeout = 10;

        yield return request.SendWebRequest();

        bool error = request.result != UnityWebRequest.Result.Success;

        if (error)
        {
            Debug.LogError($"FinishGame Error: {request.error}\nResponse: {request.downloadHandler.text}");
            yield break;
        }

        Debug.Log("FinishGame response: " + request.downloadHandler.text);
    }

    private void ResetGameState()
    {
        _currentLives = _maxLives;
        _currentScore = 0;
        _isTimeMode = false;
        _timeLeft = 0f;

        _hasSessionStartTime = false;
    }

    /// <summary>
    /// Called when a new token is validated - resets the token used state.
    /// </summary>
    public void OnNewTokenValidated()
    {
        _tokenUsed = false;
    }

    // Exposed for other systems (e.g., zones) to check current mode.
    public bool IsTimeMode => _isTimeMode;
    public bool IsGameRunning => _isGameRunning;
    public bool IsTokenUsed => _tokenUsed;
}

[System.Serializable]
public class FinishRequest
{
    public string game_token;
    public string outcome;
    public float payout;
    public int score;
}
