using UnityEngine;

public class WormMover : MonoBehaviour
{
    [Header("Path")]
    public Transform[] waypoints;
    public float speed = 1.0f;

    private int _target = 1;

    void Awake()
    {
        if (waypoints != null && waypoints.Length > 0)
            transform.position = waypoints[0].position;
    }


    public void StartMoving() { _target = 1; }

    void Update()
    {

        if (MediumGameManager.Instance == null || !MediumGameManager.Instance.IsRunning) return;
        if (waypoints == null || waypoints.Length < 2) return;

        Vector3 goal = waypoints[_target].position;
        transform.position = Vector3.MoveTowards(transform.position, goal, speed * Time.deltaTime);


        Vector3 dir = goal - transform.position;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, dir.normalized);


        if (Vector3.Distance(transform.position, goal) < 0.05f)
        {
            _target++;
            if (_target >= waypoints.Length)
                _target = 0;
        }
    }
}
