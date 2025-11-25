using UnityEngine;

public class FailCoin : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerCatch"))
        {
            PlayerController.OnFailCatch?.Invoke();
            Destroy(gameObject);
        }
    }
}