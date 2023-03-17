using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using Newtonsoft.Json;
using Firesplash.UnityAssets.SocketIO;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    
    [SerializeField] private InputAction playerControls;
    [SerializeField] private InputAction JumpControl;

    private Vector2 moveDirection;
    private float move_timer = 0.0f;
    private float move_delay = 0.1f;//0.1
    private bool wasMoving;
    public float speed = 10.0f;

    private float jump_timer = 0.0f;
    private float jump_delay = 1.0f;//0.1
    private float jump_force = 90f;
    private float gravity = -5f;//-15

    private NetworkDriver ND;
    private Vector3 previousPosition;
    


    private void Start()    { GetComponent<Rigidbody>().transform.position = new Vector3(-2, 6, 0);      JumpControl.Enable();   ND = GameObject.Find("NetworkDriver").GetComponent<NetworkDriver>(); }    
    private void OnEnable()    {        JumpControl.Enable();        playerControls.Enable();    }
    private void OnDisable()    {        JumpControl.Disable();        playerControls.Disable();     }

    /*public void Jump()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.up * jump_force, ForceMode.VelocityChange);
    }*/


    public void FixedUpdate()
    {

        //GetComponent<Rigidbody>().mass = 0f;
        GetComponent<Rigidbody>().AddForce(Vector3.up * gravity, ForceMode.VelocityChange);//gravity
        //GetComponent<Rigidbody>().AddForce(Vector3.up * gravity, ForceMode.VelocityChange);//gravity

        //---------------------------JUMP---------------------------------
        if (JumpControl.triggered){
            if (Time.time > jump_timer + jump_delay)
            {
                GetComponent<Rigidbody>().AddForce(Vector3.up * jump_force, ForceMode.VelocityChange);
                string dict = $"{{'object':'objOtherPlayer','jump':{jump_force.ToString("F2")}}}";
                ND.sioCom.Instance.Emit("jump", JsonConvert.SerializeObject(dict), false);
                jump_timer = Time.time;
            }
        }






        //-------------------------MOVEMENT--------------------------------
                moveDirection = playerControls.ReadValue<Vector2>();
                Vector3 velocity = new Vector3(moveDirection.x * speed, 0f, moveDirection.y * speed);
                GetComponent<Rigidbody>().velocity = velocity;

                if (transform.position != previousPosition)  
                {
                    if (Time.time > move_timer + move_delay)
                    {

                        GetComponent<MovementNetworker>().moveEmit(GameObject.Find("objOtherPlayer"), transform.position, false);
                        move_timer = Time.time;
                    }
                }
                previousPosition = transform.position;


                if (moveDirection.magnitude > 0) { wasMoving = true; }
                else
                {
                    if (wasMoving)
                    {
                        if (Time.time > move_timer + move_delay)
                        {
                            wasMoving = false;
                            GetComponent<MovementNetworker>().moveEmit(GameObject.Find("objOtherPlayer"), transform.position, false);
                            move_timer = Time.time;
                        }
                    }
                }


            
        
          

    }


}



