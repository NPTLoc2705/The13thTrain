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

        if (rewardObject != null && !grantDirectly)
        {
            rewardObject.SetActive(true);
            Debug.Log("🎁 Đã bật vật phẩm phần thưởng trên bản đồ.");
        }

        if (grantDirectly && !string.IsNullOrEmpty(rewardPickupItemId))
        {
            Debug.Log($"🎉 Granting reward directly: {rewardPickupItemId}");
            // PickupManager.Instance.AddItemById(rewardPickupItemId);
        }

        // Có thể gọi UI hiển thị thông báo:
        // UIManager.Instance.ShowMessage("Bạn đã hoàn thành câu đố!");
    }

    public bool IsPuzzleCompleted()
    {
        return puzzleCompleted;
    }
}
