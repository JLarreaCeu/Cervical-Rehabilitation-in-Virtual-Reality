using UnityEngine;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(LineRenderer))]
public class LaserPointer : MonoBehaviour
{
    [Header("References")]
    public Transform mazePlane;
    public Transform laserDot;
    public Transform exitStar;
    public Transform guideRing;
    public Texture2D mazeTexture;

    [Header("Movement")]
    [Tooltip("UV units per degree per second of head tilt")]
    public float sensitivity = 0.008f;
    [Tooltip("Degrees of tilt ignored to prevent drift")]
    public float deadZone    = 3f;

    [Header("Entry / Exit UV")]
    public float startU     = 0.037f;
    public float startV     = 0.970f;
    public float exitU      = 0.965f;
    public float exitV      = 0.035f;
    public float exitRadius = 0.035f;

    [Header("Grab to Navigate")]
    [Tooltip("Seconds the gaze must rest on the dot to grab it")]
    public float grabDwellTime = 1.5f;
    [Tooltip("UV-space radius around the dot that counts as 'looking at it'")]
    public float grabRadius    = 0.08f;

    [Header("Dot Colors")]
    public Color dotNormalColor  = new Color(1f, 0f,   0f,   1f);
    public Color dotWallColor    = new Color(1f, 0.9f, 0.1f, 1f);
    public Color dotUngrabColor  = new Color(1f, 0.5f, 0f,   1f);
    public Color dotDwellColor   = new Color(0f, 1f,   0f,   1f);

    [Header("Wall Detection")]
    [Range(0f, 1f)]
    public float wallThreshold = 0.45f;

    public bool IsOnWall  { get; private set; }
    public bool IsAtExit  { get; private set; }
    public bool IsGrabbed { get; private set; }

    float      _u, _v;
    Quaternion _calibRot;
    Quaternion _displayCalib; // head rotation when maze appeared, used for guide ring offset
    bool       _calibrated;
    Material   _dotMat;
    float      _dwellTimer;
    bool       _waitingForCenter; // true until player looks at maze center after each maze load
    const float CENTER_RADIUS = 0.12f; // UV-space radius around (0.5,0.5) that counts as centered

    void Awake()
    {
        var lr = GetComponent<LineRenderer>();
        lr.enabled = false;

        if (laserDot != null)
        {
            var dr = laserDot.GetComponent<Renderer>();
            if (dr != null)
            {
                Shader sh = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Standard");
                _dotMat       = new Material(sh);
                _dotMat.color = dotUngrabColor;
                dr.material   = _dotMat;
            }
            laserDot.localScale = Vector3.one * 0.03f;
        }

        if (exitStar != null)
        {
            var sr = exitStar.GetComponent<Renderer>();
            if (sr != null)
            {
                Shader sh = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Standard");
                var sm = new Material(sh);
                sm.color = new Color(1f, 0.85f, 0f, 1f);
                sr.material = sm;
            }
            exitStar.localScale = Vector3.one * 0.06f;
        }
    }

    void Start()
    {
        ResetToStart();
        CalibrateCenter();
        _displayCalib = _calibRot;
    }

    public void CalibrateCenter()
    {
        _calibRot   = ReadHeadRot();
        _calibrated = true;
    }

    public void ResetToStart()
    {
        _u               = startU;
        _v               = startV;
        IsGrabbed        = false;
        IsAtExit         = false;
        IsOnWall         = false;
        _dwellTimer      = 0f;
        _waitingForCenter = true;
        // Hide dot until player centers gaze - forces a neutral starting head position.
        if (laserDot != null) laserDot.gameObject.SetActive(false);
        PositionDot();
        PositionStar();
        if (guideRing != null) guideRing.gameObject.SetActive(true);
    }

    public void SetMaze(Texture2D tex)
    {
        mazeTexture = tex;
        ResetToStart();
        CalibrateCenter();
        _displayCalib = _calibRot; // update reference for guide ring on new maze
    }

    void Update()
    {
        if (!_calibrated) return;

        // Center-wait: guide ring tracks raw gaze so the player knows where to look.
        // Once they hit center, lock _displayCalib as the neutral reference.
        if (_waitingForCenter)
        {
            _displayCalib = ReadHeadRot(); // raw position, no delta

            float gazeU, gazeV;
            ComputeGazeUV(out gazeU, out gazeV);

            float dx = gazeU - 0.5f, dy = gazeV - 0.5f;
            if (dx * dx + dy * dy < CENTER_RADIUS * CENTER_RADIUS)
            {
                // Player centered, lock calibration and show the dot.
                _waitingForCenter = false;
                if (laserDot != null) laserDot.gameObject.SetActive(true);
                PositionDot();
            }
        }

        if (exitStar != null)
        {
            float pulse = 1f + 0.2f * Mathf.Sin(Time.time * 5f);
            exitStar.localScale = Vector3.one * 0.06f * pulse;
            PositionStar(); // keep star on the maze every frame (maze follows camera)
        }

        if (guideRing != null)
        {
            if (IsGrabbed)
            {
                if (guideRing.gameObject.activeSelf)
                    guideRing.gameObject.SetActive(false);
            }
            else
            {
                if (!guideRing.gameObject.activeSelf)
                    guideRing.gameObject.SetActive(true);

                float gazeU, gazeV;
                ComputeGazeUV(out gazeU, out gazeV);

                float dx = gazeU - _u;
                float dy = gazeV - _v;
                bool nearDot = (dx * dx + dy * dy) < (grabRadius * grabRadius);
                float ringPulse = nearDot
                    ? 1f + 0.12f * Mathf.Sin(Time.time * 8f)
                    : 1f + 0.35f * Mathf.Sin(Time.time * 3.5f);
                guideRing.localScale = Vector3.one * ringPulse;

                if (mazePlane != null)
                {
                    Vector3 localPos = new Vector3(gazeU - 0.5f, gazeV - 0.5f, -0.025f);
                    guideRing.position = mazePlane.TransformPoint(localPos);
                    guideRing.rotation = mazePlane.rotation;
                }
            }
        }

        if (_waitingForCenter) return; // dot hidden, navigation blocked until centered

        if (!IsGrabbed)
        {
            UpdateUngrabbed();
            return;
        }

        UpdateGrabbed();
    }

    void UpdateUngrabbed()
    {
        float gazeU, gazeV;
        ComputeGazeUV(out gazeU, out gazeV);

        float dx = gazeU - _u;
        float dy = gazeV - _v;
        bool gazeOnDot = (dx * dx + dy * dy) < (grabRadius * grabRadius);

        if (gazeOnDot)
        {
            // Ring reached the dot, grab instantly.
            IsGrabbed = true;
            if (_dotMat != null) _dotMat.color = dotNormalColor;
            if (laserDot != null) laserDot.localScale = Vector3.one * 0.03f;
        }
        else
        {
            // Idle pulse while waiting for player to look at the dot.
            float pulse = 0.8f + 0.2f * Mathf.Sin(Time.time * 4f);
            if (_dotMat != null)
                _dotMat.color = new Color(dotUngrabColor.r, dotUngrabColor.g * pulse,
                                          dotUngrabColor.b, 1f);
            if (laserDot != null)
                laserDot.localScale = Vector3.one * (0.025f + 0.005f * Mathf.Sin(Time.time * 4f));
        }

        PositionDot();
    }

    void UpdateGrabbed()
    {
        // Ball tracks gaze directly (same as guide ring) but stops at walls.
        float gazeU, gazeV;
        ComputeGazeUV(out gazeU, out gazeV);

        float du = gazeU - _u;
        float dv = gazeV - _v;

        const float STEP = 0.002f;
        int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(Mathf.Abs(du), Mathf.Abs(dv)) / STEP));
        float su = du / steps;
        float sv = dv / steps;
        bool hitWall = false;

        for (int i = 0; i < steps; i++)
        {
            float nu = Mathf.Clamp01(_u + su);
            float nv = Mathf.Clamp01(_v + sv);

            bool blockU = SampleWall(nu, _v);
            bool blockV = SampleWall(_u, nv);

            if (!blockU) _u = nu;
            if (!blockV) _v = nv;

            if (blockU || blockV) hitWall = true;
            if (blockU && blockV) break;
        }

        IsOnWall = hitWall;

        if (_dotMat != null)
            _dotMat.color = IsOnWall ? dotWallColor : dotNormalColor;
        if (laserDot != null)
            laserDot.localScale = Vector3.one * 0.03f;

        PositionDot();

        float ex = _u - exitU;
        float ey = _v - exitV;
        IsAtExit = (ex * ex + ey * ey) < (exitRadius * exitRadius);
    }

    void ComputeGazeUV(out float u, out float v)
    {
        u = _u;
        v = _v;
        if (mazePlane == null) return;

        // Use head rotation delta from display calibration. Absolute direction always
        // points at maze center when the plane follows the camera, so delta is what we need.
        Quaternion delta   = Quaternion.Inverse(_displayCalib) * ReadHeadRot();
        Vector3    gazeDir = delta * Vector3.forward;

        Camera cam = Camera.main;
        if (cam == null) return;

        float dist = Vector3.Distance(cam.transform.position, mazePlane.position);
        if (dist < 0.01f || gazeDir.z < 0.001f) return;

        float t    = dist / gazeDir.z;
        float hitX = gazeDir.x * t;
        float hitY = gazeDir.y * t;

        float halfW = mazePlane.lossyScale.x * 0.5f;
        float halfH = mazePlane.lossyScale.y * 0.5f;

        u = Mathf.Clamp01(hitX / (halfW * 2f) + 0.5f);
        v = Mathf.Clamp01(hitY / (halfH * 2f) + 0.5f);
    }

    bool SampleWall(float u, float v)
    {
        if (mazeTexture == null || !mazeTexture.isReadable) return false;
        int px = Mathf.Clamp(Mathf.RoundToInt(u * (mazeTexture.width  - 1)), 0, mazeTexture.width  - 1);
        int py = Mathf.Clamp(Mathf.RoundToInt(v * (mazeTexture.height - 1)), 0, mazeTexture.height - 1);
        Color c = mazeTexture.GetPixel(px, py);
        return (c.r + c.g + c.b) / 3f < wallThreshold;
    }

    void PositionDot()
    {
        if (laserDot == null || mazePlane == null) return;
        Vector3 localPos = new Vector3(_u - 0.5f, _v - 0.5f, -0.015f);
        laserDot.position = mazePlane.TransformPoint(localPos);
    }

    void PositionStar()
    {
        if (exitStar == null || mazePlane == null) return;
        Vector3 localPos = new Vector3(exitU - 0.5f, exitV - 0.5f, -0.02f);
        exitStar.position = mazePlane.TransformPoint(localPos);
    }

    static Quaternion ReadHeadRot()
    {
        var hmd = UnityEngine.InputSystem.InputSystem.GetDevice<XRHMD>();
        if (hmd != null)
        {
            Quaternion r = hmd.centerEyeRotation.ReadValue();
            if (r != Quaternion.identity) return r;
        }
        Camera cam = Camera.main;
        return cam != null ? cam.transform.rotation : Quaternion.identity;
    }
}
