using UnityEngine;

public class CatchZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController.OnCoinCatch?.Invoke();
        Destroy(other.gameObject);
    }
}