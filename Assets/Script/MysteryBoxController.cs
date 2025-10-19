using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class MysteryBoxController : MonoBehaviour
{
    [Header("Mystery Box Setup")]
    public GameObject toyTrainPrefab; // Drag toy train prefab here (inactive by default)
    public Camera renderCamera; // Drag a new camera here (set to orthographic)
    public RenderTexture renderTexture; // Drag a Render Texture asset here
    public RawImage displayImage; // Drag a UI RawImage here (toyTrainImage)
    public Button closeButton; // Drag a UI Button here (child of displayImage)
    public float cameraDistance = 2f; // Distance from train to camera
    public float orthographicSize = 2.5f; // Camera zoom level
    public Vector3 trainOffset = Vector3.up * 2f; // Offset for train position

    [Header("Video Settings")]
    public VideoPlayer videoPlayer; // Drag VideoPlayerManager GameObject here
    public RawImage videoDisplay; // Drag VideoDisplay RawImage (child of CutsceneCanvas) here
    public VideoClip videoClip; // Drag your video file here
    public string nextSceneName = "map"; // Name of the scene to load after video

    private bool isOpen = false;
    private GameObject trainInstance;
    private PlayerController playerController;

    void Start()
    {
        // Find player controller for input disabling
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }

        // Ensure UI is hidden and camera is set up
        if (displayImage != null)
        {
            displayImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("displayImage is not assigned!");
        }

        if (renderCamera != null)
        {
            renderCamera.enabled = false;
            renderCamera.orthographic = true;
            renderCamera.targetTexture = renderTexture;
        }
        else
        {
            Debug.LogError("renderCamera is not assigned!");
        }

        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(false);
            closeButton.onClick.AddListener(CloseUI);
        }
        else
        {
            Debug.LogError("closeButton is not assigned!");
        }

        // Setup video player
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.Stop();

            if (videoClip != null)
            {
                videoPlayer.clip = videoClip;
                Debug.Log("Video clip assigned: " + videoClip.name);
            }
            else
            {
                Debug.LogError("Video Clip is not assigned!");
            }

            videoPlayer.loopPointReached += OnVideoEnd;

            if (videoDisplay != null)
            {
                // Make sure CutsceneCanvas is inactive at start
                Transform canvasParent = videoDisplay.transform.parent;
                if (canvasParent != null)
                {
                    canvasParent.gameObject.SetActive(false);
                    Debug.Log("CutsceneCanvas set to inactive at start");
                }
                videoDisplay.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("videoPlayer is not assigned!");
        }
    }

    public void OpenBox()
    {
        if (isOpen)
        {
            Debug.Log("Mystery box already opened!");
            return;
        }

        if (PickupManager.Instance == null || !PickupManager.Instance.IsCollected("SafeKey"))
        {
            Debug.Log("Need the SafeKey to open the mystery box!");
            return;
        }

        isOpen = true;
        Debug.Log("Mystery box opened!");

        if (toyTrainPrefab == null || renderCamera == null || renderTexture == null || displayImage == null)
        {
            Debug.LogError("Missing required components!");
            return;
        }

        // Instantiate toy train
        Vector3 trainPosition = transform.position + trainOffset;
        trainInstance = Instantiate(toyTrainPrefab, trainPosition, Quaternion.identity);
        trainInstance.SetActive(true);
        Debug.Log("Toy train instantiated at: " + trainPosition);

        // Configure render camera
        renderCamera.enabled = true;
        renderCamera.orthographicSize = orthographicSize;
        renderCamera.transform.position = trainPosition + Vector3.back * cameraDistance;
        renderCamera.transform.LookAt(trainPosition);

        // Start the sequence
        StartCoroutine(RenderAndDisplaySequence());
    }

    private IEnumerator RenderAndDisplaySequence()
    {
        // Step 1: Render the train image
        yield return new WaitForEndOfFrame();
        renderCamera.Render();
        renderCamera.enabled = false;

        // Step 2: Display the train image
        displayImage.texture = renderTexture;
        displayImage.gameObject.SetActive(true);
        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(true);
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Train image displayed. Waiting for player to click Close button...");

        // NOW WAITING FOR PLAYER TO CLICK CLOSE BUTTON
        // The CloseUI() function will handle the rest
    }

    private void CloseUI()
    {
        Debug.Log("Close button clicked! Starting video sequence...");

        // Hide train image and close button
        displayImage.gameObject.SetActive(false);
        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(false);
        }

        // Clean up train instance
        if (trainInstance != null)
        {
            Destroy(trainInstance);
        }

        // Start the video cutscene
        PlayCutsceneVideo();
    }

    private void PlayCutsceneVideo()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer is null!");
            LoadNextScene();
            return;
        }

        if (videoClip == null)
        {
            Debug.LogError("VideoClip is null!");
            LoadNextScene();
            return;
        }

        if (videoDisplay == null)
        {
            Debug.LogError("VideoDisplay is null!");
            LoadNextScene();
            return;
        }

        Debug.Log("=== Starting Video Playback ===");

        // Disable player input
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Activate CutsceneCanvas
        Transform canvasParent = videoDisplay.transform.parent;
        if (canvasParent != null)
        {
            canvasParent.gameObject.SetActive(true);
            Debug.Log("CutsceneCanvas activated!");
        }

        // Ensure VideoPlayer is enabled
        if (!videoPlayer.enabled)
        {
            videoPlayer.enabled = true;
            Debug.Log("VideoPlayer enabled");
        }

        // Assign video clip
        videoPlayer.clip = videoClip;

        // Setup render texture
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        RenderTexture videoRT;
        if (videoPlayer.targetTexture != null)
        {
            videoRT = videoPlayer.targetTexture;
            Debug.Log("Using assigned render texture");
        }
        else
        {
            videoRT = new RenderTexture(1920, 1080, 0);
            videoPlayer.targetTexture = videoRT;
            Debug.Log("Created new render texture");
        }

        videoDisplay.texture = videoRT;
        videoDisplay.gameObject.SetActive(true);

        // Prepare and play
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log("Video prepared! Playing...");
        source.prepareCompleted -= OnVideoPrepared;
        source.Play();
        Debug.Log("Video.Play() called. IsPlaying: " + source.isPlaying);
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video finished!");

        // Hide video and canvas
        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(false);
            Transform canvasParent = videoDisplay.transform.parent;
            if (canvasParent != null)
            {
                canvasParent.gameObject.SetActive(false);
            }
        }

        // Re-enable player
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Load next scene
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        Debug.Log("Loading scene: " + nextSceneName);

        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Scene '" + nextSceneName + "' not found in Build Settings!");
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
}