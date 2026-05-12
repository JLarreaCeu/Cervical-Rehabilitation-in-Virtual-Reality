using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float spawnZ = 22f;
    public float lateralRange = 3f;
    public float verticalMin = 1.5f;
    public float verticalMax = 6f;

    private float _spawnInterval = 3f;
    private float _obstacleSpeed = 3f;
    private float _timer;
    private int _spawned;
    private int _total = 10;

    void Start()
    {
        if (DifficultyConfig.Instance != null && DifficultyConfig.Instance.Current != null)
        {
            _spawnInterval = DifficultyConfig.Instance.Current.obstacleSpawnInterval;
            _obstacleSpeed = DifficultyConfig.Instance.Current.obstacleSpeed;
        }
        if (GameManager.Instance != null)
        {
            _total = GameManager.Instance.totalPlanes;
            GameManager.Instance.StartGame();
        }
    }

    void Update()
    {
        if (_spawned >= _total) { enabled = false; return; }
        _timer += Time.deltaTime;
        if (_timer >= _spawnInterval)
        {
            _timer = 0f;
            SpawnObstacle();
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefab == null) return;
        float x = Random.Range(-lateralRange, lateralRange);
        float y = Random.Range(verticalMin, verticalMax);
        var go = Instantiate(obstaclePrefab, new Vector3(x, y, transform.position.z + spawnZ), Quaternion.identity);
        var obs = go.GetComponent<Obstacle>();
        if (obs == null) obs = go.AddComponent<Obstacle>();
        obs.speed = _obstacleSpeed;
        _spawned++;
    }
}
