using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkedEntity
{
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public Transform cameraPivot;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private float gravity = -9.81f;
    private float animSpeed = 0f;

    private Vector3 lastPosition;
    private UdpMovementSender udpSender;
    private bool jumpQueued = false;
    private float jumpQueueTimer = 0f;
    private const float jumpQueueTime = 0.2f; // seconds to buffer jump input

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning("PlayerController could not find an Animator component on the GameObject.");
        else
            Debug.Log($"Animator found: {animator.runtimeAnimatorController?.name}");
        udpSender = GetComponent<UdpMovementSender>();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpQueued = true;
            jumpQueueTimer = jumpQueueTime;
        }

        if (jumpQueued)
        {
            jumpQueueTimer -= Time.deltaTime;
            if (jumpQueueTimer <= 0f)
                jumpQueued = false;
        }

        HandleMovement();
        UpdateAnimator();
    }

    void HandleMovement()
    {
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        bool bothMouse = Input.GetMouseButton(0) && Input.GetMouseButton(1);
        if (bothMouse)
        {
            vert = 1f; // walk forward
        }

        Vector3 moveDir = new Vector3(horiz, 0, vert);
        if (moveDir.magnitude > 1f)
            moveDir = moveDir.normalized;

        // Movement relative to camera
        if (Camera.main != null)
        {
            var cam = Camera.main.transform;
            Vector3 forward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 right = cam.right;
            moveDir = forward * moveDir.z + right * moveDir.x;
        }

        // Apply movement
        Vector3 movement = moveDir * moveSpeed * Time.deltaTime;
        controller.Move(movement);

        // Apply gravity and jumping
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (jumpQueued && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpQueued = false;
            if (animator != null)
            {
                animator.SetBool("isJumping", true);
                Debug.Log("Jump triggered - setting isJumping true");
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 🔁 Send UDP packet if position changed
        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;

        if (udpSender != null && delta.sqrMagnitude > 0.0001f)
        {
            udpSender.SendMovement(delta);
            lastPosition = currentPosition;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null)
            return;

        Vector3 horizVel = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        float targetSpeed = horizVel.magnitude;
        animSpeed = Mathf.Lerp(animSpeed, targetSpeed, Time.deltaTime * 10f);
        animator.SetFloat("speed", animSpeed);
        Debug.Log($"Set speed parameter to {animSpeed}");

        if (controller.isGrounded)
        {
            animator.SetBool("isJumping", false);
            Debug.Log("Grounded - setting isJumping false");
        }
        else
        {
            animator.SetBool("isJumping", true);
            Debug.Log("Airborne - setting isJumping true");
        }
    }
}
