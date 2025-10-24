using UnityEngine;

public class KeyItem : PickupItem
{
    public override void OnPickup()
    {
        // Call base OnPickup to handle common collection logic
        base.OnPickup(); // Sets isCollected, logs pickup, and hides object if destroyOnPickup is true

        // Log specific message for the key
        Debug.Log("Safe Key picked up!");

        // Register with PickupManager if it exists
        if (PickupManager.Instance != null)
        {
            PickupManager.Instance.CollectItem(this);
        }
        else
        {
            Debug.LogWarning("PickupManager.Instance is null! Ensure PickupManager is in the scene.");
        }
    }
}