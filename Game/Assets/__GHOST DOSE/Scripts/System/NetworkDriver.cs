using Firesplash.UnityAssets.SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GameManager;
using static UnityEngine.GraphicsBuffer;

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

            //Debug.Log("SETTING UP NETWORK");

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
                    GameDriver.instance.WriteGuiMsg("Checking Room " + GameDriver.instance.ROOM,1f);
                    //Debug.Log(payload + " CONNECTING TO ROOM " + PlayerPrefs.GetString("room"));
                    sioCom.Instance.Emit("join", GameDriver.instance.ROOM, true); //PlayerPrefs.GetString("room")
                }
            });
            IEnumerator connectSIO()//--------CONNECT HELPER--------->
            {
                while (!connected)
                {
                    GameDriver.instance.WriteGuiMsg("Attempting to connect to Ghost Servers", 5f);
                    sioCom.Instance.Close();
                    yield return new WaitForSeconds(1f); //refresh socket
                    //Debug.Log("attempting connection ");
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
                    GameDriver.instance.WriteGuiMsg("Room is full! Can't join Game! ", 10f);
                    sioCom.Instance.Close();
                }
                else
                {
                    GameDriver.instance.ROOM_VALID = true;
                    GameDriver.instance.WriteGuiMsg("Found Room " + GameDriver.instance.ROOM, 1f);
                    var dict = new Dictionary<string, string> {
                    { "sid", sioCom.Instance.SocketID },
                    { "ping", PING.ToString() }
                    };
                    if (PING == 0)
                    {
                        pingTimer = Time.time; sioCom.Instance.Emit("ping", JsonConvert.SerializeObject(dict), false); //Debug.Log("PINGING");
                    }
                }
            });

            //-----------------PING----------------->
            sioCom.Instance.On("pong", (payload) =>
            {
                GameDriver.instance.WriteGuiMsg("Looking For Players - you may start alone", 10f);
                // Debug.Log("PONG RECEIVED " + payload);
                if (PING == 0) { PING = Time.time - pingTimer; Debug.Log("MY PING IS " + PING); }
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                if (dict["sid"] != sioCom.Instance.SocketID)//OTHERS PING
                { //IF ITS THEIR PING
                    //Debug.Log("THEIR PING IS " + dict["ping"]);
                    if (float.Parse(dict["ping"]) == 0) //THEY JUST JOINED
                    {
                        GameDriver.instance.WriteGuiMsg("Player Joined", 1f);
                        //Debug.Log("PLAYER JOINED SENDING MY PING SPEED TO OTHER PLAYER");
                        dict = new Dictionary<string, string> {
                        { "sid", sioCom.Instance.SocketID },
                        { "ping", PING.ToString() }
                        };
                        sioCom.Instance.Emit("ping", JsonConvert.SerializeObject(dict), false);//MAKE OTHER TEST PINGS
                    }
                    else//they were in room first
                    {
                        // COMPARE PING VALUES
                        if (float.Parse(dict["ping"]) > PING) { sioCom.Instance.Emit("host", sioCom.Instance.SocketID, true); }
                        else { if (!GameDriver.instance.NETWORK_TEST) { HOST = false; } sioCom.Instance.Emit("host", dict["sid"], true); }
                    }
                }
            });
            //-----------------HOST / GAME START / UPDATE GAME STATES----------------->
            sioCom.Instance.On("host", (payload) =>
            {
                //GameDriver.instance.MSG = "Two Player Mode - HOST " + payload + "     MY SOCKET    " + sioCom.Instance.SocketID;
                //Debug.Log("HOST DETERMINED " + payload);
                if (!GameDriver.instance.NETWORK_TEST)
                {
                    if (payload.ToString() != sioCom.Instance.SocketID) { HOST = false; }
                    else { UpdateEnemies(false); }
                }
                else { if (HOST) { UpdateEnemies(false); } }
                GameDriver.instance.Client.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                GameDriver.instance.Player.GetComponent<PlayerController>().emitFlashlight = true;
                GameDriver.instance.Player.GetComponent<PlayerController>().emitGear = true;
                GameDriver.instance.Player.GetComponent<PlayerController>().emitPos = true;//triggers position emit
                GameDriver.instance.twoPlayer = true;
                if (HOST) { GameObject.Find("ColdSpotManager").GetComponent<ColdSpotControl>().ChooseColdSpot(); }
                //GameDriver.instance.MSG = "Two Player Mode - HOST " + HOST;
                GameDriver.instance.WriteGuiMsg("Two Player Mode - HOST is " + HOST, 10f);

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
                    if (dict.ContainsKey("shoot"))
                    {
                        GameDriver.instance.Client.GetComponent<ClientPlayerController>().triggerShoot = true;
                        GameObject enemy = GameObject.Find(dict["shoot"]);
                        //DAMAGE AND KILL
                        if (int.Parse(dict["sdmg"])!=-1)
                        {
                            if (enemy != null)
                            {
                                if (!dict.ContainsKey("kill")) { enemy.GetComponent<NPCController>().TakeDamage(int.Parse(dict["sdmg"]), true); }//hurt
                                else { enemy.GetComponent<NPCController>().healthEnemy = 0; enemy.GetComponent<NPCController>().TakeDamage(0, true); }//kill
                            }
                        }else//VICTIM
                        {
                            GameObject.Find("VictimManager").GetComponent<VictimControl>().testAnswer(enemy);
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
                GameObject newDeath = Instantiate(GameDriver.instance.Client.GetComponent<ClientPlayerController>().death, GameDriver.instance.Client.transform.position, GameDriver.instance.Client.transform.rotation);
                newDeath.GetComponent<PlayerDeath>().otherPlayer = true;
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
                    if (dict["event"] == "openclose") { obj.GetComponent<Door>().OpenClose(true); }
                    if (dict["event"] == "locked") { obj.GetComponent<Door>().Locked(true); }
                }
                if (dict["event"] == "coldspot")
                {
                    GameObject.Find("ColdSpotManager").GetComponent<ColdSpotControl>().ChooseColdSpotNetwork(int.Parse(dict["q1"]), int.Parse(dict["q2"]), int.Parse(dict["q3"]));
                }

            });
            //-----------------TELEPORT  ----------------->
            sioCom.Instance.On("teleport", (payload) =>
            {
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                Debug.Log("RECEIVING TELEPORT " + data);
                GameObject enemy = GameObject.Find(dict["obj"]);
                enemy.GetComponent<Teleport>().teleport = float.Parse(dict["tp"]);
                enemy.transform.position = new Vector3(float.Parse(dict["x"]), float.Parse(dict["y"]), float.Parse(dict["z"]));

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

            //--------------------SYNC ENEMIES--------------------
            //enemyObject.SetActive(bool.Parse(obj.Value["active"]));
            sioCom.Instance.On("sync", (payload) =>
            {
                JObject data = JObject.Parse(payload);
                //Debug.Log("SYNCING " + data);
                Dictionary<string, Dictionary<string, string>> dict = data.ToObject<Dictionary<string, Dictionary<string, string>>>();
                // Log the object positions to the console
                foreach (KeyValuePair<string, Dictionary<string, string>> obj in dict)
                {
                    //search list of enemies for corresopnding obj
                    foreach (GameObject enemy in GetComponent<DisablerControl>().enemyObjects)
                    {
                        if (enemy.name == obj.Key)
                        {
                            //--------ACTIVE-----------
                            //if (obj.Value["ax"] != null) { enemy.SetActive(bool.Parse(obj.Value["ax"])); }
                            enemy.SetActive(true);
                            enemy.GetComponent<NPCController>().active_timer = timer_delay*2;//DISABLE IF NO MESSAGES BEYOND 0.6s
                            //--------POSITION---------
                            Vector3 targPos;
                            if (obj.Value["x"] != null)
                            {
                                targPos = new Vector3(float.Parse(obj.Value["x"]), float.Parse(obj.Value["y"]), float.Parse(obj.Value["z"]));
                                enemy.GetComponent<NPCController>().serverPosition = targPos;
                                //if (Vector3.Distance(targPos, enemy.transform.position) > 5 && enemy.GetComponent<Teleport>().teleport == 0) { enemy.transform.position = targPos; }
                                if (dict.ContainsKey("tele")) { enemy.transform.position = targPos; }
                            }
                            //--------TARGET-----------
                            if (obj.Value["tx"] != null)
                            {
                                string target = obj.Value["tx"];
                                if (target.Contains("Player")) { enemy.GetComponent<NPCController>().target = GameDriver.instance.Client.transform; }
                                if (target.Contains("Client")) { enemy.GetComponent<NPCController>().target = GameDriver.instance.Player.transform; }
                                if (target.Length < 2) { enemy.GetComponent<NPCController>().target = null; }
                            }
                            //--------PATROL-----------
                            if (obj.Value["dx"] != null)
                            {
                                GameObject dest = GameObject.Find(obj.Value["dx"]);
                                if (dest == GameDriver.instance.Player) { dest = GameDriver.instance.Client; }
                                else if (dest == GameDriver.instance.Client) { dest = GameDriver.instance.Player; }
                                enemy.GetComponent<NPCController>().destination = dest;//new Vector3(float.Parse(dict["dx"]), float.Parse(dict["dy"]), float.Parse(dict["dz"]));
                                                                                       //enemy.GetComponent<NPCController>().curWayPoint = int.Parse(dict["wp"]);
                            }
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



        public void UpdateEnemies(bool checkActive)
        {
            Dictionary<string, Dictionary<string, string>> syncObjects = new Dictionary<string, Dictionary<string, string>>();
            foreach (GameObject obj in GetComponent<DisablerControl>().enemyObjects)
            {

                if ((obj.activeSelf == true && checkActive) || (!checkActive))
                //if (GetComponent<DisablerControl>().closestPlayerDist<=GetComponent<DisablerControl>().disableDistance)
                {
                    Dictionary<string, string> propsDict = new Dictionary<string, string>();

                    // Add the position values to the dictionary
                    propsDict.Add("x", obj.gameObject.transform.position.x.ToString("F2"));
                    propsDict.Add("y", obj.gameObject.transform.position.y.ToString("F2"));
                    propsDict.Add("z", obj.gameObject.transform.position.z.ToString("F2"));
                    propsDict.Add("dx", obj.GetComponent<NPCController>().destination.name);
                    if (obj.GetComponent<NPCController>().target != null) { propsDict.Add("tx", obj.GetComponent<NPCController>().target.name); }
                    else { propsDict.Add("tx", ""); }
                    //propsDict.Add("ax", obj.gameObject.activeSelf.ToString());
                    //propsDict.Add("tp", obj.GetComponent<NPCController>().teleport);

                    syncObjects.Add(obj.name, propsDict);
                }
            }
            sioCom.Instance.Emit("sync", JsonConvert.SerializeObject(syncObjects), false);
            timer = Time.time;//cooldown
        }



        private float timer = 0f;
        private float timer_delay = 0.8f;//0.5
        public void Update()
        {

            //----------------------------------SYNC ACTIVE ENEMIES-----------------------------------------
            if (HOST && GameDriver.instance.twoPlayer) //&& GameDriver.instance.twoPlayer
            {
                if (Time.time > timer + timer_delay)
                {
                    UpdateEnemies(true);

                }
            }
        }





    }
}