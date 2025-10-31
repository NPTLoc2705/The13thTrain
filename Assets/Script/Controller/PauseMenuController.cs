using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuCanvas;
    public Button resumeButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Gameplay References")]
    public PlayerController playerController;

    private bool isPaused = false;
    private bool canPause = true;
    private bool recentlyClosedInteraction = false; // ✅ NEW: prevent instant ESC reopen

    void Start()
    {
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

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
            Debug.Log("Pause menu initialized and hidden");
        }
        else
        {
            Debug.LogError("PauseMenuCanvas is not assigned!");
        }

        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(Settings);
        if (quitButton != null) quitButton.onClick.AddListener(QuitToMainMenu);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && canPause)
        {
            // ✅ Skip ESC if just closed an interaction or if one is still open
            if (IsAnyInteractionActive() || recentlyClosedInteraction)
            {
                Debug.Log("⚠️ ESC pressed but interaction is active or just closed - skipping pause");
                return;
            }

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

    // ✅ NEW: Called by other scripts when an interaction (Radio, Safe, etc.) just closed
    public void NotifyInteractionClosed()
    {
        StartCoroutine(ResetRecentlyClosedFlag());
    }

    private IEnumerator ResetRecentlyClosedFlag()
    {
        recentlyClosedInteraction = true;
        yield return null; // wait 1 frame
        recentlyClosedInteraction = false;
    }

    private bool IsAnyInteractionActive()
    {
        NoteUI noteUI = FindObjectOfType<NoteUI>();
        if (noteUI != null && noteUI.IsOpen()) return true;

        RiddleUIController riddleUI = FindObjectOfType<RiddleUIController>();
        if (riddleUI != null && riddleUI.IsOpen()) return true;

        SafeController safeController = FindObjectOfType<SafeController>();
        if (safeController != null && safeController.IsPasswordUIOpen()) return true;

        RadioPuzzle activeRadio = FindObjectOfType<RadioPuzzle>();
        if (activeRadio != null && activeRadio.IsTuning) return true;

        if (CharacterMonologue.Instance != null && CharacterMonologue.Instance.IsActive()) return true;
        if (LetterUIController.Instance != null && LetterUIController.Instance.IsOpen()) return true;

        MysteryBoxController mysteryBox = FindObjectOfType<MysteryBoxController>();
        if (mysteryBox != null && mysteryBox.IsTrainDisplayOpen()) return true;

        InspectableObject[] inspectables = FindObjectsOfType<InspectableObject>();
        foreach (var obj in inspectables)
            if (obj.isInspecting) return true;

        NoteInteractable[] noteInteractables = FindObjectsOfType<NoteInteractable>();
        foreach (var n in noteInteractables)
            if (n.IsOpen()) return true;

        return false;
    }

    public void PauseGame()
    {
        if (isPaused || pauseMenuCanvas == null) return;

        Debug.Log("⏸️ Game Paused");
        isPaused = true;
        Time.timeScale = 0f;

        if (playerController != null) playerController.enabled = false;

        pauseMenuCanvas.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (!isPaused || pauseMenuCanvas == null) return;

        Debug.Log("▶️ Game Resumed");
        isPaused = false;
        Time.timeScale = 1f;

        if (playerController != null) playerController.enabled = true;

        pauseMenuCanvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Settings()
    {
        Debug.Log("Settings button clicked");
    }

    public void QuitToMainMenu()
    {
        Debug.Log("Quitting to Main Menu");
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (PickupManager.Instance != null)
        {
            PickupManager.Instance.ResetProgress();
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void SetCanPause(bool canPause)
    {
        this.canPause = canPause;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;

        if (resumeButton != null) resumeButton.onClick.RemoveListener(ResumeGame);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(Settings);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitToMainMenu);
    }
}