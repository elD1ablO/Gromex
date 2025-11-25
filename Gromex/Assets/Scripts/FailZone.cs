using UnityEngine;

public class FailZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var coin = other.GetComponent<Coin>();
        if (coin == null)
            return;

        coin.HandleMissed();
    }
}