using UnityEngine;
using System.Collections.Generic;

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
        gameObject.tag = "Player";

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
            Debug.LogError("CameraTransform chưa được gán!");
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

        bool showPrompt = false;
        string promptMessage = "";

        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 0.5f);
        if (Physics.Raycast(ray, out hit, interactionDistance, ~0, QueryTriggerInteraction.Collide))
        {
            Debug.Log("Raycast hit: " + hit.collider.name + " | Tag: " + hit.collider.tag);
            PickupItem item = hit.collider.GetComponent<PickupItem>();
            if (item != null && !item.isCollected)
            {
                showPrompt = true;
                if (item.itemID == "SafeKey")
                    promptMessage = "[E] Nhặt chìa khóa";
                else
                    promptMessage = $"[E] Nhặt: {item.itemName}";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PickupManager.Instance.CollectItem(item);
                }
            }
            else if (hit.collider.CompareTag("Safe"))
            {
                showPrompt = true;
                promptMessage = "[E] Mở két sắt";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    SafeController safe = hit.collider.GetComponent<SafeController>();
                    if (safe != null)
                    {
                        safe.ShowPasswordUI();
                    }
                }
            }
            else if (hit.collider.CompareTag("MysteryBox"))
            {
                showPrompt = true;
                if (PickupManager.Instance != null && PickupManager.Instance.IsCollected("SafeKey"))
                    promptMessage = "[E] Mở hộp bí ẩn";
                else
                    promptMessage = "[E] Cần chìa khóa để mở";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    MysteryBoxController box = hit.collider.GetComponent<MysteryBoxController>();
                    if (box != null)
                    {
                        if (PickupManager.Instance != null && PickupManager.Instance.IsCollected("SafeKey"))
                        {
                            box.OpenBox();
                        }
                    }
                }
            }
        }

        // Use TextManager for prompt
        if (showPrompt)
        {
            TextManager.Instance.ShowPrompt(promptMessage);
        }
        else
        {
            TextManager.Instance.HidePrompt();
        }
    }
    
}