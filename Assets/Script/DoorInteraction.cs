using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Header("Door Animation Settings")]
    public Transform doorWing;
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public bool isOpen = false;

    [Header("Door Interaction Settings")]
    public float interactionDistance = 3f;
    public bool requiresKey = false;
    public string requiredKeyID = "RedKey";

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine currentCoroutine;
    private Transform playerTransform;
    private bool wasInRange = false;
    private bool isAnimating = false;

    void Start()
    {
        if (doorWing == null)
        {
            doorWing = transform.Find("doorwing");
            if (doorWing == null)
            {
                Debug.LogError("doorWing not found! Please assign it in the inspector or ensure it's named 'doorwing'");
            }
        }

        if (doorWing != null)
        {
            closedRotation = doorWing.rotation;
            openRotation = Quaternion.Euler(doorWing.eulerAngles + new Vector3(0, openAngle, 0));
        }
        else
        {
            Debug.LogError("DoorWing is required for door animation!");
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
            Debug.LogError("Player not found! Make sure Player has tag 'Player'");
    }

    void Update()
    {
        // Chỉ hiện prompt khi KHÔNG đang animation
        if (!isAnimating)
        {
            UpdatePrompt();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void UpdatePrompt()
    {
        if (playerTransform == null || TextManager.Instance == null)
            return;

        float distance = Vector3.Distance(playerTransform.position, transform.position);
        bool inRange = distance <= interactionDistance;

        if (inRange)
        {
            string promptMessage = GetPromptMessage();
            TextManager.Instance.ShowPrompt(promptMessage);
            wasInRange = true;
        }
        else if (wasInRange)
        {
            TextManager.Instance.HidePrompt();
            wasInRange = false;
        }
    }

    private string GetPromptMessage()
    {
        // Nếu cửa cần chìa khóa và chưa có chìa khóa
        if (requiresKey && (PickupManager.Instance == null || !PickupManager.Instance.IsCollected(requiredKeyID)))
        {
            return $"Cửa bị khóa! Cần {requiredKeyID}";
        }

        // Nếu đã có chìa khóa hoặc cửa không cần chìa khóa
        return isOpen ? "[E] Đóng cửa" : "[E] Mở cửa";
    }

    private void TryInteract()
    {
        if (playerTransform == null || isAnimating) return;

        float distance = Vector3.Distance(playerTransform.position, transform.position);

        if (distance > interactionDistance)
            return;

        // Kiểm tra nếu cửa bị khóa và chưa có chìa
        if (requiresKey && (PickupManager.Instance == null || !PickupManager.Instance.IsCollected(requiredKeyID)))
        {
            // ẨN PROMPT trước khi hiện notice
            if (TextManager.Instance != null)
            {
                TextManager.Instance.HidePrompt();
                TextManager.Instance.ShowNotice($"Không thể mở! Cần {requiredKeyID}!", 2f);
            }
            return;
        }

        // ẨN PROMPT ngay khi nhấn E
        if (TextManager.Instance != null)
        {
            TextManager.Instance.HidePrompt();
            wasInRange = false;
        }

        // Toggle door
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(ToggleDoor());
    }

    private IEnumerator ToggleDoor()
    {
        if (doorWing == null) yield break;

        isAnimating = true;

        Collider doorCollider = GetComponent<Collider>();
        Quaternion startRotation = doorWing.rotation;
        Quaternion targetRotation = isOpen ? closedRotation : openRotation;

        // Hiển thị notice
        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowNotice(isOpen ? "Đang đóng cửa..." : "Đang mở cửa...", 1.5f);
        }

        // Set trigger ngay lập tức khi mở cửa
        if (doorCollider != null && !isOpen)
        {
            doorCollider.isTrigger = true;
        }

        float elapsedTime = 0f;
        float duration = 1f / openSpeed;

        // Smooth animation với Lerp
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Sử dụng SmoothStep để có animation mượt hơn
            t = t * t * (3f - 2f * t);

            doorWing.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        // Đảm bảo rotation chính xác
        doorWing.rotation = targetRotation;
        isOpen = !isOpen;

        // Tắt trigger khi đóng cửa
        if (doorCollider != null && !isOpen)
        {
            doorCollider.isTrigger = false;
        }

        isAnimating = false;

        // SAU KHI animation xong, kiểm tra xem player còn trong range không
        // Nếu còn thì hiện lại prompt
        if (playerTransform != null && TextManager.Instance != null)
        {
            float distance = Vector3.Distance(playerTransform.position, transform.position);
            if (distance <= interactionDistance)
            {
                string promptMessage = GetPromptMessage();
                TextManager.Instance.ShowPrompt(promptMessage);
                wasInRange = true;
            }
        }
    }

    void OnDisable()
    {
        if (wasInRange && TextManager.Instance != null)
        {
            TextManager.Instance.HidePrompt();
            wasInRange = false;
        }
    }
}