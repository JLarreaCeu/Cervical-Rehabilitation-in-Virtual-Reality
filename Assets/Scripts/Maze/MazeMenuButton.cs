using UnityEngine;

public class MazeMenuButton : MonoBehaviour, IGazeable
{
    public enum ButtonAction
    {
        StartGame,
        BackToMenu,
        SelectDuration,
        PlayAgain,
        ReturnToMenu
    }

    [Header("Action")]
    public ButtonAction action          = ButtonAction.StartGame;
    public int          durationMinutes = 1;

    [Header("Visual")]
    public GazeProgressIndicator progressIndicator;
    public Color normalColor  = new Color(0.08f, 0.04f, 0.18f, 0.92f);
    public Color hoveredColor = new Color(0.28f, 0.10f, 0.48f, 1.00f);

    UnityEngine.UI.Image _bgImage;
    bool _selected;

    void Awake()
    {
        _bgImage = GetComponent<UnityEngine.UI.Image>();
        if (progressIndicator != null) progressIndicator.Hide();
    }

    void OnEnable() { _selected = false; }

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

        var mgr = MazeGameManager.Instance
               ?? UnityEngine.Object.FindObjectOfType<MazeGameManager>();
        if (mgr == null) return;

        switch (action)
        {
            case ButtonAction.StartGame:
                mgr.ShowDurationSelect();
                break;
            case ButtonAction.BackToMenu:
            case ButtonAction.ReturnToMenu:
                mgr.ReturnToMenu();
                break;
            case ButtonAction.SelectDuration:
                mgr.BeginPlay(durationMinutes);
                break;
            case ButtonAction.PlayAgain:
                mgr.RestartGame();
                break;
        }
    }
}
