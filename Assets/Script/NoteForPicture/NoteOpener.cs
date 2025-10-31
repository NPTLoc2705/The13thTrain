using UnityEngine;

public class NoteOpener : MonoBehaviour
{
    public NoteUI noteUI;

    [Header("Interaction Settings")]
    public string interactPrompt = "[E] Đọc ghi chú";

    [Header("Monologue Settings")]
    public bool showMonologueBeforeOpen = true;
    public string beforeOpenMonologue = "Tại sao lại có mảnh giấy này ở đây... Mình nên đọc nó.";

    // Public method to check if can interact
    public bool CanInteract()
    {
        return noteUI != null && !noteUI.IsOpen();
    }

    // Public method to get prompt
    public string GetPromptMessage()
    {
        return interactPrompt;
    }

    // Public method called by PlayerController
    public void Interact()
    {
        if (noteUI == null) return;

        if (showMonologueBeforeOpen && CharacterMonologue.Instance != null)
        {
            CharacterMonologue.Instance.ShowMonologueWithCallback(
                beforeOpenMonologue,
                () => noteUI.OpenNote()
            );
            Debug.Log("[NoteOpener] Showing monologue before opening note");
        }
        else
        {
            noteUI.OpenNote();
            Debug.Log("[NoteOpener] Opening note directly");
        }
    }
}