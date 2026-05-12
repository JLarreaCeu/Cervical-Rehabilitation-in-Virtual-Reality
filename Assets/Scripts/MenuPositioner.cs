using System.Collections;
using UnityEngine;

public class MenuPositioner : MonoBehaviour
{
    [SerializeField] private float distanceFromCamera = 3.52f;

    [SerializeField] private float verticalOffset = 0f;

    private bool _positioned;

    private void OnEnable()
    {
        CameraHeightInitializer.OnStabilized += PositionMenu;
        StartCoroutine(TimeoutFallback());
    }

    private void OnDisable()
    {
        CameraHeightInitializer.OnStabilized -= PositionMenu;
    }

    private IEnumerator TimeoutFallback()
    {
        yield return new WaitForSeconds(5f);
        if (!_positioned)
        {
            Debug.LogWarning("[MenuPositioner] Timeout – positioning without stabilisation signal.");
            PositionMenu();
        }
    }

    private void PositionMenu()
    {
        if (_positioned) return;
        _positioned = true;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[MenuPositioner] Camera.main not found.");
            return;
        }

        Vector3 camPos  = cam.transform.position;
        Vector3 forward = cam.transform.forward;
        forward.y       = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        forward.Normalize();

        transform.position = camPos + forward * distanceFromCamera + Vector3.up * verticalOffset;
        transform.rotation = Quaternion.LookRotation(forward);

        Debug.Log("[MenuPositioner] Placed at " + transform.position + " (cam=" + camPos + ")");
    }
}
