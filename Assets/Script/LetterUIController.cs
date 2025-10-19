using UnityEngine;
using TMPro;

public class LetterUIController : MonoBehaviour
{
    public PickupManager pickupManager; // Drag the PickupManager GameObject here
    public Transform modelViewerTransform; // Drag ModelViewer GameObject here
    public GameObject fullLetterPrefab; // Drag FullPiece prefab here
    public TextMeshProUGUI letterText; // Drag LetterText TMP Text component here
    private GameObject instantiatedLetter;

    // Static reference to access this instance
    public static LetterUIController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keeps it across scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameObject.SetActive(false); // Hide by default
    }

    public void ShowLetterUI()
    {
        if (pickupManager.collectedItemIDs.Count == 5) // Assuming 5 pieces
        {
            gameObject.SetActive(true);
            instantiatedLetter = Instantiate(fullLetterPrefab, modelViewerTransform.position, Quaternion.identity, modelViewerTransform);
            instantiatedLetter.layer = LayerMask.NameToLayer("LetterModel");
            instantiatedLetter.SetActive(true);

            if (letterText != null)
            {
                letterText.text = "My dear son,\r\nIf you’re reading this, it means I couldn’t tell you myself. I’m sorry for leaving you alone so soon. Inside the safe lies something meant to remind you that love never fades — not even when I’m gone.\r\nThe code is 18082… use it wisely, and remember, I’m always with you.";
                Debug.Log("Text assigned to letterText: " + letterText.text);
            }
            else
            {
                Debug.LogError("letterText is null! Check the Inspector assignment.");
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("LetterUI shown with text and prefab instantiated.");
        }
    }

    public void CloseUI()
    {
        if (instantiatedLetter != null)
        {
            Destroy(instantiatedLetter);
        }
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Show "Find the safe" notice via TextManager
        TextManager.Instance.ShowNotice("Find the safe", 3f);
        Debug.Log("LetterUI closed, showing 'Find the safe' notice.");
    }
}