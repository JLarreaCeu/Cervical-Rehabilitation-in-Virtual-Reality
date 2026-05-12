using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Fallback speed (overridden by DifficultyConfig)")]
    public float moveSpeed = 3f;

    [Header("Camera reference (auto-assigned if null)")]
    public Transform cameraTransform;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main != null ? Camera.main.transform : transform;

        if (DifficultyConfig.Instance != null && DifficultyConfig.Instance.Current != null)
            moveSpeed = DifficultyConfig.Instance.Current.moveSpeed;
    }

    void Update()
    {
        if (cameraTransform == null) return;


        transform.position += Vector3.forward * moveSpeed * Time.deltaTime;
    }
}
