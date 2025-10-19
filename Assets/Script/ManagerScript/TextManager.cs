using UnityEngine;
using TMPro;
using System.Collections;

public class TextManager : MonoBehaviour
{
    public static TextManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI promptText; 
    public TextMeshProUGUI noticeText; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (promptText != null)
        {
            promptText.text = "";
            promptText.gameObject.SetActive(false);
        }
        if (noticeText != null)
        {
            noticeText.text = "";
            noticeText.gameObject.SetActive(false);
        }
    }

   
    public void ShowPrompt(string message)
    {
        if (promptText != null)
        {
            promptText.text = message;
            promptText.gameObject.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.text = "";
            promptText.gameObject.SetActive(false);
        }
    }

    public void ShowNotice(string message, float duration = 3f)
    {
        if (noticeText != null)
        {
            StartCoroutine(DisplayNotice(message, duration));
        }
    }

    private IEnumerator DisplayNotice(string message, float duration)
    {
        noticeText.text = message;
        noticeText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        noticeText.text = "";
        noticeText.gameObject.SetActive(false);
    }
}