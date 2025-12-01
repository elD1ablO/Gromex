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
            // - Lives mode: lose life + play bad-coin sound via event
            // - Time mode: ONLY play bad-coin sound (no gameplay event)
            if (!isTimeMode)
            {
                
                PlayerController.OnFailCatch?.Invoke();
            }
            else
            {
                // тільки звук, без торкання логіки гри
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayBadCoinSfx();
            }
        }
        else
        {
            // Fail coin missed:
            // - Time mode: nothing
            // - Lives mode: nothing
        }

        Destroy(coin.gameObject);
    }
}

