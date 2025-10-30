using UnityEngine;
using System.Collections;

public class TrainMover : MonoBehaviour
{
    [Header("Position Settings")]
    public float startX = 20f;
    public float leftBound = -70f;
    public float rightSpawnX = 60f;

    [Header("Speed Settings")]
    public float maxSpeed = 10f;
    public float minSpeed = 2f;
    public float acceleration = 8f;
    public float slowdownDistance = 20f;

    [Header("Timing")]
    public float pauseTime = 2f;

    [Header("Visual FX (Optional)")]
    public Light headLight;
    public float flickerSpeed = 3f;
    public float flickerIntensity = 0.3f;
    public ParticleSystem smokeEffect;

    [Header("Fade Settings (for Play transition)")]
    public Renderer[] trainRenderers;
    public float fadeDuration = 1.5f;
    private bool cinematicMode = false;

    private float currentSpeed = 0f;
    private Vector3 startPos;
    private float wavePhase = 0f;
    private float waveAmplitude = 0f;

    private bool stoppingAtStation = false;
    private bool completedCycle = false;

    void Start()
    {
        startPos = transform.position;
        startPos.x = startX;
        transform.position = startPos;

        if (smokeEffect != null && !smokeEffect.isPlaying)
            smokeEffect.Play();

        currentSpeed = 0f;
    }

    void Update()
    {
        if (cinematicMode) return; // ✅ trong chế độ cinematic thì không chạy vòng lặp
        HandleMovement();
        HandleEffects();
    }

    void HandleMovement()
    {
        waveAmplitude = Mathf.Lerp(waveAmplitude, 0.05f, Time.deltaTime * 2f);
        wavePhase += Time.deltaTime * 10f;
        float wave = Mathf.Sin(wavePhase) * waveAmplitude;

        Vector3 newPos = transform.position;
        newPos.y = startPos.y + wave;

        // 🚂 Nếu đang trong chế độ "dừng tại ga"
        if (stoppingAtStation)
        {
            HandleStopAtStation(ref newPos);
            transform.position = newPos;
            return;
        }

        // --- Di chuyển bình thường ---
        float distToStart = Mathf.Abs(newPos.x - startX);
        float targetSpeed = (distToStart < slowdownDistance)
            ? Mathf.Lerp(minSpeed, maxSpeed, distToStart / slowdownDistance)
            : maxSpeed;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        newPos.x -= currentSpeed * Time.deltaTime;

        if (newPos.x <= leftBound)
        {
            newPos.x = rightSpawnX;
            newPos.y = startPos.y;
            currentSpeed = 0f;
        }

        transform.position = newPos;
    }

    void HandleStopAtStation(ref Vector3 pos)
    {
        if (pos.x > startX)
        {
            float distTo20 = pos.x - startX;
            float targetSpeed = Mathf.Lerp(0f, maxSpeed, distTo20 / slowdownDistance);
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            pos.x -= currentSpeed * Time.deltaTime;

            if (pos.x <= startX)
            {
                pos.x = startX;
                currentSpeed = 0f;
                stoppingAtStation = false;
                Debug.Log("🚉 Train stopped at station (20)");
            }
        }
        else
        {
            if (!completedCycle)
            {
                pos.x -= currentSpeed * Time.deltaTime;

                if (pos.x <= leftBound)
                {
                    pos.x = rightSpawnX;
                    pos.y = startPos.y;
                    currentSpeed = 0f;
                    completedCycle = true;
                    Debug.Log("🔁 Completed one cycle, returning to station...");
                }
            }
            else
            {
                float distTo20 = Mathf.Abs(pos.x - startX);
                float targetSpeed = Mathf.Lerp(0f, maxSpeed, distTo20 / slowdownDistance);
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
                pos.x -= currentSpeed * Time.deltaTime;

                if (pos.x <= startX)
                {
                    pos.x = startX;
                    currentSpeed = 0f;
                    stoppingAtStation = false;
                    completedCycle = false;
                    Debug.Log("🚉 Train stopped at station after full cycle");
                }
            }
        }
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
            float targetRate = Mathf.Lerp(5f, 15f, currentSpeed / maxSpeed);
            float currentRate = emission.rateOverTime.constant;
            emission.rateOverTime = Mathf.Lerp(currentRate, targetRate, Time.deltaTime * 2f);

            var main = smokeEffect.main;
            main.startSpeed = 0.5f;
            main.startRotation = Mathf.Atan2(-transform.forward.z, transform.forward.x);
        }
    }

    // 🚆 Gọi từ MainMenuController khi bấm Play
    public void PlayCinematicStopAtStation()
    {
        StartCoroutine(PlayTransitionRoutine());
    }

    IEnumerator PlayTransitionRoutine()
    {
        cinematicMode = true; // ❌ ngừng vòng lặp tạm thời

        // 1️⃣ Fade out
        Debug.Log("🚂 Train fade-out...");
        yield return StartCoroutine(FadeTrain(1f, 0f));

        // 2️⃣ Teleport sang bên phải
        transform.position = new Vector3(rightSpawnX, startPos.y, startPos.z);
        currentSpeed = 0f;

        // 3️⃣ Đợi camera zoom (~2.5s)
        yield return new WaitForSeconds(2.5f);

        // 4️⃣ Fade in
        Debug.Log("🚆 Train fade-in & approach station...");
        yield return StartCoroutine(FadeTrain(0f, 1f));

        // 5️⃣ Di chuyển về ga
        yield return StartCoroutine(MoveToStation());

        cinematicMode = false; // ✅ khôi phục hoạt động bình thường sau khi dừng
    }

    IEnumerator MoveToStation()
    {
        Vector3 pos = transform.position;
        currentSpeed = 0f;

        while (pos.x > startX)
        {
            float dist = pos.x - startX;
            float targetSpeed = Mathf.Lerp(0f, maxSpeed, dist / slowdownDistance);
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            pos.x -= currentSpeed * Time.deltaTime;

            transform.position = pos;
            yield return null;
        }

        transform.position = new Vector3(startX, startPos.y, startPos.z);
        currentSpeed = 0f;
        Debug.Log("🚉 Train reached station (20) after cinematic");
    }

    IEnumerator FadeTrain(float fromAlpha, float toAlpha)
    {
        if (trainRenderers == null || trainRenderers.Length == 0)
            yield break;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / fadeDuration);
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            SetTrainAlpha(alpha);
            yield return null;
        }

        SetTrainAlpha(toAlpha);
    }

    void SetTrainAlpha(float alpha)
    {
        foreach (var rend in trainRenderers)
        {
            if (rend == null) continue;
            foreach (var mat in rend.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(leftBound, 0, 0), new Vector3(leftBound, 5, 0));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(startX, 0, 0), new Vector3(startX, 5, 0));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(rightSpawnX, 0, 0), new Vector3(rightSpawnX, 5, 0));
    }
}
