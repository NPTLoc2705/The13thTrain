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
        return !(openOnce && hasBeenRead);
    }

    // Public method to get the prompt message
    public string GetPromptMessage()
    {
        return interactPrompt;
    }

    // Public method to open the note
    public void Interact()
    {
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
                Destroy(uiInstance);
                return;
            }
        }

        if (uiController != null)
        {
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

                // Play monologue after close (if enabled)
                if (playMonologueAfterClose && !string.IsNullOrEmpty(afterCloseMonologue))
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
            });

            hasBeenRead = true;
        }
    }

    /// <summary>
    /// Check if this note UI is currently open
    /// </summary>
    public bool IsOpen()
    {
        return uiController != null && uiController.IsOpen();
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