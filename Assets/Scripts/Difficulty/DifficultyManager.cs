using UnityEngine;

public static class DifficultyManager
{
    private static string _selected = "Medio";


    public static string SelectedDifficulty => _selected;




    public static void SetDifficulty(string difficulty)
    {
        _selected = difficulty;
        Debug.Log("dificultad: " + difficulty);
    }
}
