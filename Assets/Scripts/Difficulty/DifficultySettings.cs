using UnityEngine;

[CreateAssetMenu(fileName = "DifficultySettings", menuName = "CervicalRehab/DifficultySettings")]
public class DifficultySettings : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Obstacles")]
    public float obstacleSpawnInterval = 2f;
    public float obstacleSpeed = 4f;

    [Header("Gaze")]
    public float gazeSelectTime = 3f;
}
