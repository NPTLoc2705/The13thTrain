using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Header("Door Animation Settings")]
    public Transform doorWing; // Assign the doorwing child object here
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool isOpen = false;

    [Header("Door Interaction Settings")]
    public float interactionDistance = 3f;
    public bool requiresKey = false;
    public string requiredKeyID = "RedKey";

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine currentCoroutine;
    private Transform playerTransform;
    private bool wasInRange = false;

    void Start()
    {
        // If doorWing is not assigned, try to find it
        if (doorWing == null)
        {
            doorWing = transform.Find("doorwing");
            if (doorWing == null)
            {
                Debug.LogError("doorWing not found! Please assign it in the inspector or ensure it's named 'doorwing'");
            }
        }

        if (doorWing != null)
        {
            closedRotation = doorWing.rotation;
            openRotation = Quaternion.Euler(doorWing.eulerAngles + new Vector3(0, openAngle, 0));
        }
        else
        {
            Debug.LogError("DoorWing is required for door animation!");
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
            Debug.LogError("Player not found! Make sure Player has tag 'Player'");
    }

    void Update()
    {
        UpdatePrompt();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void UpdatePrompt()
    {
        if (playerTransform == null || TextManager.Instance == null) return;

        float distance = Vector3.Distance(playerTransform.position, transform.position);
        bool inRange = distance <= interactionDistance;

        if (inRange)
        {
            string promptMessage = GetPromptMessage();
            TextManager.Instance.ShowPrompt(promptMessage);
            wasInRange = true;
        }
        else if (wasInRange)
        {
            // Only hide prompt if we were previously showing it
            TextManager.Instance.HidePrompt();
            wasInRange = false;
        }
    }

    private string GetPromptMessage()
    {
        if (requiresKey)
        {
            if (PickupManager.Instance == null || !PickupManager.Instance.IsCollected(requiredKeyID))
            {
                return $"[E] Need {requiredKeyID} to open door!";
            }
        }

        return isOpen ? "[E] Close Door" : "[E] Open Door";
    }

    private void TryInteract()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(playerTransform.position, transform.position);

        if (distance > interactionDistance)
        {
            if (TextManager.Instance != null)
            {
                TextManager.Instance.ShowNotice($"Too far! Need to be in {interactionDistance}m", 2f);
            }
            return;
        }

        if (requiresKey)
        {
            if (PickupManager.Instance == null || !PickupManager.Instance.IsCollected(requiredKeyID))
            {
                if (TextManager.Instance != null)
                {
                    TextManager.Instance.ShowNotice($"Need {requiredKeyID} to open door!", 2f);
                }
                return;
            }
        }

        // All conditions met, toggle door
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(ToggleDoor());
    }

    private IEnumerator ToggleDoor()
    {
        if (doorWing == null) yield break;

        Quaternion targetRotation = isOpen ? closedRotation : openRotation;

        while (Quaternion.Angle(doorWing.rotation, targetRotation) > 0.1f)
        {
            doorWing.rotation = Quaternion.Slerp(doorWing.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }

        doorWing.rotation = targetRotation;
        isOpen = !isOpen;

        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowNotice(isOpen ? "Opened" : "Closed", 1.5f);
        }
    }

    void OnDisable()
    {
        // Hide prompt when door is disabled
        if (wasInRange && TextManager.Instance != null)
        {
            TextManager.Instance.HidePrompt();
            wasInRange = false;
        }
    }
}