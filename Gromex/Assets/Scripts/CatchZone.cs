using UnityEngine;
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
            // Bad coin caught:
            // - Time mode: -5 seconds
            // - Lives mode: -1 life
            PlayerController.OnFailCatch?.Invoke();

            // Glitch FX + self destroy
            coin.PlayFailCoinCaughtEffect();
        }
        else
        {
            // Normal coin caught:
            // - Time mode: +1 score
            // - Lives mode: +1 score
            PlayerController.OnCoinCatch?.Invoke();

            // Jump + shine FX + self destroy
            coin.PlayCatchSuccessEffect();
        }
    }
}
