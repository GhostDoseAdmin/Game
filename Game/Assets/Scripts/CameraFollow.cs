using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform objectToFollow;
    public float followSpeed = 10.0f;
    public Vector3 offset;

    void LateUpdate()
    {
        // Calculate the target position for the camera
        Vector3 targetPosition = objectToFollow.position + offset;

        // Use the SmoothDamp method to move the camera smoothly
        Vector3 currentPosition = transform.position;
        Vector3 currentVelocity = Vector3.zero;
        transform.position = Vector3.SmoothDamp(currentPosition, targetPosition, ref currentVelocity, 1 / followSpeed);

        // Rotate the camera to match the rotation of the object
        //transform.rotation = objectToFollow.rotation;
    }
}