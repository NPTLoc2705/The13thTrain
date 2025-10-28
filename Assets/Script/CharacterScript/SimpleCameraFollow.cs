using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    private Transform target;
    private float distance;
    private float height;
    private float smoothSpeed;

    public void Initialize(Transform followTarget, float followDistance, float followHeight, float smooth)
    {
        target = followTarget;
        distance = followDistance;
        height = followHeight;
        smoothSpeed = smooth;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position
                                - target.forward * distance
                                + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
        transform.LookAt(target.position + Vector3.up * 1.2f);
    }
}
