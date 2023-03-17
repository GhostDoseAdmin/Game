using Firesplash.UnityAssets.SocketIO;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementNetworker : MonoBehaviour
{
    private NetworkDriver ND;
    public Vector3 destination;
    public float speed; //DEFINED IN INSPECTOR
    
    public void Start()
    {
        ND = GameObject.Find("NetworkDriver").GetComponent<NetworkDriver>();
        if (this.name.Contains("objOtherPlayer")) { speed = GameObject.Find("objPlayer").GetComponent<PlayerController>().speed;  }
        
    }
    public void FixedUpdate()
    {

        
        //MOVE TO DESTINATION
        Vector3 newPosition = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        transform.position = newPosition;
    }
    public void moveEmit(GameObject thisObj, Vector3 newDestination, bool onlyHost)
    {

        if (onlyHost && ND.HOST || (!onlyHost))
        {
            if (!this.name.Contains("objPlayer"))//DONT MOVE LOCAL PLAYER
            {
                destination = newDestination;//SET DESTINATION LOCALLY
            }

            string dict = $"{{'object':'{thisObj.name}',x:{newDestination.x.ToString("F2")},y:{newDestination.y.ToString("F2")},z:{newDestination.z.ToString("F2")}}}";
            ND.sioCom.Instance.Emit("move", JsonConvert.SerializeObject(dict), false);
        }

    }


}