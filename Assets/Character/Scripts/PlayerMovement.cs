using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController_LN_SmoothMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float rotationSpeed = 120f;
    public float jumpForce = 7f;
    public float baseGravity = 9.81f;
    public float fallMultiplier = 2.5f;

    [Header("Animation")]
    public Animator animator;
    public float pingPongSpeed = 1f;
    public float smoothAnimBlend = 8f;

    [Header("Camera Follow")]
    public Transform cameraTransform;
    public Vector3 cameraOffset = new Vector3(0, 3, -6);
    public float cameraSmoothTime = 0.2f;

    private CharacterController controller;
    private Vector3 velocity;
    private float currentSpeed;
    private float animSpeed;
    private Vector3 camVel = Vector3.zero;
    private bool isJumping = false;

    // Ping-pong
    private bool pingForward = true;
    private float animTime = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleCameraFollow();
    }

    // ---------------------- MOVEMENT ----------------------
    void HandleMovement()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Xoay nhân vật
        transform.Rotate(Vector3.up, h * rotationSpeed * Time.deltaTime);

        // Tốc độ mục tiêu
        float moveSpeed = (isRunning ? runSpeed : walkSpeed);
        float targetSpeed = Mathf.Abs(v) > 0.1f ? moveSpeed * Mathf.Sign(v) : 0f;

        // Làm mượt chuyển động vật lý
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * 6f);
        Vector3 move = transform.forward * currentSpeed * Time.deltaTime;
        controller.Move(move);

        // ---------------------- ANIMATION ----------------------
        if (animator != null)
        {
            float speedRatio = Mathf.InverseLerp(0, runSpeed, Mathf.Abs(currentSpeed));

            // Làm mượt chuyển giá trị gửi vào animator (tránh giật)
            animSpeed = Mathf.Lerp(animSpeed, speedRatio, Time.deltaTime * smoothAnimBlend);

            animator.SetFloat("Speed", animSpeed);
            animator.SetBool("isGrounded", controller.isGrounded);

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            bool isInLocomotion = state.IsName("Locomotion");

            // ✅ Ping-pong chỉ khi grounded & có di chuyển
            if (controller.isGrounded && !isJumping && animSpeed > 0.1f && isInLocomotion)
            {
                animTime += (pingForward ? 1 : -1) * Time.deltaTime * pingPongSpeed;

                if (animTime >= 1f)
                {
                    animTime = 1f;
                    pingForward = false;
                }
                else if (animTime <= 0f)
                {
                    animTime = 0f;
                    pingForward = true;
                }

                animator.Play(state.fullPathHash, 0, animTime);
            }
            else if (controller.isGrounded && !isJumping)
            {
                animTime = 0f;
                pingForward = true;
            }
        }
    }

    // ---------------------- JUMP ----------------------
    void HandleJump()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            if (isJumping)
                isJumping = false;

            velocity.y = -2f;

            if (Input.GetKeyDown(KeyCode.Space))
                DoJump();
        }
        else
        {
            if (velocity.y < 0)
                velocity.y -= baseGravity * fallMultiplier * Time.deltaTime;
            else
                velocity.y -= baseGravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    void DoJump()
    {
        if (isJumping) return;

        isJumping = true;
        velocity.y = jumpForce;

        if (animator != null)
        {
            animator.ResetTrigger("Jump");
            animator.SetTrigger("Jump");
        }
    }

    // ---------------------- CAMERA ----------------------
    void HandleCameraFollow()
    {
        if (cameraTransform == null) return;

        Vector3 targetPos = transform.position + transform.rotation * cameraOffset;
        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, targetPos, ref camVel, cameraSmoothTime);
        cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
    }
}
