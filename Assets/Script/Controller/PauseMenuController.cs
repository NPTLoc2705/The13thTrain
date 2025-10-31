using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuCanvas; // Drag PauseMenuCanvas here
    public Button resumeButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Gameplay References")]
    public PlayerController playerController; // Drag Player GameObject with PlayerController here

    private bool isPaused = false;
    private bool canPause = true; // Prevent pausing during cutscenes

    void Start()
    {
        // Auto-find PlayerController if not assigned
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
                Debug.Log("PlayerController found automatically");
            }
            else
            {
                Debug.LogWarning("Player not found! Please assign PlayerController manually.");
            }
        }

        // Ensure pause menu is hidden initially
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
            Debug.Log("Pause menu initialized and hidden");
        }
        else
        {
            Debug.LogError("PauseMenuCanvas is not assigned!");
        }

        // Add listeners to buttons
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
            Debug.Log("Resume button listener added");
        }
        else
        {
            Debug.LogWarning("Resume button not assigned!");
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(Settings);
            Debug.Log("Settings button listener added");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitToMainMenu);
            Debug.Log("Quit button listener added");
        }
        else
        {
            Debug.LogWarning("Quit button not assigned!");
        }

        // Ensure cursor is locked at start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ✅ CRITICAL: Check for interactions BEFORE processing ESC key
        // This ensures we check the state BEFORE any interaction's Update() closes it
        if (Input.GetKeyDown(KeyCode.Escape) && canPause)
        {
            // Check if any interactive UI is currently open
            if (IsAnyInteractionActive())
            {
                Debug.Log("⚠️ ESC pressed but interaction is open - letting interaction handle it");
                return; // Don't process pause - let the interaction's Update() handle ESC
            }

            // No interaction active - toggle pause menu
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    /// <summary>
    /// ✅ Check if player is currently interacting with any UI element
    /// </summary>
    private bool IsAnyInteractionActive()
    {
        // Check if Note UI is open (Paper with pages)
        NoteUI noteUI = FindObjectOfType<NoteUI>();
        if (noteUI != null)
        {
            bool isNoteOpen = noteUI.IsOpen();
            if (isNoteOpen)
            {
                Debug.Log("📖 NoteUI is open - ESC will close it");
                return true;
            }
        }
        else
        {
            Debug.Log("[PauseMenuController] NoteUI not found in scene");
        }

        // Check if Riddle UI is open (Riddle notes)
        RiddleUIController riddleUI = FindObjectOfType<RiddleUIController>();
        if (riddleUI != null && riddleUI.IsOpen())
        {
            Debug.Log("📄 RiddleUI is open - ESC will close it");
            return true;
        }

        // Check if Safe password UI is open
        SafeController safeController = FindObjectOfType<SafeController>();
        if (safeController != null && safeController.IsPasswordUIOpen())
        {
            Debug.Log("🔐 Safe UI is open - ESC will close it");
            return true;
        }

        // Check if Radio is tuning
        RadioPuzzle activeRadio = FindObjectOfType<RadioPuzzle>();
        if (activeRadio != null && activeRadio.IsTuning)
        {
            Debug.Log("📻 Radio is tuning - ESC will close it");
            return true;
        }

        // Check if Character Monologue is active
        if (CharacterMonologue.Instance != null && CharacterMonologue.Instance.IsActive())
        {
            Debug.Log("💭 Monologue is active");
            return true;
        }

        // Check if Letter UI is open
        if (LetterUIController.Instance != null && LetterUIController.Instance.IsOpen())
        {
            Debug.Log("✉️ Letter UI is open - ESC will close it");
            return true;
        }

        // Check if Mystery Box train display is open
        MysteryBoxController mysteryBox = FindObjectOfType<MysteryBoxController>();
        if (mysteryBox != null && mysteryBox.IsTrainDisplayOpen())
        {
            Debug.Log("🎁 Mystery Box display is open - ESC will close it");
            return true;
        }

        // Check if any InspectableObject is being inspected
        InspectableObject[] inspectables = FindObjectsOfType<InspectableObject>();
        foreach (var inspectable in inspectables)
        {
            if (inspectable.isInspecting)
            {
                Debug.Log("🔍 Object is being inspected - ESC will close it");
                return true;
            }
        }

        // Check if any NoteInteractable is open
        NoteInteractable[] noteInteractables = FindObjectsOfType<NoteInteractable>();
        foreach (var noteInteractable in noteInteractables)
        {
            if (noteInteractable.IsOpen())
            {
                Debug.Log("📝 NoteInteractable is open - ESC will close it");
                return true;
            }
        }

        // No interaction is active
        return false;
    }

    public void PauseGame()
    {
        if (isPaused || pauseMenuCanvas == null) return;

        Debug.Log("⏸️ Game Paused");
        isPaused = true;
        Time.timeScale = 0f; // Pause the game

        // Disable player movement
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Show pause menu
        pauseMenuCanvas.SetActive(true);

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (!isPaused || pauseMenuCanvas == null) return;

        Debug.Log("▶️ Game Resumed");
        isPaused = false;
        Time.timeScale = 1f; // Resume the game

        // Re-enable player movement
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Hide pause menu
        pauseMenuCanvas.SetActive(false);

        // Lock cursor again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Settings()
    {
        Debug.Log("Settings button clicked");
        // Add settings UI or logic here later
        // For now, just log a message
    }

    public void QuitToMainMenu()
    {
        Debug.Log("Quitting to Main Menu");
        Time.timeScale = 1f; // Reset time scale before loading new scene

        // Reset cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Optional: Reset PickupManager progress when returning to menu
        if (PickupManager.Instance != null)
        {
            PickupManager.Instance.ResetProgress();
        }

        SceneManager.LoadScene("MainMenu"); // Load the main menu scene
    }

    // Call this to disable pausing (e.g., during cutscenes)
    public void SetCanPause(bool canPause)
    {
        this.canPause = canPause;
    }

    void OnDestroy()
    {
        // Reset time scale in case this is destroyed while paused
        Time.timeScale = 1f;

        // Clean up listeners
        if (resumeButton != null) resumeButton.onClick.RemoveListener(ResumeGame);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(Settings);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitToMainMenu);
    }
}