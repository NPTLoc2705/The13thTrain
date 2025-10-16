using UnityEngine;
using System.Collections.Generic;

public class PickupManager : MonoBehaviour
{
    [Header("Danh sách vật phẩm có thể nhặt")]
    public List<PickupItem> pickupItems = new List<PickupItem>();

    [Header("Danh sách ID đã nhặt được (tự động cập nhật)")]
    public List<string> collectedItemIDs = new List<string>();

    // Singleton để PlayerController có thể gọi
    public static PickupManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPickup(PickupItem item)
    {
        if (!pickupItems.Contains(item))
        {
            pickupItems.Add(item);
        }
    }

    public void CollectItem(PickupItem item)
    {
        if (item == null || item.isCollected) return;

        item.OnPickup();

        if (!collectedItemIDs.Contains(item.itemID))
            collectedItemIDs.Add(item.itemID);
    }

    public bool IsCollected(string itemID)
    {
        return collectedItemIDs.Contains(itemID);
    }
}
