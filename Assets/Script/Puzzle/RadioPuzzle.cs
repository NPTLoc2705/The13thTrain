using UnityEngine;
using System.Collections;

public class RadioPuzzle : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource staticSound;
    [SerializeField] private AudioSource clearStationSound;
    [SerializeField] private float staticMaxVolume = 1f;

    [Header("Tuning Settings")]
    [SerializeField] private float targetFrequency = 155.0f;
    [SerializeField] private float startingFrequency = 88f;
    [SerializeField] private float minFrequency = 88f;
    [SerializeField] private float maxFrequency = 300f;
    [SerializeField] private float tuneSpeed = 10f;
    [SerializeField] private float tolerance = 0.5f;
    [SerializeField] private float driftAmount = 0.3f;
    [SerializeField] private float holdTime = 2.5f;

    [Header("Audio Feedback Settings")]
    [SerializeField] private float maxHearingDistance = 20f;
    [SerializeField] private float optimalRange = 10f;

    [Header("Puzzle Reward")]
    [SerializeField] private PickupItem pieceItem;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3f;

    [Header("UI References")]
    [SerializeField] private Canvas frequencyCanvas;
    [SerializeField] private Transform dialTransform;
    [SerializeField] private TMPro.TextMeshProUGUI frequencyText;
    [SerializeField] private UnityEngine.UI.Button leftButton;
    [SerializeField] private UnityEngine.UI.Button rightButton;
    [SerializeField] private UnityEngine.UI.Image leftHighlight;
    [SerializeField] private UnityEngine.UI.Image rightHighlight;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController;

    private float currentFrequency;
    private bool isTuning = false;
    private bool isSolved = false;
    private bool radioActivated = false;
    private bool isFrequencyLocked = false;
    private bool isWaitingForAudio = false;
    private bool hasPlayedAudio = false;

    private float timeInCorrectRange = 0f;
    private bool isInCorrectRange = false;
    private bool hasShownHoldPrompt = false;

    void Start()
    {
        currentFrequency = startingFrequency;

        if (staticSound != null)
        {
            staticSound.loop = true;
            staticSound.Stop();
        }

        if (clearStationSound != null)
        {
            clearStationSound.loop = false;
            clearStationSound.Stop();
        }

        if (pieceItem != null) pieceItem.gameObject.SetActive(false);
        if (frequencyCanvas != null) frequencyCanvas.gameObject.SetActive(false);

        if (PickupManager.Instance != null && pieceItem != null &&
            PickupManager.Instance.IsCollected(pieceItem.itemID))
        {
            isSolved = true;
            if (staticSound != null) staticSound.Stop();
            gameObject.SetActive(false);
        }

        if (leftButton != null) leftButton.onClick.AddListener(TuneLeft);
        if (rightButton != null) rightButton.onClick.AddListener(TuneRight);
    }

    void Update()
    {
        if (isSolved) return;

        if (Input.GetKeyDown(KeyCode.E) && !radioActivated && !isTuning)
        {
            if (IsPlayerLookingAtRadio())
            {
                ActivateRadio();
            }
        }

        if (radioActivated && !isTuning)
        {
            if (Input.GetKeyDown(KeyCode.E) && IsPlayerLookingAtRadio())
            {
                EnterTuningMode();
            }
        }

        if (isTuning && !isFrequencyLocked) HandleTuning();
    }

    private void ActivateRadio()
    {
        radioActivated = true;

        if (staticSound != null)
        {
            staticSound.Play();
            staticSound.volume = staticMaxVolume;
        }

        Invoke(nameof(EnterTuningMode), 0.3f);
    }

    private void EnterTuningMode()
    {
        isTuning = true;
        Debug.Log("[RadioPuzzle] ========== ENTERING TUNING MODE ==========");

        if (playerController != null)
        {
            playerController.SetMovementLocked(true);
            Debug.Log("[RadioPuzzle] Player movement LOCKED");
        }

        if (frequencyCanvas != null) frequencyCanvas.gameObject.SetActive(true);

        UpdateFrequencyUI();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowPrompt("[A/D] để điều chỉnh | [ESC] Thoát\nLắng nghe kỹ tiếng tĩnh điện!");
        }

        Debug.Log($"[RadioPuzzle] Tuning mode active. isTuning = {isTuning}");
    }

    private void HandleTuning()
    {
        bool isTuningInput = false;

        if (Input.GetKey(KeyCode.A))
        {
            TuneFrequency(-tuneSpeed * Time.deltaTime);
            isTuningInput = true;
            timeInCorrectRange = 0f;
            hasShownHoldPrompt = false;
        }
        if (Input.GetKey(KeyCode.D))
        {
            TuneFrequency(tuneSpeed * Time.deltaTime);
            isTuningInput = true;
            timeInCorrectRange = 0f;
            hasShownHoldPrompt = false;
        }

        if (!isTuningInput)
        {
            currentFrequency += Random.Range(-driftAmount, driftAmount) * Time.deltaTime;
        }

        currentFrequency = Mathf.Clamp(currentFrequency, minFrequency, maxFrequency);

        UpdateStaticVolume();
        UpdateFrequencyUI();
        UpdateDialVisual();
        UpdateButtonHighlights();

        float distance = Mathf.Abs(targetFrequency - currentFrequency);

        if (distance <= tolerance && !isTuningInput)
        {
            if (!isInCorrectRange)
            {
                isInCorrectRange = true;
                timeInCorrectRange = 0f;
            }

            timeInCorrectRange += Time.deltaTime;

            if (timeInCorrectRange > 0.5f && !hasShownHoldPrompt)
            {
                hasShownHoldPrompt = true;
                if (TextManager.Instance != null)
                {
                    TextManager.Instance.ShowPrompt($"✓ Giữ nguyên! ({Mathf.CeilToInt(holdTime - timeInCorrectRange)}s)");
                }
            }

            if (hasShownHoldPrompt && TextManager.Instance != null)
            {
                float remaining = holdTime - timeInCorrectRange;
                if (remaining > 0)
                {
                    TextManager.Instance.ShowPrompt($"✓ Giữ nguyên! ({Mathf.CeilToInt(remaining)}s)");
                }
            }

            if (timeInCorrectRange >= holdTime && !isFrequencyLocked)
            {
                LockFrequency();
            }
        }
        else
        {
            if (isInCorrectRange)
            {
                isInCorrectRange = false;
                timeInCorrectRange = 0f;
                hasShownHoldPrompt = false;

                if (TextManager.Instance != null)
                {
                    TextManager.Instance.ShowPrompt("[A/D] để điều chỉnh | [ESC] Thoát\nLắng nghe kỹ tiếng tĩnh điện!");
                }
            }
        }

        // ✅ CRITICAL: Exit tuning with ESC (only if not waiting for audio)
        if (Input.GetKeyDown(KeyCode.Escape) && !isWaitingForAudio)
        {
            Debug.Log("[RadioPuzzle] ESC pressed - exiting tuning mode");
            ExitTuningMode();
            return; // Exit immediately to prevent PauseMenuController from processing ESC
        }
    }

    private void TuneFrequency(float delta)
    {
        currentFrequency += delta;
    }

    public void TuneLeft()
    {
        if (isTuning && !isFrequencyLocked) TuneFrequency(-tuneSpeed * Time.deltaTime);
    }

    public void TuneRight()
    {
        if (isTuning && !isFrequencyLocked) TuneFrequency(tuneSpeed * Time.deltaTime);
    }

    private void UpdateStaticVolume()
    {
        if (staticSound == null) return;

        float distance = Mathf.Abs(targetFrequency - currentFrequency);
        float normalizedDistance;

        if (distance <= optimalRange)
        {
            normalizedDistance = Mathf.Pow(distance / optimalRange, 2f);
        }
        else if (distance <= maxHearingDistance)
        {
            float t = (distance - optimalRange) / (maxHearingDistance - optimalRange);
            normalizedDistance = Mathf.Lerp(1f, 0.3f, 1f - t);
        }
        else
        {
            normalizedDistance = 1f;
        }

        staticSound.volume = Mathf.Clamp01(normalizedDistance) * staticMaxVolume;

        if (distance <= tolerance * 3f)
        {
            staticSound.pitch = Mathf.Lerp(1.2f, 1.0f, distance / (tolerance * 3f));
        }
        else
        {
            staticSound.pitch = 1.0f;
        }
    }

    private void LockFrequency()
    {
        isFrequencyLocked = true;
        isWaitingForAudio = true;

        if (staticSound != null) staticSound.Stop();

        if (clearStationSound != null && !hasPlayedAudio)
        {
            hasPlayedAudio = true;
            clearStationSound.volume = 0f;
            clearStationSound.Play();
            StartCoroutine(FadeInAudio(clearStationSound, 1.5f));
        }

        if (frequencyText != null)
        {
            frequencyText.text = $"FM: {currentFrequency:F1} MHz ✓";
            frequencyText.color = Color.green;
        }

        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowPrompt("Tần số chính xác! Hãy lắng nghe đoạn ghi âm...");
        }

        if (clearStationSound != null)
        {
            float audioLength = clearStationSound.clip.length;
            StartCoroutine(WaitForAudioAndAutoCollect(audioLength));
        }
        else
        {
            StartCoroutine(WaitForAudioAndAutoCollect(3f));
        }
    }

    private IEnumerator WaitForAudioAndAutoCollect(float duration)
    {
        yield return new WaitForSeconds(duration);
        AutoCollectPieceAndFinish();
    }

    private void AutoCollectPieceAndFinish()
    {
        if (isSolved) return;

        isSolved = true;
        isTuning = false;
        isWaitingForAudio = false;

        if (playerController != null)
        {
            playerController.SetMovementLocked(false);
        }

        if (pieceItem != null && PickupManager.Instance != null)
        {
            PickupManager.Instance.CollectItem(pieceItem);

            if (pieceItem.pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pieceItem.pickupSound, Camera.main.transform.position, pieceItem.soundVolume);
            }

            if (TextManager.Instance != null)
            {
                TextManager.Instance.ShowNotice("Bạn đã nhận được 1 mảnh giấy!", 3f);
            }

            if (PickupManager.Instance.collectedItemIDs.Count == 5)
            {
                if (LetterUIController.Instance != null)
                {
                    LetterUIController.Instance.ShowLetterUI();
                }
            }

            Debug.Log($"[RadioPuzzle] Auto-collected: {pieceItem.itemID} ({PickupManager.Instance.collectedItemIDs.Count}/5)");
        }

        StartCoroutine(HideUIAfterDelay(2f));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ExitTuningMode()
    {
        isTuning = false;
        radioActivated = false; // ✅ Reset radio activation

        // ✅ STOP ALL AUDIO when exiting
        if (staticSound != null && staticSound.isPlaying)
        {
            staticSound.Stop();
            Debug.Log("[RadioPuzzle] Static sound STOPPED");
        }

        if (clearStationSound != null && clearStationSound.isPlaying)
        {
            clearStationSound.Stop();
            Debug.Log("[RadioPuzzle] Clear station sound STOPPED");
        }

        if (playerController != null)
        {
            playerController.SetMovementLocked(false);
            Debug.Log("[RadioPuzzle] Player movement UNLOCKED");
        }
        var pauseMenu = FindObjectOfType<PauseMenuController>();
        if (pauseMenu != null)
        {
            pauseMenu.NotifyInteractionClosed();
        }
        if (frequencyCanvas != null) frequencyCanvas.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (TextManager.Instance != null) TextManager.Instance.HidePrompt();

        Debug.Log($"[RadioPuzzle] Tuning mode exited. isTuning = {isTuning}");
    }

    private bool IsPlayerLookingAtRadio()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            return hit.collider.gameObject == this.gameObject || hit.collider.transform.IsChildOf(this.transform);
        }
        return false;
    }

    private IEnumerator HideUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (frequencyCanvas != null) frequencyCanvas.gameObject.SetActive(false);
    }

    private IEnumerator FadeInAudio(AudioSource audio, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audio.volume = Mathf.Lerp(0f, 1f, timer / duration);
            yield return null;
        }
        audio.volume = 1f;
    }

    private void UpdateFrequencyUI()
    {
        if (frequencyText != null)
        {
            if (isFrequencyLocked)
            {
                frequencyText.text = $"FM: {currentFrequency:F1} MHz ✓";
                frequencyText.color = Color.green;
            }
            else
            {
                frequencyText.text = $"FM: {currentFrequency:F1} MHz";
                frequencyText.color = Color.white;
            }
        }
    }

    private void UpdateDialVisual()
    {
        if (dialTransform != null)
        {
            float normalizedFreq = (currentFrequency - minFrequency) / (maxFrequency - minFrequency);
            float angle = Mathf.Lerp(-90f, 90f, normalizedFreq);
            dialTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void UpdateButtonHighlights()
    {
        if (isFrequencyLocked)
        {
            if (leftHighlight != null) leftHighlight.color = normalColor;
            if (rightHighlight != null) rightHighlight.color = normalColor;
        }
        else
        {
            if (leftHighlight != null) leftHighlight.color = Input.GetKey(KeyCode.A) ? highlightColor : normalColor;
            if (rightHighlight != null) rightHighlight.color = Input.GetKey(KeyCode.D) ? highlightColor : normalColor;
        }
    }

    public string GetPromptMessage()
    {
        if (!radioActivated) return "[E] Bật Radio lên";
        else if (!isTuning) return "[E] Chỉnh âm Radio";
        return "";
    }

    public bool IsTuning => isTuning;
}