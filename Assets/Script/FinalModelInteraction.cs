using UnityEngine;

public class FinalModelInteraction : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Tên scene sẽ load (phải có trong Build Settings)")]
    public string nextSceneName = "NextLevel";

    private bool isLoading = false;

    void Start()
    {
        // Set tag for this object so PlayerController can detect it
        if (!gameObject.CompareTag("FinalModel"))
        {
            gameObject.tag = "FinalModel";
            Debug.Log("✓ FinalModel tag set automatically");
        }
    }

    // Called by PlayerController when player presses E while looking at this object
    public void Interact()
    {
        if (isLoading) return;

        Debug.Log("✓ Player interacted with Final Model");
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (isLoading) return;

        isLoading = true;
        Debug.Log($"✓ Loading scene: {nextSceneName}");

        // Gọi FadeController để tạo hiệu ứng fade
        if (FadeController.Instance != null)
        {
            FadeController.Instance.FadeToScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("❌ FadeController not found! Loading scene directly...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}