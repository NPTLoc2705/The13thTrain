using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneSceneController : MonoBehaviour
{
    [Header("UI References")]
    public Image blackFade;
    public Image storyImage;
    public TextMeshProUGUI storyText;
    public TextMeshProUGUI finalText;
    public TextMeshProUGUI skipText;

    [Header("Audio Sources")]
    public AudioSource bgmAudio;
    public AudioSource breathingAudio;
    public AudioSource crashAudio;
    public AudioSource faintAudio; // âm thanh cậu bé ngất

    [Header("Âm lượng (Volume Settings)")]
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float breathingVolume = 0.8f;
    [Range(0f, 1f)] public float crashVolume = 1f;
    [Range(0f, 1f)] public float faintVolume = 1f;

    [Header("Cài đặt chuyển cảnh")]
    public float delayBeforeImage = 1f;
    public float imageFadeSpeed = 0.5f;
    public float textSpeed = 0.03f;
    public string nextSceneName = "NextScene"; // tên scene kế tiếp

    [TextArea(10, 20)]
    public string storyContent =
        "Cậu bé (độc thoại):\n" +
        "“...Là gì thế này?...”\n\n" +
        "“Tại sao... mình lại thấy bức tranh này?”\n\n" +
        "“Người trong tranh... cậu bé ấy... sao lại... giống mình đến thế?”\n\n" +
        "(Một khoảng lặng... hơi thở nặng nề...)\n\n" +
        "“Mình... không hiểu... chuyện gì đang xảy ra...”\n\n" +
        "“Hai người này... là ai?... Sao mình lại có cảm giác... quen thuộc đến lạ...”\n\n" +
        "“Là ai... mà tim mình lại... đau như thế này...”\n\n" +
        "(Cậu bé khẽ chạm vào bức tranh... đôi tay run nhẹ...)\n\n" +
        "“Người phụ nữ này... hình như... rất quan trọng với mình...”\n\n" +
        "“Nhưng... mình chẳng nhớ được gì cả...”\n\n" +
        "(Khoảnh khắc im lặng... chỉ còn tiếng tim đập chậm rãi trong bóng tối...)\n\n" +
        "...\n";

    private bool canContinue = false;

    void Start()
    {
        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        blackFade.color = new Color(0, 0, 0, 1);
        storyImage.color = new Color(1, 1, 1, 0);
        finalText.color = new Color(1, 1, 1, 0);
        storyText.text = "";
        skipText.gameObject.SetActive(false);

        // Bật nhạc nền
        if (bgmAudio)
        {
            bgmAudio.loop = true;
            bgmAudio.volume = bgmVolume;
            bgmAudio.Play();
        }

        yield return new WaitForSeconds(delayBeforeImage);

        // Ảnh xuất hiện dần
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * imageFadeSpeed;
            storyImage.color = new Color(1, 1, 1, Mathf.Clamp01(alpha));
            yield return null;
        }

        // Hiển thị lời thoại
        canContinue = false;
        yield return StartCoroutine(TypeTextWithBreathing(storyContent));

        canContinue = true;
        skipText.gameObject.SetActive(true);
        skipText.text = "Bấm [Space] để tiếp tục...";

        // Chờ nhấn space
        yield return new WaitUntil(() => canContinue && Input.GetKeyDown(KeyCode.Space));
        skipText.gameObject.SetActive(false);

        // Tắt tiếng thở dần
        if (breathingAudio && breathingAudio.isPlaying)
            StartCoroutine(FadeOutAudio(breathingAudio, 1f));

        storyText.text = "";

        // Rung nhẹ
        yield return StartCoroutine(ScreenShake(0.3f, 0.1f));

        // Rơi mạnh + tiếng vỡ
        yield return StartCoroutine(FallDownInstant());
        if (crashAudio)
        {
            crashAudio.volume = crashVolume;
            crashAudio.Play();
        }

        // Hiện chữ cuối
        yield return StartCoroutine(ShowFinalTextDramatic("Hãy đi tìm... món quà..."));

        yield return new WaitForSeconds(3f);

        // Dừng crash audio nếu đang phát
        if (crashAudio && crashAudio.isPlaying)
            crashAudio.Stop();

        // Chạy âm thanh ngất
        if (faintAudio)
        {
            faintAudio.volume = faintVolume;
            faintAudio.Play();
        }

        // Hiện đoạn kết
        yield return StartCoroutine(ShowFinalTextDramatic("Cậu bé cuối cùng cũng ngất đi vì quá mệt..."));

        // Làm tối dần nền
        yield return StartCoroutine(FadeToBlack(3f));

        // Biến mất chữ
        yield return StartCoroutine(FadeOutAllTexts(2f));

        // Chuyển scene
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator TypeTextWithBreathing(string content)
    {
        storyText.text = "";
        string breathingTrigger = "(Một khoảng lặng... hơi thở nặng nề...)";
        string buffer = "";

        for (int i = 0; i < content.Length; i++)
        {
            buffer += content[i];
            storyText.text = buffer;

            if (buffer.Contains(breathingTrigger) && breathingAudio && !breathingAudio.isPlaying)
            {
                breathingAudio.loop = true;
                breathingAudio.volume = breathingVolume;
                breathingAudio.Play();
                Debug.Log("▶ Tiếng thở gấp phát ra");
            }

            yield return new WaitForSeconds(textSpeed);
        }
    }

    IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        source.Stop();
        source.volume = startVolume;
    }

    IEnumerator ScreenShake(float duration, float magnitude)
    {
        Vector3 originalPos = storyImage.rectTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude * 50f;
            float y = Random.Range(-1f, 1f) * magnitude * 50f;
            storyImage.rectTransform.localPosition = new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        storyImage.rectTransform.localPosition = originalPos;
    }

    IEnumerator FallDownInstant()
    {
        RectTransform img = storyImage.rectTransform;
        Vector3 startPos = img.localPosition;
        Vector3 targetPos = startPos - new Vector3(0, 1200, 0);

        yield return StartCoroutine(ScreenShake(0.1f, 0.25f));
        img.localPosition = targetPos;
    }

    IEnumerator ShowFinalTextDramatic(string text)
    {
        finalText.text = text;
        finalText.fontSize *= 1.05f;
        float alpha = 0f;

        while (alpha < 1.2f)
        {
            alpha += Time.deltaTime * 1.2f;
            float glow = Mathf.PingPong(Time.time * 2f, 0.4f);
            finalText.color = new Color(1f, 1f - glow / 3f, 1f - glow / 3f, Mathf.Clamp01(alpha));
            yield return null;
        }

        Vector3 originalPos = finalText.rectTransform.localPosition;
        float elapsed = 0f;
        while (elapsed < 0.8f)
        {
            float x = Random.Range(-1f, 1f) * 2f;
            float y = Random.Range(-1f, 1f) * 2f;
            finalText.rectTransform.localPosition = new Vector3(x, y, 0) + originalPos;
            elapsed += Time.deltaTime;
            yield return null;
        }
        finalText.rectTransform.localPosition = originalPos;
    }

    IEnumerator FadeToBlack(float duration)
    {
        float elapsed = 0f;
        Color startColor = blackFade.color;
        while (elapsed < duration)
        {
            blackFade.color = new Color(0, 0, 0, Mathf.Lerp(startColor.a, 1f, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        blackFade.color = new Color(0, 0, 0, 1f);
    }

    IEnumerator FadeOutAllTexts(float duration)
    {
        float elapsed = 0f;
        Color storyStart = storyText.color;
        Color finalStart = finalText.color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            storyText.color = new Color(storyStart.r, storyStart.g, storyStart.b, 1f - t);
            finalText.color = new Color(finalStart.r, finalStart.g, finalStart.b, 1f - t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        storyText.color = new Color(storyStart.r, storyStart.g, storyStart.b, 0f);
        finalText.color = new Color(finalStart.r, finalStart.g, finalStart.b, 0f);
    }
}
