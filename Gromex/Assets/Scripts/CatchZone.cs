using UnityEngine;

public class CatchZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to objects that have Coin component
        var coin = other.GetComponent<Coin>();
        if (coin == null)
            return;

        coin.HandleCaught();
    }
}
