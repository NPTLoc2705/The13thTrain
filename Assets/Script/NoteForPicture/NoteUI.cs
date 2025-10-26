using UnityEngine;
using TMPro;

public class NoteUI : MonoBehaviour
{
    [Header("Page Content")]
    [TextArea(3, 10)]
    public string[] pages;

    [Header("UI References")]
    public TextMeshProUGUI noteText;
    public CanvasGroup canvasGroup;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pageSound;

    private int currentPage = 0;
    private bool isOpen = false;

    void Awake()
    {
        // Ẩn Canvas ngay khi khởi tạo (trước khi AudioSource kịp phát)
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Start()
    {
        if (audioSource != null)
            audioSource.ignoreListenerPause = true;
    }

    void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.A))
            PrevPage();
        if (Input.GetKeyDown(KeyCode.D))
            NextPage();
        if (Input.GetKeyDown(KeyCode.Escape))
            CloseNote();
    }

    public void OpenNote()
    {
        if (isOpen) return;

        isOpen = true;
        currentPage = 0;
        UpdatePage();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        PlayPageSound();
    }

    public void CloseNote()
    {
        isOpen = false;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;

        // ⚠️ KHÔNG phát âm thanh khi chỉ mới khởi động
        if (Time.timeSinceLevelLoad > 0.5f)
            PlayPageSound();
    }

    void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            UpdatePage();
            PlayPageSound();
        }
    }

    void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
            PlayPageSound();
        }
    }

    void UpdatePage()
    {
        noteText.text = pages[currentPage];
    }

    void PlayPageSound()
    {
        if (audioSource != null && pageSound != null)
            audioSource.PlayOneShot(pageSound);
    }

    public bool IsOpen() => isOpen;
}
