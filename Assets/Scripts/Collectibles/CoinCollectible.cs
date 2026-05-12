using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float rotateSpeed = 200f;

    [Header("Reward")]
    public int bonusPoints = 5;

    private Transform _player;
    private bool _collected;

    void Start()
    {
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null) playerGO = GameObject.Find("Main Camera");
        _player = playerGO != null ? playerGO.transform : null;

        if (DifficultyConfig.Instance != null && DifficultyConfig.Instance.Current != null)
            speed = DifficultyConfig.Instance.Current.obstacleSpeed;
    }

    void Update()
    {
        transform.position += Vector3.back * speed * Time.deltaTime;
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        float destroyZ = _player != null ? _player.position.z - 12f : -5f;
        if (transform.position.z < destroyZ)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_collected) return;
        if (!other.CompareTag("Player") && !other.CompareTag("MainCamera")) return;

        _collected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AddBonus(bonusPoints);

        BurstSparkle(transform.position);
        Destroy(gameObject);
    }

    static void BurstSparkle(Vector3 pos)
    {
        var fxGO = new GameObject("CoinFX");
        fxGO.transform.position = pos;
        var ps = fxGO.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration        = 0.5f;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(1f, 0.95f, 0.1f),
                                   new Color(1f, 0.55f, 0f));
        main.gravityModifier = 0.25f;
        main.maxParticles    = 40;
        main.stopAction      = ParticleSystemStopAction.Destroy;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });

        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius    = 0.15f;

        ps.Play();
    }
}
