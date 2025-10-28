using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerTutorialUI : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup tutorialCanvas;
    [Tooltip("Dùng TextMeshProUGUI (UI) hoặc TextMeshPro (3D) đều được")]
    public TMP_Text tutorialText; // hỗ trợ cả 2 loại

    [Header("Timing")]
    public float showDelay = 2f;
    public float fadeDuration = 1f;
    public float autoHideAfter = 6f;

    private bool isShown = false;

    void Start()
    {
        if (tutorialCanvas != null)
        {
            tutorialCanvas.alpha = 0f;
        }
    }

    public void ShowTutorial()
    {
        if (isShown || tutorialCanvas == null)
            return;

        isShown = true;
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        yield return new WaitForSeconds(showDelay);

        tutorialCanvas.gameObject.SetActive(true);

        float timer = 0f;

        // Fade-in
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / fadeDuration);
            tutorialCanvas.alpha = t;
            yield return null;
        }

        tutorialCanvas.alpha = 1f;

        yield return new WaitForSeconds(autoHideAfter);

        // Fade-out
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(1f, 0f, timer / fadeDuration);
            tutorialCanvas.alpha = t;
            yield return null;
        }

        tutorialCanvas.alpha = 0f;
    }
}
