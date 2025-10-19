using UnityEngine;

public class FinalModelInteraction : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Tên scene sẽ load (phải có trong Build Settings)")]
    public string nextSceneName = "NextLevel";

    [Header("Interaction Settings")]
    [Tooltip("Khoảng cách để có thể tương tác")]
    public float interactionDistance = 3f;

    [Tooltip("Góc để kiểm tra player ở phía trước (0-180 độ). 90 = phía trước, 180 = mọi hướng")]
    public float interactionAngle = 90f;

    [Tooltip("Kiểm tra vật cản giữa player và model (tường, vật thể...)")]
    public bool checkLineOfSight = true;

    [Tooltip("Layer của tường và vật cản (để raycast phát hiện)")]
    public LayerMask obstacleLayers;

    [Header("UI Prompt")]
    [Tooltip("Text hiển thị hướng dẫn (optional)")]
    public GameObject promptUI; // Ví dụ: Text "Press E to continue"

    private Transform player;
    private bool playerInRange = false;
    private bool isLoading = false;

    void Start()
    {
        // Tìm player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("⚠ Player not found! Make sure Player has 'Player' tag.");
        }

        // Ẩn prompt UI lúc đầu
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null || isLoading) return;

        // Kiểm tra khoảng cách với player
        float distance = Vector3.Distance(transform.position, player.position);

        // Kiểm tra góc - player có đứng phía trước model không?
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        // Kiểm tra có vật cản giữa model và player không (tường, vật thể...)
        bool hasLineOfSight = true;
        if (checkLineOfSight)
        {
            Vector3 rayStart = transform.position;
            Vector3 rayEnd = player.position + Vector3.up * 1f; // Bắn tia đến vị trí player (thêm 1m lên trên)
            Vector3 direction = rayEnd - rayStart;

            RaycastHit hit;
            if (Physics.Raycast(rayStart, direction, out hit, distance, obstacleLayers))
            {
                // Có vật cản chặn giữa model và player
                hasLineOfSight = false;
                Debug.DrawRay(rayStart, direction.normalized * hit.distance, Color.red);
            }
            else
            {
                // Không có vật cản - nhìn thấy player
                Debug.DrawRay(rayStart, direction, Color.green);
            }
        }

        // Player phải ở trong khoảng cách VÀ ở phía trước VÀ không có vật cản
        playerInRange = distance <= interactionDistance && angle <= interactionAngle && hasLineOfSight;

        // Debug log để kiểm tra
        if (distance <= interactionDistance)
        {
            Debug.Log($"Distance: {distance:F2}m | Angle: {angle:F1}° | LineOfSight: {hasLineOfSight} | In Range: {playerInRange}");
        }

        // Hiển thị/ẩn prompt UI
        if (promptUI != null)
        {
            promptUI.SetActive(playerInRange);
        }

        // Nhấn X để tương tác
        if (playerInRange && Input.GetKeyDown(KeyCode.X))
        {
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        if (isLoading) return;

        isLoading = true;
        Debug.Log($"✓ Loading scene: {nextSceneName}");

        // Gọi FadeController để tạo hiệu ứng fade
        if (FadeController.Instance != null)
        {
            FadeController.Instance.FadeToScene(nextSceneName);
        }
        else
        {
            Debug.LogError("❌ FadeController not found! Make sure FadeCanvas is in the scene.");
        }
    }

    // Vẽ gizmo để dễ debug
    void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn khoảng cách
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        // Vẽ góc phía trước
        Gizmos.color = Color.green;
        Vector3 forward = transform.forward * interactionDistance;

        // Vẽ hướng chính giữa
        Gizmos.DrawRay(transform.position, forward);

        // Vẽ góc tương tác
        Vector3 leftBoundary = Quaternion.Euler(0, -interactionAngle, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, interactionAngle, 0) * forward;

        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);
    }
}