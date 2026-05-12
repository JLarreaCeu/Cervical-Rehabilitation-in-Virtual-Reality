using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfirmationGazeButton : MonoBehaviour, IGazeable
{
    [Header("Action")]
    public bool isStartButton = true;
    public string menuSceneName = "MainMenu";

    [Header("Visual")]
    public GazeProgressIndicator progressIndicator;
    public Color normalColor  = new Color(0.07f, 0.22f, 0.12f, 1f);
    public Color hoveredColor = new Color(0.12f, 0.35f, 0.18f, 1f);

    private UnityEngine.UI.Image _bgImage;
    private bool _selected;


    static Camera FindCamera()
    {
        var go = UnityEngine.GameObject.Find("Main Camera");
        return go != null ? go.GetComponent<Camera>() : Camera.main;
    }

    void Awake()
    {
        _bgImage = GetComponent<UnityEngine.UI.Image>();
        if (progressIndicator != null) progressIndicator.Hide();
    }

    public void OnGazeEnter()
    {
        if (_bgImage) _bgImage.color = hoveredColor;
        if (progressIndicator != null) progressIndicator.Show();
    }

    public void OnGazeStay(float progress)
    {
        if (progressIndicator != null) progressIndicator.SetProgress(progress);
    }

    public void OnGazeExit()
    {
        if (_bgImage) _bgImage.color = normalColor;
        if (progressIndicator != null) progressIndicator.Hide();
        _selected = false;
    }

    public void OnGazeSelect()
    {
        if (_selected) return;
        _selected = true;

        if (isStartButton)
        {
            var canvas = UnityEngine.GameObject.Find("ConfirmationCanvas");
            if (canvas != null) canvas.SetActive(false);

            string difficulty = DifficultyManager.SelectedDifficulty;

            if (difficulty == "Medio")
            {

                var mgr = MediumGameManager.Instance != null
                    ? MediumGameManager.Instance
                    : UnityEngine.Object.FindObjectOfType<MediumGameManager>();
                if (mgr != null) mgr.StartGame();
            }
            else
            {

                var cam = FindCamera();
                if (cam != null)
                    cam.backgroundColor = new Color(0.30f, 0.55f, 0.25f);


                var fw = FindObjectOfType<ForestGameController>(true);
                if (fw != null)
                    fw.enabled = true;
                else
                    Debug.LogWarning("[ConfirmationGazeButton] ForestGameController not found.");

                if (GameManager.Instance != null) GameManager.Instance.StartGame();
            }
        }
        else
        {
            SceneLoader.Load(menuSceneName);
        }
    }
}
