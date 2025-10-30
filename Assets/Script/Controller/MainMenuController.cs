using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Game Title")]
    public TextMeshPro gameTitle;

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

    [Header("Character & Camera Transition")]
    public Transform playerCharacter;      // nhân vật chính
    public Transform cameraFollowTarget;   // vị trí mà camera sẽ nhìn/đặt (thường là 1 empty object đặt trước mặt nhân vật)
    public float cameraMoveDuration = 2f;  // thời gian phóng tới
    public float cameraFollowDistance = 3f; // khoảng cách giữ với nhân vật sau khi gắn theo
    public float cameraFollowHeight = 1.5f; // độ cao camera
    public float followSmoothSpeed = 3f;    // độ mượt khi follow

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
                //StartCoroutine(FadeOutAndLoad("Ohlala")); // Changed from "Scene3" to "Ohlala"
                StartCoroutine(FadeOutAllText());
                FindObjectOfType<TrainMover>()?.PlayCinematicStopAtStation();
                FindObjectOfType<CameraTransition3D_Menu>()?.StartTransition();
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

    IEnumerator FadeOutAllText()
    {
        float duration = 1.2f; // thời gian mờ dần
        float timer = 0f;

        // ✅ Thêm GameTitle vào danh sách
        TextMeshPro[] allTexts = { gameTitle, playButton, settingsButton, quitButton };

        // Lưu lại màu ban đầu
        Color[] startColors = new Color[allTexts.Length];
        for (int i = 0; i < allTexts.Length; i++)
            if (allTexts[i] != null)
                startColors[i] = allTexts[i].color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);

            for (int i = 0; i < allTexts.Length; i++)
            {
                if (allTexts[i] != null)
                {
                    Color c = startColors[i];
                    c.a = Mathf.Lerp(1f, 0f, t);
                    allTexts[i].color = c;
                }
            }

            yield return null;
        }

        // Sau khi fade xong → ẩn hoàn toàn
        for (int i = 0; i < allTexts.Length; i++)
        {
            if (allTexts[i] != null)
                allTexts[i].gameObject.SetActive(false);
        }
    }
}