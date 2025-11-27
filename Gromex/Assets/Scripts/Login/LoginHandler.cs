using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class LoginHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private TMP_Text _loginFlowText;
    [SerializeField] private TMP_InputField _tokenInputField; // optional: manual token paste

    [Header("API")]
    [Tooltip("Base url, without trailing slash")]
    [SerializeField] private string _baseUrl = "https://uni.gromex.io/28fk2";
    [Tooltip("Optional test bearer token - do NOT use in production builds.")]
    [SerializeField] private string _bearerToken = "";

    [Header("Test")]
    [Tooltip("Optional quick-test token (game_token)")]
    [SerializeField] private string _testToken = "gTok_0913d08b32f18edd82520aa595232069153427ef9147ccb64e55cabba0039fe3";

    [Header("Networking")]
    [SerializeField] private int _requestTimeoutSeconds = 12;

    private const string VALIDATE_PATH = "/api/game/ticket/validate";

    #region Public UI methods

    public void OnValidateButtonPressed()
    {
        string token = _tokenInputField != null ? _tokenInputField.text?.Trim() : _testToken?.Trim();
        if (string.IsNullOrEmpty(token))
        {
            SetFlowText("Please enter a game token.");
            return;
        }

        StartCoroutine(ValidateTokenCoroutine(token));
    }

    public void ValidateTestToken()
    {
        if (string.IsNullOrEmpty(_testToken))
        {
            SetFlowText("Test token is empty in inspector.");
            return;
        }

        StartCoroutine(ValidateTokenCoroutine(_testToken.Trim()));
    }

    /// <summary>
    /// Called by GameManager after a finished online game to request a new token.
    /// </summary>
    public void ShowLoginPanelForNewToken()
    {
        if (_loginPanel != null)
            _loginPanel.SetActive(true);

        if (_tokenInputField != null)
            _tokenInputField.text = string.Empty;

        SetFlowText("Please enter a new game token.");
    }

    #endregion

    private IEnumerator ValidateTokenCoroutine(string gameToken)
    {
        SetFlowText("Validating token...");

        string url = _baseUrl.TrimEnd('/') + VALIDATE_PATH;

        var reqObj = new ValidateRequest { game_token = gameToken };
        string bodyJson = JsonUtility.ToJson(reqObj);

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();

            // Track headers manually (Unity 6 workaround)
            Dictionary<string, string> headerMap = new Dictionary<string, string>();

            req.SetRequestHeader("Content-Type", "application/json");
            headerMap["Content-Type"] = "application/json";

            if (!string.IsNullOrWhiteSpace(_bearerToken))
            {
                req.SetRequestHeader("Authorization", "Bearer " + _bearerToken);
                headerMap["Authorization"] = "Bearer " + _bearerToken;
            }

            req.timeout = Mathf.Max(5, _requestTimeoutSeconds);

            // Debug full request before sending
            LogRequestDebug(url, bodyJson, headerMap);

            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool isNetworkOrHttpError = req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError;
#else
            bool isNetworkOrHttpError = req.isNetworkError || req.isHttpError;
#endif

            string responseText = req.downloadHandler != null ? req.downloadHandler.text : null;
            if (isNetworkOrHttpError)
            {
                if (!string.IsNullOrEmpty(responseText))
                {
                    ValidateResponse maybe = TryParseValidateResponse(responseText);
                    if (maybe != null && !string.IsNullOrEmpty(maybe.error))
                    {
                        SetFlowText($"Server error: {maybe.error}");
                        yield break;
                    }
                }

                SetFlowText($"Network/HTTP error: {(long)req.responseCode} {req.error}");
                Debug.LogWarning($"ValidateToken HTTP error. Code: {req.responseCode}. Error: {req.error}. Resp: {responseText}");
                yield break;
            }

            if (string.IsNullOrEmpty(responseText))
            {
                SetFlowText("Empty response from server.");
                Debug.LogWarning("ValidateToken empty response.");
                yield break;
            }

            ValidateResponse resp = TryParseValidateResponse(responseText);
            if (resp == null)
            {
                SetFlowText("Failed to parse server response.");
                Debug.LogWarning($"ValidateToken parse fail. Raw response: {TruncateForLog(responseText)}");
                yield break;
            }

            if (resp.success)
            {
                SetFlowText($"Token valid. Ticket: {resp.serial_number} (id {resp.ticket_id}), user {resp.user_id}. Expires: {resp.expires_at}");

                // Save session data for further requests
                SessionData.TicketId = resp.ticket_id;
                SessionData.GameToken = gameToken;
                SessionData.BearerToken = _bearerToken;
                SessionData.UserId = resp.user_id; // ensure Supabase logging has correct user id

                if (_loginPanel != null)
                    _loginPanel.SetActive(false);

                // Notify GameManager that a new valid token is present
                var gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                    gm.OnNewTokenValidated();

                Debug.Log($"ValidateToken success. TicketId={resp.ticket_id}, UserId={resp.user_id}");
            }
            else
            {
                string msg = string.IsNullOrEmpty(resp.error) ? "Invalid token" : resp.error;
                SetFlowText($"Validation failed: {msg}");
                Debug.Log($"ValidateToken failure: {msg}. Full response: {TruncateForLog(responseText)}");
            }
        }
    }

    #region Helpers

    private void SetFlowText(string text)
    {
        if (_loginFlowText != null)
            _loginFlowText.text = text;
        else
            Debug.Log("[LoginHandler] " + text);
    }

    /// <summary>
    /// Logs: URL, BODY, HEADERS, cURL
    /// Works in Unity 6 (no GetRequestHeaders).
    /// </summary>
    private void LogRequestDebug(string url, string body, Dictionary<string, string> headers)
    {
        StringBuilder headersSb = new StringBuilder();
        foreach (var kv in headers)
            headersSb.AppendLine($"{kv.Key}: {kv.Value}");

        StringBuilder curl = new StringBuilder();
        curl.AppendLine($"curl -X POST \"{url}\" \\");
        foreach (var kv in headers)
            curl.AppendLine($"  -H \"{kv.Key}: {kv.Value}\" \\");
        curl.Append($"  -d '{body}'");

        Debug.Log(
$@"[LoginHandler REQUEST] -----------------------
URL:
{url}

BODY:
{body}

HEADERS:
{headersSb}

AS cURL:
{curl}

----------------------------------------------");
    }

    private ValidateResponse TryParseValidateResponse(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            var parsed = JsonUtility.FromJson<ValidateResponse>(json);
            if (parsed != null && (parsed.success || !string.IsNullOrEmpty(parsed.error) || parsed.ticket_id != 0))
                return parsed;
        }
        catch { }

        try
        {
            string lower = json.ToLowerInvariant();
            if (lower.Contains("\"success\":true"))
            {
                var tmp = new ValidateResponse() { success = true };
                tmp.ticket_id = TryExtractInt(json, "ticket_id");
                tmp.serial_number = TryExtractString(json, "serial_number");
                tmp.user_id = TryExtractInt(json, "user_id");
                tmp.status = TryExtractString(json, "status");
                tmp.expires_at = TryExtractString(json, "expires_at");
                return tmp;
            }
            else if (lower.Contains("\"success\":false"))
            {
                var tmp = new ValidateResponse() { success = false };
                tmp.error = TryExtractString(json, "error");
                return tmp;
            }
        }
        catch { }

        return null;
    }

    private int TryExtractInt(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
            return 0;
        string pattern = $"\"{key}\":";
        int idx = json.IndexOf(pattern);
        if (idx < 0)
            return 0;
        idx += pattern.Length;

        while (idx < json.Length && char.IsWhiteSpace(json[idx]))
            idx++;

        int start = idx;
        while (idx < json.Length && (char.IsDigit(json[idx]) || json[idx] == '-'))
            idx++;

        if (int.TryParse(json.Substring(start, idx - start), out int res))
            return res;

        return 0;
    }

    private string TryExtractString(string json, string key)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        string pattern = $"\"{key}\":";
        int idx = json.IndexOf(pattern);
        if (idx < 0)
            return null;
        idx += pattern.Length;

        while (idx < json.Length && json[idx] != '"' && json[idx] != '\'')
            idx++;
        if (idx >= json.Length)
            return null;

        char quote = json[idx++];
        int start = idx;

        while (idx < json.Length && json[idx] != quote)
            idx++;
        if (idx >= json.Length)
            return null;

        return json.Substring(start, idx - start);
    }

    private string TruncateForLog(string s, int max = 1024)
    {
        if (string.IsNullOrEmpty(s))
            return s;
        return s.Length <= max ? s : s.Substring(0, max) + "...";
    }

    #endregion

    #region DTOs

    [System.Serializable]
    private class ValidateRequest
    {
        public string game_token;
    }

    [System.Serializable]
    private class ValidateResponse
    {
        public bool success;
        public int ticket_id;
        public string serial_number;
        public int user_id;
        public string status;
        public string expires_at;
        public string error;
    }

    #endregion
}
