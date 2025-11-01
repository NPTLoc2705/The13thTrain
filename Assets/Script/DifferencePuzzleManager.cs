using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Quản lý puzzle "Tìm điểm khác nhau" trong PaintingViewerUI
/// TỰ ĐỘNG BẬT khi có bút - Không cần nhấn F
/// VERSION: In-Viewer Thoughts + Key Reveal
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

    [Header("Completion Thoughts - IN VIEWER")]
    [Tooltip("Hiển thị suy nghĩ TRONG viewer sau khi hoàn thành")]
    public bool showCompletionThoughts = true;

    [Tooltip("Text object để hiển thị thoughts (thường là thoughtsText của PaintingViewerUI)")]
    public TextMeshProUGUI thoughtsText;

    [TextArea(2, 5)]
    public string[] completionThoughts = new string[]
    {
        "Mình đã tìm thấy tất cả!",
        "Những bức tranh này... có vẻ là một câu chuyện đau buồn...",
        "Mình cần tìm hiểu thêm về nơi này."
    };

    [Header("Thoughts Display Settings")]
    [Tooltip("Thời gian hiển thị mỗi dòng thoughts (giây)")]
    public float thoughtDisplayDuration = 3f;

    [Tooltip("Thời gian fade in/out")]
    public float thoughtFadeDuration = 0.5f;

    [Header("Reward Settings")]
    [Tooltip("GameObject chìa khóa (hoặc vật phẩm bất kỳ) sẽ xuất hiện sau khi hoàn thành")]
    public GameObject rewardObject;

    [Tooltip("Hiển thị thông báo về chìa khóa sau khi hoàn thành?")]
    public bool showKeyNotice = true;

    [Tooltip("Nội dung thông báo về chìa khóa")]
    public string keyNoticeMessage = "Có gì đó đã xuất hiện trong phòng...";

    [Tooltip("Delay (giây) trước khi hiện thông báo chìa khóa")]
    public float keyNoticeDelay = 1f;

    private bool isPuzzleActive = false;
    private int foundCount = 0;
    private bool isPuzzleCompleted = false;
    private PaintingViewerUI paintingViewer;
    private bool isShowingThoughts = false;

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

        // Check thoughtsText
        if (thoughtsText == null)
        {
            Debug.LogWarning("⚠️ thoughtsText chưa được gán! Thoughts sẽ không hiển thị.");
        }
        else
        {
            // Ẩn thoughts text ban đầu
            thoughtsText.text = "";
            CanvasGroup cg = thoughtsText.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = thoughtsText.gameObject.AddComponent<CanvasGroup>();
            }
            cg.alpha = 0f;
        }

        // Ẩn reward object ban đầu
        if (rewardObject != null)
        {
            rewardObject.SetActive(false);
            Debug.Log("✅ Reward object hidden initially");
        }
        else
        {
            Debug.LogWarning("⚠️ Reward object (chìa khóa) chưa được gán!");
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
            // CRITICAL FIX: Delay để đợi circle marker animation hoàn thành (0.35s)
            // Trước khi chạy thoughts và các hành động tiếp theo
            StartCoroutine(DelayedPuzzleCompletion(0.5f));
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

        // CRITICAL FIX: Chỉ disable buttons, GIỮ circle markers visible
        DisableAllSpots(hideGameObjects: false);

        // Hiển thị thông báo hoàn thành (không dùng ký tự đặc biệt)
        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowNotice("Đã tìm thấy tất cả điểm khác nhau!", 3f);
        }

        // NEW: Ẩn spots sau khi circle markers hiện đủ (1s), TRƯỚC KHI thoughts bắt đầu
        StartCoroutine(HideSpotsBeforeThoughts());
    }

    /// <summary>
    /// Ẩn tất cả spots sau khi circle markers hiện đủ, trước khi chạy thoughts
    /// </summary>
    IEnumerator HideSpotsBeforeThoughts()
    {
        // Đợi 1 giây để người chơi thấy rõ các circle markers
        yield return new WaitForSeconds(1f);

        Debug.Log("👁️ Ẩn tất cả spots trước khi chạy thoughts...");

        // Ẩn TẤT CẢ spots (kể cả circle markers)
        foreach (DifferenceSpot spot in allSpots)
        {
            if (spot != null)
            {
                spot.gameObject.SetActive(false);
            }
        }

        // Sau khi ẩn spots, chờ thêm 0.5s rồi hiển thị thoughts
        yield return new WaitForSeconds(0.5f);

        // Hiển thị thoughts
        if (showCompletionThoughts && completionThoughts.Length > 0)
        {
            StartCoroutine(ShowThoughtsSequence());
        }
        else
        {
            // Nếu không có thoughts, thực hiện hành động kết thúc ngay
            OnThoughtsComplete();
        }
    }

    /// <summary>
    /// CRITICAL FIX: Delay trước khi gọi OnPuzzleCompleted
    /// Để đảm bảo circle marker animation hoàn thành
    /// </summary>
    IEnumerator DelayedPuzzleCompletion(float delay)
    {
        Debug.Log($"⏳ Đợi {delay}s để circle marker animation hoàn thành...");
        yield return new WaitForSeconds(delay);
        OnPuzzleCompleted();
    }

    /// <summary>
    /// Coroutine: Delay trước khi hiển thị thoughts
    /// </summary>
    IEnumerator ShowThoughtsSequenceDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(ShowThoughtsSequence());
    }

    /// <summary>
    /// Hiển thị chuỗi thoughts TRONG viewer (giống first-time thoughts)
    /// </summary>
    IEnumerator ShowThoughtsSequence()
    {
        if (thoughtsText == null)
        {
            Debug.LogWarning("⚠️ thoughtsText is null, skipping thoughts display");
            OnThoughtsComplete();
            yield break;
        }

        isShowingThoughts = true;
        Debug.Log("📖 Bắt đầu hiển thị completion thoughts trong viewer");

        CanvasGroup cg = thoughtsText.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = thoughtsText.gameObject.AddComponent<CanvasGroup>();
        }

        foreach (string thought in completionThoughts)
        {
            // Fade in
            thoughtsText.text = thought;
            yield return StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, thoughtFadeDuration));

            // Hold
            yield return new WaitForSeconds(thoughtDisplayDuration);

            // Fade out
            yield return StartCoroutine(FadeCanvasGroup(cg, 1f, 0f, thoughtFadeDuration));

            // Clear text
            thoughtsText.text = "";

            // Brief pause between thoughts
            yield return new WaitForSeconds(0.3f);
        }

        isShowingThoughts = false;
        Debug.Log("✅ Hoàn thành hiển thị thoughts");

        // Sau khi thoughts xong, trigger hành động kết thúc
        OnThoughtsComplete();
    }

    /// <summary>
    /// Fade CanvasGroup helper
    /// </summary>
    IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        cg.alpha = endAlpha;
    }

    /// <summary>
    /// CALLBACK: Được gọi SAU KHI thoughts hoàn thành
    /// </summary>
    void OnThoughtsComplete()
    {
        Debug.Log("🎬 Thoughts hoàn thành - Bắt đầu hành động kết thúc!");

        // Spots đã được ẩn trong HideSpotsBeforeThoughts(), không cần ẩn nữa

        // 1. Hiện chìa khóa
        RevealRewardObject();

        // 2. Hiển thị thông báo về chìa khóa (sau delay)
        if (showKeyNotice && !string.IsNullOrEmpty(keyNoticeMessage))
        {
            Invoke("ShowKeyNotice", keyNoticeDelay);
        }
    }

    /// <summary>
    /// Ẩn tất cả circle markers (SAU thoughts)
    /// </summary>
    void HideAllCircleMarkers()
    {
        Debug.Log("👁️ Ẩn tất cả circle markers...");

        foreach (DifferenceSpot spot in allSpots)
        {
            if (spot != null && spot.circleMarker != null)
            {
                // Fade out animation (optional)
                StartCoroutine(FadeOutMarker(spot.circleMarker));
            }
        }
    }

    /// <summary>
    /// Fade out animation cho circle marker
    /// </summary>
    IEnumerator FadeOutMarker(GameObject marker)
    {
        CanvasGroup canvasGroup = marker.GetComponent<CanvasGroup>();

        // Thêm CanvasGroup nếu chưa có
        if (canvasGroup == null)
        {
            canvasGroup = marker.AddComponent<CanvasGroup>();
        }

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        // Ẩn hoàn toàn sau khi fade xong
        marker.SetActive(false);
        canvasGroup.alpha = 1f; // Reset cho lần sau (nếu cần)
    }

    /// <summary>
    /// Hiện chìa khóa (hoặc reward object)
    /// </summary>
    void RevealRewardObject()
    {
        if (rewardObject == null)
        {
            Debug.LogWarning("⚠️ Reward object không được gán, bỏ qua!");
            return;
        }

        Debug.Log("🔑 Hiện chìa khóa!");

        // Hiện object
        rewardObject.SetActive(true);

        // Optional: Thêm effect hiện lên (scale animation, particle, etc.)
        StartCoroutine(ScaleInRewardObject());
    }

    /// <summary>
    /// Scale animation khi chìa khóa xuất hiện
    /// </summary>
    IEnumerator ScaleInRewardObject()
    {
        if (rewardObject == null) yield break;

        Transform rewardTransform = rewardObject.transform;
        Vector3 originalScale = rewardTransform.localScale;
        Vector3 startScale = Vector3.zero;

        rewardTransform.localScale = startScale;

        float duration = 0.5f;
        float elapsed = 0f;

        // Scale up từ 0 → original
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease out elastic effect
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
            rewardTransform.localScale = Vector3.Lerp(startScale, originalScale, scale);

            yield return null;
        }

        rewardTransform.localScale = originalScale;
    }

    /// <summary>
    /// Hiển thị thông báo về chìa khóa
    /// </summary>
    void ShowKeyNotice()
    {
        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowNotice(keyNoticeMessage, 3f);
        }

        Debug.Log($"💬 {keyNoticeMessage}");
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
    /// FIXED: Chỉ disable button interaction, GIỮ circle markers visible khi hoàn thành
    /// </summary>
    /// <param name="hideGameObjects">True = ẩn GameObjects hoàn toàn, False = chỉ disable buttons</param>
    void DisableAllSpots(bool hideGameObjects = true)
    {
        foreach (DifferenceSpot spot in allSpots)
        {
            if (spot == null) continue;

            if (hideGameObjects)
            {
                // Ẩn hoàn toàn (khi thoát puzzle mode)
                spot.gameObject.SetActive(false);
            }
            else
            {
                // CHỈ disable button, GIỮ circle markers visible (khi hoàn thành puzzle)
                Button btn = spot.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = false;
                }
            }

            Debug.Log($"🔒 Disabled spot '{spot.spotID}' (hidden: {hideGameObjects}, found: {spot.IsFound()})");
        }

        Debug.Log($"✅ Disabled all spots (hideGameObjects: {hideGameObjects})");
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
    /// Check xem đang hiển thị thoughts không (PUBLIC)
    /// </summary>
    public bool IsShowingThoughts()
    {
        return isShowingThoughts;
    }

    /// <summary>
    /// Reset puzzle (dùng khi chơi lại)
    /// </summary>
    public void ResetPuzzle()
    {
        foundCount = 0;
        isPuzzleCompleted = false;
        isPuzzleActive = false;
        isShowingThoughts = false;

        foreach (DifferenceSpot spot in allSpots)
        {
            if (spot != null)
            {
                spot.ResetSpot();
            }
        }

        // Ẩn reward object
        if (rewardObject != null)
        {
            rewardObject.SetActive(false);
        }

        // Clear thoughts text
        if (thoughtsText != null)
        {
            thoughtsText.text = "";
            CanvasGroup cg = thoughtsText.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
            }
        }

        UpdateProgressUI();
        Debug.Log("🔄 Đã reset Difference Puzzle");
    }
}