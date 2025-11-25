using UnityEngine;

public class Coin : MonoBehaviour
{
    public enum CoinType
    {
        Normal,
        Fail
    }

    [SerializeField] private CoinType _type = CoinType.Normal;

    public void HandleCaught()
    {
        switch (_type)
        {
            case CoinType.Normal:
                // Normal coin caught: add score
                PlayerController.OnCoinCatch?.Invoke();
                break;

            case CoinType.Fail:
                // Fail coin caught: lose life
                PlayerController.OnFailCatch?.Invoke();
                break;
        }

        Destroy(gameObject);
    }

    public void HandleMissed()
    {
        switch (_type)
        {
            case CoinType.Normal:
                // Normal coin missed: lose life
                PlayerController.OnFailCatch?.Invoke();
                break;

            case CoinType.Fail:
                // Fail coin missed: nothing happens
                break;
        }

        Destroy(gameObject);
    }
}
