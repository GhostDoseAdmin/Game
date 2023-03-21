using UnityEngine;

public class CameraTPV : MonoBehaviour
{

    public Transform target; // Target object to follow
    public float distance = 10.0f; // Distance between camera and target
    public float height = 5.0f; // Height of camera relative to target
    public float rotationSpeed = 1.0f; // Speed of camera rotation
    public float zoomSpeed = 10.0f; // Speed of zooming

    private float currentDistance; // Current distance between camera and target
    private float desiredDistance; // Desired distance between camera and target
    private Vector3 targetPosition; // Position of target
    private float xRotation = 0.0f; // Current rotation around x-axis

    void Start()
    {
        // Initialize distance variables
        currentDistance = distance;
        desiredDistance = distance;

        // Set target position to target object's position
        targetPosition = target.position;
    }


    void LateUpdate()
    {
        // Use mouse input to rotate camera around target
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80.0f, 80.0f);

        // Calculate the offset position based on target position, distance, height, and shoulder offset
        Vector3 shoulderOffset = new Vector3(-1.0f, 0.0f, 1.0f); // Example shoulder offset
        Vector3 offset = shoulderOffset.normalized * shoulderOffset.magnitude;
        Vector3 targetPos = target.position + offset;
        Quaternion rotation = Quaternion.Euler(xRotation, transform.eulerAngles.y + mouseX, 0);
        Vector3 position = targetPos - (rotation * Vector3.forward * currentDistance);
        position = new Vector3(position.x, targetPos.y + height, position.z);

        // Update camera position and rotation
        transform.position = position;
        transform.rotation = rotation;

        // Use scroll wheel to zoom camera in and out
        desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
        desiredDistance = Mathf.Clamp(desiredDistance, 0.5f, 20.0f);
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomSpeed);
    }
}