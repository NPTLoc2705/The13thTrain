using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Button References (TextMeshPro Objects)")]
    public TextMeshPro playButton;
    public TextMeshPro settingsButton;
    public TextMeshPro quitButton;

    [Header("Hover Settings")]
    public float hoverHeight = 0.1f;
    public float floatSpeed = 2f;

    [Header("Click Animation")]
    public float clickScaleUp = 1.2f;        // tỉ lệ phóng to khi click
    public float clickScaleSpeed = 6f;       // tốc độ phóng to
    public float clickHoldTime = 0.15f;      // giữ phóng to trước khi fade-out

    [Header("Fade Settings")]
    public float fadeDuration = 1.2f;
    public CanvasGroup fadeCanvas;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioSource bgmSource; // nhạc nền menu (fade-out khi play)

    private TextMeshPro currentHover;
    private Vector3 basePos;
    private Vector3 targetScale = Vector3.one;
    private Color normalColor = new Color(0.8f, 0.8f, 0.8f);
    private Color hoverColor = new Color(1f, 0.889f, 0.318f);
    private bool isFading = true;
    private bool isClicking = false;

    void Start()
    {
        SetAlpha(playButton, 0f);
        SetAlpha(settingsButton, 0f);
        SetAlpha(quitButton, 0f);

        if (fadeCanvas != null)
            fadeCanvas.alpha = 1f;

        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / fadeDuration);

            SetAlpha(playButton, t);
            SetAlpha(settingsButton, t);
            SetAlpha(quitButton, t);

            if (fadeCanvas != null)
                fadeCanvas.alpha = 1f - t;

            yield return null;
        }

        if (fadeCanvas != null)
            fadeCanvas.alpha = 0f;

        isFading = false;
    }

    void Update()
    {
        if (isFading || isClicking) return;

        HandleMouseHover();
        HandleMouseClick();
        HandleHoverFloat();
    }

    void HandleMouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            TextMeshPro hovered = hit.transform.GetComponent<TextMeshPro>();
            if (hovered != null && hovered != currentHover)
            {
                // phát âm thanh hover
                if (audioSource && hoverSound)
                    audioSource.PlayOneShot(hoverSound);

                // reset nút cũ
                if (currentHover != null)
                    currentHover.color = normalColor;

                currentHover = hovered;
                basePos = currentHover.transform.position;
                currentHover.color = hoverColor;
            }
            return;
        }

        if (currentHover != null)
        {
            currentHover.color = normalColor;
            currentHover.transform.position = basePos;
            currentHover = null;
        }
    }

    void HandleHoverFloat()
    {
        if (currentHover == null) return;

        float offsetY = Mathf.Sin(Time.time * floatSpeed) * hoverHeight;
        currentHover.transform.position = basePos + Vector3.up * offsetY;
    }

    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0) && currentHover != null)
        {
            if (audioSource && clickSound)
                audioSource.PlayOneShot(clickSound);

            StartCoroutine(ClickAnimation(currentHover));
        }
    }

    IEnumerator ClickAnimation(TextMeshPro clickedButton)
    {
        isClicking = true;
        Vector3 originalScale = clickedButton.transform.localScale;
        Vector3 targetScale = originalScale * clickScaleUp;

        // hiệu ứng phóng to
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * clickScaleSpeed;
            clickedButton.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // giữ 1 chút
        yield return new WaitForSeconds(clickHoldTime);

        // thu nhỏ lại
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * clickScaleSpeed;
            clickedButton.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        // chuyển scene hoặc hành động tương ứng
        switch (clickedButton.name)
        {
            case "PlayButton":
                StartCoroutine(FadeOutAndLoad("Ohlala")); // Changed from "Scene3" to "Ohlala"
                break;
            case "SettingsButton":
                Debug.Log("Settings button clicked, but functionality is not implemented yet.");
                isClicking = false;
                break;
            case "QuitButton":
                Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        float timer = 0f;
        float startVolume = (bgmSource != null) ? bgmSource.volume : 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / fadeDuration);

            if (fadeCanvas != null)
                fadeCanvas.alpha = t;

            if (bgmSource != null)
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }

    void SetAlpha(TextMeshPro tmp, float a)
    {
        if (tmp == null) return;
        Color c = tmp.color;
        c.a = a;
        tmp.color = c;
    }
}