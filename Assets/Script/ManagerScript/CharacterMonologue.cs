using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;

public class CharacterMonologue : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI monologueText;
    [SerializeField] private GameObject monologuePanel;

    [Header("Player Reference")]
    [SerializeField] private GameObject playerObject;

    [Header("Monologue Settings")]
    [SerializeField]
    private string[] startingThoughts = new string[]
    {
        "Nơi này... trông có vẻ quen thuộc...",
        "Tôi cần phải tìm ra sự thật...",
        "Hành trình này sẽ không dễ dàng..."
    };
    [SerializeField] private bool playOnStart = true; // Bật/tắt monologue khi vào scene
    [SerializeField] private float timeBetweenThoughts = 2.5f;
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private bool useTypewriterEffect = true;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private MonoBehaviour playerController;
    private bool playerWasEnabled;
    private bool isPlaying = false;

    // Singleton
    public static CharacterMonologue Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Tìm player controller
        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = playerObject.GetComponent<PlayerController_LN_SmoothMove>();
            }
        }

        // Setup Canvas Group
        if (monologuePanel != null && monologuePanel.GetComponent<CanvasGroup>() == null)
        {
            canvasGroup = monologuePanel.AddComponent<CanvasGroup>();
        }
        else if (monologuePanel != null)
        {
            canvasGroup = monologuePanel.GetComponent<CanvasGroup>();
        }

        // Ẩn panel ban đầu
        if (monologuePanel != null)
        {
            monologuePanel.SetActive(false);
        }

        // Tự động chạy monologue ban đầu nếu được bật
        if (playOnStart && startingThoughts.Length > 0)
        {
            StartCoroutine(PlayStartingMonologue());
        }
    }

    /// <summary>
    /// Chạy monologue ban đầu khi vào scene (không cần callback)
    /// </summary>
    IEnumerator PlayStartingMonologue()
    {
        // Đợi một chút trước khi bắt đầu
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(PlayMonologue(startingThoughts, null));
    }

    /// <summary>
    /// Hiển thị monologue với một hoặc nhiều câu, sau đó gọi callback
    /// </summary>
    public void ShowMonologueWithCallback(string[] thoughts, Action onComplete = null)
    {
        if (isPlaying)
        {
            Debug.LogWarning("Monologue đang chạy, bỏ qua yêu cầu mới");
            return;
        }

        StartCoroutine(PlayMonologue(thoughts, onComplete));
    }

    /// <summary>
    /// Hiển thị monologue với một câu duy nhất
    /// </summary>
    public void ShowMonologueWithCallback(string thought, Action onComplete = null)
    {
        ShowMonologueWithCallback(new string[] { thought }, onComplete);
    }

    IEnumerator PlayMonologue(string[] thoughts, Action onComplete)
    {
        isPlaying = true;

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

        isPlaying = false;

        // Gọi callback nếu có
        onComplete?.Invoke();
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

        // Hiển thị cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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