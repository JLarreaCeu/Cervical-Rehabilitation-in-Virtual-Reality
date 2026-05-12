using UnityEngine;

public class WormSegment : MonoBehaviour
{
    [Header("Follow")]
    public Transform leader;
    public float followDistance = 0.35f;
    public float smoothSpeed    = 8f;

    void Update()
    {
        if (leader == null) return;


        Vector3 leaderXY = new Vector3(leader.position.x, leader.position.y, transform.position.z);
        Vector3 toLeader = leaderXY - new Vector3(transform.position.x, transform.position.y, transform.position.z);
        toLeader.z = 0f;

        if (toLeader.magnitude > followDistance)
        {
            Vector3 targetPos = leaderXY - toLeader.normalized * followDistance;
            targetPos.z = transform.position.z;
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        }
    }
}
