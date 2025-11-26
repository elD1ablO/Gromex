using UnityEngine;

public class FailZone : MonoBehaviour
{
    private GameManager _gameManager;

    private void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Coin coin = other.GetComponent<Coin>();
        if (coin == null)
            return;

        bool isTimeMode = _gameManager != null && _gameManager.IsTimeMode;

        if (!coin.IsFailCoin)
        {
            // Normal coin missed:
            // - Time mode: nothing
            // - Lives mode: -1 life
            if (!isTimeMode)
            {
                PlayerController.OnFailCatch?.Invoke();
            }
        }
        // Fail coin missed:
        // - Time mode: NOTHING
        // - Lives mode: NOTHING

        Destroy(coin.gameObject);
    }
}