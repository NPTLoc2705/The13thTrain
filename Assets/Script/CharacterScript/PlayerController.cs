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

    private PickupItem collidedItem = null;

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
        HandleCollisionPickup();
    }

    // ==============================
    // ====== MOVEMENT CONTROL ======
    // ==============================
    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        animator.SetBool("isGrounded", isGrounded);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(h, 0, v);

        Vector3 move = transform.TransformDirection(direction).normalized;

        bool isMoving = move.magnitude > 0.1f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        animator.SetFloat("Speed", isMoving ? (isRunning ? 3f : 1f) : 0f);

        if (isGrounded && Input.GetButtonDown("Jump"))
            animator.SetTrigger("JumpTrigger");

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // =================================
    // ====== INTERACTION SYSTEM =======
    // =================================
    void HandleInteraction()
    {
        // Nếu đang đọc note thì không cho tương tác khác
        if (FindObjectOfType<NoteUI>()?.IsOpen() == true)
            return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // ===== Kiểm tra CỬA gần đó =====
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach (Collider col in nearbyColliders)
        {
            DoorInteraction door = col.GetComponent<DoorInteraction>();
            if (door != null)
            {
                // nếu đang trong vùng cửa thì bỏ qua các tương tác khác
                return;
            }
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;
        bool showPrompt = false;
        string promptMessage = "";

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // 1️⃣ Kiểm tra FINAL MODEL
            FinalModelInteraction finalModel = hit.collider.GetComponent<FinalModelInteraction>();
            if (finalModel != null)
            {
                showPrompt = true;
                promptMessage = "[E]";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    finalModel.Interact();
                    Debug.Log("✓ Player pressed E on Final Model");
                }
            }

            // 2️⃣ Kiểm tra TRANH XOAY
            RotatePicture picture = hit.collider.GetComponentInParent<RotatePicture>();
            if (picture != null)
            {
                showPrompt = true;
                promptMessage = "[E] Xoay bức tranh";
                if (Input.GetKeyDown(KeyCode.E))
                    picture.Rotate();
            }

            // 3️⃣ Kiểm tra NOTE
            NoteOpener note = hit.collider.GetComponentInParent<NoteOpener>();
            if (note != null)
            {
                showPrompt = true;
                promptMessage = "[E] Đọc ghi chú\n\"Tại sao lại có mảnh giấy này ở đây... Mình nên đọc nó.\"";
                if (Input.GetKeyDown(KeyCode.E))
                    note.noteUI.OpenNote();
            }

            // 4️⃣ Kiểm tra OBJECT CÓ THỂ XEM
            InspectableObject inspectable = hit.collider.GetComponent<InspectableObject>();
            if (inspectable != null && inspectable.CanInspect())
            {
                showPrompt = true;
                promptMessage = inspectable.GetPromptMessage();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    inspectable.Inspect();
                    Debug.Log($"✓ Player inspecting: {inspectable.objectName}");
                }
            }

            // 5️⃣ Kiểm tra PICKUP ITEM
            PickupItem item = hit.collider.GetComponentInParent<PickupItem>();
            if (item != null && !item.isCollected)
            {
                showPrompt = true;
                promptMessage = $"[E] Nhặt: {item.itemName}";

                if (Input.GetKeyDown(KeyCode.E))
                    PickupManager.Instance.CollectItem(item);
            }
        }

        // ====== Cập nhật PROMPT UI ======
        if (TextManager.Instance == null) return;

        if (showPrompt)
            TextManager.Instance.ShowPrompt(promptMessage);
        else
            TextManager.Instance.HidePrompt();
    }

    // ===============================
    // ====== NHẶT KHI VA CHẠM ======
    // ===============================
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PickupItem item = hit.collider.GetComponentInParent<PickupItem>();

        if (item != null && !item.isCollected)
        {
            collidedItem = item;
            if (TextManager.Instance != null)
                TextManager.Instance.ShowPrompt($"[E] Nhặt: {item.itemName}");
        }
    }

    void HandleCollisionPickup()
    {
        if (collidedItem != null && !collidedItem.isCollected)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                PickupManager.Instance.CollectItem(collidedItem);
                collidedItem = null;

                if (TextManager.Instance != null)
                    TextManager.Instance.HidePrompt();
            }
        }
    }
}
