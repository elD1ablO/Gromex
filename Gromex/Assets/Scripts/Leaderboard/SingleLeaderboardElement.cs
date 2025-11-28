using TMPro;
using UnityEngine;

public class SingleLeaderboardElement : MonoBehaviour
{
    [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _usernameText;
    [SerializeField] private TMP_Text _scoreText;

    public void Setup(int rank, string username, int score)
    {
        if (_rankText != null)
            _rankText.text = $"{rank.ToString()}.";

        if (_usernameText != null)
            _usernameText.text = username;

        if (_scoreText != null)
            _scoreText.text = score.ToString();
    }
}