using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RiddleUIController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI riddleText;
    public Button closeButton;
    public Button hintButton; // optional
    public GameObject panel; // Main panel to show/hide
    public RawImage backgroundImage; // Optional: for displaying 3D note render

    [Header("3D Background (Optional)")]
    public Note3DBackground note3DBackground; // Optional: for 3D note rendering

    [Header("Settings")]
    public float defaultAutoCloseSeconds = 0f; // 0 = don't auto close
    public string hintText = "[Gợi ý] Hãy nhìn vào những phòng nơi mẹ đã ở nhiều nhất.";

    [Header("After Close Monologue")]
    public bool playMonologueAfterClose = false;
    public string afterCloseMonologue = "Lần đầu?, căn phòng? mẹ đang muốn ám chỉ điều gì cho mình sao";

    private System.Action onCloseCallback;
    private bool hintShown = false;

    void Awake()
    {
        // Setup button listeners
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (hintButton != null)
        {
            hintButton.onClick.AddListener(OnHintClicked);
        }

        // Start with UI hidden
        if (panel != null)
        {
            panel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Allow ESC key to close the UI
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Show(string text, System.Action onClose = null)
    {
        // Show the 3D background if available
        if (note3DBackground != null)
        {
            note3DBackground.Show();
        }

        // Show the UI
        if (panel != null)
        {
            panel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        // Set the text
        if (riddleText != null)
        {
            riddleText.text = text;
        }

        // Store callback
        onCloseCallback = onClose;
        hintShown = false;

        // Auto-close after delay if set
        if (defaultAutoCloseSeconds > 0f)
        {
            Invoke(nameof(Close), defaultAutoCloseSeconds);
        }

    }

    public void Close()
    {
        // Cancel any pending auto-close
        CancelInvoke();

        // Hide the 3D background if available
        if (note3DBackground != null)
        {
            note3DBackground.Hide();
        }

        // Hide the UI
        if (panel != null)
        {
            panel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        // Invoke the callback
        onCloseCallback?.Invoke();

       

    }
    public bool IsOpen()
    {
        if (panel != null)
            return panel.activeSelf;
        else
            return gameObject.activeSelf;
    }
    private void OnHintClicked()
    {
        if (riddleText != null && !hintShown)
        {
            // Append hint to the existing text
            riddleText.text += "\n\n" + hintText;
            hintShown = true;

        }
        
    }
}