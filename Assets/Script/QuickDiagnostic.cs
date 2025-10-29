using UnityEngine;

/// <summary>
/// Script chẩn đoán nhanh - Gắn vào bất kỳ GameObject nào để test
/// </summary>
public class QuickDiagnostic : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== QUICK DIAGNOSTIC START ===");

        // Check PaintingViewerUI
        if (PaintingViewerUI.Instance != null)
        {
            Debug.Log("✅ PaintingViewerUI.Instance EXISTS!");
            Debug.Log($"   GameObject: {PaintingViewerUI.Instance.gameObject.name}");
            Debug.Log($"   IsOpen: {PaintingViewerUI.Instance.IsOpen()}");
        }
        else
        {
            Debug.LogError("❌ PaintingViewerUI.Instance is NULL!");
            Debug.LogError("   → Bạn chưa gắn script PaintingViewerUI vào Canvas!");
        }

        // Check PauseMenuController
        PauseMenuController pauseController = FindObjectOfType<PauseMenuController>();
        if (pauseController != null)
        {
            Debug.Log("✅ PauseMenuController found!");
            Debug.Log($"   GameObject: {pauseController.gameObject.name}");
        }
        else
        {
            Debug.LogError("❌ PauseMenuController not found!");
        }

        Debug.Log("=== DIAGNOSTIC END ===");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // Nhấn T để test
        {
            Debug.Log("=== MANUAL TEST (T key) ===");

            if (PaintingViewerUI.Instance != null)
            {
                Debug.Log($"PaintingViewer IsOpen: {PaintingViewerUI.Instance.IsOpen()}");
            }
            else
            {
                Debug.LogError("PaintingViewerUI.Instance is NULL!");
            }
        }
    }
}