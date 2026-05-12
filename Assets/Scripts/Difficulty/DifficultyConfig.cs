using UnityEngine;

public class DifficultyConfig : MonoBehaviour
{
    public static DifficultyConfig Instance { get; private set; }

    [Header("Presets — assign in Inspector")]
    public DifficultySettings easy;
    public DifficultySettings medium;
    public DifficultySettings hard;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public DifficultySettings Current
    {
        get
        {
            switch (DifficultyManager.SelectedDifficulty)
            {
                case "Facil":   return easy;
                case "Medio":   return medium;
                case "Dificil": return hard;
                default:         return medium;
            }
        }
    }
}
