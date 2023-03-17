using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using Newtonsoft.Json;
using Firesplash.UnityAssets.SocketIO;

public class CollisionNetworker : MonoBehaviour
{

    private NetworkDriver ND;
    private float col_timer = 0.0f;
    private float col_delay = 0.75f;


    private void Start()
    {
        //Physics.IgnoreCollision(GameObject.Find("objOtherPLayer").GetComponent<Collider>(), GetComponent<Collider>());
        ND = GameObject.Find("NetworkDriver").GetComponent<NetworkDriver>();
    }

    void OnCollisionEnter(Collision collision)
    {
        //if (ND.HOST)
        {
            if (Time.time > col_timer + col_delay)
            {
                Debug.Log("COLLIDED " + collision.gameObject.tag);
                if (collision.gameObject.GetComponent<Rigidbody>() && !collision.gameObject.tag.Contains("Player")) //collision.gameObject == other && 
                {

                    // Calculate the collision normal
                    Vector3 collisionNormal = collision.contacts[0].normal;
                    collision.gameObject.GetComponent<Rigidbody>().AddForce(-collisionNormal*1.5f, ForceMode.Impulse);
                    

                    // EMIT COLLISION
                    Dictionary<string, Dictionary<string, string>> objStates = new Dictionary<string, Dictionary<string, string>>();
                    Dictionary<string, string> positionDict = new Dictionary<string, string>();
                    positionDict.Add("collide", collisionNormal.ToString());
                    positionDict.Add("x", collision.gameObject.transform.position.x.ToString());
                    positionDict.Add("y", collision.gameObject.transform.position.y.ToString());
                    positionDict.Add("z", collision.gameObject.transform.position.z.ToString());
                    // Add the object's dictionary to the main dictionary with the object name as the key
                    string objName = collision.gameObject.name;
                    objStates.Add(objName, positionDict);
                    ND.sioCom.Instance.Emit("collide", JsonConvert.SerializeObject(objStates), false);

                }
                col_timer = Time.time;
            }
        }
    }


}