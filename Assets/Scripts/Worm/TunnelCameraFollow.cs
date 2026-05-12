using UnityEngine;

public class TunnelCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform wormHead;

    [Header("Follow Settings")]
    public float followSpeed = 0.3f;

    public float leadOffset = 1.5f;

    private float _startX;
    private bool  _following;

    void Awake()
    {
        _startX = transform.position.x;
        _following = true;
    }

    public void StartFollowing() { _following = true; }
    public void StopFollowing()  { _following = false; }

    void LateUpdate()
    {
        if (!_following || wormHead == null) return;

        float targetX = wormHead.position.x - leadOffset;
        float newX    = Mathf.Lerp(transform.position.x, targetX, followSpeed * Time.deltaTime);

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}
