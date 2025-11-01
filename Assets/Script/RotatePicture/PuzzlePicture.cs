using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Tooltip("Danh sách các RotatePicture (4 hình)")]
    public RotatePicture[] pictures;

    [Tooltip("Vật phẩm phần thưởng (hiện ra sau khi hoàn thành)")]
    public GameObject rewardObject;

    [Tooltip("Nếu tick thì cấp vật phẩm trực tiếp, không hiện vật trên bản đồ.")]
    public bool grantDirectly = false;

    [Tooltip("ID vật phẩm nếu cấp trực tiếp")]
    public string rewardPickupItemId;

    [Tooltip("Tham chiếu đến PickupItem component trên rewardObject (để auto-collect)")]
    public PickupItem rewardPickupItem;

    private bool puzzleCompleted = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CheckPuzzleState()
    {
        if (puzzleCompleted) return;

        // Kiểm tra từng hình xem đã đúng chưa
        foreach (var pic in pictures)
        {
            if (pic == null)
            {
                Debug.LogWarning("PuzzleManager: Có phần tử hình ảnh chưa được gán!");
                return;
            }

            if (!pic.isCorrect)
            {
                Debug.Log($"❌ {pic.name} chưa đúng góc.");
                return;
            }
        }

        // Tất cả đều đúng
        puzzleCompleted = true;
        OnPuzzleCompleted();
    }

    private void OnPuzzleCompleted()
    {
        Debug.Log("✅ Puzzle completed!");

        // AUTO-COLLECT logic (like RadioPuzzle)
        if (grantDirectly && rewardPickupItem != null && PickupManager.Instance != null)
        {
            // Mark as collected immediately
            PickupManager.Instance.CollectItem(rewardPickupItem);

            // Play pickup sound if available
            if (rewardPickupItem.pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    rewardPickupItem.pickupSound,
                    Camera.main.transform.position,
                    rewardPickupItem.soundVolume
                );
            }

            // Show collection message
            if (TextManager.Instance != null)
            {
                TextManager.Instance.ShowNotice("Bạn đã nhận được 1 mảnh giấy!", 3f);
            }

            // Check if all pieces collected (trigger letter UI)
            if (PickupManager.Instance.collectedItemIDs.Count == 5)
            {
                if (LetterUIController.Instance != null)
                {
                    LetterUIController.Instance.ShowLetterUI();
                }
            }

            Debug.Log($"[PuzzleManager] Auto-collected: {rewardPickupItem.itemID} ({PickupManager.Instance.collectedItemIDs.Count}/5)");
        }
        // Original logic: Show reward object on map
        else if (rewardObject != null && !grantDirectly)
        {
            rewardObject.SetActive(true);
            Debug.Log("🎁 Đã bật vật phẩm phần thưởng trên bản đồ.");
        }

        // Có thể gọi UI hiển thị thông báo:
        // UIManager.Instance.ShowMessage("Bạn đã hoàn thành câu đố!");
    }

    public bool IsPuzzleCompleted()
    {
        return puzzleCompleted;
    }
}