using UnityEngine;
using System.Collections;

public class TrainMover : MonoBehaviour
{
    [Header("Position Settings")]
    public float startX = 20f;
    public float leftBound = -110f;
    public float rightSpawnX = 90f;

    [Header("Speed Settings")]
    public float maxSpeed = 10f;
    public float minSpeed = 2f; // Tốc độ chậm nhất khi qua điểm 20
    public float acceleration = 8f;
    public float slowdownDistance = 20f; // Khoảng cách bắt đầu chậm lại trước điểm 20

    [Header("Timing")]
    public float pauseTime = 2f; // Chỉ dùng khi spawn

    [Header("Visual FX (Optional)")]
    public Light headLight;
    public float flickerSpeed = 3f;
    public float flickerIntensity = 0.3f;
    public ParticleSystem smokeEffect;

    private float currentSpeed = 0f;
    private Vector3 startPos;

    private float wavePhase = 0f;
    private float waveAmplitude = 0f;

    void Start()
    {
        startPos = transform.position;
        startPos.x = startX;
        transform.position = startPos;

        if (smokeEffect != null && !smokeEffect.isPlaying)
            smokeEffect.Play();

        currentSpeed = 0f;
        Debug.Log("Train Start at x=" + startX);
    }

    void Update()
    {
        HandleMovement();
        HandleEffects();
    }

    void HandleMovement()
    {
        // Luôn có hiệu ứng rung khi chạy
        waveAmplitude = Mathf.Lerp(waveAmplitude, 0.05f, Time.deltaTime * 2f);
        wavePhase += Time.deltaTime * 10f;
        float wave = Mathf.Sin(wavePhase) * waveAmplitude;

        Vector3 newPos = transform.position;
        newPos.y = startPos.y + wave;

        // Tính khoảng cách đến điểm 20
        float distToStart = Mathf.Abs(newPos.x - startX);

        // Tính tốc độ mục tiêu dựa trên khoảng cách đến điểm 20
        float targetSpeed;
        if (distToStart < slowdownDistance)
        {
            // Gần điểm 20 → chậm lại
            targetSpeed = Mathf.Lerp(minSpeed, maxSpeed, distToStart / slowdownDistance);
        }
        else
        {
            // Xa điểm 20 → tốc độ tối đa
            targetSpeed = maxSpeed;
        }

        // Tăng/giảm tốc mượt mà
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        // Di chuyển sang trái
        newPos.x -= currentSpeed * Time.deltaTime;

        // Khi đến biên trái → spawn lại bên phải
        if (newPos.x <= leftBound)
        {
            newPos.x = rightSpawnX;
            newPos.y = startPos.y;
            currentSpeed = 0f; // Reset tốc độ khi spawn
            Debug.Log("Reached leftBound, teleporting to rightSpawnX=" + rightSpawnX);
        }

        transform.position = newPos;
    }

    void HandleEffects()
    {
        if (headLight != null)
        {
            float baseIntensity = 1f;
            float flicker = Mathf.Sin(Time.time * flickerSpeed) * flickerIntensity;
            headLight.intensity = baseIntensity + flicker;
        }

        if (smokeEffect != null)
        {
            var emission = smokeEffect.emission;
            // Khói nhiều hơn khi chạy nhanh
            float targetRate = Mathf.Lerp(5f, 15f, currentSpeed / maxSpeed);
            float currentRate = emission.rateOverTime.constant;
            emission.rateOverTime = Mathf.Lerp(currentRate, targetRate, Time.deltaTime * 2f);

            var main = smokeEffect.main;
            main.startSpeed = 0.5f;
            main.startRotation = Mathf.Atan2(-transform.forward.z, transform.forward.x);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(leftBound, 0, 0), new Vector3(leftBound, 5, 0));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(startX, 0, 0), new Vector3(startX, 5, 0));

        // Vẽ vùng chậm lại
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(startX - slowdownDistance, 0, 0), new Vector3(startX - slowdownDistance, 3, 0));
        Gizmos.DrawLine(new Vector3(startX + slowdownDistance, 0, 0), new Vector3(startX + slowdownDistance, 3, 0));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(rightSpawnX, 0, 0), new Vector3(rightSpawnX, 5, 0));
    }
}