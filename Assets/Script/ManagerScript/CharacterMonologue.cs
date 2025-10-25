using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CharacterMonologue : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI monologueText;
    [SerializeField] private GameObject monologuePanel;

    [Header("Player Reference")]
    [SerializeField] private GameObject playerObject; // Kéo thả GameObject của Player vào đây

    [Header("Monologue Settings")]
    [SerializeField]
    private string[] thoughts = new string[]
    {
        "Nơi này... trông có vẻ quen thuộc...",
        "Tôi cần phải tìm ra sự thật...",
        "Hành trình này sẽ không dễ dàng..."
    };

    [SerializeField] private float timeBetweenThoughts = 2.5f;
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private bool useTypewriterEffect = true;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private MonoBehaviour playerController;
    private bool playerWasEnabled;

    void Start()
    {
        // Tìm player controller (hỗ trợ cả 2 loại)
        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = playerObject.GetComponent<PlayerController_LN_SmoothMove>();
            }
        }

        // Setup Canvas Group cho fade effect
        if (monologuePanel != null && monologuePanel.GetComponent<CanvasGroup>() == null)
        {
            canvasGroup = monologuePanel.AddComponent<CanvasGroup>();
        }
        else if (monologuePanel != null)
        {
            canvasGroup = monologuePanel.GetComponent<CanvasGroup>();
        }

        // Bắt đầu hiển thị monologue
        StartCoroutine(ShowMonologue());
    }

    IEnumerator ShowMonologue()
    {
        // Đợi một chút trước khi bắt đầu
        yield return new WaitForSeconds(0.5f);

        // Khóa di chuyển của player
        DisablePlayerMovement();

        if (monologuePanel != null)
        {
            monologuePanel.SetActive(true);
        }

        foreach (string thought in thoughts)
        {
            // Fade in
            if (canvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeInDuration));
            }

            // Hiển thị text
            if (useTypewriterEffect)
            {
                yield return StartCoroutine(TypeText(thought));
            }
            else
            {
                monologueText.text = thought;
                yield return new WaitForSeconds(timeBetweenThoughts);
            }

            // Đợi trước khi fade out
            yield return new WaitForSeconds(1f);

            // Fade out
            if (canvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeOutDuration));
            }

            // Clear text
            monologueText.text = "";

            // Đợi giữa các dòng text
            yield return new WaitForSeconds(0.3f);
        }

        // Ẩn panel sau khi hoàn thành
        if (monologuePanel != null)
        {
            monologuePanel.SetActive(false);
        }

        // Mở khóa di chuyển của player
        EnablePlayerMovement();
    }

    IEnumerator TypeText(string text)
    {
        monologueText.text = "";

        foreach (char c in text)
        {
            monologueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        yield return new WaitForSeconds(timeBetweenThoughts - text.Length * typeSpeed);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        cg.alpha = endAlpha;
    }

    void DisablePlayerMovement()
    {
        if (playerController != null)
        {
            playerWasEnabled = playerController.enabled;
            playerController.enabled = false;
        }

        // Khóa cursor nếu cần
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void EnablePlayerMovement()
    {
        if (playerController != null)
        {
            playerController.enabled = playerWasEnabled;
        }

        // Khóa lại cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}