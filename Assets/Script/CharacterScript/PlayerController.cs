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
    private bool movementLocked = false; // Lock movement during radio tuning
    private Candle nearbyCandle = null; //Detech the nearby candle for interaction
    private PickupItem collidedItem = null;
    private TrainDoorController nearbyTrainDoor = null;

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

    // Public method to lock/unlock movement (called by RadioPuzzle)
    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
        if (locked)
        {
            animator.SetFloat("Speed", 0f); // Stop animation
            velocity = Vector3.zero; // Stop any vertical movement
        }
        else
        {
            Debug.Log("✅ Movement unlocked!");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleInteraction();
        HandleCollisionPickup();
        if (nearbyCandle != null && Input.GetKeyDown(KeyCode.E))
        {
            nearbyCandle.LightCandle();
        }

        // ===== Train Door Interaction =====
        if (nearbyTrainDoor != null && nearbyTrainDoor.isActiveAndEnabled)
        {
            // Nếu cửa đã mở, cho phép hiển thị prompt và vào tàu
            if (nearbyTrainDoor != null && Input.GetKeyDown(KeyCode.E))
            {
                nearbyTrainDoor.StartCoroutine("EnterTrainRoutine");
            }
        }
    }

    // ==============================
    // ====== MOVEMENT CONTROL ======
    // ==============================
    //void HandleMovement()
    //{
    //    // CRITICAL: Exit early if movement is locked (during radio tuning)
    //    if (movementLocked)
    //    {
    //        // Allow camera rotation even when locked
    //        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
    //        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    //        return;
    //    }

    //    isGrounded = controller.isGrounded;
    //    animator.SetBool("isGrounded", isGrounded);
    //    if (isGrounded && velocity.y < 0)
    //        velocity.y = -2f;

    //    // Player rotation with mouse
    //    yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
    //    transform.rotation = Quaternion.Euler(0f, yaw, 0f);

    //    // Movement
    //    float h = Input.GetAxis("Horizontal");
    //    float v = Input.GetAxis("Vertical");
    //    Vector3 direction = new Vector3(h, 0, v).normalized;

    //    if (direction.magnitude >= 0.1f)
    //    {
    //        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + yaw;
    //        Quaternion rot = Quaternion.Euler(0f, targetAngle, 0f);
    //        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * 10f);

    //        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
    //        bool isRunning = Input.GetKey(KeyCode.LeftShift);
    //        float currentSpeed = isRunning ? runSpeed : walkSpeed;

    //        controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
    //        animator.SetFloat("Speed", isRunning ? 3f : 1f);
    //    }
    //    else
    //    {
    //        animator.SetFloat("Speed", 0f);
    //    }

    //    // Jump
    //    if (isGrounded && Input.GetButtonDown("Jump"))
    //    {
    //        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    //        animator.SetTrigger("JumpTrigger");
    //    }

    //    // Gravity
    //    velocity.y += gravity * Time.deltaTime;
    //    controller.Move(velocity * Time.deltaTime);
    //}

    void HandleMovement()
    {
        // CRITICAL: Exit early if movement is locked (during radio tuning)
        if (movementLocked)
            return;

        isGrounded = controller.isGrounded;
        animator.SetBool("isGrounded", isGrounded);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Lấy input di chuyển
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            // ✅ Hướng di chuyển theo hướng nhìn của camera
            Transform cam = Camera.main.transform;
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;

            camForward.y = 0f;
            camRight.y = 0f;

            Vector3 moveDir = camForward * v + camRight * h;
            moveDir.Normalize();

            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            controller.Move(moveDir * currentSpeed * Time.deltaTime);
            animator.SetFloat("Speed", isRunning ? 3f : 1f);
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }

        // Jump
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("JumpTrigger");
        }

        // Gravity
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

        // ===== CRITICAL: If painting viewer is open, don't show prompts! =====
        if (PaintingViewerUI.Instance != null && PaintingViewerUI.Instance.IsOpen())
        {
            return; // Exit early - painting viewer is active
        }

        // ===== CRITICAL: If radio is tuning, don't interfere with its prompts! =====
        RadioPuzzle activeRadio = FindObjectOfType<RadioPuzzle>();
        if (activeRadio != null && activeRadio.IsTuning)
        {
            return; // Exit early - radio controls its own prompts
        }

        // ===== CRITICAL: If character monologue is active, don't interfere! =====
        if (CharacterMonologue.Instance != null && CharacterMonologue.Instance.IsActive())
        {
            return; // Exit early - monologue controls its own prompts
        }

        // ===== PRIORITY: If near a candle, show candle prompt and skip other interactions =====
        if (nearbyCandle != null && !nearbyCandle.isLit)
        {
            if (TextManager.Instance != null)
                TextManager.Instance.ShowPrompt(nearbyCandle.inspectPrompt);
            return; // Exit early, don't check other interactions
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        // ===== Kiểm tra CỬA gần đó =====
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach (Collider col in nearbyColliders)
        {
            if (col.GetComponent<DoorInteraction>() != null)
            {
                // Nếu gần cửa, ưu tiên cửa và bỏ qua các tương tác khác
                return;
            }
        }

        // ===== RAYCAST CHECK - Use RaycastAll to detect triggers =====
        Ray ray = new(cam.transform.position, cam.transform.forward);
        RaycastHit[] allHits = Physics.RaycastAll(ray, interactionDistance);
        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance)); // Sort by distance

        bool showPrompt = false;
        string promptMessage = "";
        bool foundInteractable = false;

        foreach (RaycastHit hit in allHits)
        {
            // Skip player collider
            if (hit.collider.gameObject.CompareTag("Player"))
                continue;

            // If we already found something to interact with, stop checking
            if (foundInteractable)
                break;

            // ===== CHECK RADIO PUZZLE (HIGH PRIORITY) =====
            RadioPuzzle radio = hit.collider.GetComponentInParent<RadioPuzzle>();
            if (radio != null)
            {
                if (!radio.IsTuning)
                {
                    showPrompt = true;
                    promptMessage = radio.GetPromptMessage();
                }
                foundInteractable = true;
                continue; // Radio handles its own E key input
            }

            // ===== CHECK FINAL MODEL =====
            FinalModelInteraction finalModel = hit.collider.GetComponent<FinalModelInteraction>();
            if (finalModel != null)
            {
                showPrompt = true;
                promptMessage = "[E] Đụng vào bức tranh"; // Simplified prompt
                if (Input.GetKeyDown(KeyCode.E))
                {
                    finalModel.Interact();
                }
                foundInteractable = true;
                continue;
            }

            // 2️⃣ Kiểm tra TRANH XOAY
            RotatePicture picture = hit.collider.GetComponentInParent<RotatePicture>();
            if (picture != null)
            {
                // 🟢 Chỉ cho hiện chữ E nếu puzzle CHƯA hoàn thành
                if (!PuzzleManager.Instance.IsPuzzleCompleted())
                {
                    showPrompt = true;
                    promptMessage = "[E] Xoay bức tranh";

                    if (Input.GetKeyDown(KeyCode.E))
                        picture.Rotate();
                }
            }

            //Check Note Interactable
            NoteInteractable noteInteractable = hit.collider.GetComponentInParent<NoteInteractable>();
            if (noteInteractable != null && noteInteractable.CanInteract())
            {
                showPrompt = true;
                promptMessage = noteInteractable.GetPromptMessage();
                if (Input.GetKeyDown(KeyCode.E))
                {
                    noteInteractable.Interact();
                }
                foundInteractable = true;
                continue;
            }

            // ===== CHECK NOTE =====
            NoteOpener noteOpener = hit.collider.GetComponentInParent<NoteOpener>();
            if (noteOpener != null)
            {
                showPrompt = true;
                promptMessage = "[E] Đọc ghi chú";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (CharacterMonologue.Instance != null)
                    {
                        CharacterMonologue.Instance.ShowMonologueWithCallback(
                            "Tại sao lại có mảnh giấy này ở đây... Mình nên đọc nó.",
                            () => noteOpener.noteUI.OpenNote()
                        );
                    }
                    else
                    {
                        noteOpener.noteUI.OpenNote();
                    }
                }
                foundInteractable = true;
                continue;
            }

            // ===== CHECK PAINTING BOOK =====
            PaintingBookOpener paintingBook = hit.collider.GetComponentInParent<PaintingBookOpener>();
            if (paintingBook != null)
            {
                string bookPrompt = paintingBook.GetPromptMessage();

                // Nếu prompt là null (viewer đang mở), skip interaction này
                if (bookPrompt == null)
                {
                    foundInteractable = true;
                    continue;
                }

                showPrompt = true;
                promptMessage = bookPrompt;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Ẩn prompt NGAY KHI NHẤN E
                    if (TextManager.Instance != null)
                        TextManager.Instance.HidePrompt();

                    paintingBook.OpenBook();
                }

                foundInteractable = true;
                continue;
            }

            // ===== CHECK INSPECTABLE OBJECT =====
            InspectableObject inspectable = hit.collider.GetComponent<InspectableObject>();
            if (inspectable != null && inspectable.CanInspect())
            {
                showPrompt = true;
                promptMessage = inspectable.GetPromptMessage();
                if (Input.GetKeyDown(KeyCode.E))
                {
                    inspectable.Inspect();
                }
                foundInteractable = true;
                continue;
            }

            // ===== CHECK MYSTERY BOX =====
            if (hit.collider.CompareTag("MysteryBox"))
            {
                showPrompt = true;
                if (PickupManager.Instance != null && PickupManager.Instance.IsCollected("SafeKey"))
                    promptMessage = "[E] Mở hộp bí ẩn";
                else
                    promptMessage = "[E] Cần chìa khóa để mở";
                if (Input.GetKeyDown(KeyCode.E))
                {
                    MysteryBoxController box = hit.collider.GetComponent<MysteryBoxController>();
                    if (box != null && PickupManager.Instance != null && PickupManager.Instance.IsCollected("SafeKey"))
                    {
                        box.OpenBox();
                    }
                }
                foundInteractable = true;
                continue;
            }

            // ===== CHECK PICKUP ITEM =====
            PickupItem item = hit.collider.GetComponentInParent<PickupItem>();
            if (item != null && !item.isCollected)
            {
                showPrompt = true;
                promptMessage = $"[E] Nhặt: {item.itemName}";
                if (Input.GetKeyDown(KeyCode.E))
                    PickupManager.Instance.CollectItem(item);
                foundInteractable = true;
                continue;
            }
        }

        // ====== Cập nhật PROMPT UI ======
        if (TextManager.Instance != null)
        {
            if (showPrompt)
                TextManager.Instance.ShowPrompt(promptMessage);
            else
                TextManager.Instance.HidePrompt();
        }
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

    // ==========================================
    // ====== CANDLE TRIGGER INTERACTION ========
    // ==========================================
    private void OnTriggerEnter(Collider other)
    {
        Candle candle = other.GetComponentInParent<Candle>();
        if (candle != null && !candle.isLit)
        {
            nearbyCandle = candle;
            // Don't show prompt here - let HandleInteraction do it
        }

        TrainDoorController door = other.GetComponent<TrainDoorController>();
        if (door != null)
        {
            nearbyTrainDoor = door;
            Debug.Log("🚪 Player entered train door trigger!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Candle candle = other.GetComponentInParent<Candle>();
        if (candle != null && candle == nearbyCandle)
        {
            nearbyCandle = null;
            if (TextManager.Instance != null)
                TextManager.Instance.HidePrompt();
        }

        TrainDoorController door = other.GetComponent<TrainDoorController>();
        if (door != null && door == nearbyTrainDoor)
        {
            nearbyTrainDoor = null;
            Debug.Log("🚶 Player exited train door trigger.");
        }
    }
}