using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class MouseGazeSimulator : MonoBehaviour
{
    [Header("Settings")]
    public float maxDistance = 100f;
    public LayerMask gazeLayer = ~0;
    public float selectTime = 2f;

    [Header("Debug")]
    public bool showRayInEditor = true;

    private IGazeable _current;
    private float _gazeTimer;
    private Camera _cam;

    void Start()
    {
        // MouseGaze removed — GazeSelector handles all gaze input (VR head tracking).
        enabled = false;
    }

    void Update() { }

    void ExitCurrent() { }
}
