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

    private int _currentLives;
    private int _currentScore;
    private int _bestScore;

    private bool _isTimeMode = false;
    private float _timeLeft = 0f;
    private bool _isGameRunning = false;
    private bool _tokenUsed = false;

    private void Awake()
    {
        _uiManager = FindFirstObjectByType<UIManager>();
        _playerController = FindFirstObjectByType<PlayerController>();
        _coinSpawner = FindFirstObjectByType<CoinSpawner>();
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

        _coinSpawner?.StartCoinSpawning();

        _uiManager?.EnterTimeMode(_timeLeft);
        _uiManager?.UIUpdate(_currentScore, _currentLives);
    }

    #endregion

    private void HandleCoinCatch()
    {
        _currentScore++;

        if (_uiManager != null)
            _uiManager.UIUpdate(_currentScore, _currentLives);
    }

    private void HandleFailCatch()
    {
        if (_isTimeMode)
            return;

        _currentLives--;

        if (_uiManager != null)
            _uiManager.UIUpdate(_currentScore, _currentLives);

        if (_currentLives <= 0)
            EndGameSession();
    }

    private void EndGameSession()
    {
        _isGameRunning = false;

        _coinSpawner?.StopCoinSpawning();

        if (_currentScore > _bestScore)
        {
            _bestScore = _currentScore;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, _bestScore);
            PlayerPrefs.Save();
        }

        // send finish request and mark token as used
        StartCoroutine(SendFinishRequestAndReturnToMenu(_currentScore));
    }

    private IEnumerator SendFinishRequestAndReturnToMenu(int finalScore)
    {
        // send finish request first
        yield return StartCoroutine(SendFinishRequest(finalScore));

        // mark token as used
        _tokenUsed = true;

        // clear session data since token is now used
        SessionData.TicketId = 0;
        SessionData.GameToken = "";

        // return to menu with buttons hidden
        _uiManager?.GoToMenuTokenUsed();

        // reset game state
        _currentLives = _maxLives;
        _currentScore = 0;
        _isTimeMode = false;
        _timeLeft = 0f;
    }

    private IEnumerator SendFinishRequest(int finalScore)
    {
        if (SessionData.TicketId == 0 || string.IsNullOrEmpty(SessionData.GameToken))
        {
            Debug.LogError("FinishGame: Missing session data.");
            yield break;
        }

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

    /// <summary>
    /// Called when a new token is validated - resets the token used state
    /// </summary>
    public void OnNewTokenValidated()
    {
        _tokenUsed = false;
    }

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