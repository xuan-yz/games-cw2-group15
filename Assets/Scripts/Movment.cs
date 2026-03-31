using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform model;
    public Transform cameraTransform;

    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float gravity = -20f;

    [Header("Jump Settings")]
    public float jumpHeight = 2f;

    private CharacterController controller;
    private Vector3 moveDir;
    private Vector3 velocity; 

    public bool IsGrounded => controller.isGrounded;


    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleInput();
        ApplyGravity();
        ApplyMovement();
    }

    private void HandleInput()
    { 
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;

        moveDir = (forward.normalized * v + right.normalized * h).normalized;

        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        if (IsGrounded)
        {
            //Debug.LogError("grounded");
        }
        else {
            //Debug.LogError("not grounded");
        }
    }

    private void ApplyGravity()
    {
        if (IsGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
    }

    private void ApplyMovement()
    {
        Vector3 Movement = (moveDir * moveSpeed) + velocity;
        controller.Move(Movement * Time.deltaTime);
    }
}