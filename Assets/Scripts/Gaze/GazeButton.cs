using UnityEngine;
using UnityEngine.SceneManagement;

public class GazeButton : MonoBehaviour, IGazeable
{
    [Header("Config")]
    public string difficultyLevel   = "Facil";
    public string gameplaySceneName = "Easy";

    [Header("Visuals")]
    public GazeProgressIndicator progressIndicator;
    public Color normalColor  = Color.white;
    public Color hoveredColor = Color.yellow;

    private Renderer _renderer;
    private UnityEngine.UI.Image _bgImage;
    private bool _selected;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _bgImage  = GetComponent<UnityEngine.UI.Image>();
        if (progressIndicator != null) progressIndicator.Hide();
    }

    public void OnGazeEnter()
    {
        if (_bgImage)  _bgImage.color  = hoveredColor;
        if (_renderer) _renderer.material.color = hoveredColor;
        if (progressIndicator != null) progressIndicator.Show();
    }

    public void OnGazeStay(float progress)
    {
        if (progressIndicator != null) progressIndicator.SetProgress(progress);
    }

    public void OnGazeExit()
    {
        if (_bgImage)  _bgImage.color  = normalColor;
        if (_renderer) _renderer.material.color = normalColor;
        if (progressIndicator != null) progressIndicator.Hide();
        _selected = false;
    }

    public void OnGazeSelect()
    {
        if (_selected) return;
        _selected = true;


        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == gameplaySceneName) { sceneExists = true; break; }
        }

        if (!sceneExists)
        {
            Debug.Log($"[GazeButton] Scene '{gameplaySceneName}' not available yet — Coming Soon!");
            _selected = false;
            return;
        }

        DifficultyManager.SetDifficulty(difficultyLevel);
        SceneLoader.Load(gameplaySceneName);
    }
}
