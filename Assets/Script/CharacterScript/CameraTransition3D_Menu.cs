using UnityEngine;
using System.Collections;

public class CameraTransition3D_Menu : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;                  // Camera menu (Main Camera) - sẽ bị tắt sau transition
    public Transform playerCharacter;          // Nhân vật (Transform)
    public CameraFollowPUBG cameraFollow;      // Camera follow (đang inactive / disabled trước khi zoom)
    public PlayerController playerController;  // Player controller (không sửa)
    public AudioSource bgmSource;              // Nhạc nền menu (tùy chọn)

    [Header("Transition Settings")]
    public float zoomDuration = 2.5f;
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isZooming = false;

    void Start()
    {
        if (cameraFollow != null)
            cameraFollow.enabled = false;

        if (playerController != null)
            playerController.enabled = false;
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
            Debug.LogError("CameraTransition3D_Menu: thiếu reference!");
            isZooming = false;
            yield break;
        }

        // Tắt camera follow tạm thời (để mainCamera dùng làm cinematic)
        cameraFollow.gameObject.SetActive(false);

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        Vector3 targetPos = cameraFollow.transform.position;
        Quaternion targetRot = cameraFollow.transform.rotation;

        float timer = 0f;
        float startVol = bgmSource ? bgmSource.volume : 0f;

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

        // Đặt chính xác vị trí & hướng cinematic cuối cùng
        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;

        // ---- CRITICAL: Chuẩn bị chuyển sang camera follow ----
        // 1) Đồng bộ vị trí/hướng của cameraFollow với mainCamera (để không có "nhảy" khi active)
        cameraFollow.transform.position = mainCamera.transform.position;
        cameraFollow.transform.rotation = mainCamera.transform.rotation;

        // 2) Nếu có 1 camera menu đang là "MainCamera", bỏ tag đó đi trước khi gán tag cho cameraFollow
        //    Vì Camera.main lấy camera theo tag "MainCamera"
        if (mainCamera.gameObject.CompareTag("MainCamera"))
        {
            mainCamera.gameObject.tag = "Untagged";
        }

        // 3) Tắt camera menu (nên disable để tránh 2 camera active cùng lúc)
        mainCamera.gameObject.SetActive(false);

        // 4) Gán tag MainCamera cho cameraFollow (vì PlayerController có thể dùng Camera.main mỗi frame)
        cameraFollow.gameObject.tag = "MainCamera";

        // 5) Bật cameraFollow (gameobject + script)
        cameraFollow.gameObject.SetActive(true);
        cameraFollow.enabled = true;

        // 6) Đồng bộ hướng nhân vật (chỉ thay transform.forward, không đụng tới PlayerController)
        Vector3 flatForward = cameraFollow.transform.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude > 0.0001f)
            playerCharacter.forward = flatForward.normalized;

        // 7) (TÙY CHỌN) reset/clear một số trạng thái vật lý nếu cần
        //    Nếu bạn có CharacterController hoặc Rigidbody và muố nclear momentum, làm ở đây.
        CharacterController cc = playerCharacter.GetComponent<CharacterController>();
        if (cc != null)
        {
            // Di chuyển 1 delta nhỏ không làm thay đổi vị trí, nhưng đảm bảo ổn định
            // (Không có API set velocity cho CharacterController, nên ta không chạm vào)
        }

        // 😎 Bật PlayerController sau khi đã sắp xếp xong camera và hướng nhân vật
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Tắt nhạc nếu có
        if (bgmSource)
            bgmSource.Stop();

        // Chờ 1s cho cinematic kết thúc trước khi show tutorial/UI
        yield return new WaitForSeconds(1f);

        PlayerTutorialUI tutorial = FindObjectOfType<PlayerTutorialUI>();
        if (tutorial != null)
            tutorial.ShowTutorial();

        isZooming = false;
    }
}