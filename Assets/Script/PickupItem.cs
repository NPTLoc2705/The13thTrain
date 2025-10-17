using UnityEngine;

[System.Serializable]
public class PickupItem : MonoBehaviour
{
    [Header("Thông tin vật phẩm")]
    public string itemID;        // ID duy nhất, ví dụ "Piece1"
    public string itemName;      // Tên hiển thị: "Mảnh tranh 1"
    public GameObject model;     // Model của vật phẩm (kéo prefab vào)

    [Tooltip("Nếu true thì vật phẩm sẽ biến mất khi nhặt")]
    public bool destroyOnPickup = true;

    public bool isCollected = false;

    public void OnPickup()
    {
        if (isCollected) return;

        isCollected = true;
        Debug.Log($"✅ Đã nhặt: {itemName}");

        if (destroyOnPickup)
            gameObject.SetActive(false);
    }
}
