using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfirmationUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI difficultyLabel;

    static Camera FindCamera()
    {
        var go = UnityEngine.GameObject.Find("Main Camera");
        return go != null ? go.GetComponent<Camera>() : Camera.main;
    }

    void Start()
    {
        string level   = DifficultyManager.SelectedDifficulty;
        string display = level == "Facil" ? "Easy" : level == "Medio" ? "Medium" : "Hard";
        if (difficultyLabel != null)
            difficultyLabel.text = "You have selected\n<size=120%><b>" + display.ToUpper() + " MODE</b></size>";

        if (GameManager.PlayAgain)
        {
            GameManager.PlayAgain = false;
            BeginGameplay();
        }
    }

    void BeginGameplay()
    {
        gameObject.SetActive(false);
        string level = DifficultyManager.SelectedDifficulty;

        if (level == "Medio")
            StartMediumGame();
        else
            StartEasyGame();
    }

    void StartEasyGame()
    {

        var cam = FindCamera();
        if (cam != null)
            cam.backgroundColor = new Color(0.30f, 0.55f, 0.25f);



        var fw = FindObjectOfType<ForestGameController>(includeInactive: true);
        if (fw != null)
            fw.enabled = true;
        else
            Debug.LogWarning("[ConfirmationUI] ForestGameController not found in scene.");

        if (GameManager.Instance != null) GameManager.Instance.StartGame();
    }

    void StartMediumGame()
    {

        Debug.Log("[ConfirmationUI] StartMediumGame — Instance=" + (MediumGameManager.Instance != null ? MediumGameManager.Instance.gameObject.name : "NULL"));

        if (MediumGameManager.Instance != null)
        {
            MediumGameManager.Instance.StartGame();
        }
        else
        {

            var mgr = FindObjectOfType<MediumGameManager>();
            Debug.LogWarning("[ConfirmationUI] Instance was null, found via FindObjectOfType: " + (mgr != null ? mgr.gameObject.name : "STILL NULL"));
            if (mgr != null) mgr.StartGame();
        }
    }

    public void StartGame()    { BeginGameplay(); }
    public void ReturnToMenu() { SceneLoader.Load("MainMenu"); }
}
