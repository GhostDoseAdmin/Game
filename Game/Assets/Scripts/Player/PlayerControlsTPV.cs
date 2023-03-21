using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using Newtonsoft.Json;
using Firesplash.UnityAssets.SocketIO;
using Unity.VisualScripting;

public class PlayerControlsTPV : MonoBehaviour
{


    public float moveSpeed = 5.0f; // Speed of movement
    public float rotationSpeed = 10.0f; // Speed of rotation
    public Transform cameraTransform; // Reference to the camera transform

    private Vector3 moveDirection; // Direction of movement
    private void Start()
    {
        GetComponent<Rigidbody>().transform.position = new Vector3(-2, 6, 0);
    }

    public void FixedUpdate()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.up * -5f, ForceMode.VelocityChange);//gravity


        // Get input from WASD keys
        // Get input from WASD keys
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate move direction based on camera rotation
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        // Set move direction based on input and camera rotation
        moveDirection = (cameraForward * vertical + cameraRight * horizontal);

        // Normalize move direction to avoid faster diagonal movement
        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize();
        }

        // Get a reference to the Rigidbody component
        Rigidbody rb = GetComponent<Rigidbody>();

        // Calculate velocity based on move direction and speed
        Vector3 velocity = moveDirection * moveSpeed;

        // Apply the velocity to the Rigidbody
        rb.velocity = velocity;

        // Rotate player to face the direction of movement
        if (moveDirection.magnitude > 0.01f)
        {
            Quaternion newRotation = Quaternion.LookRotation(moveDirection);
            newRotation *= Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
        }
    }


}


