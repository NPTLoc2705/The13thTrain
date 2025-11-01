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

    [Header("Optional: Monologue")]
    [Tooltip("Hiển thị suy nghĩ trước khi mở sách")]
    public bool showMonologueBeforeOpen = false;

    [TextArea(2, 5)]
    public string[] monologueBeforeOpen = new string[]
    {
        "Một cuốn sách cũ...",
        "Bên trong có gì nhỉ?"
    };

    [Header("Sound Settings")]
    [Tooltip("Âm thanh khi mở sách (book open sound)")]
    public AudioClip bookOpenSound;

    [Range(0f, 1f)]
    [Tooltip("Âm lượng của sound effect")]
    public float soundVolume = 0.7f;

    private bool hasBeenOpened = false;
    private AudioSource audioSource;

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

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 10f;
    }

    void Update()
    {
        // CRITICAL: Force ẩn prompt khi viewer đang mở
        if (paintingViewer != null && paintingViewer.IsOpen())
        {
            if (TextManager.Instance != null)
            {
                TextManager.Instance.HidePrompt();
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

        // Ẩn prompt ngay khi nhấn E
        if (TextManager.Instance != null)
        {
            TextManager.Instance.HidePrompt();
        }

        // Play sound khi mở sách
        PlayOpenSound();

        // Nếu có monologue và chưa mở lần nào
        if (showMonologueBeforeOpen && !hasBeenOpened && monologueBeforeOpen.Length > 0)
        {
            if (CharacterMonologue.Instance != null)
            {
                CharacterMonologue.Instance.ShowMonologueWithCallback(monologueBeforeOpen, () =>
                {
                    // Sau khi xem xong monologue, mở viewer
                    paintingViewer.OpenViewer();
                    // Đánh dấu đã mở SAU KHI viewer mở xong
                    hasBeenOpened = true;
                });

                return;
            }
        }

        // Mở viewer trực tiếp (nếu đã mở lần đầu rồi)
        paintingViewer.OpenViewer();
        hasBeenOpened = true;
    }

    /// <summary>
    /// Phát âm thanh khi mở sách
    /// </summary>
    void PlayOpenSound()
    {
        if (audioSource != null && bookOpenSound != null)
        {
            audioSource.PlayOneShot(bookOpenSound, soundVolume);
            Debug.Log("🔊 Playing book open sound");
        }
    }

    /// <summary>
    /// Lấy prompt message để hiển thị
    /// </summary>
    public string GetPromptMessage()
    {
        // Chỉ hiện prompt khi viewer đóng
        if (paintingViewer != null && paintingViewer.IsOpen())
        {
            return null; // Viewer đang mở → ẩn prompt
        }

        // Viewer đóng → hiện prompt
        return "[E] Mở cuốn sách";
    }

    // Visualize interaction range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 5f); // Interaction range visualization
    }
}