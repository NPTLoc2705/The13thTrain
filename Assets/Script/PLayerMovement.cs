using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    Animator animator;
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;  // Reuse for sitting if desired
    public float crouchSpeed = 3f;   // Sitting speed
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Sitting input (R or Left Control)
        bool sitPressed = Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.LeftControl);
        animator.SetBool("isSitting", sitPressed);

        // Separate crouch input if needed (e.g., only R for crouch, Ctrl for pure sit without height change)
        bool crouchPressed = Input.GetKey(KeyCode.R); 

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButtonDown("Jump") && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
            animator.SetTrigger("Jump"); // use Trigger parameter
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Handle crouch/sit height adjustment (only on R for now; extend to sitPressed if needed)
        if (crouchPressed && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
            // Adjust camera position to match lowered height
            playerCamera.transform.localPosition = new Vector3(0, crouchHeight * 0.8f, 0);
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
            //  Reset camera
            playerCamera.transform.localPosition = new Vector3(0, defaultHeight * 0.8f, 0);
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            playerCamera.transform.position = transform.position + new Vector3(0, 2f, -6f);
            playerCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
        }
    }
}
