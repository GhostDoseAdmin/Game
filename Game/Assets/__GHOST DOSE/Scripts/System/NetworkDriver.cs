using Firesplash.UnityAssets.SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GameManager;

namespace NetworkSystem
{

    public class NetworkDriver : MonoBehaviour
    {
        public static NetworkDriver instance;

        public SocketIOCommunicator sioCom;

        //private float sync_timer = 0.0f;
        //private float delay = 15f;//SYNC DELAY

        public bool HOST = true;
        public bool connected = false;
        private float pingTimer = 0.0f;
        private float PING = 0.0f;

        public void Awake() { instance = this; }

        public void NetworkSetup()
        {
            //=================================================================  S E T  U P  ===============================================================

            Debug.Log("SETTING UP NETWORK");

            sioCom = gameObject.AddComponent<SocketIOCommunicator>();
            sioCom.secureConnection = true;

            //-----------------CONNECT TO SERVER----------------->
            sioCom = GetComponent<SocketIOCommunicator>();
            StartCoroutine(connectSIO());
            sioCom.Instance.On("connect", (payload) =>
            {
                if (payload != null)
                {
                    connected = true;
                    GameDriver.instance.MSG = "Checking Room " + GameDriver.instance.ROOM;
                    //Debug.Log(payload + " CONNECTING TO ROOM " + PlayerPrefs.GetString("room"));
                    sioCom.Instance.Emit("join", GameDriver.instance.ROOM, true); //PlayerPrefs.GetString("room")
                }
            });
            IEnumerator connectSIO()//--------CONNECT HELPER--------->
            {
                while (!connected)
                {
                    GameDriver.instance.MSG = "Attempting to connect to Ghost Servers";
                    sioCom.Instance.Close();
                    yield return new WaitForSeconds(1f); //refresh socket
                    Debug.Log("attempting connection ");
                    sioCom.Instance.Connect("https://twrecks.io:8080", true);
                    yield return new WaitForSeconds(1f);
                }
            }

            //-----------------JOIN ROOM----------------->
            sioCom.Instance.On("join", (payload) =>
            {
                GameDriver.instance.ROOM_VALID = false;
                Debug.Log(payload);
                if (payload == "full")
                {
                    GameDriver.instance.MSG = "Room is full!";
                    sioCom.Instance.Close();
                }
                else
                {
                    GameDriver.instance.ROOM_VALID = true;
                    GameDriver.instance.MSG = "Found Room " + GameDriver.instance.ROOM;
                    var dict = new Dictionary<string, string> {
                    { "sid", sioCom.Instance.SocketID },
                    { "ping", PING.ToString() }
                    };
                    if (PING == 0)
                    {
                        pingTimer = Time.time; sioCom.Instance.Emit("ping", JsonConvert.SerializeObject(dict), false); Debug.Log("PINGING");
                    }
                }
            });

            //-----------------PING----------------->
            sioCom.Instance.On("pong", (payload) =>
            {
                GameDriver.instance.MSG = "Looking For Players - you may start alone";
                Debug.Log("PONG RECEIVED " + payload);
                if (PING == 0) { PING = Time.time - pingTimer; Debug.Log("MY PING IS " + PING); }
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                if (dict["sid"] != sioCom.Instance.SocketID)//OTHERS PING
                { //IF ITS THEIR PING
                    //Debug.Log("THEIR PING IS " + dict["ping"]);
                    if (float.Parse(dict["ping"]) == 0) //THEY JUST JOINED
                    {
                        GameDriver.instance.MSG = "Player Joined";
                        //Debug.Log("PLAYER JOINED SENDING MY PING SPEED TO OTHER PLAYER");
                        GameDriver.instance.twoPlayer = true;

                        dict = new Dictionary<string, string> {
                        { "sid", sioCom.Instance.SocketID },
                        { "ping", PING.ToString() }
                        };
                        sioCom.Instance.Emit("ping", JsonConvert.SerializeObject(dict), false);//MAKE OTHER TEST PINGS
                    }
                    else//they were in room first
                    {
                        GameDriver.instance.twoPlayer = true;
                        // COMPARE PING VALUES
                        if (float.Parse(dict["ping"]) > PING) { Debug.Log("IM HOST"); sioCom.Instance.Emit("host", sioCom.Instance.SocketID, true); }
                        else { Debug.Log("THEYRE HOST"); if (!GameDriver.instance.NETWORK_TEST) { HOST = false; } sioCom.Instance.Emit("host", dict["sid"], true); }
                    }
                }
            });
            //-----------------HOST / GAME START / UPDATE GAME STATES----------------->
            sioCom.Instance.On("host", (payload) =>
            {
                //GameDriver.instance.MSG = "Two Player Mode - HOST " + payload + "     MY SOCKET    " + sioCom.Instance.SocketID;
                Debug.Log("HOST DETERMINED " + payload);
                if (payload.ToString() != sioCom.Instance.SocketID) { if (!GameDriver.instance.NETWORK_TEST) { HOST = false; } }
                else { UpdateEnemies(); }
                GameDriver.instance.Player.GetComponent<PlayerController>().emitFlashlight = true;
                GameDriver.instance.Player.GetComponent<PlayerController>().emitGear = true;
                GameDriver.instance.Player.GetComponent<PlayerController>().emitPos = true;//triggers position emit
                GameDriver.instance.MSG = "Two Player Mode - HOST " + HOST;

            });
            //-----------------CHOOSE BRO----------------->
            sioCom.Instance.On("bro", (payload) =>
            {
                // Debug.Log(" RECEIVED BRO " + payload);
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                GetComponent<LobbyControl>().otherBro = dict["bro"];
                // GetComponent<GameDriver>().otherBroRig = dict["rig"];
                GetComponent<LobbyControl>().otherSelects = true;
                GetComponent<LobbyControl>().otherIndex = int.Parse(dict["index"]);
                GetComponent<LobbyControl>().BroSelector();
            });
            //-----------------READY----------------->
            sioCom.Instance.On("start", (payload) =>
            {
                Debug.Log(" RECEIVED start " + payload);
                GetComponent<LobbyControl>().startOther = bool.Parse(payload.ToString());
                //GetComponent<LobbyControl>().BroSelector();
            });

            //=================================================================E N D   S E T   U P ===============================================================





            //-----------------PLAYER ACTION ----------------->
            sioCom.Instance.On("player_action", (payload) =>
            {
                if (GameDriver.instance.GAMESTART)
                {
                    JObject data = JObject.Parse(payload);
                    //Debug.Log("PLAYER ACTION" + data);
                    Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                    GameDriver.instance.Client.GetComponent<ClientPlayerController>().targetPos.position = new Vector3(float.Parse(dict["ax"]), float.Parse(dict["ay"]), float.Parse(dict["az"]));
                    if (dict.ContainsKey("x")) { GameDriver.instance.Client.GetComponent<ClientPlayerController>().destination = new Vector3(float.Parse(dict["x"]), float.Parse(dict["y"]), float.Parse(dict["z"])); }
                    if (dict.ContainsKey("r")) { GameDriver.instance.Client.GetComponent<ClientPlayerController>().running = true; } else { GameDriver.instance.Client.GetComponent<ClientPlayerController>().running = false; }
                    if (dict.ContainsKey("w")) { GameDriver.instance.Client.GetComponent<ClientPlayerController>().targWalk = float.Parse(dict["w"]); } else { GameDriver.instance.Client.GetComponent<ClientPlayerController>().targWalk = 0; } //WALK
                    if (dict.ContainsKey("s")) { GameDriver.instance.Client.GetComponent<ClientPlayerController>().targStrafe = float.Parse(dict["s"]); } else { GameDriver.instance.Client.GetComponent<ClientPlayerController>().targStrafe = 0; } //STRAFE
                    if (dict.ContainsKey("aim")) { GameDriver.instance.Client.GetComponent<ClientPlayerController>().aim = true; } else { GameDriver.instance.Client.GetComponent<ClientPlayerController>().aim = false; }
                    GameDriver.instance.Client.GetComponent<ClientPlayerController>().gameObject.GetComponent<ClientFlashlightSystem>().FlashLight.intensity = float.Parse(dict["flintensity"]);
                    if (dict.ContainsKey("fl")) { GameDriver.instance.Client.GetComponent<ClientPlayerController>().ToggleFlashlight(bool.Parse(dict["fl"])); }//FLASHLIGHT
                   if (dict.ContainsKey("k2")) { GameDriver.instance.Client.GetComponent<ClientPlayerController>().k2.GetComponent<K2>().fire(true); }
                    if (dict.ContainsKey("gear")) { if (GameDriver.instance.Client.GetComponent<ClientPlayerController>().gear != int.Parse(dict["gear"])) { GameDriver.instance.Client.GetComponent<ClientPlayerController>().ChangeGear(int.Parse(dict["gear"])); } }//gear changes
                    if (dict.ContainsKey("dmg")) { if (bool.Parse(dict["dmg"])) { GameDriver.instance.Client.GetComponent<ClientPlayerController>().Flinch(new Vector3(float.Parse(dict["fx"]), float.Parse(dict["fy"]), float.Parse(dict["fz"]))); } }
                    if (dict.ContainsKey("shoot")) { //DAMAGE AND KILL
                        GameDriver.instance.Client.GetComponent<ClientPlayerController>().triggerShoot = true;
                        GameObject enemy = GameObject.Find(dict["shoot"]); 
                        if (enemy != null) {
                            if (!dict.ContainsKey("kill")) { enemy.GetComponent<NPCController>().TakeDamage(int.Parse(dict["sdmg"]), true); }//hurt
                            else { enemy.GetComponent<NPCController>().healthEnemy = 0; enemy.GetComponent<NPCController>().TakeDamage(0 , true); }//kill
                        } 
                    }
                }

            });
            //-----------------SHOOT  ----------------->
            sioCom.Instance.On("shoot", (payload) =>
            {
                //Debug.Log(" RECEIVED SHOOT  " + payload);
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                GameObject enemy = GameObject.Find(dict["name"]);

                GameDriver.instance.Client.GetComponent<ClientPlayerController>().triggerShoot = true;//shoot ani
                if (enemy != null) { enemy.GetComponent<NPCController>().TakeDamage(int.Parse(dict["damage"]), true); } //do flinch

            });
            //-----------------DEATH  ----------------->
            sioCom.Instance.On("death", (payload) =>
            {
                Debug.Log(" RECEIVED DEATH  " + payload);
                //JObject data = JObject.Parse(payload);
                //Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                Instantiate(GameDriver.instance.Client.GetComponent<ClientPlayerController>().death, GameDriver.instance.Client.transform.position, GameDriver.instance.Client.transform.rotation);
                GameDriver.instance.Client.SetActive(false);
                GameDriver.instance.Client.GetComponent<ClientPlayerController>().hp = 0;
            });
            //-----------------EVENT  ----------------->
            sioCom.Instance.On("event", (payload) =>
            {
                Debug.Log(" RECEIVED EVENT  " + payload);
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                GameObject obj = GameObject.Find(dict["obj"]);
                if (dict["event"] == "pickup")
                {
                    if (obj != null) { DestroyImmediate(obj); }
                    else { if (dict["type"] == "key") { KeyInventory.instance.RemoveKey(dict["pass"]); } }//local player already picked up
                }
                if (dict["type"] == "door")
                {
                    if (dict["event"] == "openclose") { obj.GetComponent<Door>().OpenClose(); }
                    if (dict["event"] == "locked") { obj.GetComponent<Door>().Locked(); }
                }

            });
            //-----------------DISABLE  ----------------->
            sioCom.Instance.On("disable", (payload) =>
            {
                JObject data = JObject.Parse(payload);
                Debug.Log("RECEIVED DISABLE " + data);
                Dictionary<string, Dictionary<string, string>> dict = data.ToObject<Dictionary<string, Dictionary<string, string>>>();
                // Log the object positions to the console
                foreach (KeyValuePair<string, Dictionary<string, string>> obj in dict)
                {
                    //search list of enemies for corresopnding obj
                    foreach (GameObject enemyObject in GetComponent<DisablerControl>().enemyObjects)
                    {
                        if (enemyObject.name == obj.Key)
                        {
                            enemyObject.SetActive(bool.Parse(obj.Value["active"]));
                            break;
                        }
                    }
                }
            });

            //-----------------ENEMY  ----------------->
            sioCom.Instance.On("enemy", (payload) =>
            {

                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                Debug.Log("RECEIVING enemy " + data);
                GameObject enemy = GameObject.Find(dict["obj"]);
                if (enemy != null)
                {

                    //--------POSITION---------
                    Vector3 targPos;
                    if (dict.ContainsKey("x")) { 
                        targPos = new Vector3(float.Parse(dict["x"]), float.Parse(dict["y"]), float.Parse(dict["z"]));
                        if (Vector3.Distance(targPos, enemy.transform.position) > 5 && enemy.GetComponent<Teleport>().teleport == 0) { enemy.transform.position = targPos; }
                        if (dict.ContainsKey("tele")) { enemy.transform.position = targPos; }
                    }
                    //--------TARGET-----------
                    if (dict.ContainsKey("targ"))
                    {
                        string target = dict["targ"];
                        if (target.Contains("Player")) { enemy.GetComponent<NPCController>().target = GameDriver.instance.Client.transform; }
                        if (target.Contains("Client")) { enemy.GetComponent<NPCController>().target = GameDriver.instance.Player.transform; }
                        if (target.Length<2) { enemy.GetComponent<NPCController>().target = null; }
                    }
                    //--------TELEPORT-----------
                    if (dict.ContainsKey("tele"))
                    {
                        enemy.GetComponent<Teleport>().teleport = float.Parse(dict["tele"]); 
                    }
                    //--------PATROL-----------
                    if (dict.ContainsKey("dx")) { 
                        enemy.GetComponent<NPCController>().clientWaypointDest = new Vector3(float.Parse(dict["dx"]), float.Parse(dict["dy"]), float.Parse(dict["dz"]));
                        enemy.GetComponent<NPCController>().curWayPoint = int.Parse(dict["wp"]);
                    }
                    //-------ATTACK-------------
                    if (dict.ContainsKey("attk")) { enemy.GetComponent<NPCController>().attacking = bool.Parse(dict["attk"]); }
                    //-------DEAD-------------
                    //if (dict.ContainsKey("dead")) { enemy.GetComponent<NPCController>().TakeDamage(enemy.GetComponent<NPCController>().healthEnemy, true); }
                }
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
                    thisObj.gameObject.GetComponent<Rigidbody>().AddForce(-velocity * 1.5f, ForceMode.Impulse);
                }
            });
            //--------------------SYNC ENEMIES--------------------
            sioCom.Instance.On("sync", (payload) =>
            {
                JObject data = JObject.Parse(payload);
                Debug.Log("SYNCING " + data);
                Dictionary<string, Dictionary<string, string>> dict = data.ToObject<Dictionary<string, Dictionary<string, string>>>();
                // Log the object positions to the console
                foreach (KeyValuePair<string, Dictionary<string, string>> obj in dict)
                {
                    //search list of enemies for corresopnding obj
                    foreach (GameObject enemyObject in GetComponent<DisablerControl>().enemyObjects)
                    {
                        if (enemyObject.name == obj.Key)
                        {
                            enemyObject.transform.position = new Vector3(float.Parse(obj.Value["x"]), float.Parse(obj.Value["y"]), float.Parse(obj.Value["z"]));
                            enemyObject.SetActive(bool.Parse(obj.Value["active"]));
                            break;
                        }
                    }
                }

            });
            //-----------------CREATE----------------->
            sioCom.Instance.On("create", (payload) =>
            {
                JObject data = JObject.Parse(payload);
                Debug.Log("RECEIVING CREATE " + data);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                Vector3 newPosition = new Vector3(float.Parse(dict["x"]), float.Parse(dict["y"]), float.Parse(dict["z"]));
                Spawner spawner = GameObject.Find("Spawner").GetComponent<Spawner>();
                GameObject newEnemy = Instantiate(spawner.enemies[int.Parse(dict["z"])], newPosition, Quaternion.identity);
                newEnemy.name = dict["name"];

            });

            //--------------DISCONNECT-----------------
            sioCom.Instance.On("disconnect", (payload) => { Debug.LogWarning("Disconnected: " + payload); });
            //--------------PLAYER DISCONNECT-----------------
            sioCom.Instance.On("player_disconnect", (payload) => { Debug.LogWarning("GAME ENDING PLAYER DISCONNECTED "); PlayerPrefs.SetString("message", "PLAYER DISCONNECTED"); HOST = true; }); // sioCom.Instance.Close(); SceneManager.LoadScene("Lobby");
        }



        //-----------------------------SYNC UP EVERYTHING----------------------------
        public void UpdateEnemies()
        {
            
                //Debug.Log("SYNCING ");
                //Create a dictionary for this object's position in structure {"objPlayer":{"x":-2.17,"y":-0.01,"z":0.0},"objOtherPlayer":{"x":4.06,"y":-0.01,"z":0.0}}
               Dictionary<string, Dictionary<string, string>> objStates = new Dictionary<string, Dictionary<string, string>>();
               foreach (GameObject obj in GetComponent<DisablerControl>().enemyObjects)
               {
                   {
                       string objName;
                       Dictionary<string, string> propsDict = new Dictionary<string, string>();

                        // Add the position values to the dictionary
                        propsDict.Add("x", obj.gameObject.transform.position.x.ToString("F2"));
                        propsDict.Add("y", obj.gameObject.transform.position.y.ToString("F2"));
                        propsDict.Add("z", obj.gameObject.transform.position.z.ToString("F2"));
                        propsDict.Add("active", obj.gameObject.activeSelf.ToString());

                        // Add the object's dictionary to the main dictionary with the object name as the key
                        objName = obj.name;
                       objStates.Add(objName, propsDict);
                   }
               }
               sioCom.Instance.Emit("sync", JsonConvert.SerializeObject(objStates), false);
        }

    }
}