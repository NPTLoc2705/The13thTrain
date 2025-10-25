using UnityEngine;

public class CameraFollowPUBG : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;

    [Header("Camera Settings")]
    public float distance = 5f;
    public float height = 1.7f;
    public float followSmoothness = 12f;
    public float mouseSensitivity = 200f;

    [Header("Pitch Limit")]
    public float verticalMinAngle = -40f;
    public float verticalMaxAngle = 65f;

    [Header("Anti Clip")]
    public LayerMask collisionMask;
    public float clipOffset = 0.2f;

    private float pitch;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (!target) return;

        HandleMouseLook();
        UpdateCameraPosition();
    }

    void HandleMouseLook()
    {
        // Camera cúi lên xuống theo chuột
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, verticalMinAngle, verticalMaxAngle);

        // Nhân vật xoay thân theo chuột trái phải
        float yawDelta = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        target.Rotate(Vector3.up, yawDelta);
    }

    void UpdateCameraPosition()
    {
        Vector3 headPos = target.position + Vector3.up * height;

        Quaternion rotation = Quaternion.Euler(pitch, target.eulerAngles.y, 0);
        Vector3 desiredPosition = headPos - rotation * Vector3.forward * distance;

        // Di chuyển camera mượt
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            1f / followSmoothness
        );

        // Chống camera xuyên tường
        Vector3 dir = (transform.position - headPos).normalized;
        float targetDist = distance;

        if (Physics.Raycast(headPos, dir, out RaycastHit hit, distance, collisionMask))
        {
            targetDist = hit.distance - clipOffset;
        }

        targetDist = Mathf.Clamp(targetDist, 0.5f, distance);
        transform.position = headPos + dir * targetDist;

        // Camera luôn nhìn về nhân vật
        transform.LookAt(headPos);
    }
}
