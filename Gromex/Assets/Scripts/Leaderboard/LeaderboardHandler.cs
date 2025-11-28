using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardHandler : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Optional: root panel of the leaderboard (can be the same as UIManager._leaderboardUI).")]
    [SerializeField] private GameObject _leaderboardPanel;

    [Tooltip("Prefab for a single leaderboard row (must have SingleLeaderboardElement).")]
    [SerializeField] private GameObject _leaderboardElement;

    [Tooltip("Parent transform for instantiated leaderboard elements (Content of ScrollView).")]
    [SerializeField] private Transform _elementsParent;

    [Header("Mode switch buttons")]
    [Tooltip("Button that activates Time mode leaderboard.")]
    [SerializeField] private Button _timeModeButton;

    [Tooltip("Button that activates Lives/Score mode leaderboard.")]
    [SerializeField] private Button _livesModeButton;

    [Header("Settings")]
    [SerializeField] private int _maxEntries = 10;

    private SupabaseLeaderboard _supabaseLeaderboard;
    private readonly List<SingleLeaderboardElement> _spawnedElements = new();

    // Current mode: true = time mode, false = lives mode
    private bool _currentIsTimeMode = true;

    // Cached default (normal) sprites for buttons
    private Sprite _timeNormalSprite;
    private Sprite _livesNormalSprite;

    private void Awake()
    {
        _supabaseLeaderboard = FindFirstObjectByType<SupabaseLeaderboard>();

        // Optional: ensure panel is initially hidden
        if (_leaderboardPanel != null)
            _leaderboardPanel.SetActive(false);

        // Cache normal sprites (what button has in Image initially)
        if (_timeModeButton != null && _timeModeButton.targetGraphic is Image timeImg)
            _timeNormalSprite = timeImg.sprite;

        if (_livesModeButton != null && _livesModeButton.targetGraphic is Image livesImg)
            _livesNormalSprite = livesImg.sprite;

        // Wire buttons
        if (_timeModeButton != null)
            _timeModeButton.onClick.AddListener(OnTimeModeClicked);

        if (_livesModeButton != null)
            _livesModeButton.onClick.AddListener(OnLivesModeClicked);

        UpdateModeButtonsVisual();
    }

    public void ShowLeaderboard(bool _ignoredGameModeFlag)
    {
        if (_leaderboardPanel != null)
            _leaderboardPanel.SetActive(true);

        _currentIsTimeMode = true; // default
        UpdateModeButtonsVisual();
        ReloadLeaderboard(_currentIsTimeMode);
    }

    public void HideLeaderboard()
    {
        if (_leaderboardPanel != null)
            _leaderboardPanel.SetActive(false);
    }

    public void ReloadLeaderboard(bool isTimeMode)
    {
        _currentIsTimeMode = isTimeMode;
        UpdateModeButtonsVisual();

        if (_supabaseLeaderboard == null)
        {
            Debug.LogWarning("LeaderboardHandler: SupabaseLeaderboard not found in scene.");
            BuildUI(null);
            return;
        }

        StartCoroutine(LoadLeaderboardRoutine(isTimeMode));
    }

    private IEnumerator LoadLeaderboardRoutine(bool isTimeMode)
    {
        bool isDone = false;
        List<SupabaseLeaderboard.LeaderboardEntry> result = null;

        // Request top N scores for the selected mode
        yield return StartCoroutine(_supabaseLeaderboard.GetTopScoresByMode(
            _maxEntries,
            isTimeMode,
            list =>
            {
                result = list;
                isDone = true;
            }));

        if (!isDone || result == null)
        {
            BuildUI(null);
            yield break;
        }

        BuildUI(result);
    }

    private void BuildUI(List<SupabaseLeaderboard.LeaderboardEntry> entries)
    {
        // Clear old UI elements
        foreach (var e in _spawnedElements)
        {
            if (e != null)
                Destroy(e.gameObject);
        }
        _spawnedElements.Clear();

        if (_elementsParent == null || _leaderboardElement == null)
        {
            Debug.LogWarning("LeaderboardHandler: ElementsParent or LeaderboardElement prefab is not assigned.");
            return;
        }

        if (entries == null || entries.Count == 0)
        {
            // No data – you could instantiate a "No entries" row here if desired.
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            SupabaseLeaderboard.LeaderboardEntry entry = entries[i];

            GameObject go = Instantiate(_leaderboardElement, _elementsParent);
            var element = go.GetComponent<SingleLeaderboardElement>();

            if (element != null)
            {
                int rank = i + 1;
                // For now we do not have username, only userId – show it as "User {id}"
                string username = $"{entry.userId}";
                int scoreInt = Mathf.RoundToInt(entry.score);

                element.Setup(rank, username, scoreInt);
                _spawnedElements.Add(element);
            }
        }
    }

    // ==========================
    //     MODE SWITCH LOGIC
    // ==========================

    private void OnTimeModeClicked()
    {
        if (!_currentIsTimeMode)
        {
            ReloadLeaderboard(true);
        }
    }

    private void OnLivesModeClicked()
    {
        if (_currentIsTimeMode)
        {
            ReloadLeaderboard(false);
        }
    }

    private void UpdateModeButtonsVisual()
    {
        // TIME BUTTON
        if (_timeModeButton != null && _timeModeButton.targetGraphic is Image timeImg)
        {
            var state = _timeModeButton.spriteState;
            if (_currentIsTimeMode && state.selectedSprite != null)
            {
                timeImg.sprite = state.selectedSprite;
            }
            else if (_timeNormalSprite != null)
            {
                timeImg.sprite = _timeNormalSprite;
            }

            _timeModeButton.interactable = !_currentIsTimeMode;
        }

        // LIVES BUTTON
        if (_livesModeButton != null && _livesModeButton.targetGraphic is Image livesImg)
        {
            var state = _livesModeButton.spriteState;
            if (!_currentIsTimeMode && state.selectedSprite != null)
            {
                livesImg.sprite = state.selectedSprite;
            }
            else if (_livesNormalSprite != null)
            {
                livesImg.sprite = _livesNormalSprite;
            }

            _livesModeButton.interactable = _currentIsTimeMode;
        }
    }
}
