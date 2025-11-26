using UnityEngine;

public class CatchZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Coin coin = other.GetComponent<Coin>();
        if (coin == null)
            return;

        if (coin.IsFailCoin)
        {
            // Fail coin caught:
            // - Time mode: -5s
            // - Lives mode: -1 life
            PlayerController.OnFailCatch?.Invoke();
        }
        else
        {
            // Normal coin caught:
            // - Time mode: +1 score, no penalty for miss
            // - Lives mode: +1 score
            PlayerController.OnCoinCatch?.Invoke();
        }

        Destroy(coin.gameObject);
    }
}