using UnityEngine;
using System.Collections.Generic;
using TMPro; // dùng cho UI TextMeshPro

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 5f;

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;

    [Header("Jump Settings")]
    public float jumpHeight = 2.5f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public Vector3 cameraOffset = new Vector3(0, 3f, -5f);
    public float cameraSmoothSpeed = 10f;
    public float mouseSensitivity = 120f;
    public bool lockCursor = true;

    [Header("References")]
    public Animator animator;
    public TextMeshProUGUI pickupText;  // Tham chiếu tới TMP UI

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float yaw;
    private float pitch;

    // Danh sách mảnh tranh đã nhặt
    private List<string> collectedPieces = new List<string>();

    // Tổng số mảnh (bạn có thể chỉnh trong Inspector)
    public int totalPieces = 5;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (cameraTransform != null)
        {
            yaw = cameraTransform.eulerAngles.y;
            pitch = cameraTransform.eulerAngles.x;
        }
        else
        {
            Debug.LogError("❌ CameraTransform chưa được gán!");
        }

        if (pickupText != null)
        {
            pickupText.text = ""; // Ẩn khi bắt đầu
        }
    }

    void Update()
    {
        HandleMovement();
        HandleInteraction();
    }

    // ---------------------- DI CHUYỂN ----------------------
    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        animator.SetBool("isGrounded", isGrounded);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Camera xoay
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

        // Di chuyển
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(h, 0, v).normalized;

        if (direction.magnitude >= 0.1f)
        {
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

        // Nhảy
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("JumpTrigger");
        }

        // Trọng lực
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ---------------------- NHẶT ĐỒ ----------------------
    void HandleInteraction()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, ~0, QueryTriggerInteraction.Collide))

        {
            PickupItem item = hit.collider.GetComponent<PickupItem>();
            if (item != null && !item.isCollected)
            {
                if (pickupText != null)
                    pickupText.text = $"[E] Nhặt: {item.itemName}";

                if (Input.GetKeyDown(KeyCode.E))
                {
                    PickupManager.Instance.CollectItem(item);
                }
            }
            else
            {
                if (pickupText != null)
                    pickupText.text = "";
            }
        }
        else
        {
            if (pickupText != null)
                pickupText.text = "";
        }
    }
}
