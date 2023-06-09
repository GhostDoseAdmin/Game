﻿using InteractionSystem;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using GameManager;
using UnityEngine.UIElements;

[RequireComponent(typeof(Camera_Controller))]
public class Camera_Supplyer : MonoBehaviour
{
    [Space]
    public bool cameraEnabled = true;
    
    [Space]
    public bool controllerEnabled = false;
    public bool controllerInvertY = true;
    public bool mouseInvertY = false;
    public bool lockMouseCursor = true;

    [Space]
    public float minDistance = 1;
    public float maxDistance = 5;

    [Space]
    public Vector2 mouseSensitivity = new Vector2(1.5f, 1.0f);
    public Vector2 controllerSensitivity = new Vector2(1.0f, 0.7f);
    private Vector2 mouseSensitivityStart;


    [Space]
    public float yAngleLimitMin = 0.0f;
    public float yAngleLimitMax = 180.0f;

    [HideInInspector]
    public float x;
    [HideInInspector]
    public float y;
    private float yAngle;
    private float angle;

    private string rightAxisXName;
    private string rightAxisYName;

    private Vector3 upVector;
    private Vector3 downVector;

    private bool smartPivotInit;

    private float smoothX;
    private float smoothY;
    public bool smoothing = false;
    public float smoothSpeed = 3.0f;

    public bool forceCharacterDirection = false;

    private Camera_Controller cameraController;
    private RectTransform gamePad;

    private PlayerController Player;
    Quaternion currentRotation;
    Vector3 spinDirection;
    public bool CAMFIX;
    public void Start()
    {
        Player = GameDriver.instance.Player.GetComponent<PlayerController>();
        if (NetworkDriver.instance.isMobile) { gamePad = GameObject.Find("GamePad").GetComponent<RectTransform>(); }
        cameraController = GetComponent<Camera_Controller>();

        //targetRotation = Quaternion.identity;
        mouseSensitivityStart = mouseSensitivity;
        x = 0;
        y = 0;

        smartPivotInit = true;

        upVector = Vector3.up;
        downVector = Vector3.down;

        string platform = Application.platform.ToString().ToLower();

        if (platform.Contains("windows") || platform.Contains("linux"))
        {
            rightAxisXName = "Right_4";
            rightAxisYName = "Right_5";
        }
        else
        {
            rightAxisXName = "Right_3";
            rightAxisYName = "Right_4";
        }

        // test if the controller axis are setup
        try
        {
            Input.GetAxis(rightAxisXName);
            Input.GetAxis(rightAxisYName);
        }
        catch
        {
            controllerEnabled = false;
        }
    }

    void ReEnableCam()
    {
        CancelInvoke("ReEnableCam");
        cameraEnabled = true;
    }

    

    public void Update()
    {

        float facingForwardFromCam = Vector3.Dot(GameDriver.instance.Player.transform.forward, Camera.main.transform.forward);


        if (cameraController == null || cameraController.player == null)
            return;

        //DISABLE CAM for GAMEPAD AREA OF SCREEN 
        /*if (Input.touchCount > 0 && NetworkDriver.instance.isMobile)
        {
            // Iterate through each touch
            foreach (Touch touch in Input.touches)
            {
                // Check if the touch is in the lower right quadrant
                //if (touch.position.x >= Screen.width / 2f && touch.position.y <= Screen.height / 2f)
                    if (RectTransformUtility.RectangleContainsScreenPoint(gamePad, touch.position))
                    {
                    cameraEnabled = false;
                    Invoke("ReEnableCam", 0.1f);
                }
            }
        }*/


        if (cameraEnabled)
        {
            mouseSensitivity = mouseSensitivityStart;
            x = 0; y = 0;

            if (NetworkDriver.instance.isMobile)
            {
                cameraController.offsetVector = new Vector3(0f, 2f, 0f);
                if (!CAMFIX)
                {
                    //MOVE JOY
                    if (!Player.gamePad.aimer.AIMING)
                    {
                        if (Player.gamePad.joystick.Horizontal != 0 || Player.gamePad.joystick.Vertical != 0)
                        {
                            //cameraController.offsetVector = new Vector3(0f, 1.4f, 0f);
                            smoothing = true;
                            yAngleLimitMin = 160f; yAngleLimitMax = 110f;
                            cameraController.desiredDistance = 7f;
                            //move targlook in position of joystick relative to cam
                            Player.targetPos.position = Player.transform.position + (Camera.main.transform.forward * Player.gamePad.joystick.Vertical + Camera.main.transform.right * Player.gamePad.joystick.Horizontal).normalized * 5f;
                        }

                    }
                    //AIMER JOY
                    if (Player.gamePad.joystickAim.Horizontal != 0 || Player.gamePad.joystickAim.Vertical != 0)
                    {
                        
                        yAngleLimitMin = 110; yAngleLimitMax = 110;
                        cameraController.desiredDistance = 5f;
                        mouseSensitivity.x = 5f; mouseSensitivity.y = 1f;
                        float magnitude = Mathf.Sqrt(Mathf.Pow(Player.gamePad.joystickAim.Vertical, 2) + Mathf.Pow(Player.gamePad.joystickAim.Horizontal, 2));
                        smoothing = false;
                        //Sticky scope
                        if (Player.gamePad.aimer.RayTarget != null) { mouseSensitivity.x = 1f; mouseSensitivity.y = 0.2f; }
                        x = Player.gamePad.joystickAim.Horizontal * mouseSensitivity.x;
                        y = Player.gamePad.joystickAim.Vertical * mouseSensitivity.y;
                        if (magnitude > 0.9f) { x = x * 2; }

                        /*if (Player.GetComponent<ShootingSystem>().target != null)
                        {
                            Player.targetPos.position = Player.GetComponent<ShootingSystem>().target.transform.position + Vector3.up;
                        }*/
                    }
                    //AIMER JOY HELD
                    if (Player.gamePad.aimer.AIMING)
                    {
                        if (facingForwardFromCam < 0.6f)
                        { //facing away from cam
                            if (!CAMFIX) { CAMFIX = true; currentRotation = Player.transform.rotation; spinDirection = Vector3.Cross(GameDriver.instance.Player.transform.forward, Camera.main.transform.forward); return; }
                        }
                        Player.targetPos.position = (Player.transform.position + Camera.main.transform.forward * 5f) + Vector3.up;//move targlook to forward cam
                    }
                }
            }

            //PC
            else
            {
                x = Input.GetAxis("Mouse X") * mouseSensitivity.x;
                y = Input.GetAxis("Mouse Y") * mouseSensitivity.y;
            }

            //CAM FIX
            if(CAMFIX)
            {
                if (facingForwardFromCam <= 0.90f)//rotate cam
                {
                    Player.gamePad.aimer.fov = Player.gamePad.aimer.startFov;
                    if (spinDirection.y > 0) { x = -10; } else { x = 10; }
                    Player.transform.rotation = currentRotation; Player.targetPos.position = Player.transform.position + Player.transform.forward * 3;
                }
                else { CAMFIX = false; } //cam fixed
            }

            

            if (mouseInvertY)
                y *= -1.0f;

            if (lockMouseCursor)
            {
                UnityEngine.Cursor.visible = false;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            }

            if (controllerEnabled && x == 0 && y == 0)
            {
                x = Input.GetAxis(rightAxisXName) * controllerSensitivity.x;
                y = Input.GetAxis(rightAxisYName) * controllerSensitivity.y;

                if (controllerInvertY)
                    y *= -1.0f;
            }
            if (!NetworkDriver.instance.isMobile)
            {
                if (Input.GetAxis("Mouse ScrollWheel") < 0) // back
                {
                    cameraController.desiredDistance += cameraController.zoomOutStepValue;

                    if (cameraController.desiredDistance > maxDistance)
                        cameraController.desiredDistance = maxDistance;
                }
                if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
                {
                    cameraController.desiredDistance -= cameraController.zoomOutStepValue;

                    if (cameraController.desiredDistance < minDistance)
                        cameraController.desiredDistance = minDistance;
                }
            }

            if (cameraController.desiredDistance < 0)
                cameraController.desiredDistance = 0;

            if (smoothing)
            {
                smoothX = Mathf.Lerp(smoothX, x, Time.deltaTime * smoothSpeed);
                smoothY = Mathf.Lerp(smoothY, y, Time.deltaTime * smoothSpeed);
            }
            else
            {
                smoothX = x;
                smoothY = y;
            }
            
            Vector3 offsetVectorTransformed = cameraController.player.transform.rotation * cameraController.offsetVector;

            transform.RotateAround(cameraController.player.position + offsetVectorTransformed, cameraController.player.up, smoothX);



            yAngle = -smoothY;
            angle = Vector3.Angle(transform.forward, upVector);

            if (angle <= yAngleLimitMin && yAngle < 0)
            {
                yAngle = 0;
            }
            if (angle >= yAngleLimitMax && yAngle > 0)
            {
                yAngle = 0;
            }

            if (yAngle > 0)
            {
                if (angle + yAngle > 180.0f)
                {
                    yAngle = Vector3.Angle(transform.forward, upVector) - 180;

                    if (yAngle < 0)
                        yAngle = 0;
                }
            }
            else
            {
                if (angle + yAngle < 0.0f)
                {
                    yAngle = Vector3.Angle(transform.forward, downVector) - 180;
                }
            }                               

            if (!cameraController.smartPivot || cameraController.cameraNormalMode
                && (!cameraController.bGroundHit || (cameraController.bGroundHit && y < 0) || transform.position.y > (cameraController.player.position.y + cameraController.offsetVector.y)))
            {
                transform.RotateAround(cameraController.player.position + offsetVectorTransformed, transform.right, yAngle);
            }
            else
            {
                if (smartPivotInit)
                {
                    smartPivotInit = false;
                    cameraController.InitSmartPivot();
                }

                transform.RotateAround(transform.position, transform.right, yAngle);

                if (transform.rotation.eulerAngles.x > cameraController.startingY || (transform.rotation.eulerAngles.x >= 0 && transform.rotation.eulerAngles.x < 90))
                {
                    smartPivotInit = true;

                    cameraController.DisableSmartPivot();
                }
            }

            if (forceCharacterDirection)
            {
                cameraController.player.rotation = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up);
            }
        }
    }
}

