using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Universal gaze dot for all menus. Created once at startup, persists across
/// scene loads (DontDestroyOnLoad). Follows Camera.main every frame.
/// Hides when GazeSelector is disabled (ForestGameController active gameplay).
/// </summary>
public class MenuGazeCursor : MonoBehaviour
{
    public float distance = 1.5f;

    static MenuGazeCursor _instance;

    Transform _canvasT;
    GameObject _canvasGO;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateOnce()
    {
        // Use Unity's == null which also catches destroyed objects (fake null).
        if (_instance != null && _instance) return;
        _instance = null;
        var go = new GameObject("GlobalMenuGazeCursor");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<MenuGazeCursor>();
    }

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        BuildDot();
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    void BuildDot()
    {
        _canvasGO = new GameObject("MenuGazeCursorCanvas");
        _canvasGO.transform.SetParent(transform);
        _canvasT  = _canvasGO.transform;

        var cv        = _canvasGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.WorldSpace;
        cv.sortingOrder = 200;
        _canvasGO.AddComponent<CanvasScaler>();

        var rt        = _canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta  = new Vector2(100f, 100f);
        rt.localScale = Vector3.one * 0.001f;

        var dotGO              = new GameObject("Dot");
        dotGO.transform.SetParent(_canvasGO.transform, false);
        var dotRT              = dotGO.AddComponent<RectTransform>();
        dotRT.sizeDelta        = new Vector2(7f, 7f);
        dotRT.anchorMin        = dotRT.anchorMax = new Vector2(0.5f, 0.5f);
        dotRT.anchoredPosition = Vector2.zero;

        var img           = dotGO.AddComponent<Image>();
        img.sprite        = MakeCircleSprite(32);
        img.color         = new Color(1f, 1f, 1f, 0.85f);
        img.raycastTarget = false;
    }

    void LateUpdate()
    {
        // Guard against destroyed children (happens when play mode stops in editor).
        if (_canvasGO == null || _canvasT == null) return;

        Camera cam = Camera.main;
        if (cam == null) { _canvasGO.SetActive(false); return; }

        // Hide when GazeSelector is disabled — active Forest gameplay has its own crosshair.
        var gs    = cam.GetComponent<GazeSelector>();
        bool show = gs == null || gs.enabled;

        if (_canvasGO.activeSelf != show)
            _canvasGO.SetActive(show);

        if (!show) return;

        _canvasT.position = cam.transform.position + cam.transform.forward * distance;
        _canvasT.rotation = cam.transform.rotation;
    }

    static Sprite MakeCircleSprite(int size)
    {
        var tex    = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        float r    = size * 0.5f;
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
