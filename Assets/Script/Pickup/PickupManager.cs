using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PickupManager : MonoBehaviour
{
    [Header("Danh sách vật phẩm có thể nhặt")]
    public List<PickupItem> pickupItems = new List<PickupItem>();

    [Header("Danh sách ID đã nhặt được (tự động cập nhật)")]
    public List<string> collectedItemIDs = new List<string>();

    [Header("Scene Settings")]
    [Tooltip("If true, keeps puzzle piece progress across scenes. If false, resets everything.")]
    public bool keepPuzzleProgressAcrossScenes = false;

    // Track puzzle pieces separately for clarity
    private int puzzlePiecesCollected = 0;

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
            Debug.LogWarning("Duplicate PickupManager found in new scene. Destroying duplicate but keeping singleton.");
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Debug.Log("PickupManager singleton destroyed");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Clean null references to prevent ghost objects
        pickupItems.RemoveAll(x => x == null);

        if (keepPuzzleProgressAcrossScenes)
        {
            // OPTION A: Keep puzzle pieces, reset everything else
            // Remove non-puzzle items from collected list
            collectedItemIDs.RemoveAll(id =>
            {
                PickupItem foundItem = pickupItems.Find(x => x != null && x.itemID == id);
                return foundItem == null || !foundItem.CompareTag("PuzzlePiece");
            });

            Debug.Log($"🔄 Entered {scene.name} - Kept puzzle progress ({puzzlePiecesCollected}/5), reset other items");

            // Check if we should show letter in SampleScene
            if (scene.name == "SampleScene" && puzzlePiecesCollected >= 5)
            {
                Debug.Log("🧩 All puzzle pieces collected — showing letter.");
                StartCoroutine(ShowLetterAfterDelay());
            }
        }
        else
        {
            // OPTION B: Reset EVERYTHING when entering new scene
            collectedItemIDs.Clear();
            puzzlePiecesCollected = 0;

            Debug.Log($"🔄 Entered {scene.name} - Reset all pickup progress (fresh start)");
        }
    }

    private IEnumerator ShowLetterAfterDelay()
    {
        yield return new WaitForSeconds(0.25f);

        if (LetterUIController.Instance != null)
        {
            LetterUIController.Instance.ShowLetterUI();
        }
        else
        {
            Debug.LogWarning("🧩 LetterUIController not found in scene!");
        }
    }

    // Call this when starting a new game to reset progress completely
    public void ResetProgress()
    {
        collectedItemIDs.Clear();
        pickupItems.Clear();
        puzzlePiecesCollected = 0;
        Debug.Log("PickupManager progress reset");
    }

    public void RegisterPickup(PickupItem item)
    {
        if (item != null && !pickupItems.Contains(item))
            pickupItems.Add(item);
    }

    public void CollectItem(PickupItem item)
    {
        if (item == null || item.isCollected) return;

        // Execute item pickup
        item.OnPickup();

        // Track the item ID
        if (!collectedItemIDs.Contains(item.itemID))
            collectedItemIDs.Add(item.itemID);

        Debug.Log($"✅ Collected: {item.itemID} ({collectedItemIDs.Count} total items)");

        // If this item is a PuzzlePiece
        if (item.CompareTag("PuzzlePiece"))
        {
            puzzlePiecesCollected++;
            Debug.Log($"🧩 Puzzle pieces collected: {puzzlePiecesCollected}/5");

            // Only show the letter if we're in SampleScene and have all 5
            if (puzzlePiecesCollected >= 5 && SceneManager.GetActiveScene().name == "SampleScene")
            {
                Debug.Log("🧩 All 5 puzzle pieces collected in SampleScene!");
                if (LetterUIController.Instance != null)
                {
                    LetterUIController.Instance.ShowLetterUI();
                }
                else
                {
                    Debug.LogWarning("⚠️ LetterUIController not found in SampleScene! Make sure LetterUI GameObject exists.");
                }
            }
            else if (puzzlePiecesCollected >= 5)
            {
                Debug.Log("🧩 All 5 puzzle pieces collected! Letter will show when entering SampleScene.");
            }
        }
    }

    public bool IsCollected(string itemID)
    {
        return collectedItemIDs.Contains(itemID);
    }

    // Helper method to check puzzle piece count
    public int GetPuzzlePieceCount()
    {
        return puzzlePiecesCollected;
    }
}