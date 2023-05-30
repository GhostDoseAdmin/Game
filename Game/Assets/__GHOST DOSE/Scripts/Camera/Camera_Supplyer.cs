using InteractionSystem;
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


    public void Start()
    {
        gamePad = GameObject.Find("GamePad").GetComponent<RectTransform>();
        cameraController = GetComponent<Camera_Controller>();
        //targetRotation = Quaternion.identity;

        if (NetworkDriver.instance.isMobile) { yAngleLimitMin = 160f; yAngleLimitMax = 110f; }

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
    public void Update()
    {
        /*if (NetworkDriver.instance.isMobile)
        {
            Vector3 playerDirection = GameDriver.instance.Player.transform.forward;
            Vector3 cameraDirection = Camera.main.transform.forward;
            // Check if the player is facing the camera
            bool isFacingCamera = Vector3.Dot(playerDirection, cameraDirection) <= 0.2f;  // Adjust the threshold as needed

            // If the player is facing the camera, rotate the camera around to see what the player is looking at
            if (isFacingCamera)
            {
                if(!fixCamDir) {
                    fixCamDir = true;
                    Translate camera behind player, stop target look from tracking
                   // targetRotation = Quaternion.LookRotation(GameDriver.instance.Player.transform.position - GameDriver.instance.Player.GetComponent<PlayerController>().targetPos.position, Vector3.up);
                    Debug.Log("FACING CAMERA");
                    cameraEnabled = false;
                }
                if (fixCamDir)
                {
                    Debug.Log("FIXING CAMERA");
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
                }
            }
            else
            {
                if (fixCamDir)
                {
                    Debug.Log("CAMERA FIXED");
                    fixCamDir = false;
                    cameraEnabled = true;
                }
            }
 

        }*/


        //DISABLE CAM for GAMEPAD AREA OF SCREEN 
        if (Input.touchCount > 0 && NetworkDriver.instance.isMobile)
        {
            // Iterate through each touch
            foreach (Touch touch in Input.touches)
            {
                // Check if the touch is in the lower right quadrant
                if (RectTransformUtility.RectangleContainsScreenPoint(gamePad, touch.position))
                {
                    cameraEnabled = false;
                    Invoke("ReEnableCam", 0.1f);
                }
            }
        }
    }
    void ReEnableCam()
    {
        CancelInvoke("ReEnableCam");
        cameraEnabled = true;
    }

    public void LateUpdate()
    {
        if (cameraController == null || cameraController.player == null)
            return;

        //DISABLE CAM for GAMEPAD AREA OF SCREEN 
        if (Input.touchCount > 0 && NetworkDriver.instance.isMobile)
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
        }


        if (cameraEnabled)
        {

            x = Input.GetAxis("Mouse X") * mouseSensitivity.x;
            y = Input.GetAxis("Mouse Y") * mouseSensitivity.y;

            if (NetworkDriver.instance.isMobile)
            {
                if (GameDriver.instance.Player.GetComponent<PlayerController>().joystick.Horizontal != 0 || GameDriver.instance.Player.GetComponent<PlayerController>().joystick.Vertical != 0) {
                    x = GameDriver.instance.Player.GetComponent<PlayerController>().joystick.Horizontal * mouseSensitivity.x;
                    y = GameDriver.instance.Player.GetComponent<PlayerController>().joystick.Vertical * mouseSensitivity.y;
                }
               
                

            }
            //Debug.Log(x.ToString() + " AND " + y.ToString());

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

