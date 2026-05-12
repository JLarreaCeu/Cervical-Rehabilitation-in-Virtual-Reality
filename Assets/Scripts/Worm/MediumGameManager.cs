using UnityEngine;
using UnityEngine.SceneManagement;

public class MediumGameManager : MonoBehaviour
{
    public static MediumGameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float gameDuration = 60f;

    [Header("UI References — HUD")]
    public TMPro.TextMeshProUGUI scoreHUDText;
    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI gazeStatusText;
    public UnityEngine.GameObject hudCanvas;

    [Header("UI References — Results")]
    public UnityEngine.GameObject resultsCanvas;
    public TMPro.TextMeshProUGUI finalScoreText;
    public TMPro.TextMeshProUGUI finalAccuracyText;

    [Header("References")]
    public WormMover   wormMover;
    public GazeCursor  gazeCursor;
    public TunnelCameraFollow cameraFollow;
    public string menuSceneName = "MainMenu";

    private float _totalTime;
    private float _gazeTime;
    private float _timeRemaining;
    private bool  _running;

    public bool IsRunning => _running;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void StartGame()
    {
        _totalTime     = 0f;
        _gazeTime      = 0f;
        _timeRemaining = gameDuration;
        _running       = true;

        if (resultsCanvas != null) resultsCanvas.SetActive(false);


        if (wormMover == null)
            wormMover = FindObjectOfType<WormMover>();

        if (wormMover != null)
        {
            wormMover.StartMoving();
        }
        else
            Debug.LogWarning("wormMover not found");


        if (cameraFollow == null)
            cameraFollow = FindObjectOfType<TunnelCameraFollow>();
        if (cameraFollow != null)
            cameraFollow.StartFollowing();


        if (gazeCursor == null)
            gazeCursor = FindObjectOfType<GazeCursor>();

        if (hudCanvas  != null) hudCanvas.SetActive(true);
        if (gazeCursor != null) gazeCursor.gameObject.SetActive(true);
        UpdateHUD();
    }

    void Update()
    {
        if (!_running) return;

        _totalTime     += Time.deltaTime;
        _timeRemaining -= Time.deltaTime;


        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            EndGame();
            return;
        }

        bool onTarget = gazeCursor != null && gazeCursor.OnTarget;

        if (onTarget)
            _gazeTime += Time.deltaTime;

        if (gazeStatusText != null)
            gazeStatusText.text = onTarget
                ? "<color=#44FF44>\u25cf Tracking!</color>"
                : "<color=#FF6644>\u25cf Look at the worm!</color>";

        UpdateHUD();
    }


    public void EndGame()
    {
        _running = false;
        ShowResults();
    }


    public void RestartGame()
    {
        StartGame();
    }


    public void ReturnToMenu()
    {
        SceneLoader.Load(menuSceneName);
    }

    void UpdateHUD()
    {

        int displayScore = Mathf.Min(Mathf.FloorToInt(_gazeTime), Mathf.RoundToInt(gameDuration));
        if (scoreHUDText != null)
            scoreHUDText.text = "Score: " + displayScore + " / " + Mathf.RoundToInt(gameDuration);

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(_timeRemaining).ToString() + "s";
    }

    void ShowResults()
    {
        if (hudCanvas  != null) hudCanvas.SetActive(false);
        if (gazeCursor != null) gazeCursor.gameObject.SetActive(false);

        if (resultsCanvas != null)
        {
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                Vector3 forward = mainCam.transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
                forward.Normalize();
                resultsCanvas.transform.position = mainCam.transform.position + forward * 3.5f + Vector3.up * 0.5f;
                resultsCanvas.transform.rotation = Quaternion.LookRotation(forward);
            }
            resultsCanvas.SetActive(true);
        }

        int   maxPts     = Mathf.RoundToInt(gameDuration);
        int   finalScore = Mathf.Min(Mathf.FloorToInt(_gazeTime), maxPts);
        float accuracy   = _totalTime > 0f
            ? Mathf.Clamp01(_gazeTime / _totalTime) * 100f : 0f;

        if (finalScoreText != null)
            finalScoreText.text = finalScore + " / " + maxPts + " pts";
        if (finalAccuracyText != null)
            finalAccuracyText.text = "Gaze accuracy: " + accuracy.ToString("F0") + "%";
    }
}
