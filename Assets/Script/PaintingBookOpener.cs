using UnityEngine;

/// <summary>
/// Script gắn vào cuốn sách/object để mở Painting Viewer khi nhấn E
/// Tương tự như NoteOpener trong code hiện có
/// </summary>
public class PaintingBookOpener : MonoBehaviour
{
    [Header("Viewer Reference")]
    [Tooltip("Kéo PaintingViewerUI vào đây")]
    public PaintingViewerUI paintingViewer;

    [Header("Prompt Settings")]
    [Tooltip("Tên hiển thị của vật phẩm")]
    public string bookName = "Cuốn sách bí ẩn";

    [Tooltip("Prompt message khi nhìn vào")]
    [TextArea(2, 4)]
    public string promptMessage = "[E] Mở cuốn sách\n\"Có vẻ như có gì đó bên trong...\"";

    [Header("Optional: Monologue")]
    [Tooltip("Hiển thị suy nghĩ trước khi mở sách")]
    public bool showMonologueBeforeOpen = false;

    [TextArea(2, 5)]
    public string[] monologueBeforeOpen = new string[]
    {
        "Một cuốn sách cũ...",
        "Bên trong có gì nhỉ?"
    };

    private bool hasBeenOpened = false;

    void Start()
    {
        // Tự động tìm PaintingViewerUI nếu chưa gán
        if (paintingViewer == null)
        {
            paintingViewer = FindObjectOfType<PaintingViewerUI>();

            if (paintingViewer == null)
            {
                Debug.LogError("❌ PaintingViewerUI không tìm thấy! Hãy đảm bảo có PaintingViewerUI trong scene.");
            }
        }
    }

    /// <summary>
    /// Được gọi bởi PlayerController khi nhấn E
    /// </summary>
    public void OpenBook()
    {
        if (paintingViewer == null)
        {
            Debug.LogError("❌ PaintingViewer chưa được gán!");
            return;
        }

        // Nếu có monologue và chưa mở lần nào
        if (showMonologueBeforeOpen && !hasBeenOpened && monologueBeforeOpen.Length > 0)
        {
            if (CharacterMonologue.Instance != null)
            {
                CharacterMonologue.Instance.ShowMonologueWithCallback(monologueBeforeOpen, () =>
                {
                    // Sau khi xem xong monologue, mở viewer
                    paintingViewer.OpenViewer();
                    hasBeenOpened = true;
                });

                return;
            }
        }

        // Mở viewer trực tiếp
        paintingViewer.OpenViewer();
        hasBeenOpened = true;
    }

    /// <summary>
    /// Lấy prompt message để hiển thị
    /// </summary>
    public string GetPromptMessage()
    {
        return promptMessage;
    }

    // Visualize interaction range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 5f); // Interaction range visualization
    }
}