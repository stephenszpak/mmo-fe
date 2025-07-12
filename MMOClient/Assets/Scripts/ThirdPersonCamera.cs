using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target; // CameraPivot
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float zoomSpeed = 2f;
    public float rotationSpeed = 120f;
    public float followSmooth = 10f;

    private float yaw;
    private float pitch;

    void Start()
    {
        if (target != null)
        {
            var angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleInput();
        Vector3 desiredPos = target.position - transform.forward * distance;

        if (Physics.Linecast(target.position, desiredPos, out RaycastHit hit))
        {
            desiredPos = hit.point;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followSmooth);
        transform.LookAt(target);
    }

    void HandleInput()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -40f, 80f);
        }
        else if (Input.GetMouseButton(0))
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        }

        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        transform.rotation = rot;
    }
}
