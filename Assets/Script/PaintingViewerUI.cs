using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Controller để xem các bức tranh/vẽ
/// Cho phép người chơi xem qua lại các bức tranh
/// </summary>
public class PaintingViewerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject paintingPanel;
    [SerializeField] private RawImage paintingDisplay;
    [SerializeField] private TextMeshProUGUI instructionText; // Text hướng dẫn
    [SerializeField] private TextMeshProUGUI pageCounterText; // "1/5"

    [Header("Painting Materials")]
    [SerializeField] private Material[] paintingMaterials;

    [Header("Settings")]
    [SerializeField] private KeyCode closeKey = KeyCode.Z; // Dùng Z để đóng, tránh conflict với ESC Pause Menu
    [SerializeField] private string instructionMessage = "◄ ► : Xem tranh | Z : Đóng";

    private int currentPaintingIndex = 0;
    private bool isOpen = false;
    private MonoBehaviour playerController;

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

        // Đóng bằng phím X
        if (Input.GetKeyDown(closeKey))
        {
            Debug.Log("✅ Đang gọi CloseViewer()...");
            CloseViewer();
        }

        // Navigation bằng phím mũi tên hoặc A/D
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            NextPainting();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            PreviousPainting();
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