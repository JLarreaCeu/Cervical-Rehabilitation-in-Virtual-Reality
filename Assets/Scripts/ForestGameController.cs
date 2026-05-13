using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Controls;

[DisallowMultipleComponent]
public class ForestGameController : MonoBehaviour
{
    public float walkSpeed = 8f;

    public float gazeDistance    = 12f;
    public float gazeTimeDefault = 1.0f;


    private IGazeable _current;
    private float _gazeTimer;
    private float _gazeTime;


    private float _startX;
    private float _startZ;
    private float _minZ;


    private Image     _crosshairFill;
    private Image     _crosshairDot;
    private Transform _crosshairCanvasT;



    void Awake()
    {
        _startX = transform.position.x;
        _startZ = transform.position.z;
        BuildCrosshair();
    }

    void OnEnable()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            var gs  = cam.GetComponent<GazeSelector>();
            if (gs  != null) gs.enabled  = false;
            var mgs = cam.GetComponent<MouseGazeSimulator>();
            if (mgs != null) mgs.enabled = false;
        }

        Vector3 p = transform.position;
        p.x = _startX;
        p.z = _startZ;
        transform.position = p;

        _minZ = _startZ;


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;


        _gazeTime = gazeTimeDefault;

        SetCrosshairProgress(0f, false);
    }

    void OnDisable()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            var gs  = cam.GetComponent<GazeSelector>();
            if (gs  != null) gs.enabled  = true;
            var mgs = cam.GetComponent<MouseGazeSimulator>();
            if (mgs != null) mgs.enabled = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        ExitGaze();
        SetCrosshairProgress(0f, false);

        Vector3 pos = transform.position;
        pos.x = _startX;
        pos.z = _startZ;
        transform.position = pos;
    }

    void Update()
    {
        MoveForward();
        DoGaze();
    }

    void LateUpdate()
    {
        // Lock X so movement stays strictly forward/backward.
        var pos = transform.position;
        pos.x = _startX;
        transform.position = pos;

        // Keep the world-space crosshair canvas centered in front of the camera.
        if (_crosshairCanvasT != null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                _crosshairCanvasT.position = cam.transform.position + cam.transform.forward * 1.5f;
                _crosshairCanvasT.rotation = cam.transform.rotation;
            }
        }
    }



    void MoveForward()
    {
        bool forward  = false;
        bool backward = false;

        // Editor / keyboard fallback
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) forward  = true;
            if (keyboard.sKey.isPressed) backward = true;
        }

        // VR: right controller B = forward, A = backward
        var rc = XRController.rightHand;
        if (rc != null)
        {
            var btnB = rc.TryGetChildControl<ButtonControl>("secondaryButton");
            var btnA = rc.TryGetChildControl<ButtonControl>("primaryButton");
            if (btnB != null && btnB.isPressed) forward  = true;
            if (btnA != null && btnA.isPressed) backward = true;
        }

        if (forward)
            transform.position += Vector3.forward * walkSpeed * Time.deltaTime;
        else if (backward)
            transform.position -= Vector3.forward * walkSpeed * Time.deltaTime;
    }

    void DoGaze()
    {
        Camera gazeCam = Camera.main;
        if (gazeCam == null) return;
        Ray ray = new Ray(gazeCam.transform.position, gazeCam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, gazeDistance))
        {
            var gazeable = hit.collider.GetComponentInParent<IGazeable>();
            if (gazeable != null)
            {

                if (gazeable != _current)
                {
                    ExitGaze();
                    _current    = gazeable;
                    _gazeTimer  = 0f;
                    _current.OnGazeEnter();
                }

                _gazeTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(_gazeTimer / _gazeTime);
                _current.OnGazeStay(progress);
                SetCrosshairProgress(progress, true);

                if (_gazeTimer >= _gazeTime)
                {
                    _current.OnGazeSelect();
                    ExitGaze();
                }
                return;
            }
        }

        ExitGaze();
        SetCrosshairProgress(0f, false);
    }

    void ExitGaze()
    {
        if (_current == null) return;
        _current.OnGazeExit();
        _current   = null;
        _gazeTimer = 0f;
    }

    void BuildCrosshair()
    {
        var cvGO  = new GameObject("CrosshairCanvas");
        cvGO.transform.SetParent(transform);
        var cv    = cvGO.AddComponent<Canvas>();
        // WorldSpace renders correctly in both eyes. Position updated each LateUpdate.
        cv.renderMode   = RenderMode.WorldSpace;
        cv.sortingOrder = 100;
        _crosshairCanvasT = cvGO.transform;

        var cvRT      = cvGO.GetComponent<RectTransform>();
        cvRT.sizeDelta   = new Vector2(200f, 200f); // canvas units
        cvRT.localScale  = Vector3.one * 0.001f;    // 1 canvas unit = 1 mm
        cvGO.AddComponent<UnityEngine.UI.CanvasScaler>();


        var ringGO = new GameObject("Ring");
        ringGO.transform.SetParent(cvGO.transform, false);
        var ringRT = ringGO.AddComponent<RectTransform>();
        ringRT.sizeDelta = new Vector2(36f, 36f);
        ringRT.anchorMin = ringRT.anchorMax = new Vector2(0.5f, 0.5f);
        ringRT.anchoredPosition = Vector2.zero;

        _crosshairFill = ringGO.AddComponent<Image>();
        _crosshairFill.sprite        = MakeCircleSprite(64);
        _crosshairFill.type          = Image.Type.Filled;
        _crosshairFill.fillMethod    = Image.FillMethod.Radial360;
        _crosshairFill.fillOrigin    = (int)Image.Origin360.Top;
        _crosshairFill.fillClockwise = true;
        _crosshairFill.fillAmount    = 0f;
        _crosshairFill.color         = new Color(1f, 1f, 1f, 0f);
        _crosshairFill.raycastTarget = false;


        var dotGO = new GameObject("Dot");
        dotGO.transform.SetParent(cvGO.transform, false);
        var dotRT = dotGO.AddComponent<RectTransform>();
        dotRT.sizeDelta = new Vector2(8f, 8f);
        dotRT.anchorMin = dotRT.anchorMax = new Vector2(0.5f, 0.5f);
        dotRT.anchoredPosition = Vector2.zero;

        _crosshairDot = dotGO.AddComponent<Image>();
        _crosshairDot.sprite        = MakeCircleSprite(32);
        _crosshairDot.color         = new Color(1f, 1f, 1f, 0.7f);
        _crosshairDot.raycastTarget = false;
    }

    void SetCrosshairProgress(float t, bool gazing)
    {
        if (_crosshairFill == null) return;
        _crosshairFill.fillAmount = t;

        Color ringColor = gazing
            ? Color.Lerp(new Color(1f, 1f, 1f, 0.3f), new Color(0.3f, 1f, 0.4f, 0.9f), t)
            : new Color(1f, 1f, 1f, 0f);
        _crosshairFill.color = ringColor;

        if (_crosshairDot != null)
            _crosshairDot.color = gazing
                ? Color.Lerp(new Color(1f, 1f, 1f, 0.7f), new Color(0.3f, 1f, 0.4f, 1f), t)
                : new Color(1f, 1f, 1f, 0.5f);
    }

    public void ResetAndStop()
    {
        Vector3 p = transform.position;
        p.x = _startX;
        p.z = _startZ;
        transform.position = p;
        enabled = false;
    }

    public void StopWalking()
    {
        enabled = false;
    }

    static Sprite MakeCircleSprite(int size)
    {
        var tex    = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - r + 0.5f, dy = y - r + 0.5f;
                float a  = Mathf.Clamp01((r - Mathf.Sqrt(dx * dx + dy * dy)) / 1.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
