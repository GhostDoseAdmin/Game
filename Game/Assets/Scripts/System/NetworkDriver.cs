using Firesplash.UnityAssets.SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using UnityEngine.UIElements;


public class NetworkDriver : MonoBehaviour
{
    public SocketIOCommunicator sioCom;

    private float sync_timer = 0.0f;
    private float delay = 15f;//SYNC DELAY

    public bool HOST = false;
    private bool connected = false;
    private float pingTimer = 0.0f;
    private float PING = 0.0f;
    void Start()
    {
        Debug.Log("GIT IS FUCKING DOPE");
//=================================================================  S E T  U P  ===============================================================
        
        //-----------------CONNECT TO SERVER----------------->
        sioCom = GetComponent<SocketIOCommunicator>();
        StartCoroutine(connectSIO());
        sioCom.Instance.On("connect", (payload) =>
        {
            if (payload != null)
            {
                connected = true;
                Debug.Log(payload + " CONNECTING TO ROOM " + PlayerPrefs.GetString("room"));
                sioCom.Instance.Emit("join", PlayerPrefs.GetString("room"), true); //PlayerPrefs.GetString("room")
            }
        });
        IEnumerator connectSIO()//--------CONNECT HELPER--------->
        {
            while (!connected)
            {
                sioCom.Instance.Close();
                yield return new WaitForSeconds(1f); //refresh socket
                Debug.Log("attempting connection ");
                sioCom.Instance.Connect();
                yield return new WaitForSeconds(1f);
            }
        }

        //-----------------JOIN ROOM----------------->
        sioCom.Instance.On("join", (payload) =>
        {
            Debug.Log(payload);
            if (payload == "full"){SceneManager.LoadScene("Lobby");}
            else
            {
                var dict = new Dictionary<string, string> {
                    { "sid", sioCom.Instance.SocketID },
                    { "ping", PING.ToString() }
                };
                if (PING == 0) { pingTimer = Time.time; sioCom.Instance.Emit("ping", JsonConvert.SerializeObject(dict), false); Debug.Log("PINGING"); }
            }
        });

        //-----------------PING----------------->
        sioCom.Instance.On("pong", (payload) =>
        {
            Debug.Log("PONG RECEIVED " + payload);
            if (PING == 0) { PING = Time.time - pingTimer; Debug.Log("MY PING IS " + PING); }
            JObject data = JObject.Parse(payload);
            Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
            if (dict["sid"] != sioCom.Instance.SocketID)//OTHERS PING
            { //IF ITS THEIR PING
                Debug.Log("THEIR PING IS " + dict["ping"]);
                if (float.Parse(dict["ping"]) == 0) //THEY JUST JOINED
                {
                    Debug.Log("SENDING MY PING SPEED TO OTHER PLAYER");
                    dict = new Dictionary<string, string> {
                        { "sid", sioCom.Instance.SocketID },
                        { "ping", PING.ToString() }
                    };
                    sioCom.Instance.Emit("ping", JsonConvert.SerializeObject(dict), false);//MAKE OTHER TEST PINGS
                }
                else
                { // COMPARE PING VALUES
                    if (float.Parse(dict["ping"]) > PING) { Debug.Log("IM HOST"); sioCom.Instance.Emit("host", sioCom.Instance.SocketID, true); }
                    else { Debug.Log("THEYRE HOST"); sioCom.Instance.Emit("host", dict["sid"], true); }
                }
            }
        });
        //-----------------HOST / GAME START----------------->
        sioCom.Instance.On("host", (payload) =>
        {
            Debug.Log("HOST DETERMINED " + payload);
            if (payload.ToString() == sioCom.Instance.SocketID) { HOST = true; }
            //if (!HOST) { GameObject.Find("objPlayer").GetComponent<Rigidbody>().mass = 0; }
        });

//=================================================================E N D   S E T   U P ===============================================================
        //-----------------MOVE ----------------->
        sioCom.Instance.On("move", (payload) =>
        {
            JObject data = JObject.Parse(payload);
            Debug.Log("RECEIVING UPDATE " + data);
            Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
            GameObject thisObj = GameObject.Find(dict["object"]);
            Vector3 newPosition = new Vector3(float.Parse(dict["x"]), float.Parse(dict["y"]), float.Parse(dict["z"]));
            thisObj.GetComponent<MovementNetworker>().destination = newPosition; 

        });
        //-----------------JUMP ----------------->
        sioCom.Instance.On("JUPM", (payload) =>
        {
            JObject data = JObject.Parse(payload);
            Debug.Log("RECEIVING JUMP " + data);
            Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
            GameObject thisObj = GameObject.Find(dict["object"]);
            thisObj.GetComponent<Rigidbody>().AddForce(Vector3.up * float.Parse(dict["jump"]), ForceMode.VelocityChange);


        });
        //-----------------UPDATE POSITIONS----------------->
        sioCom.Instance.On("collide", (payload) =>
        {
            JObject data = JObject.Parse(payload);
            Debug.Log("COLLISION DATA " + data);
            Dictionary<string, Dictionary<string, string>> dict = data.ToObject<Dictionary<string, Dictionary<string, string>>>();

            foreach (KeyValuePair<string, Dictionary<string, string>> obj in dict)
            {
                GameObject thisObj = GameObject.Find(obj.Key);

                Vector3 newPosition = new Vector3(float.Parse(obj.Value["x"]), float.Parse(obj.Value["y"]), float.Parse(obj.Value["z"]));
                    obj.Value["collide"] = obj.Value["collide"].Replace("(", "").Replace(")", "");
                    string[] velocityArr = obj.Value["collide"].Split(',');
                    Vector3 velocity = new Vector3(float.Parse(velocityArr[0]), float.Parse(velocityArr[1]), float.Parse(velocityArr[2]));
                    thisObj.transform.position = newPosition;
                    thisObj.gameObject.GetComponent<Rigidbody>().AddForce(-velocity*1.5f, ForceMode.Impulse);
            }
        });
        //--------------------SYNC ROOM--------------------
        sioCom.Instance.On("sync", (payload) =>
        {
            JObject data = JObject.Parse(payload);
            Debug.Log("SYNCING " + data);
            Dictionary<string, Dictionary<string, string>> dict = data.ToObject<Dictionary<string, Dictionary<string, string>>>();
            // Log the object positions to the console
            foreach (KeyValuePair<string, Dictionary<string, string>> obj in dict)
            {
                GameObject thisObj = GameObject.Find(obj.Key);
                Vector3 newPosition = new Vector3(float.Parse(obj.Value["x"]), float.Parse(obj.Value["y"]), float.Parse(obj.Value["z"]));
                thisObj.transform.position = newPosition;
            }

        });
        //-----------------CREATE----------------->
        sioCom.Instance.On("create", (payload) =>
        {
            JObject data = JObject.Parse(payload);
            Debug.Log("RECEIVING CREATE " + data);
            Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
            Vector3 newPosition = new Vector3(float.Parse(dict["x"]), float.Parse(dict["y"]), float.Parse(dict["z"]));
            GameObject newObject = Instantiate(Resources.Load<GameObject>(dict["object"]), newPosition, Quaternion.identity);
            newObject.name = dict["name"];

        });

        //--------------DISCONNECT-----------------
        sioCom.Instance.On("disconnect", (payload) => { Debug.LogWarning("Disconnected: " + payload); });
        //--------------PLAYER DISCONNECT-----------------
        sioCom.Instance.On("player_disconnect", (payload) => { Debug.LogWarning("GAME ENDING PLAYER DISCONNECTED "); HOST = true; }); // sioCom.Instance.Close(); SceneManager.LoadScene("Lobby");
    }

    //-----------------------------SYNC UP EVERYTHING----------------------------
    public void Update()
    {
        
             if ((Time.time > sync_timer + delay) && HOST)
             {
                 Debug.Log("SYNCING ");
                 //Create a dictionary for this object's position in structure {"objPlayer":{"x":-2.17,"y":-0.01,"z":0.0},"objOtherPlayer":{"x":4.06,"y":-0.01,"z":0.0}}
                Dictionary<string, Dictionary<string, string>> objStates = new Dictionary<string, Dictionary<string, string>>();
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("sync"))
                {
                    //if (GetComponent<Rigidbody>().velocity.magnitude > 0.1f)//if its moving
                    {
                        string objName;
                        Dictionary<string, string> positionDict = new Dictionary<string, string>();

                        // Add the position values to the dictionary
                        positionDict.Add("x", obj.transform.position.x.ToString("F2"));
                        positionDict.Add("y", obj.transform.position.y.ToString("F2"));
                        positionDict.Add("z", obj.transform.position.z.ToString("F2"));

                        // Add the object's dictionary to the main dictionary with the object name as the key
                        objName = obj.name;
                        objStates.Add(objName, positionDict);
                    }
                }
                sioCom.Instance.Emit("sync", JsonConvert.SerializeObject(objStates), false);
                sync_timer = Time.time;
            }

    }

}
