using UnityEngine;
using System.Collections;

public class CameraTransition3D_Smooth : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;                  // Camera menu (Main Camera)
    public Transform playerCharacter;          // Nhân vật
    public Transform followCameraTransform;    // CameraFollow (con của Player)
    public PlayerController playerController;  // PlayerController
    public CameraFollowPUBG cameraFollow;      // Script CameraFollow
    public AudioSource bgmSource;              // Nhạc nền menu (tùy chọn)

    [Header("Transition Settings")]
    public float transitionDuration = 2.5f;
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float finalBlendTime = 0.3f; // khoảng thời gian blend giữa MainCamera và CameraFollow

    private bool isTransitioning = false;

    void Start()
    {
        // Ẩn script follow và player khi khởi đầu
        if (cameraFollow) cameraFollow.enabled = false;
        if (playerController) playerController.enabled = false;

        // Ẩn camera follow object (nếu khác main camera)
        if (followCameraTransform && followCameraTransform.gameObject != mainCamera.gameObject)
            followCameraTransform.gameObject.SetActive(false);
    }

    public void StartTransition()
    {
        if (!isTransitioning)
            StartCoroutine(SmoothZoomRoutine());
    }

    IEnumerator SmoothZoomRoutine()
    {
        isTransitioning = true;

        // Bật CameraFollow object nhưng KHÔNG bật script
        if (followCameraTransform && followCameraTransform.gameObject != mainCamera.gameObject)
            followCameraTransform.gameObject.SetActive(true);

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        Vector3 endPos = followCameraTransform.position;
        Quaternion endRot = followCameraTransform.rotation;

        float timer = 0f;
        float startVol = bgmSource ? bgmSource.volume : 0f;

        // 🔸 Zoom mượt dựa trên animation curve
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = zoomCurve.Evaluate(timer / transitionDuration);

            // Interpolate position + rotation
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            // Fade nhạc nếu có
            if (bgmSource)
                bgmSource.volume = Mathf.Lerp(startVol, 0f, t * 0.8f);

            yield return null;
        }

        // Chốt vị trí cuối cùng
        mainCamera.transform.position = endPos;
        mainCamera.transform.rotation = endRot;

        // 🔹 Tắt nhạc menu
        if (bgmSource) bgmSource.Stop();

        // Bật script follow (bắt đầu blend nhẹ để không giật)
        if (cameraFollow != null)
        {
            StartCoroutine(BlendToFollowCamera());
        }

        // Bật player control
        if (playerController != null)
            playerController.enabled = true;

        isTransitioning = false;
    }

    IEnumerator BlendToFollowCamera()
    {
        // Kích hoạt camera follow
        cameraFollow.enabled = true;

        // Blend nhẹ vị trí để tránh nhảy khung
        Vector3 smoothPos = mainCamera.transform.position;
        Quaternion smoothRot = mainCamera.transform.rotation;

        float timer = 0f;
        while (timer < finalBlendTime)
        {
            timer += Time.deltaTime;
            smoothPos = Vector3.Lerp(smoothPos, followCameraTransform.position, timer / finalBlendTime);
            smoothRot = Quaternion.Slerp(smoothRot, followCameraTransform.rotation, timer / finalBlendTime);

            mainCamera.transform.position = smoothPos;
            mainCamera.transform.rotation = smoothRot;

            yield return null;
        }
    }
}
