using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LetterUIController : MonoBehaviour
{
    public PickupManager pickupManager; // Drag the PickupManager GameObject here
    public Transform modelViewerTransform; // Drag ModelViewer GameObject here

    [Header("Prefab Settings")]
    [Tooltip("Either assign the prefab here OR place it in Resources/Prefabs/FullLetter")]
    public GameObject fullLetterPrefab; // Drag FullPiece prefab here
    public string prefabResourcePath = "Prefabs/FullLetter"; // Path in Resources folder

    public TextMeshProUGUI letterText; // Drag LetterText TMP Text component here

    private GameObject instantiatedLetter;

    // Static reference to access this instance
    public static LetterUIController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("LetterUIController created and persisted");
        }
        else if (Instance != this)
        {
            Debug.LogWarning(" Duplicate LetterUIController found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

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
        Debug.Log($" Scene '{scene.name}' loaded. Refreshing LetterUIController references...");

        // Refresh references from the new scene
        RefreshReferences();
    }

    void Start()
    {
        // Initial setup
        RefreshReferences();
        HideUI();
    }

    private void RefreshReferences()
    {
        // Find PickupManager if not assigned or destroyed
        if (pickupManager == null)
        {
            pickupManager = PickupManager.Instance;
            if (pickupManager != null)
            {
                Debug.Log(" PickupManager reference refreshed");
            }
        }

        // Find ModelViewer in the current scene
        if (modelViewerTransform == null)
        {
            GameObject modelViewer = GameObject.Find("ModelViewer");
            if (modelViewer != null)
            {
                modelViewerTransform = modelViewer.transform;
                Debug.Log(" ModelViewerTransform reference refreshed");
            }
            else
            {
                Debug.LogWarning(" ModelViewer not found in scene. Will be searched again when needed.");
            }
        }

        // Find LetterText in the current scene (it's likely a child of this GameObject)
        if (letterText == null)
        {
            letterText = GetComponentInChildren<TextMeshProUGUI>();
            if (letterText != null)
            {
                Debug.Log(" LetterText reference refreshed");
            }
        }
    }

    private void HideUI()
    {
        // Hide UI without deactivating GameObject (to keep references)
        Canvas canvas = GetComponent<Canvas>();
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

    public void ShowLetterUI()
    {
        // Refresh references in case they're still null
        RefreshReferences();

        if (pickupManager == null)
        {
            Debug.LogError("PickupManager is not assigned!");
            return;
        }

        if (pickupManager.collectedItemIDs.Count == 5) // Assuming 5 pieces
        {
            // Show the UI
            Canvas canvas = GetComponent<Canvas>();
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
                    Debug.Log("Found ModelViewer at last attempt");
                }
                else
                {
                    Debug.LogError(" ModelViewerTransform is still null! Creating temporary parent...");
                    GameObject tempParent = new GameObject("TempModelViewer");
                    modelViewerTransform = tempParent.transform;
                    modelViewerTransform.position = Vector3.zero;
                }
            }

            if (fullLetterPrefab == null)
            {
                Debug.LogError(" FullLetterPrefab is not assigned!");
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
                letterText.text = "My dear son,\r\nIf you're reading this, it means I couldn't tell you myself. I'm sorry for leaving you alone so soon. Inside the safe lies something meant to remind you that love never fades — not even when I'm gone.\r\nThe code is 18082… use it wisely, and remember, I'm always with you.";
                Debug.Log(" Text assigned to letterText");
            }
            else
            {
                Debug.LogError(" letterText is null! Check the Inspector assignment.");
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("LetterUI shown with all 5 pieces collected!");
        }
        else
        {
            Debug.Log($"Not all pieces collected yet. Current: {pickupManager.collectedItemIDs.Count}/5");
        }
    }

    public void CloseUI()
    {
        if (instantiatedLetter != null)
        {
            Destroy(instantiatedLetter);
        }

        // Hide UI
        Canvas canvas = GetComponent<Canvas>();
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Show "Find the safe" notice via TextManager
        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowNotice("Find the safe", 3f);
            Debug.Log(" Showing 'Find the safe' notice");
        }
        else
        {
            Debug.LogWarning(" TextManager.Instance is null!");
        }

        Debug.Log("LetterUI closed.");
    }
}