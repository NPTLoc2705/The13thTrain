using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Renders a 3D note model to display behind UI text as background
/// </summary>
public class Note3DBackground : MonoBehaviour
{
    [Header("3D Note Setup")]
    [SerializeField] private GameObject note3DModel; // Your yellow note 3D model
    [SerializeField] private Camera renderCamera; // Dedicated camera to render the note

    [Header("UI Setup")]
    [SerializeField] private RawImage backgroundImage; // UI RawImage to display the note
    [SerializeField] private RenderTexture renderTexture; // RenderTexture to capture the note

    [Header("Camera Settings")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0, -2f);
    [SerializeField] private float cameraSize = 1.5f;
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    void Start()
    {
        SetupRenderCamera();
        SetupRenderTexture();
    }

    private void SetupRenderCamera()
    {
        // Create camera if not assigned
        if (renderCamera == null)
        {
            GameObject camObj = new GameObject("NoteRenderCamera");
            camObj.transform.SetParent(transform);
            renderCamera = camObj.AddComponent<Camera>();
        }

        // Configure camera
        renderCamera.orthographic = true;
        renderCamera.orthographicSize = cameraSize;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = backgroundColor;

        // Only render the NoteRender layer
        int noteLayer = LayerMask.NameToLayer("NoteRender");
        if (noteLayer == -1)
        {
            Debug.LogError("NoteRender layer not found! Please create it in Project Settings → Tags and Layers");
            noteLayer = 0; // Fallback to Default layer
        }
        renderCamera.cullingMask = 1 << noteLayer;
        renderCamera.depth = -10; // Render before main camera
        renderCamera.enabled = false; // Start disabled

        // Position camera to look at note
        if (note3DModel != null)
        {
            renderCamera.transform.position = note3DModel.transform.position + cameraOffset;
            renderCamera.transform.LookAt(note3DModel.transform);
        }
    }

    private void SetupRenderTexture()
    {
        // Create RenderTexture if not assigned
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(1024, 1024, 16);
            renderTexture.format = RenderTextureFormat.ARGB32;
        }

        // Assign to camera
        if (renderCamera != null)
        {
            renderCamera.targetTexture = renderTexture;
        }

        // Assign to UI background image
        if (backgroundImage != null)
        {
            backgroundImage.texture = renderTexture;
        }
    }

    public void Show()
    {
        if (note3DModel != null) note3DModel.SetActive(true);
        if (renderCamera != null) renderCamera.enabled = true;
    }

    public void Hide()
    {
        if (note3DModel != null) note3DModel.SetActive(false);
        if (renderCamera != null) renderCamera.enabled = false;
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
        }
    }
}