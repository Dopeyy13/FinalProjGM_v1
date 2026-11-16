
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 4.5f;
    public float turnSmoothTime = 0.25f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.2f;
    public float gravity = -20f;
    public float coyoteTime = 0.15f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 1.5f;
    public float cameraDistance = 3.5f;
    public float cameraHeight = 1.5f;

    private CharacterController controller;
    private Animator animator;

    private float turnVelocity;
    private float verticalVelocity;
    private float groundTimer;

    private float yaw;
    private float pitch = 15f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        yaw = transform.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // MOUSE
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -25f, 60f);
        }

        // MOVEMENT INPUT
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        input = Vector3.ClampMagnitude(input, 1f);

        bool moving = input.sqrMagnitude > 0.01f;
        bool running = moving && Input.GetKey(KeyCode.LeftShift);

        float speed = running ? runSpeed : walkSpeed;

        Vector3 movement = Vector3.zero;

        // SMOOTH ROTATION
        if (moving)
        {
            Vector3 direction =
                Quaternion.Euler(0f, yaw, 0f) * input;

            float targetAngle =
                Mathf.Atan2(direction.x, direction.z)
                * Mathf.Rad2Deg;

            float smoothAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnVelocity,
                turnSmoothTime
            );

            transform.rotation =
                Quaternion.Euler(0f, smoothAngle, 0f);

            movement = direction.normalized * speed;
        }

        // GROUND CHECK
        if (controller.isGrounded)
        {
            groundTimer = coyoteTime;

            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
        }
        else
        {
            groundTimer -= Time.deltaTime;
        }

        // JUMP - SPACE
        if (Input.GetKeyDown(KeyCode.Space) && groundTimer > 0f)
        {
            verticalVelocity = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );

            groundTimer = 0f;
        }

        // GRAVITY
        verticalVelocity += gravity * Time.deltaTime;

        movement.y = verticalVelocity;

        // MOVE PLAYER
        CollisionFlags flags =
            controller.Move(movement * Time.deltaTime);

        if ((flags & CollisionFlags.Above) != 0 &&
            verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }

        // ANIMATIONS

        if (animator != null)
        {
            animator.SetBool("IsJumping", !controller.isGrounded);

            float animationSpeed = 0f;

            if (moving)
            {
                animationSpeed = running ? 1f : 0.5f;
            }

            animator.SetFloat(
                "Speed",
                animationSpeed,
                0.15f,
                Time.deltaTime
            );
        }

    
}

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Vector3 target =
            transform.position + Vector3.up * cameraHeight;

        Quaternion rotation =
            Quaternion.Euler(pitch, yaw, 0f);

        cameraTransform.position =
            target - rotation * Vector3.forward * cameraDistance;

        cameraTransform.rotation = rotation;
    }
}
