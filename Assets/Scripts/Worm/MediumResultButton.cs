using UnityEngine;

public class MediumResultButton : MonoBehaviour, IGazeable
{
    public enum ButtonAction { Restart, ReturnToMenu }

    [Header("Action")]
    public ButtonAction action = ButtonAction.ReturnToMenu;

    [Header("Visual")]
    public GazeProgressIndicator progressIndicator;
    public Color normalColor  = new Color(0.07f, 0.22f, 0.12f, 1f);
    public Color hoveredColor = new Color(0.12f, 0.35f, 0.18f, 1f);

    private UnityEngine.UI.Image _bgImage;
    private bool _selected;

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

        var mgr = MediumGameManager.Instance != null
            ? MediumGameManager.Instance
            : UnityEngine.Object.FindObjectOfType<MediumGameManager>();

        if (mgr == null) return;

        if (action == ButtonAction.Restart)
            mgr.RestartGame();
        else
            mgr.ReturnToMenu();
    }
}
