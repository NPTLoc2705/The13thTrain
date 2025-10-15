using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;

    [Header("Jump Settings")]
    [Tooltip("Độ cao cú nhảy — tăng để nhảy cao hơn")]
    public float jumpHeight = 2.5f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform cameraTransform;         // Tham chiếu tới camera (kéo vào trong Inspector)
    public Vector3 cameraOffset = new Vector3(0, 3f, -5f);
    public float cameraSmoothSpeed = 10f;
    public float mouseSensitivity = 120f;
    public bool lockCursor = true;            // Ẩn và khóa chuột vào giữa màn hình

    [Header("References")]
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private float yaw;
    private float pitch;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Lấy góc hiện tại của camera ban đầu
        if (cameraTransform != null)
        {
            yaw = cameraTransform.eulerAngles.y;
            pitch = cameraTransform.eulerAngles.x;
        }
    }

    void Update()
    {
        // --- 1. Kiểm tra chạm đất ---
        isGrounded = controller.isGrounded;
        animator.SetBool("isGrounded", isGrounded);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // --- 2. Camera xoay theo chuột ---
        if (cameraTransform != null)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -30f, 60f);

            Quaternion camRot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPos = transform.position + camRot * cameraOffset;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPos, cameraSmoothSpeed * Time.deltaTime);
            cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
        }

        // --- 3. Nhận input di chuyển ---
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(h, 0, v).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Hướng di chuyển dựa trên góc quay của camera
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + yaw;
            Quaternion rot = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * 10f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
            animator.SetFloat("Speed", isRunning ? 3f : 1f);
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }

        // --- 4. Nhảy ---
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("JumpTrigger");
        }

        // --- 5. Trọng lực ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
