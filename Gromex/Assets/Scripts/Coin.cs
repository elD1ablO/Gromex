using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin type")]
    [Tooltip("If true, this is a fail (red) coin. Otherwise this is a normal coin.")]
    [SerializeField] private bool _isFailCoin = false;

    /// <summary>
    /// True if this is a fail (red) coin.
    /// </summary>
    public bool IsFailCoin => _isFailCoin;
}
