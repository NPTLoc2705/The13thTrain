using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    public float openFloat = 90f;
    public float openSpeed = 2f;
    public bool isOpen = false;

    [Header("Door Interaction Settings")]
    public float interactionDistance = 3f;
    public bool requiresKey = false;  // Tích vào nếu cửa cần chìa khóa
    public string requiredKeyID = "RedKey"; // ID của chìa khóa (chỉ dùng nếu requiresKey = true)

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;
    private Transform playerTransform;

    void Start()
    {
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openFloat, 0));

        // Lấy transform của player
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
            Debug.LogError("Player không tìm thấy! Đảm bảo Player có tag 'Player'");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Kiểm tra khoảng cách
            if (playerTransform != null)
            {
                float distance = Vector3.Distance(playerTransform.position, transform.position);
                if (distance > interactionDistance)
                {
                    Debug.LogWarning($"Quá xa! Cần ở trong {interactionDistance}m để mở cửa");
                    if (TextManager.Instance != null)
                        TextManager.Instance.ShowPrompt($"[!] Quá xa cửa!");
                    return;
                }
            }

            // Kiểm tra chìa khóa (chỉ nếu requiresKey = true)
            if (requiresKey)
            {
                if (PickupManager.Instance != null && !PickupManager.Instance.IsCollected(requiredKeyID))
                {
                    Debug.LogWarning($"Cần {requiredKeyID} để mở cửa!");
                    if (TextManager.Instance != null)
                        TextManager.Instance.ShowPrompt($"[!] Cần chìa khóa để mở cửa!");
                    return;
                }
            }

            // Nếu tất cả điều kiện thỏa mãn, mở cửa
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }
            _currentCoroutine = StartCoroutine(ToggleDoor());
        }
    }

    private IEnumerator ToggleDoor()
    {
        Quaternion targetRotation = isOpen ? _closedRotation : _openRotation;
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }
        transform.rotation = targetRotation;
        isOpen = !isOpen;
        Debug.Log(isOpen ? "Cửa đã mở" : "Cửa đã đóng");
    }
}