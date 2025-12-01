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
        bool isFailCoin = coin.IsFailCoin || other.GetComponent<FailCoin>() != null;

        if (!isFailCoin)
        {
            // ---------------- NORMAL COIN MISSED ----------------
            if (!isTimeMode)
            {                
                PlayerController.OnFailCatch?.Invoke();
            }
            else
            {
                
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayBadCoinSfx();
            }

            
            coin.PlayMissEffect();
        }
        else
        {
            
            coin.PlayFailCoinMissEffect();

        }

    }
}


