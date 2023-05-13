using Firesplash.UnityAssets.SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GameManager;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SceneManagement;

namespace NetworkSystem
{

    public class NetworkDriver : MonoBehaviour
    {
        public static NetworkDriver instance;

        public SocketIOCommunicator sioCom;


        //private float sync_timer = 0.0f;
        //private float delay = 15f;//SYNC DELAY

        public bool HOST = true;
        public string ROOM;
        public bool TWOPLAYER = false;
        public string USERNAME;
        public string otherUSERNAME;
        public bool isTRAVIS = true;
        public bool otherIsTravis = false;

        public bool NETWORK_TEST;
        public bool HOSTOVERRIDE;

        public bool connected = false;
        private float pingTimer = 0.0f;
        private float PING = 0.0f;
        private bool timeout = false;

        public bool OTHERS_SCENE_READY = false;
        public bool SCENE_READY = false;

        //public bool otherSCENESETUP = false;
        public void Awake()
        {
            //ONLY ONE CAN EXIST
            if (instance == null) { instance = this;  DontDestroyOnLoad(gameObject); }
            else { DestroyImmediate(gameObject); }

            NetworkSetup();
        }
        public void Start()
        {
            StartCoroutine(connectSIO());
            //FindObjectsOfType<GameDriver>(true)[0].gameObject.SetActive(true);
        }

        private void ConnectionTimeout() { if (!connected) { GameDriver.instance.WriteGuiMsg("Trouble reaching servers!", 30f, false, Color.red); timeout = true; } }
       
        
        public void Reconnect()
        {
            connected = false;
            sioCom.Instance.Close();
            StartCoroutine(connectSIO());
            Invoke("ConnectionTimeout", 10f);
        }
        IEnumerator connectSIO()//--------CONNECT HELPER--------->
        {
            while (!connected && !timeout)
            {
                GameDriver.instance.WriteGuiMsg("Attempting to connect to Ghost Servers", 5f, true, Color.white);
                sioCom.Instance.Close();
                yield return new WaitForSeconds(1f); //refresh socket
                                                     //Debug.Log("attempting connection ");
                sioCom.Instance.Connect("https://twrecks.io:8080", true);
                yield return new WaitForSeconds(1f);
            }
        }
        public void NetworkSetup()
        {
            //=================================================================  S E T  U P  ===============================================================

            sioCom = gameObject.AddComponent<SocketIOCommunicator>();
            sioCom.secureConnection = true;

            //-----------------CONNECT TO SERVER----------------->
            sioCom.Instance.On("connect", (payload) =>
            {
                if (payload != null)
                {
                    connected = true;
                    GameDriver.instance.WriteGuiMsg("Connected Successfully!", 5f, false, Color.white);
                    if (SceneManager.GetActiveScene().name == "Lobby") { GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().loginCanvas.SetActive(true); }
                    else { sioCom.Instance.Emit("join", ROOM, true); } //PlayerPrefs.GetString("room")}
                    //GameDriver.instance.WriteGuiMsg("Checking Room " + GameDriver.instance.ROOM,1f, true);
                    //Debug.Log(payload + " CONNECTING TO ROOM " + PlayerPrefs.GetString("room"));
                    //sioCom.Instance.Emit("join", GameDriver.instance.ROOM, true); //PlayerPrefs.GetString("room")
                }
            });
            Invoke("ConnectionTimeout",10f);
            //-----------------CHECK USERNAME ----------------->
            sioCom.Instance.On("check_username", (payload) =>
            {
                if (payload == "None") {  GameObject.Find("LoginControl").GetComponent<LoginControl>().NoUserFound(); }
                else { GameObject.Find("LoginControl").GetComponent<LoginControl>().UserFound(); }
            });
            //-----------------CONFIRMED SAVED ----------------->
            sioCom.Instance.On("save_user", (payload) =>
            {
                if (payload == "success") { GameObject.Find("LoginControl").GetComponent<LoginControl>().SavingSuccess(); }
                else {  GameObject.Find("LoginControl").GetComponent<LoginControl>().SavingFailed(); }
            });
            //-----------------LOGIN ----------------->
            sioCom.Instance.On("login", (payload) =>
            {
                if (payload == "true") { GameObject.Find("LoginControl").GetComponent<LoginControl>().LoginSuccess(); }
                else { GameObject.Find("LoginControl").GetComponent<LoginControl>().LoginFail(); }
            });
            //-----------------LEVEL1 SPEED ----------------->
            sioCom.Instance.On("get_level_speed", (payload) =>
            {
                Debug.Log("LEVEL SPEED RECEIVED" + payload);
                string data = payload;
                string[] splitData = data.Split(',');
                string level = splitData[0]; level = level.Replace("level", ""); level = level.Replace("speed", "");
                string speed = splitData[1];
                GameObject.Find("LobbyManager").GetComponent<RigManager>().ReceivedLevelData(int.Parse(level), int.Parse(speed));
            });

            //-----------------JOIN ROOM----------------->
            sioCom.Instance.On("join", (payload) =>
            {
                //GameDriver.instance.ROOM_VALID = false;
                Debug.Log(payload);
                if (payload == "full")
                {
                    GameDriver.instance.WriteGuiMsg("Room is full! Can't join Game! ", 10f, false, Color.red);
                    //GameObject.Find("LobbyManager").GetComponent<LobbyControl>().checkingRoom = false;
                    //sioCom.Instance.Close();
                }
                else
                {
                    //GameDriver.instance.ROOM_VALID = true;
                    if (SceneManager.GetActiveScene().name == "Lobby") { GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().RoomFound(); }
                    GameDriver.instance.WriteGuiMsg("Found Room " + ROOM, 1f, false, Color.white);
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
                //GameDriver.instance.WriteGuiMsg("Waiting for another player", 10f, false, Color.white);
                 Debug.Log("PONG RECEIVED " + payload);
                if (PING == 0) { PING = Time.time - pingTimer; Debug.Log("MY PING IS " + PING); }
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                if (dict["sid"] != sioCom.Instance.SocketID)//OTHERS PING
                { //IF ITS THEIR PING
                    //Debug.Log("THEIR PING IS " + dict["ping"]);
                    if (float.Parse(dict["ping"]) == 0) //THEY JUST JOINED
                    {
                        GameDriver.instance.WriteGuiMsg("Player Joined", 1f, false, Color.white);
                        Debug.Log("PLAYER JOINED SENDING MY PING SPEED TO OTHER PLAYER");
                        dict = new Dictionary<string, string> {
                        { "sid", sioCom.Instance.SocketID },
                        { "ping", PING.ToString() }
                        };
                        sioCom.Instance.Emit("ping", JsonConvert.SerializeObject(dict), false);//MAKE OTHER TEST PINGS
                    }
                    else//they were in room first
                    {
                        // COMPARE PING VALUES
                        //im host
                        if (float.Parse(dict["ping"]) > PING) {
                            Debug.Log("SENDING PING");
                            sioCom.Instance.Emit("host", JsonConvert.SerializeObject(new { host = sioCom.Instance.SocketID, username = USERNAME }), false); 
                        
                        }
                        //they are host
                        else { if (!NETWORK_TEST) { HOST = false; }
                            Debug.Log("SENDING PING");
                            sioCom.Instance.Emit("host", JsonConvert.SerializeObject(new { host = dict["sid"], username = USERNAME }), false); 
                        
                        }
                    }
                }
            });
            //-----------------HOST / GAME START / UPDATE GAME STATES----------------->
            sioCom.Instance.On("host", (payload) =>
            {
                Debug.Log("HOST DETERMINED " + payload);
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();

                TWOPLAYER = true;
                otherUSERNAME = dict["username"];
                if (!NETWORK_TEST) { if (dict["host"] != sioCom.Instance.SocketID) { HOST = false; } }
                if (SceneManager.GetActiveScene().name != "Lobby") { UpdateGameState(); OTHERS_SCENE_READY = true; SCENE_READY = true; }
                //GameDriver.instance.WriteGuiMsg("Two Player Mode - HOST " + payload + "     MY SOCKET    " + sioCom.Instance.SocketID,999f,false,Color.white);
                //Debug.Log("HOST DETERMINED " + payload);
            });
            //-----------------CHOOSE BRO----------------->
            sioCom.Instance.On("bro", (payload) =>
            {
                // Debug.Log(" RECEIVED BRO " + payload);
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                GetComponent<LobbyControl>().otherBro = dict["bro"];
                // GameDriver.instance.otherBroRig = dict["rig"];
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
                //Debug.Log("PLAYER ACTION" + payload);
                //GameDriver.instance.WriteGuiMsg("OTHER PLAYER LOADED - GAME START" + GameDriver.instance.GAMESTART + " opl " + otherPlayerLoaded, 999f, false, Color.white);
                if (OTHERS_SCENE_READY && SCENE_READY)
                {
                    //if (!otherPlayerLoaded) { otherPlayerLoaded = true; UpdateGameState(); }
                    
                    JObject data = JObject.Parse(payload);
                   
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
                    if (dict.ContainsKey("dg")) { GameDriver.instance.Client.GetComponent<ClientPlayerController>().dodge = int.Parse(dict["dg"]); }
                    if (dict.ContainsKey("shoot"))
                    {
                        GameDriver.instance.Client.GetComponent<ClientPlayerController>().triggerShoot = true;
                        GameObject enemy = GameObject.Find(dict["shoot"]);
                        //DAMAGE AND KILL
                        if (dict.ContainsKey("sdmg"))
                        {
                            if (int.Parse(dict["sdmg"]) != -1)//victim kill
                            {
                                if (enemy != null)
                                {
                                    if (!dict.ContainsKey("kill")) { enemy.GetComponent<NPCController>().TakeDamage(int.Parse(dict["sdmg"]), true); }//hurt
                                    else { enemy.GetComponent<NPCController>().healthEnemy = 0; enemy.GetComponent<NPCController>().TakeDamage(0, true); }//kill
                                }
                            }
                            /*else//VICTIM
                            {
                                Debug.Log("CLIENT CHOSE " + enemy.name);
                                GetComponentInChildren<VictimControl>().testAnswer(enemy, true);
                            }*/
                        }


                    }
                }

            });
            //-----------------SHOOT  ----------------->
            sioCom.Instance.On("shoot", (payload) =>
            {
                if (OTHERS_SCENE_READY && SCENE_READY)
                {
                    //Debug.Log(" RECEIVED SHOOT  " + payload);
                    JObject data = JObject.Parse(payload);
                    Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                    GameObject enemy = GameObject.Find(dict["name"]);


                }

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
            //-----------------LASER  ----------------->
            sioCom.Instance.On("laser", (payload) =>
            {
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                Debug.Log("RECEIVING LASER " + data);
                //GetComponentInChildren<VictimControl>().ZOZO.SetActive(true);
                //GameObject enemy = GameObject.Find(dict["obj"]);
                StopCoroutine(waitForZozoActive());
                StartCoroutine(waitForZozoActive());
                
            });
            IEnumerator waitForZozoActive()
            {
                
                while (!GameDriver.instance.GetComponentInChildren<VictimControl>().ZOZO.activeSelf)
                {
                    Debug.Log("WAIT FOR ACTIVE");
                    yield return new WaitForSeconds(0.2f);
                }
                GameDriver.instance.GetComponentInChildren<VictimControl>().ZOZO.GetComponent<ZozoControl>().ChargeLaser(true);
            }
            //-----------------EVENT  ----------------->
            sioCom.Instance.On("event", (payload) =>
            {
                Debug.Log(" RECEIVED EVENT  " + payload);
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                //LOBBY
                if (dict.ContainsKey("isTRAVIS")) { otherIsTravis = bool.Parse(dict["isTRAVIS"]); }
                if (dict.ContainsKey("skin")) { GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().UpdateOtherRig(dict["skin"]); }
                if (dict.ContainsKey("level")) { GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().UpdateOtherLevel(dict["level"]); }
                //LOADS GAME SCENE
                if (dict.ContainsKey("otherssceneready")) { 
                    Debug.Log("---YOUR SCENE IS READY"); 
                        OTHERS_SCENE_READY = true;
                        if (SceneManager.GetActiveScene().name != "Lobby") { UpdateGameState();  } 
                    } 

                if (OTHERS_SCENE_READY && SCENE_READY)
                {
                    GameObject obj = null;
                    if (dict.ContainsKey("obj")) { obj = GameObject.Find(dict["obj"]); }
                    //PLAYER
                    if (dict.ContainsKey("shoot")) {
                        if (int.Parse(dict["dmg"]) != -1)
                        {
                            GameDriver.instance.Client.GetComponent<ClientPlayerController>().triggerShoot = true;//shoot ani
                            if (obj != null) { obj.GetComponent<NPCController>().TakeDamage(int.Parse(dict["dmg"]), true); } //do flinch
                        }
                        else { GameDriver.instance.GetComponentInChildren<VictimControl>().testAnswer(obj); }
                    }


                    if (dict.ContainsKey("event"))
                    {
                        if (dict["event"] == "setfree") { obj.GetComponent<VictimControl>().SetSpiritsFree(); }
                        if (dict["event"] == "summon") { obj.GetComponent<VictimControl>().SummonZozo(); }
                        if (dict["event"] == "zozo") { obj.GetComponent<VictimControl>().DestroyZozo(); }
                        
                        if (dict["event"] == "pickup")
                        {
                            if (obj != null)
                            {
                                if (dict["type"] == "key") { obj.GetComponent<Key>().DestroyWithSound(true); }
                                if (dict["type"] == "med") { obj.GetComponent<FirstAidKit>().DestroyWithSound(true); }
                                if (dict["type"] == "bat") { obj.GetComponent<Battery>().DestroyWithSound(true); }
                                if (dict["type"] == "cand") { obj.GetComponent<Candle>().DestroyWithSound(true); GameDriver.instance.GetComponentInChildren<VictimControl>().candleCount++; }
                            }
                            else { if (dict["type"] == "key") { KeyInventory.instance.RemoveKey(dict["pass"]); } }//local player already picked up
                        }
                        if (dict["type"] == "door")
                        {
                            if (dict["event"] == "openclose") { obj.GetComponent<Door>().OpenClose(true); }
                            if (dict["event"] == "locked") { obj.GetComponent<Door>().Locked(true); }
                        }
                        if (dict["event"] == "coldspot")
                        {
                            if (dict["type"] == "respawn") { obj.GetComponent<ColdSpot>().Respawn(GameObject.Find(dict["loc"])); }
                            if (dict["type"] == "expose") { obj.GetComponent<ColdSpot>().Exposed(true); }
                            //GameObject.Find("ColdSpotManager").GetComponent<ColdSpotControl>().ChooseColdSpotNetwork(int.Parse(dict["q1"]), int.Parse(dict["q2"]), int.Parse(dict["q3"]));
                        }
                        if (dict["event"] == "randomvictim") { Debug.Log(" RECEIVED RANDOM VICTIM  " + obj.name); GameDriver.instance.GetComponentInChildren<VictimControl>().RandomVictim(obj); }
                        if (dict["event"] == "startcircle") { GameDriver.instance.GetComponentInChildren<VictimControl>().ActivateCircle(true); }
                    }
                }
            });
            //-----------------TELEPORT  ----------------->
            sioCom.Instance.On("teleport", (payload) =>
            {
            if (OTHERS_SCENE_READY && SCENE_READY)
            {
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                Debug.Log("RECEIVING TELEPORT " + data);
                GameObject enemy = GameObject.Find(dict["obj"]);
                    if (enemy.GetComponent<Teleport>().teleport == 0 || (enemy.GetComponent<Teleport>().teleport == 1.5 && float.Parse(dict["tp"]) == 3f))
                    {
                        enemy.GetComponent<Teleport>().teleport = float.Parse(dict["tp"]);
                        enemy.transform.position = new Vector3(float.Parse(dict["x"]), float.Parse(dict["y"]), float.Parse(dict["z"]));
                    }
                }
            });

            //--------------------SYNC ENEMIES--------------------
            //enemyObject.SetActive(bool.Parse(obj.Value["active"]));
            sioCom.Instance.On("sync", (payload) =>
            {
                if (OTHERS_SCENE_READY && SCENE_READY)
                {
                    JObject data = JObject.Parse(payload);
                    //Debug.Log("SYNCING " + data);
                    Dictionary<string, Dictionary<string, string>> dict = data.ToObject<Dictionary<string, Dictionary<string, string>>>();
                    // Log the object positions to the console
                    foreach (KeyValuePair<string, Dictionary<string, string>> obj in dict)
                    {
                        //search list of enemies for corresopnding obj
                        foreach (GameObject enemy in GameDriver.instance.GetComponent<DisablerControl>().enemyObjects)
                        {
                            if (enemy.name == obj.Key)
                            {
                                //--------ACTIVE-----------
                                //if (obj.Value["ax"] != null) { enemy.SetActive(bool.Parse(obj.Value["ax"])); }
                                if (!enemy.activeSelf) {  enemy.SetActive(true); } //if (!enemy.tag.Contains("ZOZO")){
                                enemy.GetComponent<NPCController>().active_timer = timer_delay * 2;//DISABLE IF NO MESSAGES BEYOND 0.6s
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
                                    if (target.Contains("Player")) { enemy.GetComponent<NPCController>().Engage(GameDriver.instance.Client.transform); }
                                    if (target.Contains("Client")) { enemy.GetComponent<NPCController>().Engage(GameDriver.instance.Player.transform); }
                                    if (target.Length < 2) { enemy.GetComponent<NPCController>().target = null; }
                                }
                                //--------PATROL-----------
                                if (obj.Value["dx"] != null)
                                {
                                    GameObject dest = GameObject.Find(obj.Value["dx"]);
                                    if (dest == GameDriver.instance.Player) { dest = GameDriver.instance.Client; }
                                    else if (dest == GameDriver.instance.Client) { dest = GameDriver.instance.Player; }
                                    enemy.GetComponent<NPCController>().ClientUpdateDestination(dest); //new Vector3(float.Parse(dict["dx"]), float.Parse(dict["dy"]), float.Parse(dict["dz"]));
                                                                                                       //enemy.GetComponent<NPCController>().curWayPoint = int.Parse(dict["wp"]);
                                }
                                break;
                            }
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
            //if (GameDriver.instance.GAMESTART && TWOPLAYER)
            {
                Dictionary<string, Dictionary<string, string>> syncObjects = new Dictionary<string, Dictionary<string, string>>();
                //---------------ADD REGULAR ENEMIES----------------------
                foreach (GameObject obj in GameDriver.instance.GetComponent<DisablerControl>().enemyObjects)
                {

                    if ((obj.activeSelf == true && checkActive) || (!checkActive))
                    //if (GetComponent<DisablerControl>().closestPlayerDist<=GetComponent<DisablerControl>().disableDistance)
                    {
                        Dictionary<string, string> propsDict = new Dictionary<string, string>();
                        //Debug.Log("PREPARING UPDATE FOR OBJ" + obj.name);
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



                if (syncObjects.Count > 0) { sioCom.Instance.Emit("sync", JsonConvert.SerializeObject(syncObjects), false); }
                timer = Time.time;//cooldown
            }
        }



        private float timer = 0f;
        private float timer_delay = 0.8f;//0.5
        public void Update()
        {
            //----------------------------------SYNC ACTIVE ENEMIES-----------------------------------------
            if (OTHERS_SCENE_READY && SCENE_READY && HOST) //&& GameDriver.instance.twoPlayer
            {
                if (Time.time > timer + timer_delay)
                {
                    UpdateEnemies(true);

                }
            }

            if (NETWORK_TEST) { if (HOSTOVERRIDE) { HOST = true; } else { HOST = false; } }
           // GameDriver.instance.WriteGuiMsg("HOST " + HOST, 10f, false, Color.red);

        }

        public void UpdateGameState()
        {
            Debug.Log("-----------------UPDATING GAME STATES");
            GameDriver.instance.Client.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            GameDriver.instance.Player.GetComponent<PlayerController>().emitFlashlight = true;
            GameDriver.instance.Player.GetComponent<PlayerController>().emitGear = true;
            GameDriver.instance.Player.GetComponent<PlayerController>().emitPos = true;//triggers position emit
            
            if (HOST)
            {
                UpdateEnemies(false);
                ColdSpot[] coldSpots = FindObjectsOfType<ColdSpot>();
                foreach (ColdSpot coldspot in coldSpots) { coldspot.Respawn(null); }
                GameDriver.instance.GetComponentInChildren<VictimControl>().RandomVictim(null);
            }

           // sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { setup = true }), false);
        }




    }
}