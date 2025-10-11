using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Animator animator;          // Kéo Animator của nhân vật vào đây
    public CharacterController controller; // Gắn CharacterController

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 4.5f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isRunning;
    private bool isJumping;
    void Update()
    {
        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        // Kiểm tra chạm đất
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Nhập phím WASD
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        bool hasInput = move.magnitude > 0.1f;

        // Shift để chạy
        isRunning = Input.GetKey(KeyCode.LeftShift) && hasInput;

        // Tính tốc độ di chuyển
        float targetSpeed = 0f;
        if (hasInput)
            targetSpeed = isRunning ? runSpeed : walkSpeed;

        // Làm mượt tốc độ di chuyển
        if (targetSpeed < 0.01f)
            targetSpeed = 0f;

        // Di chuyển
        controller.Move(move.normalized * targetSpeed * Time.deltaTime);


        // Gán vào Animator
        animator.SetFloat("Speed", targetSpeed, 0.1f, Time.deltaTime);
        animator.SetBool("isRunning", isRunning);
    }

    void HandleJump()
    {
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);

        // Nếu nhấn Space và đang chạm đất
        if (jumpPressed && isGrounded)
        {
            Debug.Log("Jump triggered");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
            animator.SetBool("IsJumping", true);
        }

        // Áp dụng trọng lực
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Khi rơi chạm đất → kết thúc nhảy
        if (isGrounded && isJumping)
        {
            Debug.Log("Landed, IsJumping = false");
            isJumping = false;
            animator.SetBool("IsJumping", false);
        }
    }
}
