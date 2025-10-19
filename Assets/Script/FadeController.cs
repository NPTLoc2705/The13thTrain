using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("Thời gian fade (giây)")]
    public float fadeDuration = 1.5f;

    [Tooltip("Màu fade (thường là đen)")]
    public Color fadeColor = Color.black;

    [Header("References")]
    public Image fadeImage;

    // Singleton
    public static FadeController Instance;

    void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✓ FadeController created and set to DontDestroyOnLoad");
        }
        else if (Instance != this)
        {
            Debug.LogWarning("⚠ Duplicate FadeController found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        // Setup fade image
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }
        else
        {
            Debug.LogError("❌ Fade Image not assigned! Please assign the Image component in Inspector.");
        }

        // Subscribe to scene loaded event to fade in
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Fade in when new scene loads
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Fade to black, then load scene
    /// </summary>
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    /// <summary>
    /// Fade out (to black)
    /// </summary>
    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        if (fadeImage == null)
        {
            Debug.LogError("❌ Cannot fade: fadeImage is null!");
            yield break;
        }

        Debug.Log($"🎬 Fading out... Duration: {fadeDuration}s");

        float elapsed = 0f;
        Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        // Fade to black
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        fadeImage.color = endColor;

        Debug.Log($"✓ Loading scene: {sceneName}");

        // Load scene
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Fade in (from black to transparent)
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        Debug.Log($"🎬 Fading in... Duration: {fadeDuration}s");

        float elapsed = 0f;
        Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        // Fade from black to transparent
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        fadeImage.color = endColor;
        Debug.Log("✓ Fade in complete");
    }
}