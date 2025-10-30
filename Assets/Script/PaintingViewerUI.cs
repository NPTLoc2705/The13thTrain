using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Controller để xem các bức tranh/vẽ
/// Cho phép người chơi xem qua lại các bức tranh
/// UPDATED: Thêm tích hợp Puzzle Mode - Tìm điểm khác nhau
/// </summary>
public class PaintingViewerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject paintingPanel;
    [SerializeField] private RawImage paintingDisplay;
    [SerializeField] private TextMeshProUGUI instructionText; // Text hướng dẫn
    [SerializeField] private TextMeshProUGUI pageCounterText; // "1/5"
    [SerializeField] private TextMeshProUGUI thoughtsText; // Text hiển thị suy nghĩ (optional)

    [Header("Painting Materials")]
    [SerializeField] private Material[] paintingMaterials;

    [Header("Settings")]
    [SerializeField] private KeyCode closeKey = KeyCode.Z; // Dùng Z để đóng, tránh conflict với ESC Pause Menu
    [SerializeField] private string instructionMessage = "◄ ► : Xem tranh | Z : Đóng";

    [Header("Puzzle Mode")]
    [Tooltip("Cho phép puzzle mode (tìm điểm khác nhau)")]
    [SerializeField] private bool enablePuzzleMode = true;

    [Header("First Time Thoughts")]
    [Tooltip("Hiển thị suy nghĩ khi xem tranh lần đầu")]
    [SerializeField] private bool showFirstTimeThoughts = true;
    [Tooltip("Dùng thoughtsText riêng (nếu có) thay vì CharacterMonologue")]
    [SerializeField] private bool useInternalThoughtsText = false;
    [TextArea(2, 5)]
    [SerializeField]
    private string[] firstTimeThoughts = new string[]
    {
        "Những bức vẽ này...",
        "Chúng có ý nghĩa gì đây?",
        "Có vẻ như chúng đang kể một câu chuyện..."
    };

    [Header("Puzzle Hint - Pen Requirement")]
    [Tooltip("Item ID của cây bút cần tìm")]
    [SerializeField] private string penItemID = "Pen";
    [Tooltip("Hiển thị hint khi chưa có bút (các lần xem sau lần đầu)")]
    [SerializeField] private bool showPenHint = true;
    [Tooltip("Nội dung hint khi chưa có bút")]
    [TextArea(1, 3)]
    [SerializeField] private string penHintMessage = "Mình cần tìm một cây bút...";

    private int currentPaintingIndex = 0;
    private bool isOpen = false;
    private bool hasViewedOnce = false; // Track xem đã xem lần đầu chưa
    private MonoBehaviour playerController;
    private DifferencePuzzleManager puzzleManager; // Reference to puzzle system

    // Singleton
    public static PaintingViewerUI Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // FORCE: Đảm bảo closeKey luôn là Z, tránh conflict với ESC của Pause Menu
        closeKey = KeyCode.Z;

        // Get puzzle manager
        puzzleManager = GetComponent<DifferencePuzzleManager>();
        if (puzzleManager == null && enablePuzzleMode)
        {
            Debug.LogWarning("⚠️ EnablePuzzleMode = true nhưng không tìm thấy DifferencePuzzleManager!");
        }

        // Tìm player controller
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = player.GetComponent<PlayerController_LN_SmoothMove>();
            }
            Debug.Log("✅ Player controller found!");
        }
        else
        {
            Debug.LogWarning("⚠️ Player not found!");
        }

        // Validate UI References
        ValidateUIReferences();

        // Setup instruction text
        if (instructionText != null)
        {
            instructionText.text = instructionMessage;
            Debug.Log("✅ Instruction text set");
        }
        else
        {
            Debug.LogWarning("⚠️ Instruction Text chưa được gán (optional)");
        }

        // Setup thoughts text (ẩn ban đầu)
        if (thoughtsText != null)
        {
            thoughtsText.text = "";
            Debug.Log("✅ Thoughts text initialized");
        }

        // Ẩn panel ban đầu
        if (paintingPanel != null)
        {
            paintingPanel.SetActive(false);
            Debug.Log("✅ Painting panel hidden");
        }
        else
        {
            Debug.LogError("❌ Painting Panel chưa được gán!");
        }

        // Validate materials
        if (paintingMaterials == null || paintingMaterials.Length == 0)
        {
            Debug.LogError("❌ PaintingViewerUI: Chưa gán materials cho các bức tranh!");
        }
        else
        {
            Debug.Log($"✅ {paintingMaterials.Length} materials loaded");
        }
    }

    void ValidateUIReferences()
    {
        if (paintingDisplay == null)
            Debug.LogError("❌ Painting Display (RawImage) chưa được gán!");

        if (paintingPanel == null)
            Debug.LogError("❌ Painting Panel chưa được gán!");
    }

    void Update()
    {
        if (!isOpen)
        {
            // Debug: Kiểm tra xem script có chạy không
            if (Input.GetKeyDown(KeyCode.X))
            {
                Debug.Log("⚠️ Nhấn X nhưng viewer đang đóng (isOpen = false)");
            }
            return;
        }

        // Debug: Kiểm tra isOpen
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log($"🔑 Phím X được nhấn! isOpen = {isOpen}, closeKey = {closeKey}");
        }

        // Đóng bằng phím Z
        if (Input.GetKeyDown(closeKey))
        {
            Debug.Log("✅ Đang gọi CloseViewer()...");
            CloseViewer();
        }

        // Navigation bằng phím mũi tên hoặc A/D
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            NextPainting();

            // Nếu đang trong puzzle mode, cập nhật spots
            if (enablePuzzleMode && puzzleManager != null)
            {
                puzzleManager.EnterPuzzleMode(); // Re-enable spots cho bức mới
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            PreviousPainting();

            // Nếu đang trong puzzle mode, cập nhật spots
            if (enablePuzzleMode && puzzleManager != null)
            {
                puzzleManager.EnterPuzzleMode(); // Re-enable spots cho bức mới
            }
        }
    }

    /// <summary>
    /// Mở viewer và hiển thị bức tranh đầu tiên
    /// </summary>
    public void OpenViewer()
    {
        Debug.Log("📖 OpenViewer() được gọi!");

        if (paintingMaterials == null || paintingMaterials.Length == 0)
        {
            Debug.LogError("❌ Không có bức tranh nào để hiển thị!");
            if (TextManager.Instance != null)
            {
                TextManager.Instance.ShowNotice("Không có gì để xem...", 2f);
            }
            return;
        }

        isOpen = true;
        currentPaintingIndex = 0;

        Debug.Log($"✅ isOpen = {isOpen}");

        // Hiển thị panel
        if (paintingPanel != null)
        {
            paintingPanel.SetActive(true);
            Debug.Log("✅ Panel đã được hiển thị");
        }
        else
        {
            Debug.LogError("❌ PaintingPanel is NULL!");
        }

        // Hiển thị bức tranh đầu tiên
        DisplayCurrentPainting();

        // Vô hiệu hóa player movement
        DisablePlayerMovement();

        // Ẩn prompt nếu có
        if (TextManager.Instance != null)
            TextManager.Instance.HidePrompt();

        Debug.Log($"📖 Painting Viewer đã mở! isOpen = {isOpen}, closeKey = {closeKey}");

        // Logic hiển thị thoughts/hints
        if (!hasViewedOnce && showFirstTimeThoughts && firstTimeThoughts.Length > 0)
        {
            // LẦN ĐẦU: Show first-time thoughts
            hasViewedOnce = true;
            StartCoroutine(ShowThoughtsAfterDelay(0.5f));
        }
        else if (hasViewedOnce && showPenHint && !HasPen())
        {
            // CÁC LẦN SAU + CHƯA CÓ BÚT: Show pen hint
            StartCoroutine(ShowPenHintAfterDelay(0.5f));
        }
        // Nếu đã có bút: Không hiển thị gì, xem tranh thoải mái
    }

    /// <summary>
    /// Show thoughts sau một khoảng delay ngắn (để tranh hiện lên trước)
    /// </summary>
    private System.Collections.IEnumerator ShowThoughtsAfterDelay(float delay)
    {
        yield return new UnityEngine.WaitForSeconds(delay);

        if (!isOpen) yield break;

        // Nếu dùng internal thoughts text
        if (useInternalThoughtsText && thoughtsText != null)
        {
            yield return StartCoroutine(ShowInternalThoughts());
        }
        // Nếu dùng CharacterMonologue (default)
        else if (CharacterMonologue.Instance != null)
        {
            CharacterMonologue.Instance.ShowMonologueWithCallback(firstTimeThoughts, null);
        }
    }

    /// <summary>
    /// Show pen hint sau một khoảng delay ngắn
    /// </summary>
    private System.Collections.IEnumerator ShowPenHintAfterDelay(float delay)
    {
        yield return new UnityEngine.WaitForSeconds(delay);

        if (!isOpen) yield break;

        // Nếu dùng internal thoughts text
        if (useInternalThoughtsText && thoughtsText != null)
        {
            yield return StartCoroutine(ShowInternalSingleThought(penHintMessage));
        }
        // Nếu dùng CharacterMonologue (default)
        else if (CharacterMonologue.Instance != null)
        {
            CharacterMonologue.Instance.ShowMonologueWithCallback(penHintMessage, null);
        }
    }

    /// <summary>
    /// Kiểm tra xem đã nhặt bút chưa
    /// </summary>
    private bool HasPen()
    {
        if (PickupManager.Instance == null) return false;
        return PickupManager.Instance.IsCollected(penItemID);
    }

    /// <summary>
    /// Hiển thị thoughts bằng text riêng trong painting viewer (không che tranh)
    /// </summary>
    private System.Collections.IEnumerator ShowInternalThoughts()
    {
        if (thoughtsText == null || firstTimeThoughts.Length == 0)
            yield break;

        // Hiện từng dòng thought
        foreach (string thought in firstTimeThoughts)
        {
            thoughtsText.text = thought;

            // Fade in (optional - nếu có CanvasGroup)
            CanvasGroup cg = thoughtsText.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                yield return StartCoroutine(FadeText(cg, 0f, 1f, 0.3f));
            }

            // Hiển thị trong 2.5 giây
            yield return new UnityEngine.WaitForSeconds(2.5f);

            // Fade out
            if (cg != null)
            {
                yield return StartCoroutine(FadeText(cg, 1f, 0f, 0.3f));
            }

            thoughtsText.text = "";
            yield return new UnityEngine.WaitForSeconds(0.3f);
        }

        // Xóa text sau khi xong
        thoughtsText.text = "";
    }

    /// <summary>
    /// Fade text helper
    /// </summary>
    private System.Collections.IEnumerator FadeText(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        cg.alpha = end;
    }

    /// <summary>
    /// Hiển thị 1 dòng thought (cho pen hint)
    /// </summary>
    private System.Collections.IEnumerator ShowInternalSingleThought(string thought)
    {
        if (thoughtsText == null || string.IsNullOrEmpty(thought))
            yield break;

        thoughtsText.text = thought;

        // Fade in (optional - nếu có CanvasGroup)
        CanvasGroup cg = thoughtsText.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            yield return StartCoroutine(FadeText(cg, 0f, 1f, 0.3f));
        }

        // Hiển thị trong 2.5 giây
        yield return new UnityEngine.WaitForSeconds(2.5f);

        // Fade out
        if (cg != null)
        {
            yield return StartCoroutine(FadeText(cg, 1f, 0f, 0.3f));
        }

        // Xóa text
        thoughtsText.text = "";
    }

    /// <summary>
    /// Đóng viewer
    /// </summary>
    public void CloseViewer()
    {
        Debug.Log("❌ CloseViewer() called!");

        if (!isOpen)
        {
            Debug.LogWarning("⚠️ Viewer không mở, không thể đóng");
            return;
        }

        isOpen = false;

        // Ẩn panel
        if (paintingPanel != null)
            paintingPanel.SetActive(false);

        // Kích hoạt lại player movement
        EnablePlayerMovement();

        Debug.Log("✓ Đã đóng Painting Viewer");
    }

    /// <summary>
    /// Chuyển sang bức tranh tiếp theo
    /// </summary>
    public void NextPainting()
    {
        Debug.Log("🎨 NextPainting() called!");

        if (paintingMaterials == null || paintingMaterials.Length == 0)
        {
            Debug.LogError("❌ Không có materials!");
            return;
        }

        currentPaintingIndex++;
        if (currentPaintingIndex >= paintingMaterials.Length)
        {
            currentPaintingIndex = 0; // Quay vòng về đầu
        }

        Debug.Log($"→ Chuyển sang tranh {currentPaintingIndex + 1}/{paintingMaterials.Length}");
        DisplayCurrentPainting();
    }

    /// <summary>
    /// Quay lại bức tranh trước đó
    /// </summary>
    public void PreviousPainting()
    {
        Debug.Log("🎨 PreviousPainting() called!");

        if (paintingMaterials == null || paintingMaterials.Length == 0)
        {
            Debug.LogError("❌ Không có materials!");
            return;
        }

        currentPaintingIndex--;
        if (currentPaintingIndex < 0)
        {
            currentPaintingIndex = paintingMaterials.Length - 1; // Quay vòng về cuối
        }

        Debug.Log($"← Quay lại tranh {currentPaintingIndex + 1}/{paintingMaterials.Length}");
        DisplayCurrentPainting();
    }

    /// <summary>
    /// Hiển thị bức tranh hiện tại
    /// </summary>
    private void DisplayCurrentPainting()
    {
        if (paintingDisplay == null)
        {
            Debug.LogError("❌ PaintingDisplay RawImage chưa được gán!");
            return;
        }

        if (paintingMaterials == null || currentPaintingIndex >= paintingMaterials.Length)
        {
            Debug.LogError("❌ Index vượt quá số lượng materials!");
            return;
        }

        Material currentMaterial = paintingMaterials[currentPaintingIndex];

        if (currentMaterial != null && currentMaterial.mainTexture != null)
        {
            paintingDisplay.texture = currentMaterial.mainTexture;
            Debug.Log($"🎨 Hiển thị bức tranh {currentPaintingIndex + 1}/{paintingMaterials.Length}");
        }
        else
        {
            Debug.LogError($"❌ Material hoặc texture tại index {currentPaintingIndex} bị null!");
        }

        // Cập nhật page counter nếu có
        UpdatePageCounter();
    }

    /// <summary>
    /// Cập nhật text hiển thị số trang
    /// </summary>
    private void UpdatePageCounter()
    {
        if (pageCounterText != null && paintingMaterials != null)
        {
            pageCounterText.text = $"{currentPaintingIndex + 1}/{paintingMaterials.Length}";
        }
    }

    /// <summary>
    /// Check xem viewer có đang mở không (PUBLIC - cho pause menu check)
    /// </summary>
    public bool IsOpen()
    {
        return isOpen;
    }

    /// <summary>
    /// PUBLIC: Lấy index bức tranh hiện tại (cho DifferencePuzzleManager)
    /// </summary>
    public int GetCurrentPaintingIndex()
    {
        return currentPaintingIndex;
    }

    void DisablePlayerMovement()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Hiển thị và unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void EnablePlayerMovement()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Khóa lại cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}