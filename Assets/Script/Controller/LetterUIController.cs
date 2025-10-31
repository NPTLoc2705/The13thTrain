using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LetterUIController : MonoBehaviour
{
    public PickupManager pickupManager;
    public Transform modelViewerTransform;

    [Header("Prefab Settings")]
    [Tooltip("Either assign the prefab here OR place it in Resources/Prefabs/FullLetter")]
    public GameObject fullLetterPrefab;
    public string prefabResourcePath = "Prefabs/FullLetter";

    public TextMeshProUGUI letterText;

    private GameObject instantiatedLetter;
    private Canvas canvas;
    private bool isClosing = false; // ✅ Track closing state to prevent pause menu

    // Static reference to access this instance
    public static LetterUIController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Cache canvas reference
        canvas = GetComponent<Canvas>();

        // Subscribe to scene loaded event to refresh references
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // Called whenever a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Refresh references from the new scene
        RefreshReferences();
    }

    void Start()
    {
        // Initial setup
        RefreshReferences();
        HideUI();
    }

    void Update()
    {
        // ✅ Handle ESC key with proper timing to prevent pause menu
        if (IsOpen() && Input.GetKeyDown(KeyCode.Escape) && !isClosing)
        {
            Debug.Log("[LetterUI] ESC pressed - starting close sequence");
            isClosing = true;
            StartCoroutine(CloseUIDelayed());
        }
    }

    private void RefreshReferences()
    {
        // Re-cache canvas if needed
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }

        // Find PickupManager if not assigned or destroyed
        if (pickupManager == null)
        {
            pickupManager = PickupManager.Instance;
            if (pickupManager != null)
            {
                Debug.Log("[LetterUI] PickupManager reference refreshed");
            }
        }

        // Find ModelViewer in the current scene
        if (modelViewerTransform == null)
        {
            GameObject modelViewer = GameObject.Find("ModelViewer");
            if (modelViewer != null)
            {
                modelViewerTransform = modelViewer.transform;
            }
        }

        // Find LetterText in the current scene
        if (letterText == null)
        {
            letterText = GetComponentInChildren<TextMeshProUGUI>();
            if (letterText != null)
            {
                Debug.Log("[LetterUI] LetterText reference refreshed");
            }
        }
    }

    private void HideUI()
    {
        // Hide UI without deactivating GameObject (to keep references)
        if (canvas != null)
        {
            canvas.enabled = false;
        }
        else
        {
            // Fallback: hide all child renderers
            CanvasRenderer[] renderers = GetComponentsInChildren<CanvasRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// ✅ Check if the Letter UI is currently open (includes closing state for pause menu)
    /// </summary>
    public bool IsOpen()
    {
        // Return true if open OR in the process of closing
        // This ensures PauseMenuController sees it as "active" during the close frame
        bool canvasOpen = canvas != null && canvas.enabled;
        return canvasOpen || isClosing;
    }

    public void ShowLetterUI()
    {
        // Refresh references in case they're still null
        RefreshReferences();

        if (pickupManager == null)
        {
            Debug.LogError("[LetterUI] PickupManager is null!");
            return;
        }

        if (pickupManager.collectedItemIDs.Count == 5) // Assuming 5 pieces
        {
            Debug.Log("[LetterUI] ========== OPENING LETTER UI ==========");
            isClosing = false;

            // Show the UI
            if (canvas != null)
            {
                canvas.enabled = true;
            }
            else
            {
                // Fallback: show all children
                CanvasRenderer[] renderers = GetComponentsInChildren<CanvasRenderer>(true);
                foreach (var renderer in renderers)
                {
                    renderer.gameObject.SetActive(true);
                }
            }

            // Last attempt to find ModelViewer if still null
            if (modelViewerTransform == null)
            {
                GameObject modelViewer = GameObject.Find("ModelViewer");
                if (modelViewer != null)
                {
                    modelViewerTransform = modelViewer.transform;
                }
                else
                {
                    GameObject tempParent = new GameObject("TempModelViewer");
                    modelViewerTransform = tempParent.transform;
                    modelViewerTransform.position = Vector3.zero;
                }
            }

            if (fullLetterPrefab == null)
            {
                Debug.LogError("[LetterUI] FullLetterPrefab is not assigned!");
                return;
            }

            // Instantiate the full letter
            instantiatedLetter = Instantiate(fullLetterPrefab, modelViewerTransform.position, Quaternion.identity, modelViewerTransform);

            int layerIndex = LayerMask.NameToLayer("LetterModel");
            if (layerIndex != -1)
            {
                instantiatedLetter.layer = layerIndex;
            }
            instantiatedLetter.SetActive(true);

            // Set the letter text
            if (letterText != null)
            {
                letterText.text = "Con trai yêu của mẹ,\r\n" +
                    "Nếu con đọc được tờ giấy này, nghĩa là mẹ đã không thể tự mình nói với con được nữa.\r\n" +
                    "Mẹ xin lỗi vì đã phải rời xa con quá sớm.\r\n" +
                    "Bên trong két sắt có một món đồ — thứ sẽ giúp con tìm ra một vật đặc biệt, và cũng là bằng chứng rằng tình yêu không bao giờ phai nhạt… ngay cả khi mẹ đã không còn ở bên con.\r\n" +
                    "Mật mã là 18082.\r\n" +
                    "Hãy sử dụng nó thật khôn ngoan, và nhớ rằng… dù ở bất cứ đâu, mẹ vẫn luôn ở bên con";
            }

            // ✅ Lock player movement
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null)
            {
                pc.SetMovementLocked(true);
                Debug.Log("[LetterUI] Player movement LOCKED");
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// ✅ Delayed close to prevent pause menu from opening
    /// </summary>
    private IEnumerator CloseUIDelayed()
    {
        // Wait until end of frame so PauseMenuController can check IsOpen() first
        yield return new WaitForEndOfFrame();

        CloseUI();
        isClosing = false;
    }

    public void CloseUI()
    {
        Debug.Log("[LetterUI] ========== CLOSING LETTER UI ==========");

        if (instantiatedLetter != null)
        {
            Destroy(instantiatedLetter);
        }

        // Hide UI
        if (canvas != null)
        {
            canvas.enabled = false;
        }
        else
        {
            CanvasRenderer[] renderers = GetComponentsInChildren<CanvasRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.gameObject.SetActive(false);
            }
        }

        // ✅ CRITICAL: Stop any pending monologue to prevent wrong monologue from playing
        if (CharacterMonologue.Instance != null)
        {
            // Force stop any queued monologue that might have been triggered
            StopAllCoroutines(); // Stop our own coroutines
            Debug.Log("[LetterUI] Stopped any pending monologues");
        }

        // ✅ Unlock player movement
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            pc.SetMovementLocked(false);
            Debug.Log("[LetterUI] Player movement UNLOCKED");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Show "Find the safe" notice via TextManager
        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowNotice("Hãy tìm chiếc két sắt", 3f);
        }
    }
}