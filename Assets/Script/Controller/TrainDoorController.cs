using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class TrainDoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isLocked = false;
    public float openDistanceX = 0.02f;
    public float openDistanceY = -0.00155f;
    public float openSpeed = 1f;

    [Header("Player Detection")]
    public string playerTag = "Player";
    public float interactDistance = 2f;

    [Header("UI Prompt")]
    public TextMeshProUGUI promptText; // dùng cho UI Canvas
    public CanvasGroup fadeCanvas; // canvas dùng để fade-out

    [Header("Optional Visuals")]
    public Light lockIndicatorLight;
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;

    private bool isOpen = false;
    private bool isOpening = false;
    private bool playerNear = false;
    private bool isTransitioning = false;

    private Vector3 closedPos;
    private Vector3 downPos;
    private Vector3 finalPos;

    private Transform player;

    void Start()
    {
        closedPos = transform.localPosition;
        downPos = closedPos + Vector3.up * openDistanceY;
        finalPos = downPos + Vector3.right * openDistanceX;

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }

        UpdateVisualLockState();
    }

    //void Update()
    //{
    //    // nếu player ở gần cửa mở thì hiển thị gợi ý
    //    if (isOpen && player != null && !isTransitioning)
    //    {
    //        float dist = Vector3.Distance(player.position, transform.position);

    //        if (dist <= interactDistance)
    //        {
    //            if (!promptText.gameObject.activeSelf)
    //                promptText.gameObject.SetActive(true);

    //            if (Input.GetKeyDown(KeyCode.E))
    //            {
    //                StartCoroutine(EnterTrainRoutine());
    //            }
    //        }
    //        else
    //        {
    //            if (promptText.gameObject.activeSelf)
    //                promptText.gameObject.SetActive(false);
    //        }
    //    }

    //    Debug.Log($"[DEBUG] isOpen={isOpen}, player={player != null}, isTransitioning={isTransitioning}");
    //}

    void Update()
    {
        if (isOpen && player != null && !isTransitioning)
        {
            float dist = Vector3.Distance(player.position, transform.position);

            if (dist <= interactDistance)
            {
                if (!promptText.gameObject.activeSelf)
                {
                    Debug.Log("💡 Showing prompt");
                    promptText.gameObject.SetActive(true);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("🟢 E pressed!");
                    StartCoroutine(EnterTrainRoutine());
                }
            }
            else
            {
                if (promptText.gameObject.activeSelf)
                    promptText.gameObject.SetActive(false);
            }
        }
    }

    public void TryOpenDoor()
    {
        if (isLocked || isOpen || isOpening) return;
        StartCoroutine(OpenDoorRoutine());
    }

    IEnumerator OpenDoorRoutine()
    {
        isOpening = true;

        float durationDown = 0.4f / openSpeed;
        float durationSlide = 1f / openSpeed;

        Vector3 startPos = closedPos;

        // Giai đoạn 1: trượt xuống
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / durationDown;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            transform.localPosition = Vector3.Lerp(startPos, downPos, smoothT);
            yield return null;
        }

        // Giai đoạn 2: trượt ngang
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / durationSlide;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            transform.localPosition = Vector3.Lerp(downPos, finalPos, smoothT);
            yield return null;
        }

        transform.localPosition = finalPos;
        isOpen = true;
        isOpening = false;

        Debug.Log("🚪 Door fully opened");
    }

    public void ResetDoor()
    {
        StopAllCoroutines();
        transform.localPosition = closedPos;
        isOpen = false;
        isOpening = false;
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    public void LockDoor(bool locked)
    {
        isLocked = locked;
        UpdateVisualLockState();

        if (isLocked && isOpen)
        {
            ResetDoor();
        }
    }

    private void UpdateVisualLockState()
    {
        if (lockIndicatorLight != null)
            lockIndicatorLight.color = isLocked ? lockedColor : unlockedColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            player = other.transform;
            playerNear = true;
        }

        Debug.Log($"🚶 Trigger entered by: {other.name}, tag={other.tag}");
        if (other.CompareTag(playerTag))
        {
            Debug.Log("✅ Player detected!");
            player = other.transform;
            playerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            player = null;
            playerNear = false;
            if (promptText != null)
                promptText.gameObject.SetActive(false);
        }
    }

    IEnumerator EnterTrainRoutine()
    {
        isTransitioning = true;
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        Debug.Log("🎬 Player entering the train...");

        // fade-out dần dần
        if (fadeCanvas != null)
        {
            float duration = 1.5f;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                fadeCanvas.alpha = Mathf.Lerp(0f, 1f, timer / duration);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("Ohlala");
    }
}
