using UnityEngine;

public class CameraFollow2_5D : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;       // Player or player root
    public Vector3 offset = new Vector3(0, 2, -5);
    public float followSpeed = 5f;

    [Header("Rotation Settings")]
    public float rotateSpeed = 15f;
    public bool lockYRotation = true; // Keep camera horizontal like in Little Nightmares

    private Vector3 desiredPosition;
    private Quaternion desiredRotation;

    void LateUpdate()
    {
        if (!target) return;

        // Desired position behind the target
        desiredPosition = target.position + target.TransformDirection(offset);

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Look at target smoothly
        Vector3 lookDir = target.position - transform.position;
        if (lockYRotation) lookDir.y = 0;

        desiredRotation = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotateSpeed * Time.deltaTime);
    }
}
