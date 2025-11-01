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

    [Header("Monologue Settings")]
    public bool playMonologueAfterClose = false;
    public string afterCloseMonologue = "Lần đầu?, căn phòng? mẹ đang muốn ám chỉ điều gì cho mình sao";

    [Header("Journal Settings (Optional)")]
    public bool addToJournalOnRead = false;
    public string journalEntryID = "note_riddle_01";
    public string journalTitle = "A Torn Note";

    // Runtime variables
    private GameObject uiInstance;
    private RiddleUIController uiController;
    private bool hasBeenRead = false;
    private bool isCurrentlyOpen = false; // Track if note is currently being read

    void Start()
    {
        // Ensure collider is set as trigger
        Collider c = GetComponent<Collider>();
        if (c == null)
        {
            return;
        }
        c.isTrigger = true;
    }

    // Public method to check if the note can be interacted with
    public bool CanInteract()
    {
        // Can't interact if already open or if openOnce and already read
        return !isCurrentlyOpen && !(openOnce && hasBeenRead);
    }

    // Public method to get the prompt message
    public string GetPromptMessage()
    {
        return interactPrompt;
    }

    // Public method to open the note
    public void Interact()
    {
        if (isCurrentlyOpen)
        {
            Debug.Log("[NoteInteractable] Note is already open, ignoring interaction");
            return;
        }

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
        Debug.Log("[NoteInteractable] OpenNote() called");

        // Create a fresh UI instance each time (or reuse existing one if it's still valid)
        if (uiInstance == null && riddleUIPrefab != null)
        {
            uiInstance = Instantiate(riddleUIPrefab);
            uiController = uiInstance.GetComponent<RiddleUIController>();

            if (uiController == null)
            {
                Debug.LogError("[NoteInteractable] RiddleUIController component not found!");
                Destroy(uiInstance);
                uiInstance = null;
                return;
            }
        }
        else if (uiInstance != null)
        {
            // If instance exists, make sure we have the controller reference
            if (uiController == null)
            {
                uiController = uiInstance.GetComponent<RiddleUIController>();
            }
        }

        if (uiController != null)
        {
            isCurrentlyOpen = true; // Mark as open

            // Hide the interaction prompt immediately
            if (TextManager.Instance != null)
            {
                TextManager.Instance.HidePrompt();
            }

            // Lock player movement
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null)
            {
                pc.SetMovementLocked(true);
                Debug.Log("[NoteInteractable] Player movement LOCKED");
            }

            // Show UI with callback for when it closes
            uiController.Show(noteText, () =>
            {
                Debug.Log("[NoteInteractable] RiddleUI closed callback triggered");

                // Mark as no longer open
                isCurrentlyOpen = false;

                // FIRST: Unlock player movement BEFORE playing monologue
                PlayerController pcc = FindObjectOfType<PlayerController>();
                if (pcc != null)
                {
                    pcc.SetMovementLocked(false);
                    Debug.Log("[NoteInteractable] Player movement UNLOCKED after riddle close");
                }

                // Lock cursor again
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Play monologue after close (if enabled) - ONLY FIRST TIME
                if (playMonologueAfterClose && !hasBeenRead && !string.IsNullOrEmpty(afterCloseMonologue))
                {
                    if (CharacterMonologue.Instance != null)
                    {
                        Debug.Log("[NoteInteractable] Playing monologue: " + afterCloseMonologue);
                        CharacterMonologue.Instance.ShowMonologue(afterCloseMonologue);
                    }
                }

                // Add to journal if enabled
                if (addToJournalOnRead && !hasBeenRead)
                {
                    Debug.Log($"[Note] Would add to journal: {journalTitle}");
                }

                // Mark as read AFTER all callbacks
                if (!hasBeenRead)
                {
                    hasBeenRead = true;
                }
            });
        }
    }

    /// <summary>
    /// Check if this note UI is currently open
    /// </summary>
    public bool IsOpen()
    {
        return isCurrentlyOpen && uiController != null && uiController.IsOpen();
    }

    private void OnDestroy()
    {
        // Clean up UI instance when note is destroyed
        if (uiInstance != null)
        {
            Destroy(uiInstance);
        }
        isCurrentlyOpen = false;
    }
}