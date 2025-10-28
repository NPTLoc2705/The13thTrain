using UnityEngine;
using System.Collections;

public class CameraTransition3D_Menu : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;                  // Camera menu (Main Camera)
    public Transform playerCharacter;          // Nhân vật
    public CameraFollowPUBG cameraFollow;      // Script follow đã có sẵn (trên camera con)
    public PlayerController playerController;  // Player controller
    public AudioSource bgmSource;              // Nhạc nền menu (tùy chọn)

    [Header("Transition Settings")]
    public float zoomDuration = 2.5f;
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isZooming = false;

    void Start()
    {
        if (cameraFollow != null)
            cameraFollow.enabled = false; // tắt follow trước khi zoom

        if (playerController != null)
            playerController.enabled = false; // nhân vật chưa di chuyển khi đang ở menu
    }

    public void StartTransition()
    {
        if (!isZooming)
            StartCoroutine(ZoomToPlayer());
    }

    IEnumerator ZoomToPlayer()
    {
        isZooming = true;

        if (mainCamera == null || playerCharacter == null || cameraFollow == null)
        {
            Debug.LogError("⚠️ Missing camera or player reference in CameraTransition3D_Menu!");
            yield break;
        }

        // Ẩn con camera follow để tránh can thiệp trong lúc zoom
        cameraFollow.gameObject.SetActive(false);

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        // Lấy vị trí và góc của camera follow (điểm đến)
        Vector3 targetPos = cameraFollow.transform.position;
        Quaternion targetRot = cameraFollow.transform.rotation;

        float timer = 0f;
        float startVol = bgmSource ? bgmSource.volume : 0f;

        // Zoom mượt dần
        while (timer < zoomDuration)
        {
            timer += Time.deltaTime;
            float t = zoomCurve.Evaluate(timer / zoomDuration);

            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            if (bgmSource)
                bgmSource.volume = Mathf.Lerp(startVol, 0f, t);

            yield return null;
        }

        // Đặt camera đúng vị trí đích
        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;

        // Tắt nhạc
        if (bgmSource)
            bgmSource.Stop();

        // Bật camera follow thật
        cameraFollow.gameObject.SetActive(true);
        cameraFollow.enabled = true;

        // Bật player control
        if (playerController != null)
            playerController.enabled = true;

        // ✅ Hiển thị hướng dẫn sau khi zoom hoàn tất
        yield return new WaitForSeconds(1f); // chờ 1 giây cho cinematic
        PlayerTutorialUI tutorial = FindObjectOfType<PlayerTutorialUI>();
        if (tutorial != null)
        {
            Debug.Log("🚀 Tutorial shown AFTER camera zoom!");
            tutorial.ShowTutorial();
        }

        isZooming = false;
    }
}
