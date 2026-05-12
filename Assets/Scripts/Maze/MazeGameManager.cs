using UnityEngine;
using UnityEngine.SceneManagement;

public class MazeGameManager : MonoBehaviour
{
    public static MazeGameManager Instance { get; private set; }

    [Header("Mazes (assign 5 textures)")]
    public Texture2D[] mazeTextures;
    public Renderer    mazePlaneRenderer;

    [Header("UI — Entry")]
    public GameObject entryCanvas;

    [Header("UI — Duration Select")]
    public GameObject durationCanvas;

    [Header("UI — HUD")]
    public TMPro.TextMeshProUGUI mazeCountText;
    public GameObject            hudCanvas;

    [Header("UI — Side Timer")]
    public TMPro.TextMeshProUGUI timerText;
    public GameObject            sideTimerCanvas;

    [Header("UI — Results")]
    public GameObject            resultsCanvas;
    public TMPro.TextMeshProUGUI finalMazesText;
    public TMPro.TextMeshProUGUI finalTimeText;

    [Header("References")]
    public LaserPointer cursor;
    public string       menuSceneName = "MainMenu";

    int     _slot;
    int[]   _order;
    float   _timeRemaining;
    int     _mazesSolved;
    bool    _running;
    bool    _exitCooldown;
    bool    _waitingForFirstGrab;
    Vector3    _lockedPosition;
    Quaternion _lockedRotation;

    UnityEngine.InputSystem.XR.TrackedPoseDriver _tpd;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Camera cam = Camera.main;
        if (cam != null)
            _tpd = cam.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start() => ShowEntry();

    public void ShowEntry()
    {
        _running = false;
        SetAllCanvases(false);
        SetMazeVisible(false);
        SetCursorVisible(false);

        if (entryCanvas != null)
        {
            PositionCanvasInFront(entryCanvas.transform, 2f, 0.1f);
            entryCanvas.SetActive(true);
        }
    }

    public void ShowDurationSelect()
    {
        _running = false;
        SetAllCanvases(false);
        SetMazeVisible(false);
        SetCursorVisible(false);

        if (durationCanvas != null)
        {
            PositionCanvasInFront(durationCanvas.transform, 2f, 0.1f);
            durationCanvas.SetActive(true);
        }
    }

    public void BeginPlay(int minutes)
    {
        _lockedPosition = transform.position;
        _lockedRotation = transform.rotation;

        _timeRemaining       = Mathf.Clamp(minutes, 1, 5) * 60f;
        _mazesSolved         = 0;
        _slot                = 0;
        _running             = true;
        _exitCooldown        = false;
        _waitingForFirstGrab = true;

        _order = new int[] { 0, 1, 2, 3, 4 };
        for (int i = 4; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = _order[i]; _order[i] = _order[j]; _order[j] = tmp;
        }

        if (cursor == null) cursor = FindObjectOfType<LaserPointer>();

        SetAllCanvases(false);
        SetMazeVisible(true);
        SetCursorVisible(true);
        // hudCanvas is intentionally NOT shown.
        // Side timer is activated here; position is set inside LoadMaze.
        if (sideTimerCanvas != null)
            sideTimerCanvas.SetActive(true);

        LoadMaze(_slot);
        RefreshHUD();
    }

    public void RestartGame()  => ShowDurationSelect();
    public void ReturnToMenu() => SceneLoader.Load(menuSceneName);

    void LateUpdate()
    {
        if (!_running) return;
        transform.position = _lockedPosition;
        transform.rotation = _lockedRotation;
    }

    void Update()
    {
        if (!_running) return;

        if (_waitingForFirstGrab)
        {
            if (cursor != null && cursor.IsGrabbed)
                _waitingForFirstGrab = false;
            RefreshHUD();
            return;
        }

        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _running = false;
            ShowResults();
            return;
        }

        if (!_exitCooldown && cursor != null && cursor.IsAtExit)
        {
            _exitCooldown = true;
            _mazesSolved++;
            _slot++;

            if (_slot >= 5)
            {
                _slot = 0;
                _order = new int[] { 0, 1, 2, 3, 4 };
                for (int i = 4; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    int tmp = _order[i]; _order[i] = _order[j]; _order[j] = tmp;
                }
            }

            LoadMaze(_slot);
            _exitCooldown = false;
        }

        RefreshHUD();
    }

    void LoadMaze(int slot)
    {
        if (mazeTextures == null || mazeTextures.Length == 0) return;
        int idx = _order[slot % _order.Length];
        if (idx >= mazeTextures.Length) return;
        var tex = mazeTextures[idx];

        // Re-center maze and timer on every load so each puzzle starts where the player is looking.
        Camera cam = Camera.main;
        if (cam != null && mazePlaneRenderer != null)
        {
            Vector3 fwd = cam.transform.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
            fwd.Normalize();

            const float mazeDist = 1.8f;
            Vector3 mazePos = cam.transform.position + fwd * mazeDist + Vector3.up * 0.1f;
            mazePlaneRenderer.transform.position = mazePos;
            mazePlaneRenderer.transform.rotation = Quaternion.LookRotation(fwd);
            mazePlaneRenderer.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            if (sideTimerCanvas != null)
            {
                sideTimerCanvas.transform.position = mazePos + Vector3.down * 0.75f;
                sideTimerCanvas.transform.rotation = Quaternion.LookRotation(fwd);
            }
        }

        if (mazePlaneRenderer != null && mazePlaneRenderer.sharedMaterial != null)
        {
            mazePlaneRenderer.sharedMaterial.mainTexture = tex;
            if (mazePlaneRenderer.sharedMaterial.HasProperty("_BaseMap"))
                mazePlaneRenderer.sharedMaterial.SetTexture("_BaseMap", tex);
        }

        // SetMaze recalibrates _displayCalib so the guide ring starts centered on the new maze.
        if (cursor != null) cursor.SetMaze(tex);
    }

    void RefreshHUD()
    {
        if (timerText != null)
        {
            if (_waitingForFirstGrab)
                timerText.text = (Mathf.FloorToInt(Time.time * 2f) % 2 == 0) ? "-:--" : "    ";
            else
            {
                int m = Mathf.FloorToInt(_timeRemaining / 60f);
                int s = Mathf.FloorToInt(_timeRemaining % 60f);
                timerText.text = m + ":" + s.ToString("D2");
            }
        }
        if (mazeCountText != null)
            mazeCountText.text = "Mazes\n" + _mazesSolved;
    }

    void ShowResults()
    {

        SetAllCanvases(false);
        SetMazeVisible(false);
        SetCursorVisible(false);

        if (resultsCanvas != null)
        {
            PositionCanvasInFront(resultsCanvas.transform, 2f, 0.1f);
            resultsCanvas.SetActive(true);
        }

        if (finalMazesText != null)
            finalMazesText.text = "Mazes solved: " + _mazesSolved;
        if (finalTimeText != null)
            finalTimeText.text = "Great work!";
    }

    void SetAllCanvases(bool active)
    {
        if (entryCanvas     != null) entryCanvas.SetActive(active);
        if (durationCanvas  != null) durationCanvas.SetActive(active);
        if (hudCanvas       != null) hudCanvas.SetActive(active);
        if (sideTimerCanvas != null) sideTimerCanvas.SetActive(active);
        if (resultsCanvas   != null) resultsCanvas.SetActive(active);
    }

    void SetMazeVisible(bool visible)
    {
        if (mazePlaneRenderer != null) mazePlaneRenderer.gameObject.SetActive(visible);
    }

    void SetCursorVisible(bool visible)
    {
        if (cursor == null) return;
        if (cursor.laserDot  != null) cursor.laserDot.gameObject.SetActive(visible);
        if (cursor.exitStar  != null) cursor.exitStar.gameObject.SetActive(visible);
        if (cursor.guideRing != null) cursor.guideRing.gameObject.SetActive(visible);
    }

    static void PositionCanvasInFront(Transform canvasT, float distance, float yOffset)
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        // Flatten Y so the canvas stays upright - player looks around to aim.
        Vector3 fwd = cam.transform.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
        fwd.Normalize();
        canvasT.position = cam.transform.position + fwd * distance + Vector3.up * yOffset;
        canvasT.rotation = Quaternion.LookRotation(fwd);
    }
}
