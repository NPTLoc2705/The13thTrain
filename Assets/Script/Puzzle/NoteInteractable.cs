using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NoteInteractable : MonoBehaviour
{
    [Header("Note Content")]
    [TextArea(3, 8)]
    public string noteText = "Note text goes here...";

    [Header("UI Settings")]
    public GameObject riddleUIPrefab; // assign RiddleUI prefab in inspector

    [Header("Interaction Settings")]
    public bool openOnce = false; // if true, can only open once
    public string interactPrompt = "[E] Đọc mảnh giấy";

    [Header("Journal Settings (Optional)")]
    public bool addToJournalOnRead = false;
    public string journalEntryID = "note_riddle_01";
    public string journalTitle = "A Torn Note";

    // Runtime variables
    private GameObject uiInstance;
    private RiddleUIController uiController;
    private bool hasBeenRead = false;
    private bool playerNearby = false;
    private bool isUIOpen = false;

    void Start()
    {
        // Ensure collider is set as trigger
        Collider c = GetComponent<Collider>();
        if (c == null)
        {
            Debug.LogError($"NoteInteractable on {gameObject.name} requires a Collider component!");
            return;
        }

        // Make sure it's a trigger for OnTriggerEnter/Exit to work
        c.isTrigger = true;
    }

    void Update()
    {
        // Only allow interaction when player is nearby and UI is not already open
        if (playerNearby && !isUIOpen && Input.GetKeyDown(KeyCode.E))
        {
            TryOpen();
        }
    }

    private void TryOpen()
    {
        // If set to open once and already read, don't open again
        if (openOnce && hasBeenRead)
        {
            if (TextManager.Instance != null)
            {
                TextManager.Instance.ShowNotice("Bạn đã đọc mảnh giấy này rồi.", 2f);
            }
            return;
        }

        OpenNote();
    }

    private void OpenNote()
    {
        // Instantiate UI if it doesn't exist
        if (uiInstance == null && riddleUIPrefab != null)
        {
            uiInstance = Instantiate(riddleUIPrefab);
            uiController = uiInstance.GetComponent<RiddleUIController>();

            if (uiController == null)
            {
                Debug.LogError("RiddleUIPrefab is missing RiddleUIController component!");
                Destroy(uiInstance);
                return;
            }
        }

        if (uiController != null)
        {
            isUIOpen = true;

            // Lock player movement
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null)
            {
                pc.SetMovementLocked(true);
            }

            // Hide the interaction prompt
            if (TextManager.Instance != null)
            {
                TextManager.Instance.HidePrompt();
            }

            // Show cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Show UI with callback for when it closes
            uiController.Show(noteText, () =>
            {
                // On close callback
                isUIOpen = false;

                // Unlock player movement
                PlayerController pcc = FindObjectOfType<PlayerController>();
                if (pcc != null)
                {
                    pcc.SetMovementLocked(false);
                }

                // Lock cursor again
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Add to journal if enabled (only if you have JournalManager in your project)
                if (addToJournalOnRead && !hasBeenRead)
                {
                    // TODO: Implement journal system if needed
                    Debug.Log($"[Note] Would add to journal: {journalTitle}");
                }

                // Show prompt again if player is still nearby and can read again
                if (playerNearby && !(openOnce && hasBeenRead))
                {
                    if (TextManager.Instance != null)
                    {
                        TextManager.Instance.ShowPrompt(interactPrompt);
                    }
                }
            });

            hasBeenRead = true;
        }
        else
        {
            Debug.LogWarning($"No RiddleUI assigned to {gameObject.name} or missing controller.");
        }
    }

    // Trigger-based interaction detection
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            playerNearby = true;

            // Show prompt if not already read (or if can read multiple times)
            if (!isUIOpen && !(openOnce && hasBeenRead))
            {
                if (TextManager.Instance != null)
                {
                    TextManager.Instance.ShowPrompt(interactPrompt);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            playerNearby = false;

            // Hide prompt when player leaves
            if (TextManager.Instance != null)
            {
                TextManager.Instance.HidePrompt();
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up UI instance when note is destroyed
        if (uiInstance != null)
        {
            Destroy(uiInstance);
        }
    }
}