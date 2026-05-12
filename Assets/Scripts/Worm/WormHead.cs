using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class WormHead : MonoBehaviour, IGazeable
{
    [Header("Visuals")]
    public Color normalColor = new Color(0.20f, 0.70f, 0.20f, 1f);
    public Color gazeColor   = new Color(0.60f, 1.00f, 0.30f, 1f);
    public Color lostColor   = new Color(0.90f, 0.30f, 0.10f, 1f);
    public float pulseSpeed  = 3f;

    private Renderer _rend;
    private Material _mat;
    private bool _gazing;
    private bool _pulsing;
    private float _pulseTimer;

    void Awake()
    {
        _rend = GetComponent<Renderer>();
        if (_rend != null)
        {
            _mat = _rend.material;
            SetColor(normalColor);
        }
    }

    void Update()
    {
        if (!_pulsing || _gazing) return;
        _pulseTimer += Time.deltaTime * pulseSpeed;
        float t = (Mathf.Sin(_pulseTimer) + 1f) * 0.5f;
        SetColor(Color.Lerp(normalColor, lostColor, t));
    }

    public void OnGazeEnter()
    {
        _gazing = true; _pulsing = false;
        SetColor(gazeColor);
    }

    public void OnGazeStay(float progress)
    {
        SetColor(Color.Lerp(gazeColor, Color.white, progress * 0.3f));
    }

    public void OnGazeExit()
    {
        _gazing = false; _pulsing = true; _pulseTimer = 0f;
        SetColor(normalColor);
    }

    public void OnGazeSelect() {  }

    void SetColor(Color c)
    {
        if (_mat == null) return;
        _mat.color = c;
        if (_mat.HasProperty("_BaseColor")) _mat.SetColor("_BaseColor", c);
    }
}
