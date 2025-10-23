using UnityEngine;

public class CameraFollow2_5D : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float followSpeed = 5f;

    [Header("Mouse Rotation Settings")]
    public float mouseSensitivity = 100f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 60f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("Camera target not assigned!");
            enabled = false;
            return;
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize rotation
        Vector3 eulerAngles = transform.eulerAngles;
        rotationY = eulerAngles.y;
        rotationX = eulerAngles.x;
    }

    void LateUpdate()
    {
        if (!target) return;

        // --- Mouse input for rotation ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, yMinLimit, yMaxLimit);

        // Compute rotation
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);

        // Update position (camera behind target)
        Vector3 desiredPosition = target.position + rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Look at player
        transform.LookAt(target);
    }

    public Quaternion GetCameraRotation()
    {
        return Quaternion.Euler(rotationX, rotationY, 0);
    }
}
