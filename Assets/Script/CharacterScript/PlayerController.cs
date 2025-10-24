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
    public float mouseSensitivity = 120f;
    public bool lockCursor = true;

    [Header("References")]
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float yaw;

    // Collected pieces list
    private List<string> collectedPieces = new List<string>();
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

        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        HandleMovement();
        HandleInteraction();
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        animator.SetBool("isGrounded", isGrounded);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // --- Xoay player theo chuột ---
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // --- Di chuyển ---
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S
        Vector3 direction = new Vector3(h, 0, v);

        // Di chuyển theo hướng nhìn của player
        Vector3 move = transform.TransformDirection(direction).normalized;

        bool isMoving = move.magnitude > 0.1f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Animation
        if (isMoving)
            animator.SetFloat("Speed", isRunning ? 3f : 1f);
        else
            animator.SetFloat("Speed", 0f);

        // --- Nhảy ---
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("JumpTrigger");
        }


        // --- Trọng lực ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }


    void HandleInteraction()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        bool showPrompt = false;
        string promptMessage = "";

        if (Physics.Raycast(ray, out hit, interactionDistance, ~0, QueryTriggerInteraction.Collide))
        {
            // Check for PickupItem
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
            // Check for Safe
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
            // Check for MysteryBox
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
            // Check for FinalModel
            else if (hit.collider.CompareTag("FinalModel"))
            {
                showPrompt = true;
                promptMessage = "[E] Kiểm tra tranh hoàn chỉnh";

                if (Input.GetKeyDown(KeyCode.E))
                {
                    FinalModelInteraction finalModel = hit.collider.GetComponent<FinalModelInteraction>();
                    if (finalModel != null)
                    {
                        finalModel.Interact();
                    }
                }
            }
        }

        // Use TextManager for prompt
        if (showPrompt && TextManager.Instance != null)
        {
            TextManager.Instance.ShowPrompt(promptMessage);
        }
        else if (TextManager.Instance != null)
        {
            TextManager.Instance.HidePrompt();
        }
    }
}