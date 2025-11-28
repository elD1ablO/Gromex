using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Simple Supabase client for logging game sessions into "Gromex" table.
/// Works even if there is no ticket or user id (those fields become null).
/// Also provides simple leaderboard queries.
/// </summary>
public class SupabaseLeaderboard : MonoBehaviour
{
    [Header("Supabase")]
    [SerializeField] private string _supabaseUrl = "https://cseqogjagienvdwknrrl.supabase.co";
    [Tooltip("Supabase anon public key (NOT service_role).")]
    [SerializeField] private string _anonKey = "PASTE_ANON_KEY_HERE";

    private const string TableName = "Gromex";

    [Serializable]
    public class LeaderboardEntry
    {
        public long userId;
        public float score;
        public string outcome; // "time_mode" / "lives_mode"
    }

    [Serializable]
    private class LeaderboardWrapper
    {
        public LeaderboardEntry[] items;
    }

    /// <summary>
    /// Public entry point for GameManager.
    /// </summary>
    public void LogGameSession(
        DateTime startUtc,
        DateTime endUtc,
        int score,
        bool isTimeMode,
        int? ticketId,
        int? userId)
    {
        StartCoroutine(LogGameSessionRoutine(startUtc, endUtc, score, isTimeMode, ticketId, userId));
    }

    /// <summary>
    /// Actually builds JSON and sends POST to Supabase.
    /// </summary>
    private IEnumerator LogGameSessionRoutine(
        DateTime startUtc,
        DateTime endUtc,
        int score,
        bool isTimeMode,
        int? ticketId,
        int? userId)
    {
        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_anonKey))
        {
            Debug.LogWarning("SupabaseLeaderboard: Supabase URL or anon key is not set.");
            yield break;
        }

        string url = $"{_supabaseUrl}/rest/v1/{TableName}";

        string startIso = startUtc.ToString("o");
        string endIso = endUtc.ToString("o");

        string status = "finished";
        string outcome = isTimeMode ? "time_mode" : "lives_mode";
        float payout = score;

        string json = BuildSessionJson(
            userId: userId,
            startTimeIso: startIso,
            stopTimeIso: endIso,
            ticketId: ticketId,
            status: status,
            outcome: outcome,
            payout: payout,
            score: score);

        byte[] body = Encoding.UTF8.GetBytes(json);

        var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("apikey", _anonKey);
        request.SetRequestHeader("Authorization", "Bearer " + _anonKey);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Prefer", "return=representation");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Supabase LogGameSession error: {request.result} | {request.error} | {request.downloadHandler.text}");
        }
        else
        {
            Debug.Log($"Supabase LogGameSession OK: {request.downloadHandler.text}");
        }
    }

    /// <summary>
    /// Load top scores (all outcomes, no filter).
    /// Kept for flexibility, but for your case use GetTopScoresByMode.
    /// </summary>
    public IEnumerator GetTopScores(int limit, Action<List<LeaderboardEntry>> onCompleted)
    {
        yield return GetTopScoresInternal(limit, null, onCompleted);
    }

    /// <summary>
    /// Load top scores for specific mode:
    /// isTimeMode = true  => "time_mode"
    /// isTimeMode = false => "lives_mode"
    /// </summary>
    public IEnumerator GetTopScoresByMode(int limit, bool isTimeMode, Action<List<LeaderboardEntry>> onCompleted)
    {
        string outcomeFilter = isTimeMode ? "time_mode" : "lives_mode";
        yield return GetTopScoresInternal(limit, outcomeFilter, onCompleted);
    }

    /// <summary>
    /// Internal helper for leaderboard queries.
    /// </summary>
    private IEnumerator GetTopScoresInternal(int limit, string outcomeFilter, Action<List<LeaderboardEntry>> onCompleted)
    {
        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_anonKey))
        {
            Debug.LogWarning("SupabaseLeaderboard: Supabase URL or anon key is not set.");
            onCompleted?.Invoke(null);
            yield break;
        }

        // Base query: select userId, score, outcome ordered by score desc
        string url = $"{_supabaseUrl}/rest/v1/{TableName}" +
                     $"?select=userId,score,outcome&order=score.desc&limit={limit}";

        // Optional: filter by outcome ("time_mode" / "lives_mode")
        if (!string.IsNullOrEmpty(outcomeFilter))
        {
            url += $"&outcome=eq.{outcomeFilter}";
        }

        var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", _anonKey);
        request.SetRequestHeader("Authorization", "Bearer " + _anonKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Supabase GetTopScores error: {request.result} | {request.error} | {request.downloadHandler.text}");
            onCompleted?.Invoke(null);
            yield break;
        }

        string json = request.downloadHandler.text;

        // JsonUtility does not parse top-level arrays, so wrap it.
        string wrapped = "{\"items\":" + json + "}";

        LeaderboardWrapper wrapper = JsonUtility.FromJson<LeaderboardWrapper>(wrapped);

        var list = (wrapper != null && wrapper.items != null)
            ? new List<LeaderboardEntry>(wrapper.items)
            : new List<LeaderboardEntry>();

        onCompleted?.Invoke(list);
    }

    /// <summary>
    /// Builds JSON object for Supabase INSERT with support of null values.
    /// </summary>
    private string BuildSessionJson(
        int? userId,
        string startTimeIso,
        string stopTimeIso,
        int? ticketId,
        string status,
        string outcome,
        float? payout,
        int score)
    {
        var sb = new StringBuilder();
        sb.Append('{');

        // startTime and stopTime (required)
        sb.Append("\"startTime\":\"").Append(startTimeIso).Append("\",");
        sb.Append("\"stopTime\":\"").Append(stopTimeIso).Append("\",");

        // userId (nullable)
        sb.Append("\"userId\":");
        if (userId.HasValue)
            sb.Append(userId.Value.ToString(CultureInfo.InvariantCulture));
        else
            sb.Append("null");
        sb.Append(',');

        // ticketId (nullable)
        sb.Append("\"ticketId\":");
        if (ticketId.HasValue)
            sb.Append(ticketId.Value.ToString(CultureInfo.InvariantCulture));
        else
            sb.Append("null");
        sb.Append(',');

        // status (nullable text)
        sb.Append("\"status\":");
        if (!string.IsNullOrEmpty(status))
            sb.Append('"').Append(EscapeJsonString(status)).Append('"');
        else
            sb.Append("null");
        sb.Append(',');

        // outcome (nullable text)
        sb.Append("\"outcome\":");
        if (!string.IsNullOrEmpty(outcome))
            sb.Append('"').Append(EscapeJsonString(outcome)).Append('"');
        else
            sb.Append("null");
        sb.Append(',');

        // payout (nullable numeric)
        sb.Append("\"payout\":");
        if (payout.HasValue)
            sb.Append(payout.Value.ToString(CultureInfo.InvariantCulture));
        else
            sb.Append("null");
        sb.Append(',');

        // score (required numeric)
        sb.Append("\"score\":");
        sb.Append(score.ToString(CultureInfo.InvariantCulture));

        sb.Append('}');
        return sb.ToString();
    }

    private string EscapeJsonString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }
}
