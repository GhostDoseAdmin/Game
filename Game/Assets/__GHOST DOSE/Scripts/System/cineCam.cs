using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class cineCam : MonoBehaviour
{
    public float moveSpeed = 5.0f; // Movement speed
    public float sensitivity = 2.0f; // Mouse sensitivity
    private Vector2 currentRotation = Vector2.zero;
    public bool lockPos = false;
    void Update()
    {
        if (!lockPos)
        {
            // Camera Rotation (Mouse Input)
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            currentRotation.x += mouseX * sensitivity;
            currentRotation.y -= mouseY * sensitivity;

            currentRotation.x = Mathf.Clamp(currentRotation.x, -360, 360);

            transform.localRotation = Quaternion.AngleAxis(currentRotation.x, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(currentRotation.y, Vector3.right);

            // Camera Movement (WASD Input)
            float horizontalMovement = Input.GetAxis("Horizontal");
            float verticalMovement = Input.GetAxis("Vertical");

            Vector3 moveDirection = new Vector3(horizontalMovement, 0, verticalMovement).normalized;
            moveDirection = transform.TransformDirection(moveDirection);

            transform.position += moveDirection * moveSpeed * Time.deltaTime;


        }


    }
}
