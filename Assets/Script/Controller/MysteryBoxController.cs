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
    private bool isDisplayingTrain = false;

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
        

        if (renderCamera != null)
        {
            renderCamera.enabled = false;
            renderCamera.orthographic = true;
            renderCamera.targetTexture = renderTexture;
        }
      

        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(false);
            closeButton.onClick.AddListener(CloseUI);
        }
       

        // Setup video player
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.Stop();

            if (videoClip != null)
            {
                videoPlayer.clip = videoClip;
            }
           

            videoPlayer.loopPointReached += OnVideoEnd;

            if (videoDisplay != null)
            {
                // Make sure CutsceneCanvas is inactive at start
                Transform canvasParent = videoDisplay.transform.parent;
                if (canvasParent != null)
                {
                    canvasParent.gameObject.SetActive(false);
                }
                videoDisplay.gameObject.SetActive(false);
            }
        }
        else
        {
        }
    }

    void Update()
    {
        // Allow closing train display with Escape key
        if (isDisplayingTrain && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseUI();
        }
    }

    public void OpenBox()
    {
        if (isOpen)
        {
            return;
        }

        if (PickupManager.Instance == null || !PickupManager.Instance.IsCollected("SafeKey"))
        {
            return;
        }

        isOpen = true;

        if (toyTrainPrefab == null || renderCamera == null || renderTexture == null || displayImage == null)
        {
            return;
        }

        // Instantiate toy train
        Vector3 trainPosition = transform.position + trainOffset;
        trainInstance = Instantiate(toyTrainPrefab, trainPosition, Quaternion.identity);
        trainInstance.SetActive(true);

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
        isDisplayingTrain = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // NOW WAITING FOR PLAYER TO CLICK CLOSE BUTTON OR PRESS ESC
        // The CloseUI() function will handle the rest
    }

    private void CloseUI()
    {
        if (!isDisplayingTrain) return;

        isDisplayingTrain = false;

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

    /// <summary>
    /// Check if the train display is currently open
    /// </summary>
    public bool IsTrainDisplayOpen()
    {
        return isDisplayingTrain;
    }

    private void PlayCutsceneVideo()
    {
        if (videoPlayer == null)
        {
            LoadNextScene();
            return;
        }

        if (videoClip == null)
        {
            LoadNextScene();
            return;
        }

        if (videoDisplay == null)
        {
            LoadNextScene();
            return;
        }


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
        }

        // Ensure VideoPlayer is enabled
        if (!videoPlayer.enabled)
        {
            videoPlayer.enabled = true;
        }

        // Assign video clip
        videoPlayer.clip = videoClip;

        // Setup render texture
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        RenderTexture videoRT;
        if (videoPlayer.targetTexture != null)
        {
            videoRT = videoPlayer.targetTexture;
        }
        else
        {
            videoRT = new RenderTexture(1920, 1080, 0);
            videoPlayer.targetTexture = videoRT;
        }

        videoDisplay.texture = videoRT;
        videoDisplay.gameObject.SetActive(true);

        // Prepare and play
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        source.prepareCompleted -= OnVideoPrepared;
        source.Play();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {

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
        // Destroy the mystery box before loading next scene
        Destroy(gameObject);

        
        // Load next scene
        LoadNextScene();
    }

    private void LoadNextScene()
    {

        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
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