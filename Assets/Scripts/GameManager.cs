using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool PlayAgain = false;

    [Header("Apple Game Settings")]
    public int pointsPerApple = 10;
    public int totalApples    = 20;

    [Header("Legacy Obstacle Settings (unused in apple mode)")]
    public int pointsPerDodge  = 10;
    public int pointsLostOnHit = 5;
    public int totalPlanes     = 10;

    [Header("UI References")]
    public TMPro.TextMeshProUGUI scoreHUDText;
    public TMPro.TextMeshProUGUI applesHUDText;
    public UnityEngine.GameObject hudCanvas;
    public UnityEngine.GameObject resultsCanvas;
    public TMPro.TextMeshProUGUI finalScoreText;

    public int Score          { get; private set; }
    public int ApplesCollected { get; private set; }
    private int _planesResolved;

    static Camera FindCamera()
    {
        var go = UnityEngine.GameObject.Find("Main Camera");
        return go != null ? go.GetComponent<Camera>() : Camera.main;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }



    public void StartGame()
    {
        Score           = 0;
        ApplesCollected = 0;
        _planesResolved = 0;


        if (applesHUDText == null && hudCanvas != null)
        {
            var t = hudCanvas.transform.Find("ApplesText");
            if (t != null) applesHUDText = t.GetComponent<TMPro.TextMeshProUGUI>();
        }

        if (hudCanvas != null) hudCanvas.SetActive(true);
        UpdateHUD();
    }

    public void CollectApple()
    {
        Score           += pointsPerApple;
        ApplesCollected++;
        UpdateHUD();
        if (ApplesCollected >= totalApples)
            ShowResults();
    }



    public void RegisterDodge()
    {
        Score += pointsPerDodge;
        _planesResolved++;
        UpdateHUD();
        if (_planesResolved >= totalPlanes) ShowResults();
    }

    public void RegisterHit()
    {
        Score -= pointsLostOnHit;
        if (Score < 0) Score = 0;
        _planesResolved++;
        UpdateHUD();
        if (_planesResolved >= totalPlanes) ShowResults();
    }

    public void AddBonus(int points) { Score += points; UpdateHUD(); }



    void UpdateHUD()
    {
        if (scoreHUDText  != null)
            scoreHUDText.text  = "Score: " + Score;

        if (applesHUDText != null)
            applesHUDText.text = "🍎 " + ApplesCollected + " / " + totalApples;
    }



    public void ShowResults()
    {

        var fw = FindObjectOfType<ForestGameController>();
        if (fw != null)
        {
            fw.ResetAndStop();
        }


        var cam = FindCamera();
        if (cam != null)
        {
            var pm = cam.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;
            var sp = cam.GetComponent<ObstacleSpawner>();
            if (sp != null) sp.enabled = false;
        }

        if (hudCanvas     != null) hudCanvas.SetActive(false);


        if (resultsCanvas != null)
        {
            // Deparent from camera so the canvas stays fixed in world space
            // (if still parented to camera it would follow head movement and feel "stuck on face").
            resultsCanvas.transform.SetParent(null, false);

            var mainCam = Camera.main;
            if (mainCam != null)
            {
                Vector3 forward = mainCam.transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
                forward.Normalize();
                resultsCanvas.transform.position = mainCam.transform.position
                    + forward * 3.5f
                    + Vector3.up * 0.5f;
                resultsCanvas.transform.rotation = Quaternion.LookRotation(forward);
            }
            resultsCanvas.SetActive(true);
        }

        if (finalScoreText != null)
            finalScoreText.text = Score + " pts\n" + ApplesCollected + " / " + totalApples + " apples";
    }
}
