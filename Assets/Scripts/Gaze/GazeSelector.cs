using UnityEngine;

public class GazeSelector : MonoBehaviour
{
    [Header("Settings")]
    public float maxDistance = 20f;
    public LayerMask gazeLayer = ~0;
    public float selectTime = 2f;

    private IGazeable _current;
    private float _gazeTimer;

    void Update()
    {
        if (DifficultyConfig.Instance != null && DifficultyConfig.Instance.Current != null)
            selectTime = DifficultyConfig.Instance.Current.gazeSelectTime;

        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, gazeLayer))
        {
            var gazeable = hit.collider.GetComponentInParent<IGazeable>();
            if (gazeable != null)
            {
                if (gazeable != _current)
                {
                    ExitCurrent();
                    _current = gazeable;
                    _gazeTimer = 0f;
                    _current.OnGazeEnter();
                }
                _gazeTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(_gazeTimer / selectTime);
                _current.OnGazeStay(progress);
                if (_gazeTimer >= selectTime)
                {
                    _current.OnGazeSelect();
                    _gazeTimer = 0f;
                }
                return;
            }
        }
        ExitCurrent();
    }

    void ExitCurrent()
    {
        if (_current == null) return;
        _current.OnGazeExit();
        _current = null;
        _gazeTimer = 0f;
    }
}
