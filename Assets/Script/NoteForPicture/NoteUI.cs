using UnityEngine;
using TMPro;

public class NoteUI : MonoBehaviour
{
    [Header("Page Content")]
    [TextArea(3, 10)]
    public string[] pages;

    [Header("UI References")]
    public TextMeshProUGUI noteText;
    public CanvasGroup canvasGroup;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pageSound;

    private int currentPage = 0;
    private bool isOpen = false;

    void Awake()
    {
        // Hide Canvas immediately when created
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    void Start()
    {
        if (audioSource != null)
            audioSource.ignoreListenerPause = true;
    }

    void Update()
    {
        if (!isOpen) return;

        // Navigation keys
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            PrevPage();
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            NextPage();

        // ✅ CRITICAL FIX: Consume ESC input completely
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseNote();

            // CONSUME the input so other scripts don't see it
            Input.ResetInputAxes(); // This helps but isn't perfect for GetKeyDown

            return;
        }
    }

    public void OpenNote()
    {
        if (isOpen)
        {
            Debug.Log("[NoteUI] OpenNote() called but already open!");
            return;
        }

        isOpen = true;
        currentPage = 0;
        UpdatePage();

        // Show UI
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
        }

        // Lock player movement
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            pc.SetMovementLocked(true);
        }
      

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayPageSound();
    }

    public void CloseNote()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;

        // Hide UI
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // Unlock player movement
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            pc.SetMovementLocked(false);
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Play sound (but not when game just started)
        if (Time.timeSinceLevelLoad > 0.5f)
            PlayPageSound();

    }

    void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            UpdatePage();
            PlayPageSound();
        }
    }

    void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
            PlayPageSound();
        }
    }

    void UpdatePage()
    {
        if (noteText != null && pages != null && pages.Length > 0 && currentPage < pages.Length)
        {
            noteText.text = pages[currentPage];
        }
    }

    void PlayPageSound()
    {
        if (audioSource != null && pageSound != null)
            audioSource.PlayOneShot(pageSound);
    }

    /// <summary>
    /// Check if note UI is currently open
    /// </summary>
    public bool IsOpen()
    {
        return isOpen;
    }
}