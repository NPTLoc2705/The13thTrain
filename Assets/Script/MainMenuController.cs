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
    public float hoverHeight = 0.1f;      // độ cao khi hover
    public float floatSpeed = 2f;         // tốc độ dao động lên xuống

    [Header("Fade Settings")]
    public float fadeDuration = 1.2f;
    public CanvasGroup fadeCanvas;        // màn hình đen fade-in/out

    private TextMeshPro currentHover;
    private Vector3 basePos;
    private Color normalColor = new Color(0.8f, 0.8f, 0.8f);
    private Color hoverColor = new Color(1f, 0.889f, 0.318f);

    private bool isFading = true;

    void Start()
    {
        // Ẩn chữ ban đầu (alpha = 0)
        SetAlpha(playButton, 0f);
        SetAlpha(settingsButton, 0f);
        SetAlpha(quitButton, 0f);

        // Nếu có CanvasGroup (màn hình đen), set alpha 1 (đen)
        if (fadeCanvas != null)
            fadeCanvas.alpha = 1f;

        // Bắt đầu fade-in
        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / fadeDuration);

            // Hiện chữ dần
            SetAlpha(playButton, t);
            SetAlpha(settingsButton, t);
            SetAlpha(quitButton, t);

            // Làm sáng dần màn hình
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
        if (isFading) return;

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
            // đưa về vị trí gốc nếu không hover
            currentHover.transform.position = basePos;
            currentHover = null;
        }
    }

    void HandleHoverFloat()
    {
        if (currentHover == null) return;

        // Hiệu ứng “nhấp nhô” nhẹ bằng sóng sin
        float offsetY = Mathf.Sin(Time.time * floatSpeed) * hoverHeight;
        currentHover.transform.position = basePos + Vector3.up * offsetY;
    }

    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0) && currentHover != null)
        {
            switch (currentHover.name)
            {
                case "PlayButton":
                    StartCoroutine(FadeOutAndLoad("GameScene"));
                    break;
                case "SettingsButton":
                    Debug.Log("Open Settings...");
                    break;
                case "QuitButton":
                    Application.Quit();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                    break;
            }
        }
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        if (fadeCanvas == null)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / fadeDuration);
            fadeCanvas.alpha = t;
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
