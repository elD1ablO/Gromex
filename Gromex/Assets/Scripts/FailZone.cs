using UnityEngine;

public class FailZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {        
        PlayerController.OnFailCatch?.Invoke();
    }
}
