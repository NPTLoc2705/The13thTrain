using UnityEngine;
using UnityEngine.SceneManagement;
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
        // Persistent singleton across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("PickupManager created and set to DontDestroyOnLoad");
        }
        else if (Instance != this)
        {
            // Destroy duplicate but keep the data
            Debug.LogWarning(" Duplicate PickupManager found in new scene. Destroying duplicate but keeping singleton.");
            Destroy(gameObject);
            return;
        }

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Only clear reference if THIS is the singleton
        if (Instance == this)
        {
            Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Debug.Log(" PickupManager singleton destroyed");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($" Scene '{scene.name}' loaded. PickupManager ready.");

        // If entering gameplay scene from main menu, might want to reset
        if (scene.name == "SampleTest" || scene.name == "MainMenu")
        {
            // Optionally reset if coming from main menu
            // ResetProgress();
        }
    }

    // Call this when starting a new game to reset progress
    public void ResetProgress()
    {
        collectedItemIDs.Clear();
        pickupItems.Clear();
        Debug.Log(" PickupManager progress reset");
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

        Debug.Log($" Collected: {item.itemID} ({collectedItemIDs.Count}/5)");

        // Check if all pieces are collected and show UI
        if (collectedItemIDs.Count == 5)
        {
            // Use singleton to access LetterUIController
            if (LetterUIController.Instance != null)
            {
                LetterUIController.Instance.ShowLetterUI();
            }
            else
            {
                Debug.LogError(" LetterUIController instance not found! Ensure the LetterUI GameObject is in the scene.");
            }
        }
    }

    public bool IsCollected(string itemID)
    {
        return collectedItemIDs.Contains(itemID);
    }
}