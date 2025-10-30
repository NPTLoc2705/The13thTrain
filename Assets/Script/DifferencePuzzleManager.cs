using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Quản lý puzzle "Tìm điểm khác nhau" trong PaintingViewerUI
/// TỰ ĐỘNG BẬT khi có bút - Không cần nhấn F
/// </summary>
public class DifferencePuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("Tổng số điểm khác nhau cần tìm")]
    public int totalDifferences = 4;

    [Tooltip("Tất cả các điểm khác nhau trong 4 bức tranh")]
    public List<DifferenceSpot> allSpots = new List<DifferenceSpot>();

    [Header("UI References - Optional")]
    [Tooltip("Text hiển thị progress (optional) - vd: 2/4")]
    public TextMeshProUGUI progressText;

    [Header("Completion")]
    public bool showCompletionMonologue = true;
    [TextArea(2, 5)]
    public string[] completionMonologue = new string[]
    {
        "Mình đã tìm thấy tất cả!",
        "Những bức tranh này... có vẻ là một câu chuyện đau buồn...",
        "Mình cần tìm hiểu thêm về nơi này."
    };

    private bool isPuzzleActive = false;
    private int foundCount = 0;
    private bool isPuzzleCompleted = false;
    private PaintingViewerUI paintingViewer;

    void Start()
    {
        Debug.Log("🎯 DifferencePuzzleManager Start()");

        // Get PaintingViewerUI reference
        paintingViewer = GetComponent<PaintingViewerUI>();
        if (paintingViewer == null)
        {
            paintingViewer = FindObjectOfType<PaintingViewerUI>();
            Debug.Log(paintingViewer != null ? "✅ Found PaintingViewerUI" : "❌ PaintingViewerUI not found!");
        }

        // Auto-find all DifferenceSpots nếu chưa gán
        if (allSpots.Count == 0)
        {
            DifferenceSpot[] foundSpots = FindObjectsOfType<DifferenceSpot>();
            allSpots.AddRange(foundSpots);
            Debug.Log($"🔍 Tìm thấy {allSpots.Count} difference spots");
        }

        // Validate
        if (allSpots.Count == 0)
        {
            Debug.LogWarning("⚠️ Không tìm thấy DifferenceSpot nào! Hãy tạo các button với DifferenceSpot script.");
        }

        UpdateProgressUI();
    }

    void Update()
    {
        // TỰ ĐỘNG bật puzzle mode nếu có bút và viewer đang mở
        if (!isPuzzleActive && HasPen())
        {
            if (paintingViewer != null && paintingViewer.IsOpen() && !isPuzzleCompleted)
            {
                EnterPuzzleMode();
            }
        }

        // Tự động tắt nếu viewer đóng
        if (isPuzzleActive && (paintingViewer == null || !paintingViewer.IsOpen()))
        {
            ExitPuzzleMode();
        }
    }

    /// <summary>
    /// PUBLIC: Refresh spots khi chuyển tranh
    /// </summary>
    public void RefreshSpotsForCurrentPainting()
    {
        if (!isPuzzleActive) return;

        Debug.Log("🔄 RefreshSpotsForCurrentPainting() called");
        EnableCurrentPaintingSpots();
    }

    /// <summary>
    /// Bật puzzle mode - TỰ ĐỘNG khi có bút
    /// </summary>
    public void EnterPuzzleMode()
    {
        if (!HasPen())
        {
            Debug.Log("⚠️ Chưa có bút!");
            return;
        }

        if (isPuzzleCompleted)
        {
            Debug.Log("⚠️ Puzzle đã hoàn thành rồi!");
            return;
        }

        if (!isPuzzleActive)
        {
            isPuzzleActive = true;
            Debug.Log("✅ Puzzle Mode TỰ ĐỘNG BẬT - Click vào điểm khác nhau!");
        }

        // ALWAYS refresh spots khi gọi method này (dù đã active hay chưa)
        EnableCurrentPaintingSpots();
        UpdateProgressUI();
    }

    /// <summary>
    /// Thoát puzzle mode
    /// </summary>
    public void ExitPuzzleMode()
    {
        if (!isPuzzleActive) return;

        Debug.Log("🚪 ExitPuzzleMode() called");

        isPuzzleActive = false;

        // Disable tất cả spots
        DisableAllSpots();

        Debug.Log("✅ Đã thoát Puzzle Mode");
    }

    /// <summary>
    /// Được gọi khi người chơi click vào một spot
    /// </summary>
    public void OnSpotFound(DifferenceSpot spot)
    {
        Debug.Log($"🎯 OnSpotFound() called for spot: {spot.spotID}");

        if (!isPuzzleActive)
        {
            Debug.Log("⚠️ Puzzle mode chưa bật tự động - Kiểm tra xem có bút chưa!");
            return;
        }

        if (spot == null || spot.IsFound())
        {
            Debug.Log("⚠️ Spot đã được tìm rồi hoặc null!");
            return;
        }

        // Check xem có đang xem đúng bức tranh chứa spot này không
        if (paintingViewer != null)
        {
            int currentIndex = paintingViewer.GetCurrentPaintingIndex();
            if (currentIndex != spot.paintingIndex)
            {
                Debug.Log($"⚠️ Spot này thuộc bức tranh {spot.paintingIndex}, đang xem bức {currentIndex}");
                return;
            }
        }

        // Đánh dấu spot là đã tìm thấy
        spot.MarkAsFound();
        foundCount++;

        Debug.Log($"✅ Tìm thấy spot! Progress: {foundCount}/{totalDifferences}");

        // Cập nhật UI
        UpdateProgressUI();

        // Check hoàn thành
        if (foundCount >= totalDifferences)
        {
            OnPuzzleCompleted();
        }
    }

    /// <summary>
    /// Khi hoàn thành puzzle
    /// </summary>
    void OnPuzzleCompleted()
    {
        isPuzzleCompleted = true;
        isPuzzleActive = false;

        Debug.Log("🎉 ĐÃ HOÀN THÀNH PUZZLE - TÌM HẾT ĐIỂM KHÁC NHAU!");

        // Disable tất cả spots
        DisableAllSpots();

        // Hiển thị thông báo hoàn thành
        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowNotice("✓ Đã tìm thấy tất cả điểm khác nhau!", 3f);
        }

        // Hiển thị monologue sau 1 giây
        if (showCompletionMonologue && completionMonologue.Length > 0)
        {
            Invoke("ShowCompletionMonologue", 1.5f);
        }

        // TODO: Unlock điều gì đó hoặc trigger event tiếp theo
    }

    void ShowCompletionMonologue()
    {
        if (CharacterMonologue.Instance != null)
        {
            CharacterMonologue.Instance.ShowMonologueWithCallback(completionMonologue, null);
        }
    }

    /// <summary>
    /// Enable spots của bức tranh hiện tại
    /// </summary>
    void EnableCurrentPaintingSpots()
    {
        if (paintingViewer == null)
        {
            Debug.LogWarning("⚠️ PaintingViewer is null!");
            return;
        }

        int currentIndex = paintingViewer.GetCurrentPaintingIndex();
        Debug.Log($"🎨 Enabling spots for painting index: {currentIndex}");
        Debug.Log($"📋 Total spots to check: {allSpots.Count}");

        int enabledCount = 0;
        foreach (DifferenceSpot spot in allSpots)
        {
            if (spot == null) continue;

            Debug.Log($"🔍 Checking spot '{spot.spotID}': paintingIndex={spot.paintingIndex}, currentIndex={currentIndex}, isFound={spot.IsFound()}");

            // Chỉ hiện spots của tranh hiện tại (kể cả đã tìm thấy)
            if (spot.paintingIndex == currentIndex)
            {
                spot.gameObject.SetActive(true);

                // Nếu chưa tìm thấy, cho phép click
                if (!spot.IsFound())
                {
                    enabledCount++;
                }

                Debug.Log($"✅ SHOWN spot '{spot.spotID}' (found: {spot.IsFound()})");
            }
            else
            {
                // Ẩn spots của tranh khác
                spot.gameObject.SetActive(false);
                Debug.Log($"❌ HIDDEN spot '{spot.spotID}' (different painting)");
            }
        }

        Debug.Log($"✅ Enabled {enabledCount} active spots for painting {currentIndex}");
    }

    /// <summary>
    /// Disable tất cả spots
    /// </summary>
    void DisableAllSpots()
    {
        foreach (DifferenceSpot spot in allSpots)
        {
            if (spot == null) continue;

            // Ẩn TẤT CẢ spots (kể cả đã tìm thấy) khi thoát puzzle mode
            spot.gameObject.SetActive(false);
        }

        Debug.Log("✅ Disabled all spots");
    }

    /// <summary>
    /// Cập nhật UI hiển thị tiến trình (optional)
    /// </summary>
    void UpdateProgressUI()
    {
        if (progressText != null)
        {
            if (isPuzzleCompleted)
            {
                progressText.text = "Hoàn thành!";
            }
            else
            {
                progressText.text = $"{foundCount}/{totalDifferences}";
            }
        }
    }

    /// <summary>
    /// Check xem đã nhặt bút chưa
    /// </summary>
    bool HasPen()
    {
        if (PickupManager.Instance == null) return false;
        return PickupManager.Instance.IsCollected("Pen");
    }

    /// <summary>
    /// Check xem puzzle đã hoàn thành chưa (PUBLIC)
    /// </summary>
    public bool IsCompleted()
    {
        return isPuzzleCompleted;
    }

    /// <summary>
    /// Reset puzzle (dùng khi chơi lại)
    /// </summary>
    public void ResetPuzzle()
    {
        foundCount = 0;
        isPuzzleCompleted = false;
        isPuzzleActive = false;

        foreach (DifferenceSpot spot in allSpots)
        {
            if (spot != null)
            {
                spot.ResetSpot();
            }
        }

        UpdateProgressUI();
        Debug.Log("🔄 Đã reset Difference Puzzle");
    }
}