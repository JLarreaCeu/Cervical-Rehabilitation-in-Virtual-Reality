using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultsGazeButton : MonoBehaviour, IGazeable
{
    public bool isPlayAgain = true;
    public string menuSceneName = "MainMenu";
    public string gameSceneName = "Easy";
    public GazeProgressIndicator progressIndicator;
    public UnityEngine.Color normalColor;
    public UnityEngine.Color hoveredColor;
    private UnityEngine.UI.Image _bgImage;
    private bool _selected;
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
        if (isPlayAgain)
        {
            GameManager.PlayAgain = true;
            SceneLoader.Load(gameSceneName);
        }
        else
        {
            SceneLoader.Load(menuSceneName);
        }
    }
}
