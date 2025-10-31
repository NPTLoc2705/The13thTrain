using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandlePuzzleManager : MonoBehaviour
{
    [Header("Candle Setup")]
    [Tooltip("List of Candle objects in the scene. Order doesn't matter, they are identified by candleIndex.")]
    public List<Candle> candles = new List<Candle>();

    [Tooltip("Correct sequence of candleIndex values (e.g., 3,1,4,2,5)")]
    public List<int> correctOrder = new List<int>() { 1, 2, 3, 4, 5 };

    [Header("Puzzle Settings")]
    [Tooltip("Time (s) before wrong sequence resets and extinguishes all candles")]
    public float resetDelay = 0.6f;

    [Header("Narrative Messages (shown each correct step)")]
    [Tooltip("One message per step (position in the sequence). These display when player lights the next correct candle.")]
    [TextArea(2, 6)]
    public string[] motherMessages = new string[5]
    {
        "Con trai à ... Ngày con chào đời, mẹ đã hứa sẽ luôn bảo vệ con. Mẹ vẫn nhớ nụ cười của con – như một ngọn đèn nhỏ soi sáng cuộc đời của mẹ vậy. Con có biết không, con chính là món quà tuyệt vời nhất của mẹ đó ",
        "Con còn nhớ không, khi những lần mẹ tắm cho con? Chúng ta đã cùng nô đùa, tạo hình kiểu tóc, cũng như giọc nước. Lúc ấy tiếng cười của con khiến tim mẹ ấm áp như gió mùa xuân vậy. Những khoảnh khắc ấy mẹ mang theo suốt đời. ",
        " Có những ngày... mẹ thấy quá mệt mỏi. Mẹ lo rằng mình không còn đủ sức để che chở con như trước. Nhưng mẹ vẫn cố, vì con. ",
        "Mẹ... xin lỗi con. Mẹ ước mẹ có thể ở bên con lâu hơn, nhưng mẹ... vì sức khỏe của mẹ đang dần một yếu đi. Mong con đừng oán giận mẹ... ",
        "Dù mẹ không còn bên con... Mẹ vẫn ở đây, trong ánh sáng và ký ức con mang theo. Hãy sống mạnh mẽ nhé, con trai của mẹ. Mẹ luôn yêu con. "
    };

    [Tooltip("Optional voice clips to play with each message (same length as motherMessages). Leave empty if not using voice.")]
    public AudioClip[] motherVoiceClips = new AudioClip[5];

    [Tooltip("How long to show each text message if no voice clip is present")]
    public float messageDisplayTime = 3.0f;

    [Header("Reveal")]
    [Tooltip("The torn piece PickupItem to reveal when solved. Keep it inactive at start.")]
    public PickupItem tornPiece;
    public AudioClip successClip;
    public AudioSource sfxSource;

    // internal
    private List<int> playerSequence = new List<int>();
    private bool solved = false;
    private Coroutine resetCoroutine;

    // message handling
    private Coroutine messageCoroutine = null;
    private AudioSource voiceSource;
    private Queue<MessageData> messageQueue = new Queue<MessageData>(); // ✅ Queue for pending messages
    private bool isPlayingMessage = false; // ✅ Track if currently playing a message

    // Helper struct to store message data
    private struct MessageData
    {
        public string message;
        public AudioClip voiceClip;
        public bool isFinalStep;

        public MessageData(string msg, AudioClip clip, bool final)
        {
            message = msg;
            voiceClip = clip;
            isFinalStep = final;
        }
    }

    void Start()
    {
        // Optional: auto-find candles if the list is empty
        if (candles.Count == 0)
        {
            candles.AddRange(FindObjectsOfType<Candle>());
        }

        // Ensure the torn piece starts disabled
        if (tornPiece != null)
            tornPiece.gameObject.SetActive(false);

        // Ensure sfxSource exists if clips are provided
        if (sfxSource == null && successClip != null)
        {
            sfxSource = gameObject.GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
        }

        // Create a dedicated voiceSource for mother voice clips
        bool hasVoice = false;
        if (motherVoiceClips != null)
        {
            for (int i = 0; i < motherVoiceClips.Length; i++)
            {
                if (motherVoiceClips[i] != null)
                {
                    hasVoice = true;
                    break;
                }
            }
        }

        if (hasVoice)
        {
            voiceSource = gameObject.GetComponent<AudioSource>();
            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.playOnAwake = false;
            }
            voiceSource.spatialBlend = 0f; // 2D voice
            voiceSource.volume = 1f;
            Debug.Log("[CandleManager] Voice source ready");
        }
    }

    /// <summary>
    /// Called by Candle.LightCandle() to register a lit candle index.
    /// </summary>
    public void RegisterLitCandle(int index)
    {
        if (solved) return;

        playerSequence.Add(index);

        // cancel any pending reset coroutine
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        int pos = playerSequence.Count - 1;

        // Partial correctness check
        if (pos < correctOrder.Count)
        {
            if (playerSequence[pos] != correctOrder[pos])
            {
                // Wrong candle -> reset immediately and stop all messages
                ForceStopAllMessages();
                resetCoroutine = StartCoroutine(ResetAfterDelay(resetDelay));
                return;
            }
            else
            {
                // Correct step: queue the message
                bool isFinalStep = (pos == correctOrder.Count - 1);
                Debug.Log($"[CandleManager] Correct candle at step {pos}. isFinalStep={isFinalStep}");
                StartMotherMessageForStep(pos, isFinalStep);
            }
        }
    }

    private void StartMotherMessageForStep(int stepIndex, bool isFinalStep)
    {
        string msg = null;
        AudioClip clip = null;

        if (motherMessages != null && stepIndex < motherMessages.Length)
            msg = motherMessages[stepIndex];

        if (motherVoiceClips != null && motherVoiceClips.Length > stepIndex)
            clip = motherVoiceClips[stepIndex];

        // ✅ Add message to queue
        MessageData data = new MessageData(msg, clip, isFinalStep);
        messageQueue.Enqueue(data);
        Debug.Log($"[CandleManager] Message queued for step {stepIndex}. Queue size: {messageQueue.Count}");

        // ✅ If not currently playing, start playing from queue
        if (!isPlayingMessage)
        {
            StartCoroutine(ProcessMessageQueue());
        }
    }

    /// <summary>
    /// ✅ Process messages from queue one by one, waiting for each to complete
    /// </summary>
    private IEnumerator ProcessMessageQueue()
    {
        isPlayingMessage = true;

        while (messageQueue.Count > 0)
        {
            MessageData data = messageQueue.Dequeue();
            Debug.Log($"[CandleManager] Playing message. Remaining in queue: {messageQueue.Count}");

            // Play this message and wait for it to complete
            yield return StartCoroutine(ShowMotherMessageCoroutine(data.message, data.voiceClip, data.isFinalStep));

            // Small delay between messages for better pacing
            if (messageQueue.Count > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        isPlayingMessage = false;
        Debug.Log("[CandleManager] All messages played");
    }

    private IEnumerator ShowMotherMessageCoroutine(string message, AudioClip voiceClip, bool isFinalStep)
    {
        // ✅ Calculate display duration: use audio length if available, otherwise use default
        float displayDuration = messageDisplayTime;
        if (voiceClip != null)
        {
            displayDuration = voiceClip.length;
            Debug.Log($"[CandleManager] Using audio clip length: {displayDuration}s");
        }
        else
        {
            Debug.Log($"[CandleManager] No audio clip, using default displayTime: {displayDuration}s");
        }

        // ✅ Show text with calculated duration (text will stay for the full audio length)
        if (!string.IsNullOrEmpty(message) && TextManager.Instance != null)
        {
            TextManager.Instance.ShowNotice(message, displayDuration);
            Debug.Log($"[CandleManager] Showing text for {displayDuration}s");
        }

        // ✅ Play voice if provided (starts at the same time as text)
        if (voiceClip != null && voiceSource != null)
        {
            voiceSource.clip = voiceClip;
            voiceSource.Play();
            Debug.Log("[CandleManager] Playing voiceClip: " + voiceClip.name);
        }

        // ✅ Wait for the full duration (audio length or default time)
        yield return new WaitForSeconds(displayDuration);

        // After duration ends, hide prompt/notice
        if (TextManager.Instance != null)
            TextManager.Instance.HidePrompt();

        Debug.Log("[CandleManager] Message completed");

        // If this was the final step, call solve now (after message finishes)
        if (isFinalStep)
        {
            Debug.Log("[CandleManager] Final message finished – calling OnPuzzleSolved()");
            OnPuzzleSolved();
        }
    }

    /// <summary>
    /// ✅ Only used when wrong candle is lit - forcefully stops everything
    /// </summary>
    private void ForceStopAllMessages()
    {
        // Clear the queue
        messageQueue.Clear();
        isPlayingMessage = false;

        // Stop any running message coroutine
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }

        // Stop voice playback
        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Stop();
        }

        // Hide text
        if (TextManager.Instance != null)
            TextManager.Instance.HidePrompt();

        Debug.Log("[CandleManager] Force stopped all messages and cleared queue");
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetAllCandles();
        playerSequence.Clear();
        resetCoroutine = null;

        if (CharacterMonologue.Instance != null)
        {
            CharacterMonologue.Instance.ShowCandleWrongOrderMonologue();
        }
        else if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowNotice("Thứ tự không đúng. Các ngọn nến tắt hết.", 2f);
        }
    }

    private void ResetAllCandles()
    {
        foreach (var c in candles)
        {
            if (c != null) c.Extinguish(true);
        }
    }

    private void OnPuzzleSolved()
    {
        if (solved) return;
        solved = true;

        // Play success fx
        if (sfxSource != null && successClip != null)
        {
            sfxSource.PlayOneShot(successClip);
        }

        // AUTO-COLLECT the torn piece
        if (tornPiece != null && PickupManager.Instance != null)
        {
            tornPiece.isCollected = true;

            if (!PickupManager.Instance.collectedItemIDs.Contains(tornPiece.itemID))
            {
                PickupManager.Instance.collectedItemIDs.Add(tornPiece.itemID);
            }

            if (tornPiece.pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(tornPiece.pickupSound, Camera.main.transform.position, tornPiece.soundVolume);
            }

            if (TextManager.Instance != null)
            {
                TextManager.Instance.ShowNotice("Bạn đã nhận được 1 mảnh giấy!", 3f);
            }

            if (CharacterMonologue.Instance != null)
            {
                StartCoroutine(ShowCompletionMonologueAfterDelay(2.5f));
            }

            // Check if all pieces collected
            if (PickupManager.Instance.collectedItemIDs.Count == 5)
            {
                if (LetterUIController.Instance != null)
                {
                    LetterUIController.Instance.ShowLetterUI();
                }
            }
        }
    }

    [ContextMenu("Force Solve")]
    public void ForceSolve()
    {
        playerSequence = new List<int>(correctOrder);
        OnPuzzleSolved();
    }

    private IEnumerator ShowCompletionMonologueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (CharacterMonologue.Instance != null)
        {
            CharacterMonologue.Instance.ShowCandleCompletedMonologue(() =>
            {
                if (PickupManager.Instance != null && PickupManager.Instance.collectedItemIDs.Count == 5)
                {
                    if (LetterUIController.Instance != null)
                    {
                        LetterUIController.Instance.ShowLetterUI();
                    }
                }
            });
        }
    }
}