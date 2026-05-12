using UnityEngine;
public class Obstacle : MonoBehaviour
{
    public float speed = 3f;
    private Transform _player;
    private bool _resolved;

    void Start()
    {

        var playerGO = UnityEngine.GameObject.FindWithTag("Player");
        if (playerGO == null) playerGO = UnityEngine.GameObject.Find("Main Camera");
        _player = playerGO != null ? playerGO.transform : null;
    }

    void Update()
    {
        transform.position += Vector3.back * speed * Time.deltaTime;
        float destroyZ = _player != null ? _player.position.z - 15f : -5f;
        if (!_resolved && transform.position.z < destroyZ)
        {
            _resolved = true;
            if (GameManager.Instance != null) GameManager.Instance.RegisterDodge();
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_resolved) return;
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            _resolved = true;
            if (GameManager.Instance != null) GameManager.Instance.RegisterHit();
            Destroy(gameObject);
        }
    }
}
