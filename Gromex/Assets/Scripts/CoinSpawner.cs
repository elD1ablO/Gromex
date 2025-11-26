using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnTransforms;
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private GameObject _failCoinPrefab;

    [Header("Fail coin chance (0..1)")]
    [SerializeField] private float _failCoinChance = 0.2f;

    [Header("Initial values")]
    [SerializeField] private float _initialCoinSpeed = 1f;
    [SerializeField] private float _initialSpawnInterval = 2f;

    [Header("Speed up settings")]
    [SerializeField] private float _spawnIntervalSpeedUp = 0.1f;
    [SerializeField] private float _coinSpeedUp = 0.2f;
    [SerializeField] private float _speedUpTreshold = 30f;

    private float _currentSpawnInterval;
    private float _currentCoinSpeed;

    private float _spawnTimer;
    private float _speedUpTimer;

    private bool _isSpawning;

    private readonly List<GameObject> _spawnedCoins = new List<GameObject>();
    private readonly List<GameObject> _spawnedFailCoins = new List<GameObject>();

    private void Start()
    {
        _currentSpawnInterval = _initialSpawnInterval;
        _currentCoinSpeed = _initialCoinSpeed;
    }

    private void Update()
    {
        if (!_isSpawning || _spawnTransforms == null || _spawnTransforms.Length == 0)
            return;

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= _currentSpawnInterval)
        {
            _spawnTimer = 0f;
            SpawnRandomCoin();
        }

        _speedUpTimer += Time.deltaTime;
        if (_speedUpTimer >= _speedUpTreshold)
        {
            _speedUpTimer = 0f;

            _currentSpawnInterval = Mathf.Max(0.2f, _currentSpawnInterval - _spawnIntervalSpeedUp);
            _currentCoinSpeed += _coinSpeedUp;
        }
    }

    public void StartCoinSpawning()
    {
        _isSpawning = true;
        _spawnTimer = 0f;
        _speedUpTimer = 0f;

        _currentSpawnInterval = _initialSpawnInterval;
        _currentCoinSpeed = _initialCoinSpeed;

        ReleaseStaticCoins();
    }

    public void StopCoinSpawning()
    {
        _isSpawning = false;
    }

    private void SpawnRandomCoin()
    {
        float r = Random.value;

        if (r <= _failCoinChance && _failCoinPrefab != null)
            SpawnFailCoin();
        else
            SpawnCoin();
    }

    private void SpawnCoin()
    {
        Transform spawn = _spawnTransforms[Random.Range(0, _spawnTransforms.Length)];
        GameObject coin = Instantiate(_coinPrefab, spawn.position, spawn.rotation);
        _spawnedCoins.Add(coin);

        SetCoinPhysics(coin);
    }

    private void SpawnFailCoin()
    {
        if (_failCoinPrefab == null)
        {
            SpawnCoin();
            return;
        }

        Transform spawn = _spawnTransforms[Random.Range(0, _spawnTransforms.Length)];
        GameObject coin = Instantiate(_failCoinPrefab, spawn.position, spawn.rotation);

        _spawnedFailCoins.Add(coin);

        SetCoinPhysics(coin);

    }

    private void SetCoinPhysics(GameObject coin)
    {
        Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
            rb.gravityScale = _currentCoinSpeed;
        }
    }

    public void SpawnPreviewCoin()
    {
        Transform spawn = _spawnTransforms[Random.Range(0, _spawnTransforms.Length)];
        GameObject coin = Instantiate(_coinPrefab, spawn.position, spawn.rotation);

        _spawnedCoins.Add(coin);

        Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void ReleaseStaticCoins()
    {
        foreach (var c in _spawnedCoins)
        {
            if (c == null)
                continue;
            Rigidbody2D rb = c.GetComponent<Rigidbody2D>();
            if (rb == null)
                continue;

            if (!rb.simulated || Mathf.Approximately(rb.gravityScale, 0))
            {
                rb.simulated = true;
                rb.gravityScale = _currentCoinSpeed;
            }
        }
    }

    public void ClearAllCoins()
    {
        for (int i = _spawnedCoins.Count - 1; i >= 0; i--)
            if (_spawnedCoins[i] != null)
                Destroy(_spawnedCoins[i]);

        for (int i = _spawnedFailCoins.Count - 1; i >= 0; i--)
            if (_spawnedFailCoins[i] != null)
                Destroy(_spawnedFailCoins[i]);

        _spawnedCoins.Clear();
        _spawnedFailCoins.Clear();
    }
}
