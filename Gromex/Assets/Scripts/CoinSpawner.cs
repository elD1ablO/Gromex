using System.Threading;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnTransforms;
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private float _initialSpawnInterval = 2f;
    [SerializeField] private float _initialCoinSpeed = 0f;

    private float _spawnIntervalSpeedUp = 0.1f;
    private float _coinSpeedUp = 0.2f;

    private float speedUpTimer;
    private float speedUpTresold = 30f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //randomly spawn coins at intervals
    }
}
