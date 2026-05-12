using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GazeTimerButton : MonoBehaviour
{


    [Header("Gaze Settings")]
    public float gazeTimeRequired = 3f;

    [Header("Colores del cubo")]
    public Color normalColor    = new Color(0.20f, 0.50f, 1.00f, 1f);
    public Color gazeColor      = new Color(1.00f, 0.80f, 0.00f, 1f);
    public Color activatedColor = new Color(0.00f, 0.90f, 0.30f, 1f);

    [Header("Colores de la barra de progreso")]
    public Color barStartColor  = new Color(0.00f, 0.85f, 1.00f, 1f);
    public Color barEndColor    = new Color(0.00f, 1.00f, 0.40f, 1f);

    [Header("Acción al confirmar")]
    public string targetScene  = "";
    public bool   isBackButton = false;
    public string difficulty   = "";



    private float _gazeTimer;
    private bool _confirmed;
    private Renderer _rend;
    private Material _cubeMat;
    private Vector3 _baseScale;
    private Shader _urpShader;
    private GameObject _barBg;
    private GameObject _barFill;
    private Material _fillMat;

    const float BAR_W = 0.86f;
    const float BAR_H = 0.07f;
    const float BAR_Y = -0.42f;
    const float BAR_Z = -0.52f;
    const float FILL_Z = -0.53f;



    void Awake()
    {
        _rend = GetComponent<Renderer>();
        _baseScale = transform.localScale;

        _urpShader = Shader.Find("Universal Render Pipeline/Unlit")
                  ?? Shader.Find("Unlit/Color")
                  ?? Shader.Find("Standard");

        _cubeMat = new Material(_urpShader);
        _rend.material = _cubeMat;

        SetCubeColor(normalColor);
        BuildProgressBar();
    }

    void Update()
    {
        if (_confirmed) return;

        bool gazed = IsBeingGazed();

        if (gazed)
        {
            _gazeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_gazeTimer / gazeTimeRequired);


            SetCubeColor(Color.Lerp(normalColor, gazeColor, t));
            transform.localScale = _baseScale * (1f + 0.10f * Mathf.Sin(t * Mathf.PI));

            ShowBar(true);
            UpdateBar(t);

            if (_gazeTimer >= gazeTimeRequired)
                Confirm();
        }
        else
        {
            _gazeTimer = 0f;
            SetCubeColor(normalColor);
            transform.localScale = _baseScale;
            ShowBar(false);
        }
    }

    bool IsBeingGazed()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        return Physics.Raycast(ray, out RaycastHit hit, 50f) && hit.transform == transform;
    }

    void Confirm()
    {
        _confirmed = true;
        SetCubeColor(activatedColor);
        transform.localScale = _baseScale;

        UpdateBar(1f);
        _fillMat.color = barEndColor;

        if (!string.IsNullOrEmpty(difficulty))
            DifficultyManager.SetDifficulty(difficulty);

        string scene = isBackButton ? "MainMenu" : targetScene;
        if (!string.IsNullOrEmpty(scene))
            StartCoroutine(LoadAfterDelay(scene, 0.5f));
        else
            Debug.Log(gameObject.name + " confirmado, sin escena asignada");
    }

    IEnumerator LoadAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    void BuildProgressBar()
    {
        _barBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _barBg.name = "ProgressBar_BG";
        _barBg.transform.SetParent(transform);
        _barBg.transform.localPosition = new Vector3(0f, BAR_Y, BAR_Z);
        _barBg.transform.localScale    = new Vector3(BAR_W, BAR_H, 0.01f);


        var bgCol = _barBg.GetComponent<BoxCollider>();
        if (bgCol != null) bgCol.enabled = false;

        var bgMat = new Material(_urpShader);
        bgMat.color = new Color(0.08f, 0.08f, 0.08f, 1f);
        _barBg.GetComponent<Renderer>().material = bgMat;

        _barFill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _barFill.name = "ProgressBar_Fill";
        _barFill.transform.SetParent(transform);
        _barFill.transform.localPosition = new Vector3(-BAR_W * 0.5f, BAR_Y, FILL_Z);
        _barFill.transform.localScale = new Vector3(0.001f, BAR_H * 0.85f, 0.01f);

        var fillCol = _barFill.GetComponent<BoxCollider>();
        if (fillCol != null) fillCol.enabled = false;

        _fillMat = new Material(_urpShader);
        _fillMat.color = barStartColor;
        _barFill.GetComponent<Renderer>().material = _fillMat;

        ShowBar(false);
    }

    void UpdateBar(float t)
    {
        float fillW = Mathf.Max(0.001f, t * BAR_W);
        float centerX = -BAR_W * 0.5f + fillW * 0.5f;

        _barFill.transform.localPosition = new Vector3(centerX, BAR_Y, FILL_Z);
        _barFill.transform.localScale = new Vector3(fillW, BAR_H * 0.85f, 0.01f);

        _fillMat.color = Color.Lerp(barStartColor, barEndColor, t);
    }

    void ShowBar(bool show)
    {
        if (_barBg != null) _barBg.SetActive(show);
        if (_barFill != null) _barFill.SetActive(show);
    }

    void SetCubeColor(Color c) => _cubeMat.color = c;

    public void ResetButton()
    {
        _confirmed = false;
        _gazeTimer = 0f;
        SetCubeColor(normalColor);
        transform.localScale = _baseScale;
        ShowBar(false);
    }
}
