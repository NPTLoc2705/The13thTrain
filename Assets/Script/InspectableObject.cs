using UnityEngine;

/// <summary>
/// Script cho object chỉ xem/quan sát (hiện suy nghĩ) mà KHÔNG nhặt được
/// Được gọi bởi PlayerController thông qua Raycast
/// </summary>
public class InspectableObject : MonoBehaviour
{
    [Header("Object Info")]
    [Tooltip("Tên object để hiển thị prompt")]
    public string objectName = "Vật thể";

    [Header("Inspection Settings")]
    [Tooltip("Có thể xem nhiều lần hay chỉ 1 lần?")]
    public bool canInspectMultipleTimes = true;

    [Tooltip("Nội dung suy nghĩ khi nhấn E")]
    [TextArea(3, 10)]
    public string[] inspectionThoughts = new string[]
    {
        "Đây là gì nhỉ?",
        "Trông có vẻ thú vị..."
    };

    [Header("Optional: Different thoughts on re-inspect")]
    [Tooltip("Nếu bật, lần xem thứ 2 trở đi sẽ hiện những câu khác")]
    public bool useDifferentThoughtsOnRevisit = false;

    [Tooltip("Nội dung suy nghĩ khi xem lại (lần 2+)")]
    [TextArea(3, 5)]
    public string[] revisitThoughts = new string[]
    {
        "Không có gì đặc biệt..."
    };

    private bool hasBeenInspected = false;
    public bool isInspecting = false; // Đang trong quá trình xem

    /// <summary>
    /// Kiểm tra có thể inspect không
    /// </summary>
    public bool CanInspect()
    {
        if (isInspecting) return false;
        if (hasBeenInspected && !canInspectMultipleTimes) return false;
        return true;
    }

    /// <summary>
    /// Lấy prompt message
    /// </summary>
    public string GetPromptMessage()
    {
        if (!CanInspect())
            return "";

        return $"[E] Xem {objectName}";
    }

    /// <summary>
    /// Được gọi khi player nhấn E
    /// </summary>
    public void Inspect()
    {
        if (!CanInspect())
        {
            if (hasBeenInspected && !canInspectMultipleTimes && TextManager.Instance != null)
            {
                TextManager.Instance.ShowNotice("Đã xem rồi.", 1.5f);
            }
            return;
        }

        isInspecting = true;

        // Ẩn prompt khi bắt đầu xem
        if (TextManager.Instance != null)
        {
            TextManager.Instance.HidePrompt();
        }

        // Hiển thị monologue
        if (CharacterMonologue.Instance != null)
        {
            string[] thoughtsToShow;

            // Chọn nội dung suy nghĩ
            if (hasBeenInspected && useDifferentThoughtsOnRevisit && revisitThoughts.Length > 0)
            {
                thoughtsToShow = revisitThoughts;
            }
            else
            {
                thoughtsToShow = inspectionThoughts;
            }

            CharacterMonologue.Instance.ShowMonologueWithCallback(thoughtsToShow, () =>
            {
                // Callback sau khi xem xong
                OnInspectionComplete();
            });

            Debug.Log($"👁️ Đang xem: {objectName}");
        }
        else
        {
            Debug.LogError("❌ CharacterMonologue.Instance not found!");
            isInspecting = false;
        }
    }

    private void OnInspectionComplete()
    {
        hasBeenInspected = true;
        isInspecting = false;
        Debug.Log($"✓ Đã xem xong: {objectName}");
    }

    // Reset state (dùng khi load lại game)
    public void ResetInspection()
    {
        hasBeenInspected = false;
        isInspecting = false;
    }

    // Draw interaction range in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 3f); // Visualize approximate range
    }
}