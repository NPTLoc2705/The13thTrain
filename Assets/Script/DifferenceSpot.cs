using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script đại diện cho một điểm khác nhau trên bức tranh
/// Gắn vào các button/area có thể click trên painting
/// Phiên bản đơn giản - không dùng LeanTween
/// FIXED: Thêm raycastTarget và debug logging để sửa lỗi click
/// </summary>
public class DifferenceSpot : MonoBehaviour
{
    [Header("Spot Settings")]
    [Tooltip("ID duy nhất của điểm này (vd: spot_1, spot_2...)")]
    public string spotID;

    [Tooltip("Bức tranh nào chứa điểm này (0-3 tương ứng với 4 bức tranh)")]
    public int paintingIndex;

    [Header("Visual Feedback")]
    [Tooltip("Vòng tròn đánh dấu (sẽ hiện lên khi tìm đúng)")]
    public GameObject circleMarker;

    [Header("Sound")]
    public AudioClip foundSound;
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    private Button button;
    private bool isFound = false;
    private AudioSource audioSource;

    void Awake()
    {
        Debug.Log($"🎯 DifferenceSpot '{spotID}' Awake() - Painting Index: {paintingIndex}");

        // Get Button component
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"❌ DifferenceSpot '{spotID}' thiếu Button component!");
        }
        else
        {
            // Add click listener
            button.onClick.AddListener(OnSpotClicked);
            Debug.Log($"✅ Button listener added for '{spotID}'");
        }

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound

        // Ẩn circle marker ban đầu
        if (circleMarker != null)
        {
            circleMarker.SetActive(false);
            Debug.Log($"✅ Circle marker hidden for '{spotID}'");
        }
        else
        {
            Debug.LogWarning($"⚠️ Circle marker chưa gán cho '{spotID}'");
        }

        // CRITICAL FIX: Ẩn button visual nhưng GIỮ raycastTarget = true
        Image buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            Color transparent = buttonImage.color;
            transparent.a = 0.01f; // Gần như trong suốt nhưng vẫn có thể raycast
            buttonImage.color = transparent;
            buttonImage.raycastTarget = true; // QUAN TRỌNG: Phải bật để nhận click!
            Debug.Log($"✅ Button image setup for '{spotID}' - raycastTarget: {buttonImage.raycastTarget}");
        }
        else
        {
            Debug.LogWarning($"⚠️ '{spotID}' không có Image component - sẽ không nhận click được!");
        }
    }

    void OnSpotClicked()
    {
        Debug.Log($"🖱️ CLICK DETECTED on Spot '{spotID}' (Painting {paintingIndex}) at position {transform.position}");

        if (isFound)
        {
            Debug.Log($"⚠️ Điểm '{spotID}' đã được tìm thấy rồi!");
            return;
        }

        // Check xem người chơi đã có bút chưa
        if (PickupManager.Instance == null || !PickupManager.Instance.IsCollected("Pen"))
        {
            Debug.Log("⚠️ Chưa có bút, không thể đánh dấu!");
            if (TextManager.Instance != null)
            {
                TextManager.Instance.ShowNotice("Mình cần một cây bút để đánh dấu...", 2f);
            }
            return;
        }

        // Check xem có đang xem đúng bức tranh không
        if (PaintingViewerUI.Instance != null)
        {
            // Tìm puzzle manager
            DifferencePuzzleManager puzzleManager = FindObjectOfType<DifferencePuzzleManager>();
            if (puzzleManager != null)
            {
                Debug.Log($"✅ Forwarding click to PuzzleManager for spot '{spotID}'");
                puzzleManager.OnSpotFound(this);
            }
            else
            {
                Debug.LogError("❌ Không tìm thấy DifferencePuzzleManager!");
            }
        }
    }

    /// <summary>
    /// Đánh dấu điểm này là đã tìm thấy
    /// </summary>
    public void MarkAsFound()
    {
        if (isFound) return;

        Debug.Log($"✅ Marking spot '{spotID}' as found");

        isFound = true;

        // Hiển thị vòng tròn đánh dấu
        if (circleMarker != null)
        {
            circleMarker.SetActive(true);

            // Simple scale animation (không dùng LeanTween)
            StartCoroutine(SimpleScaleAnimation());
        }

        // Play sound
        if (audioSource != null && foundSound != null)
        {
            audioSource.PlayOneShot(foundSound, soundVolume);
        }

        // Disable button để không click lại được
        if (button != null)
        {
            button.interactable = false;
        }

        Debug.Log($"✅ Đã tìm thấy điểm khác nhau: {spotID}");
    }

    /// <summary>
    /// Simple scale animation (thay thế LeanTween)
    /// </summary>
    System.Collections.IEnumerator SimpleScaleAnimation()
    {
        if (circleMarker == null) yield break;

        Transform marker = circleMarker.transform;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * 1.2f;

        // Scale up
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            marker.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Scale down
        elapsed = 0f;
        duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            marker.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        marker.localScale = originalScale;
    }

    /// <summary>
    /// Reset trạng thái (dùng khi chơi lại)
    /// </summary>
    public void ResetSpot()
    {
        isFound = false;

        if (circleMarker != null)
        {
            circleMarker.SetActive(false);
        }

        if (button != null)
        {
            button.interactable = true;
        }

        Debug.Log($"🔄 Reset spot '{spotID}'");
    }

    public bool IsFound()
    {
        return isFound;
    }

    // Visualize trong editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 20f);

        // Hiển thị spot ID
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position, $"Spot: {spotID}\nPainting: {paintingIndex}");
#endif
    }
}