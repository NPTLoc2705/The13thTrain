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
            Debug.Log(" Quit button listener added");
        }
        else
        {
            Debug.LogWarning(" Quit button not assigned!");
        }

        // Ensure cursor is locked at start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Toggle pause menu with Escape key
        if (Input.GetKeyDown(KeyCode.Escape) && canPause)
        {
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

    public void PauseGame()
    {
        if (isPaused || pauseMenuCanvas == null) return;

        Debug.Log(" Game Paused");
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

        Debug.Log(" Game Resumed");
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