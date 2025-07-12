using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkedEntity
{
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public Transform cameraPivot;

    private CharacterController controller;
    private Vector3 velocity;
    private float gravity = -9.81f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
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

        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
