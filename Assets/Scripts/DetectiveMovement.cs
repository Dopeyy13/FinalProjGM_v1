
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DetectiveMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2.2f;
    public float runSpeed = 4f;
    public float gravity = -15f;

    [Header("Camera")]
    public Transform cameraTransform;

    private CharacterController controller;
    private float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 moveDirection = input;

        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            moveDirection =
                forward * input.z + right * input.x;

            moveDirection = Vector3.ClampMagnitude(
                moveDirection, 1f
            );
        }

        float speed = Input.GetKey(KeyCode.LeftShift)
            ? runSpeed
            : walkSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity =
            moveDirection * speed +
            Vector3.up * verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        // ვატრიალებთ მხოლოდ ჰორიზონტალურად — Y ღერძის გარშემო.
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
            Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Euler(
                -90f,
                targetRotation.eulerAngles.y,
                0f
            );
        }
    }
}
