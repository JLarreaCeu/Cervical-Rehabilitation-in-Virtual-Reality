using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Apple : MonoBehaviour, IGazeable
{
    [Header("Animation")]
    public float bobSpeed  = 1.5f;
    public float bobHeight = 0.06f;

    [Header("Points")]
    public int pointValue = 10;

    [Header("Game Flow")]
    [Tooltip("Tick this on the last apple in the scene. Collecting it ends the game.")]
    public bool triggersFinalMenu = false;


    private Vector3 _basePos;
    private Vector3 _baseScale;
    private bool _collected;
    private Renderer _bodyRend;
    private Material _bodyMat;

    static readonly Color NormalColor = new Color(1.00f, 0.84f, 0.00f, 1f);
    static readonly Color GazeColor = new Color(1.00f, 0.95f, 0.20f, 1f);

    void Awake()
    {
        _basePos   = transform.position;
        _baseScale = transform.localScale;
        _bodyRend  = GetComponentInChildren<Renderer>();
        if (_bodyRend != null) _bodyMat = _bodyRend.material;

        SetBodyColor(NormalColor, emit: false);
    }

    void Update()
    {
        if (_collected) return;
        float y = _basePos.y + Mathf.Sin(Time.time * bobSpeed + transform.position.z * 0.3f) * bobHeight;
        transform.position = new Vector3(_basePos.x, y, _basePos.z);
    }



    public void OnGazeEnter()
    {
        if (_collected) return;
        SetBodyColor(GazeColor, emit: true);
    }

    public void OnGazeStay(float progress)
    {
        if (_collected) return;
        float s = 1f + 0.15f * progress;
        transform.localScale = _baseScale * s;
        Color c = Color.Lerp(GazeColor, new Color(1f, 1f, 0.5f), progress);
        SetBodyColor(c, emit: true, emitIntensity: progress * 0.5f);
    }

    public void OnGazeExit()
    {
        if (_collected) return;
        transform.localScale = _baseScale;
        SetBodyColor(NormalColor, emit: false);
    }

    public void OnGazeSelect()
    {
        if (_collected) return;
        _collected = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectApple();
            if (triggersFinalMenu)
                GameManager.Instance.ShowResults();
        }

        SpawnFX(transform.position);
        gameObject.SetActive(false);
    }



    void SetBodyColor(Color c, bool emit, float emitIntensity = 0.3f)
    {
        if (_bodyMat == null) return;
        _bodyMat.color = c;
        if (_bodyMat.HasProperty("_BaseColor"))
            _bodyMat.SetColor("_BaseColor", c);
        if (_bodyMat.HasProperty("_EmissionColor"))
        {
            if (emit)
            {
                _bodyMat.EnableKeyword("_EMISSION");
                _bodyMat.SetColor("_EmissionColor", c * emitIntensity);
            }
            else
            {
                _bodyMat.DisableKeyword("_EMISSION");
            }
        }
    }

    static void SpawnFX(Vector3 pos)
    {
        var fxGO = new GameObject("AppleFX");
        fxGO.transform.position = pos;
        var ps = fxGO.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration        = 0.5f;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(1f, 0.84f, 0.0f),
                                   new Color(1f, 1f,  0.4f));
        main.gravityModifier = 0.6f;
        main.stopAction      = ParticleSystemStopAction.Destroy;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius    = 0.1f;

        ps.Play();
    }
}
