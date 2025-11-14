using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnTransforms;
    [SerializeField] private GameObject _coinPrefab;

    [Header("Initial values")]
    [Tooltip("Initial gravity scale for coins")]
    [SerializeField] private float _initialCoinSpeed = 1f; // used as gravityScale

    [Tooltip("Initial time between spawns")]
    [SerializeField] private float _initialSpawnInterval = 2f;

    [Header("Speed up settings")]
    [Tooltip("How much faster (interval smaller) every threshold")]
    [SerializeField] private float _spawnIntervalSpeedUp = 0.1f;

    [Tooltip("How much gravityScale grows every threshold")]
    [SerializeField] private float _coinSpeedUp = 0.2f;

    [Tooltip("Every N seconds difficulty increases")]
    [SerializeField] private float _speedUpTreshold = 30f;

    // runtime values
    private float _currentSpawnInterval;
    private float _currentCoinSpeed;   // used as current gravityScale

    private float _spawnTimer;
    private float _speedUpTimer;

    private bool _isSpawning;
    private void Start()
    {
        _currentSpawnInterval = _initialSpawnInterval;
        _currentCoinSpeed = _initialCoinSpeed;
    }

    private void Update()
    {
        if (!_isSpawning || _spawnTransforms == null || _spawnTransforms.Length == 0 || _coinPrefab == null)
            return;

        // spawn timer
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= _currentSpawnInterval)
        {
            _spawnTimer = 0f;
            SpawnCoin();
        }

        // difficulty / speed-up timer
        _speedUpTimer += Time.deltaTime;
        if (_speedUpTimer >= _speedUpTreshold)
        {
            _speedUpTimer = 0f;

            // spawn more often (interval smaller, but not below some minimum)
            _currentSpawnInterval = Mathf.Max(0.2f, _currentSpawnInterval - _spawnIntervalSpeedUp);

            // increase gravityScale for newly spawned coins
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
    }

    public void StopCoinSpawning()
    {
        _isSpawning = false;
    }

    private void SpawnCoin()
    {
        Transform spawnPoint = _spawnTransforms[Random.Range(0, _spawnTransforms.Length)];
        GameObject coin = Instantiate(_coinPrefab, spawnPoint.position, spawnPoint.rotation);

        Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // reset any previous motion (if prefab had some)
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // let gravity + slope do all the work
            rb.gravityScale = _currentCoinSpeed;
        }
    }
}
