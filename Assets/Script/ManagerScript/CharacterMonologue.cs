using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class CharacterMonologue : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI monologueText;
    [SerializeField] private GameObject monologuePanel;

    [Header("Player Reference")]
    [SerializeField] private GameObject playerObject;

    [Header("Monologue Settings - Default (Ohlala Scene)")]
    [SerializeField]
    private string[] defaultStartingThoughts = new string[]
    {
        "Nơi này... trông có vẻ quen thuộc...",
        "Tôi cần phải tìm ra sự thật...",
        "Hành trình này sẽ không dễ dàng..."
    };

    [Header("Monologue Settings - SampleScene")]
    [SerializeField]
    private string[] sampleSceneThoughts = new string[]
    {
        "Lại là ác mộng à, hình như có gì đó khác trong ngôi nhà này, mình phải đi xem thử mới được"
    };

    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float timeBetweenThoughts = 2.5f;
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private bool useTypewriterEffect = true;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private PlayerController playerController;
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
        // Find player controller
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("[CharacterMonologue] PlayerController not found!");
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

        // Hide panel initially
        if (monologuePanel != null)
        {
            monologuePanel.SetActive(false);
        }

        // Auto-play starting monologue based on scene
        if (playOnStart)
        {
            StartCoroutine(PlayStartingMonologue());
        }
    }

    /// <summary>
    /// PUBLIC METHOD: Check if monologue is currently active (for PlayerController)
    /// </summary>
    public bool IsActive()
    {
        return isPlaying;
    }

    /// <summary>
    /// Play starting monologue when entering scene - auto-select text based on scene
    /// </summary>
    IEnumerator PlayStartingMonologue()
    {
        // Wait a bit before starting
        yield return new WaitForSeconds(0.5f);

        // Get current scene name
        string currentScene = SceneManager.GetActiveScene().name;
        string[] thoughtsToPlay;

        // Select monologue based on scene
        if (currentScene == "SampleScene")
        {
            thoughtsToPlay = sampleSceneThoughts;
            Debug.Log("🎬 Playing SampleScene monologue");
        }
        else // Default is Ohlala scene or any other scene
        {
            thoughtsToPlay = defaultStartingThoughts;
            Debug.Log("🎬 Playing default (Ohlala) monologue");
        }

        // Only play if there are thoughts
        if (thoughtsToPlay.Length > 0)
        {
            yield return StartCoroutine(PlayMonologue(thoughtsToPlay, null));
        }
    }

    /// <summary>
    /// Show monologue with one or more sentences, then call callback
    /// </summary>
    public void ShowMonologueWithCallback(string[] thoughts, Action onComplete = null)
    {
        if (isPlaying)
        {
            Debug.LogWarning("[CharacterMonologue] Already playing a monologue!");
            return;
        }

        StartCoroutine(PlayMonologue(thoughts, onComplete));
    }

    /// <summary>
    /// Show monologue with a single sentence
    /// </summary>
    public void ShowMonologueWithCallback(string thought, Action onComplete = null)
    {
        ShowMonologueWithCallback(new string[] { thought }, onComplete);
    }

    /// <summary>
    /// Show monologue with a single sentence (no callback)
    /// </summary>
    public void ShowMonologue(string thought)
    {
        ShowMonologueWithCallback(new string[] { thought }, null);
    }

    /// <summary>
    /// Show monologue with multiple sentences (no callback)
    /// </summary>
    public void ShowMonologue(string[] thoughts)
    {
        ShowMonologueWithCallback(thoughts, null);
    }

    /// <summary>
    /// Show monologue for wrong candle order
    /// </summary>
    public void ShowCandleWrongOrderMonologue()
    {
        ShowMonologue("Có vẻ không đúng, chắc phải có ghi chú gì đó quanh đây");
    }

    /// <summary>
    /// Show monologue when candle puzzle is completed
    /// </summary>
    public void ShowCandleCompletedMonologue(Action onComplete = null)
    {
        ShowMonologueWithCallback("Mẹ ơi, sao con không thể nhớ được gương mặt của mẹ nữa vậy?", onComplete);
    }

    IEnumerator PlayMonologue(string[] thoughts, Action onComplete)
    {
        isPlaying = true;
        Debug.Log("[CharacterMonologue] Starting monologue playback");

        // Lock player movement
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

            // Display text
            if (useTypewriterEffect)
            {
                yield return StartCoroutine(TypeText(thought));
            }
            else
            {
                monologueText.text = thought;
                yield return new WaitForSeconds(timeBetweenThoughts);
            }

            // Wait before fade out
            yield return new WaitForSeconds(1f);

            // Fade out
            if (canvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeOutDuration));
            }

            // Clear text
            monologueText.text = "";

            // Wait between lines
            yield return new WaitForSeconds(0.3f);
        }

        // Hide panel after completion
        if (monologuePanel != null)
        {
            monologuePanel.SetActive(false);
        }

        // CRITICAL: Unlock player movement BEFORE calling callback
        EnablePlayerMovement();

        isPlaying = false;
        Debug.Log("[CharacterMonologue] Monologue finished, movement unlocked");

        // Call callback if provided
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
            Debug.Log("[CharacterMonologue] Player movement disabled");
        }

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void EnablePlayerMovement()
    {
        if (playerController != null)
        {
            playerController.enabled = playerWasEnabled;
            Debug.Log("[CharacterMonologue] Player movement enabled");
        }

        // Lock cursor again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}