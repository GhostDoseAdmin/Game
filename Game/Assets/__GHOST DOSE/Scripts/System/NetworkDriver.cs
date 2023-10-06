using Firesplash.UnityAssets.SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GameManager;
using UnityEngine.SceneManagement;
using TMPro;

namespace NetworkSystem
{

    public class NetworkDriver : MonoBehaviour
    {
        public static NetworkDriver instance;

        public SocketIOCommunicator sioCom;

        public string myRig;
        public string theirRig;

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

        public float startTime = 0f;
        public float timeElapsed;
        public bool GAMESTARTED;
        public int LEVELINDEX;
        // public float SPEEDSCORE;
        public GameObject PlayerScores;
        public GameObject LevelManager;
        private GameObject otherPlayerDeath;
         public bool isMobile = false;
        public bool FORCEMOBILE;
        public bool lostGame = false;
        private bool hasEverConnected = false;
        public bool getLeaderboard = false; //used to determine if this client was the one that sent the request

        public bool VIBRATE;
        public void Awake()
        {
            Debug.Log("-----------------------NETWORK DRIVER");
            if (Application.isMobilePlatform) { isMobile = true; }
            if (FORCEMOBILE) { isMobile = true; }
           // isMobile = true;
            //ONLY ONE CAN EXIST
            if (instance == null) { instance = this;  DontDestroyOnLoad(gameObject); }
            else { DestroyImmediate(gameObject); }

            NetworkSetup();
        }
        public void Start()
        {
            StartCoroutine(connectSIO());
            if (PlayerPrefs.GetInt("Vibrate") == 0) { VIBRATE = true; } else { VIBRATE = false; }
            
            //FindObjectsOfType<GameDriver>(true)[0].gameObject.SetActive(true);
        }

        private void ConnectionTimeout() { if (!connected) { GameDriver.instance.WriteGuiMsg("Trouble reaching servers!", 30f, false, Color.red); timeout = true; } }
       
        
        public void Reconnect()
        {
            connected = false;
            PING = 0;
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
                sioCom.Instance.Connect("https://ghostdose.net:8080", true);
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
                    if (!hasEverConnected) { GameDriver.instance.WriteGuiMsg("Connected Successfully!", 5f, false, Color.white); hasEverConnected = true; }
                    if (SceneManager.GetActiveScene().name == "Lobby") { GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().loginCanvas.SetActive(true); }
                    else { sioCom.Instance.Emit("join", ROOM, true); } //PlayerPrefs.GetString("room")}
                    //GameDriver.instance.WriteGuiMsg("Checking Room " + GameDriver.instance.ROOM,1f, true);
                    //Debug.Log(payload + " CONNECTING TO ROOM " + PlayerPrefs.GetString("room"));
                    //sioCom.Instance.Emit("join", GameDriver.instance.ROOM, true); //PlayerPrefs.GetString("room")
                }
            });
            Invoke("ConnectionTimeout", 10f);
            //-----------------CHECK USERNAME ----------------->
            sioCom.Instance.On("check_username", (payload) =>
            {
                if (payload == "None") { GameObject.Find("LoginControl").GetComponent<LoginControl>().NoUserFound(); }
                else { GameObject.Find("LoginControl").GetComponent<LoginControl>().UserFound(); }
            });
            //-----------------CONFIRMED SAVED ----------------->
            sioCom.Instance.On("save_user", (payload) =>
            {
                if (payload == "success") { GameObject.Find("LoginControl").GetComponent<LoginControl>().SavingSuccess(); }
                else { GameObject.Find("LoginControl").GetComponent<LoginControl>().SavingFailed(); }
            });
            //-----------------LOGIN ----------------->
            sioCom.Instance.On("login", (payload) =>
            {
                if (payload == "true") { GameObject.Find("LoginControl").GetComponent<LoginControl>().LoginSuccess(); }
                else { GameObject.Find("LoginControl").GetComponent<LoginControl>().LoginFail(); }
            });
            //-----------------LEVEL1 SPEED ----------------->
            //socket removed from own room SID, so need to receive it like and parse like so
            sioCom.Instance.On("get_level_speed", (payload) =>
            {
                Debug.Log("LEVEL SPEED RECEIVED" + payload);
                string data = payload;
                string[] splitSID = data.Split(';');
                if (splitSID[1] == sioCom.Instance.SocketID)
                {
                    string[] splitData = splitSID[0].Split(',');
                    string level = splitData[0]; level = level.Replace("level", ""); level = level.Replace("speed", "");
                    string speed = splitData[1];
                    if (speed.Contains("None")) { speed = "-1"; }
                    GetComponent<RigManager>().ReceivedLevelData(int.Parse(level), float.Parse(speed));
                }

            });
            //-----------------GET LEADERBOARD ----------------->
            sioCom.Instance.On("get_leaderboard", (payload) =>
            {
                if (getLeaderboard)
                {
                    GameObject list = GameObject.Find("PlayerScoreList");
                    Debug.Log("RECEIVING LEADERBOARD " + payload);
                    JArray jsonArray = JArray.Parse(payload);
                    // DATA FORMAT AS -->> array of dicts -->>[{"username":"user1","leveldata":2.12},{"username":"tt","leveldata":2.853588}] 
                    Debug.Log(list.name);
                    int place = 1;
                    foreach (JObject obj in jsonArray)
                    {
                        Dictionary<string, string> dict = obj.ToObject<Dictionary<string, string>>();
                        GameObject playerScoresItem = Instantiate(PlayerScores, list.transform);
                        playerScoresItem.transform.GetChild(0).GetComponent<TextMeshPro>().text = place.ToString() + ") " + dict["username"] + " : " + dict["leveldata"] + " seconds";
                        place++;

                    }
                }
            });
            //-----------------JOIN ROOM----------------->
            sioCom.Instance.On("join", (payload) =>
            {
                //GameDriver.instance.ROOM_VALID = false;
                Debug.Log(payload);
                if (payload == "full")
                {
                    GameDriver.instance.WriteGuiMsg("Room is full! Can't join Game! ", 10f, false, Color.red);
                    GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().LeaveRoom();
                    //GameObject.Find("LobbyManager").GetComponent<LobbyControl>().checkingRoom = false;
                    //sioCom.Instance.Close();
                }
                else
                {
                    ROOM = payload;
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
                // GameDriver.instance.WriteGuiMsg("IN ROOM " + ROOM, 9999f, false, Color.magenta);
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
                        GameDriver.instance.WriteGuiMsg("Player Joined", 5f, false, Color.white);
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
                            sioCom.Instance.Emit("host", JsonConvert.SerializeObject(new { host = sioCom.Instance.SocketID }), false);

                        }
                        //they are host
                        else { if (!NETWORK_TEST) { HOST = false; }
                            Debug.Log("SENDING PING");
                            sioCom.Instance.Emit("host", JsonConvert.SerializeObject(new { host = dict["sid"] }), false);

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
                if (!NETWORK_TEST) { if (dict["host"] != sioCom.Instance.SocketID) { HOST = false; } }
                if (SceneManager.GetActiveScene().name != "Lobby") {
                    UpdateGameState();
                    //OTHERS_SCENE_READY = true; 
                    //SCENE_READY = true; 
                }
                sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { username = USERNAME }), false);
                if (SceneManager.GetActiveScene().name == "Lobby") {
                    GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().EmitSkin();
                    GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().EmitLevel();
                }
                //GameDriver.instance.WriteGuiMsg("Two Player Mode - HOST " + payload + "     MY SOCKET    " + sioCom.Instance.SocketID,999f,false,Color.white);
                //Debug.Log("HOST DETERMINED " + payload);
            });
            //=================================================================E N D   S E T   U P ===============================================================





            //-----------------PLAYER ACTION ----------------->
            sioCom.Instance.On("player_action", (payload) =>
            {
                if (!OTHERS_SCENE_READY && SCENE_READY) { OTHERS_SCENE_READY = true; UpdateGameState(); }
                // Debug.Log("PLAYER ACTION" + payload);
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
                }

            });
            //-----------------DEATH  ----------------->
            sioCom.Instance.On("death", (payload) =>
            {
                Debug.Log(" RECEIVED DEATH  " + payload);
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                otherPlayerDeath = Instantiate(GameDriver.instance.Client.GetComponent<ClientPlayerController>().death, new Vector3(float.Parse(dict["x"]), float.Parse(dict["y"]), float.Parse(dict["z"])), GameDriver.instance.Client.transform.rotation);
                otherPlayerDeath.GetComponent<PlayerDeath>().otherPlayer = true;
                GameDriver.instance.Client.SetActive(false);
                GameDriver.instance.Client.GetComponent<ClientPlayerController>().hp = 0;
                if(GameDriver.instance.Player.GetComponent<HealthSystem>().Health<=0)
                {
                    GameDriver.instance.Player.GetComponent<HealthSystem>().CancelInvoke("CamSwitch");
                    GameDriver.instance.DeathCam.SetActive(false);
                    GameDriver.instance.mainCam.SetActive(true);
                }


            });
            //-----------------LASER  ----------------->
            sioCom.Instance.On("laser", (payload) =>
            {
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                Debug.Log("RECEIVING LASER " + data);
                //if (bool.Parse(dict["on"]))
                {
                    StopCoroutine(waitForZozoActive());
                    StartCoroutine(waitForZozoActive());
                }
                // else { GameDriver.instance.GetComponentInChildren<VictimControl>().ZOZO.GetComponent<ZozoControl>().blocked = true; GameDriver.instance.GetComponentInChildren<VictimControl>().ZOZO.GetComponent<ZozoControl>().StopLaser(); }

            });
            IEnumerator waitForZozoActive()
            {

                while (!LevelManager.GetComponentInChildren<VictimControl>().ZOZO.activeSelf)
                {
                    Debug.Log("WAIT FOR ACTIVE");
                    yield return new WaitForSeconds(0.2f);
                }
                LevelManager.GetComponentInChildren<VictimControl>().ZOZO.GetComponent<ZozoControl>().ChargeLaser(true);
            }
            //-----------------EVENT  ----------------->
            sioCom.Instance.On("event", (payload) =>
            {
                //Debug.Log(" RECEIVED EVENT  " + payload);
                JObject data = JObject.Parse(payload);
                Dictionary<string, string> dict = data.ToObject<Dictionary<string, string>>();
                //LOBBY
                if (dict.ContainsKey("username")) { otherUSERNAME = dict["username"]; }
                if (dict.ContainsKey("skin")) { GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().UpdateOtherRig(dict["skin"]); otherIsTravis = bool.Parse(dict["isTRAVIS"]); }
                if (dict.ContainsKey("level")) { GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().UpdateOtherLevel(dict["level"]); }
                if (dict.ContainsKey("ready")) { GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().OtherReady(); }
                //LOADS GAME SCENE
                if (dict.ContainsKey("otherssceneready")) {
                    Debug.Log("---YOUR SCENE IS READY");
                    //OTHERS_SCENE_READY = true;
                    //if (SceneManager.GetActiveScene().name != "Lobby") { UpdateGameState();  } 
                }

                if (SCENE_READY)
                {
                    GameObject obj = null;
                    if (dict.ContainsKey("obj")) { obj = GameObject.Find(dict["obj"]); }
                    //PLAYER
                    if (dict.ContainsKey("shoot")) {
                        if (int.Parse(dict["dmg"]) != -1)
                        {
                            Debug.Log(" SHOOT EVENT  " + payload);
                            GameDriver.instance.Client.GetComponent<ClientPlayerController>().triggerShoot = true;//shoot ani
                            if (obj != null && obj.GetComponent<NPCController>()!=null) { obj.GetComponent<NPCController>().TakeDamage(int.Parse(dict["dmg"]), true); } //do flinch
                        }
                        else { LevelManager.GetComponentInChildren<VictimControl>().testAnswer(obj); }
                    }
                    //REM POD
                    if (dict.ContainsKey("remthrow"))
                    {
                        GameDriver.instance.Client.GetComponent<Animator>().SetBool("Throw", true);
                        GameDriver.instance.Client.GetComponent<ClientPlayerController>().throwing = true;

                    }
                    if (dict.ContainsKey("remrelease"))
                    {
                        GameDriver.instance.Client.GetComponentInChildren<RemPod>().ReleaseClient(new Vector3(float.Parse(dict["x"]), float.Parse(dict["y"]), float.Parse(dict["z"])));
                    }
                     if (dict.ContainsKey("revive"))
                    {
                        Destroy(otherPlayerDeath);
                        GameDriver.instance.Client.SetActive(true);
                        GameDriver.instance.Client.GetComponent<ClientPlayerController>().hp = 99999;
                        GameDriver.instance.reviveIndicator.SetActive(false);
                    }
                    //ENEMY
                    if (dict.ContainsKey("zap")) {
                        foreach (GameObject enemy in GameDriver.instance.GetComponent<DisablerControl>().enemyObjects)
                        {
                            if (enemy.name == dict["obj"])
                            {
                                if (!enemy.activeSelf) { enemy.SetActive(true); }
                                //enemy.GetComponent<NPCController>().active_timer = timer_delay * 5;//DISABLE IF NO MESSAGES BEYOND 0.6s
                                enemy.GetComponent<NPCController>().KeepActive(timer_delay * 5, true);
                                enemy.GetComponent<NPCController>().zapClient = 30;
                            }
                        }
                    }
                    //VICTIMS
                    if (dict.ContainsKey("isMurdered"))
                    {
                        obj.GetComponent<Person>().isEvil = bool.Parse(dict["isEvil"]);
                        obj.GetComponent<Person>().isMurdered = bool.Parse(dict["isMurdered"]);
                        obj.GetComponent<Person>().isGirl = bool.Parse(dict["isGirl"]);
                        obj.GetComponent<Person>().UpdateTraits();
                    }

                    if (dict.ContainsKey("event"))
                    {
                        if (dict["event"] == "setfree") { obj.GetComponent<VictimControl>().SetSpiritsFree(); }
                        if (dict["event"] == "summon") { obj.GetComponent<VictimControl>().SummonZozo(); }
                        if (dict["event"] == "zozo") { obj.GetComponent<VictimControl>().DestroyZozo1(); }
                        if (dict["event"] == "zozohp") { Debug.Log(dict["type"]); if (obj != null) { obj.GetComponent<ZozoControl>().HP = float.Parse(dict["type"]); } }
                        if (dict["event"] == "captures") {GameDriver.instance.OTHER_KILLS = int.Parse(dict["amount"]); }

                        if (dict["event"] == "pickup")
                        {
                            if (obj != null)
                            {
                                if (dict["type"] == "key") { obj.GetComponent<Key>().DestroyWithSound(true); }
                                if (dict["type"] == "med") { obj.GetComponent<FirstAidKit>().DestroyWithSound(true); }
                                if (dict["type"] == "bat") { obj.GetComponent<Battery>().DestroyWithSound(true); }
                                if (dict["type"] == "grid") { obj.GetComponent<laserGridItem>().DestroyWithSound(true); }
                                if (dict["type"] == "rem") { obj.GetComponent<remPodItem>().DestroyWithSound(true); }
                                if (dict["type"] == "cand") { obj.GetComponent<Candle>().DestroyWithSound(true); LevelManager.GetComponentInChildren<VictimControl>().candleCount++; }
                            }
                            //else { if (dict["type"] == "key") { KeyInventory.instance.RemoveKey(dict["pass"]); } }//local player already picked up
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
                        if (dict["event"] == "randomvictim") { Debug.Log(" RECEIVED RANDOM VICTIM  " + obj.name); LevelManager.GetComponentInChildren<VictimControl>().RandomVictim(obj); }
                        if (dict["event"] == "startcircle") { LevelManager.GetComponentInChildren<VictimControl>().ActivateCircle(true); }
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
                    foreach (GameObject enemy in GameDriver.instance.GetComponent<DisablerControl>().enemyObjects)
                    {
                        if (enemy.name == dict["obj"])
                        {
                            if (!enemy.activeSelf) { enemy.SetActive(true); }
                            //enemy.GetComponent<NPCController>().active_timer = timer_delay * 5;//DISABLE IF NO MESSAGES BEYOND 0.6s
                            enemy.GetComponent<NPCController>().KeepActive(timer_delay * 5, false);
                            if (enemy.GetComponent<Teleport>().teleport == 0 || (enemy.GetComponent<Teleport>().teleport == 1.5 && float.Parse(dict["tp"]) == 3f))
                            {
                                enemy.GetComponent<Teleport>().teleport = float.Parse(dict["tp"]);
                                enemy.transform.position = new Vector3(float.Parse(dict["x"]), float.Parse(dict["y"]), float.Parse(dict["z"]));
                            }
                        }
                    }
                }
            });
            //--------------------GROUP DAMAGE ENEMIES--------------------
            sioCom.Instance.On("rem_pod", (payload) =>
            {
                if (OTHERS_SCENE_READY && SCENE_READY)
                {

                    JObject data = JObject.Parse(payload);
                    Debug.Log("REM POD-------------------------------------- " + data);
                    //Debug.Log("SYNCING " + data);
                    Dictionary<string, Dictionary<string, int>> dict = data.ToObject<Dictionary<string, Dictionary<string, int>>>();
                    // Log the object positions to the console
                    foreach (KeyValuePair<string, Dictionary<string, int>> obj in dict)
                    {
                        //search list of enemies for corresopnding obj
                        foreach (GameObject enemy in GameDriver.instance.GetComponent<DisablerControl>().enemyObjects)
                        {
                            if (enemy.name == obj.Key)
                            {
                                //--------ACTIVE-----------
                                //if (obj.Value["ax"] != null) { enemy.SetActive(bool.Parse(obj.Value["ax"])); }
                                if (!enemy.activeSelf) { enemy.SetActive(true); } //if (!enemy.tag.Contains("ZOZO")){
                                //enemy.GetComponent<NPCController>().active_timer = timer_delay * 5;//DISABLE IF NO MESSAGES BEYOND 0.6s
                                enemy.GetComponent<NPCController>().KeepActive(timer_delay * 5, true);
                                //--------TARGET-----------
                                if (obj.Value.ContainsKey("dmg"))
                                {
                                    enemy.GetComponent<NPCController>().TakeDamage(obj.Value["dmg"], true);
                                }
                            }
                        }
                    }
                }

            });
            //enemyObject.SetActive(bool.Parse(obj.Value["active"]));
            sioCom.Instance.On("laser_grid", (payload) =>
            {
                if (OTHERS_SCENE_READY && SCENE_READY)
                {

                    GameDriver.instance.Client.GetComponentInChildren<laserGrid>().Shoot(true);
                    GameDriver.instance.Client.GetComponent<ClientPlayerController>().triggerShoot = true;

                    JObject data = JObject.Parse(payload);
                    Debug.Log("laser grid-------------------------------------- " + data);
                    //Debug.Log("SYNCING " + data);
                    Dictionary<string, Dictionary<string, int>> dict = data.ToObject<Dictionary<string, Dictionary<string, int>>>();
                    // Log the object positions to the console
                    foreach (KeyValuePair<string, Dictionary<string, int>> obj in dict)
                    {
                        //search list of enemies for corresopnding obj
                        foreach (GameObject enemy in GameDriver.instance.GetComponent<DisablerControl>().enemyObjects)
                        {
                            if (enemy.name == obj.Key)
                            {
                                //--------ACTIVE-----------
                                //if (obj.Value["ax"] != null) { enemy.SetActive(bool.Parse(obj.Value["ax"])); }
                                if (!enemy.activeSelf) { enemy.SetActive(true); } //if (!enemy.tag.Contains("ZOZO")){
                                //enemy.GetComponent<NPCController>().active_timer = timer_delay * 5;//DISABLE IF NO MESSAGES BEYOND 0.6s
                                enemy.GetComponent<NPCController>().KeepActive(timer_delay * 5, true);
                                //--------TARGET-----------
                                if (obj.Value.ContainsKey("dmg"))
                                {
                                    enemy.GetComponent<NPCController>().TakeDamage(obj.Value["dmg"], true);
                                }
                            }
                        }
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
                                if (!enemy.activeSelf) { enemy.SetActive(true); } //if (!enemy.tag.Contains("ZOZO")){
                                enemy.GetComponent<NPCController>().KeepActive(timer_delay * 5, true);
                                //enemy.GetComponent<NPCController>().active_timer = timer_delay * 5;//DISABLE IF NO MESSAGES BEYOND 0.6s
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
                                if (obj.Value.ContainsKey("tx"))
                                {
                                    string target = obj.Value["tx"];
                                    if (target.Contains("Player")) { enemy.GetComponent<NPCController>().Engage(GameDriver.instance.Client.transform); }
                                    if (target.Contains("Client")) { enemy.GetComponent<NPCController>().Engage(GameDriver.instance.Player.transform); }
                                    if (target.Length < 2) { enemy.GetComponent<NPCController>().target = null; }
                                }
                                //--------PATROL-----------
                                if (obj.Value.ContainsKey("dx"))
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
            sioCom.Instance.On("player_disconnect", (payload) => { if (SceneManager.GetActiveScene().name == "Lobby") { GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().LeaveRoom(); }
                if (SceneManager.GetActiveScene().name != "EndGame") { GameDriver.instance.WriteGuiMsg("Other Player Disconnected! ", 10f, false, Color.red); HOST = true; }
                //EndGame();
            });
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
                        //if (!GameDriver.instance.GetComponent<DisablerControl>().enemyObjects.Contains(obj))
                        {
                        Dictionary<string, string> propsDict = new Dictionary<string, string>();
                        //Debug.Log("PREPARING UPDATE FOR OBJ" + obj.name);
                        // Add the position values to the dictionary
                        propsDict.Add("x", obj.gameObject.transform.position.x.ToString("F2"));
                        propsDict.Add("y", obj.gameObject.transform.position.y.ToString("F2"));
                        propsDict.Add("z", obj.gameObject.transform.position.z.ToString("F2"));
                        if (obj.GetComponent<NPCController>().prev_dest != obj.GetComponent<NPCController>().destination) { propsDict.Add("dx", obj.GetComponent<NPCController>().destination.name); }
                        obj.GetComponent<NPCController>().prev_dest = obj.GetComponent<NPCController>().destination;
                        if (obj.GetComponent<NPCController>().prev_targ != obj.GetComponent<NPCController>().target) { if (obj.GetComponent<NPCController>().target != null) { propsDict.Add("tx", obj.GetComponent<NPCController>().target.name); } else { propsDict.Add("tx", ""); } }
                        obj.GetComponent<NPCController>().prev_targ = obj.GetComponent<NPCController>().target;
                        //DISABLE IS DONE LOCALLY ON NPC CONTROLLER FOR CLIENT & DISABLECONTROLLER ON HOST
                        
                        //obj.Value.ContainsKey("dx")
                       
                        //propsDict.Add("ax", obj.gameObject.activeSelf.ToString());
                        //propsDict.Add("tp", obj.GetComponent<NPCController>().teleport);

                        syncObjects.Add(obj.name, propsDict);
                    }
                }


                Debug.Log("SYNCING ----------------------------------------------");
                if (syncObjects.Count > 0) { sioCom.Instance.Emit("sync", JsonConvert.SerializeObject(syncObjects), false); }
                timer = Time.time;//cooldown
            }
        }



        private float timer = 0f;
        private float timer_delay = 0.8f;//0.5
        public void Update()
        {
            //GameDriver.instance.WriteGuiMsg("OTHERS_SCENE_READY " + OTHERS_SCENE_READY, 9999f, false, Color.red);

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

            //-------------------------------------GAME TIMER---------------------------------------------------
            if(!GAMESTARTED)
            {
                if (!TWOPLAYER && SCENE_READY) { startTime = Time.time; GAMESTARTED = true; }
                if (TWOPLAYER && SCENE_READY && OTHERS_SCENE_READY) { startTime = Time.time; GAMESTARTED = true; }
            }

            //PLAYER PLACEMENT IN END GAME SCREEN
            if (SceneManager.GetActiveScene().name == "EndGame") { GameDriver.instance.Player.transform.position = new Vector3(-0.9f, -1.3f, 3.27f); }

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
                //EMIT COLD SPOTS
                ColdSpot[] coldSpots = FindObjectsOfType<ColdSpot>();
                foreach (ColdSpot coldspot in coldSpots) { coldspot.Respawn(null); }
                //EMIT CHOSEN VICTIM
                LevelManager.GetComponentInChildren<VictimControl>().RandomVictim(null);
                //EMIT VICTIM PROPERTIES
                Person[] victims = FindObjectsOfType<Person>();
                foreach (Person victim in victims) { victim.EmitTraits(); }
               
            }


        }

        // BEAT LEVEL
        public void EndGame()
        {
            GameDriver.instance.mainCam.SetActive(true);

            SCENE_READY = false; OTHERS_SCENE_READY = false;
            timeElapsed = Time.time -startTime;
            GameObject.Find("PlayerCamera").transform.SetParent(GameDriver.instance.gameObject.transform);

            if (SceneManager.GetActiveScene().name.Contains("DarkEchoes") || SceneManager.GetActiveScene().name == "Experiment") { LEVELINDEX = 1; }
            if (SceneManager.GetActiveScene().name.Contains("Forsaken") ){ LEVELINDEX = 3; }
            if (SceneManager.GetActiveScene().name.Contains("HollowAngel") ) { LEVELINDEX = 2; }
            if (SceneManager.GetActiveScene().name.Contains("Saint")) { LEVELINDEX = 4; }

            SceneManager.LoadScene("EndGame");

            //PLAYER PERSIST
            GameObject Player = GameDriver.instance.Player;
            Player.SetActive(true);
            Player.transform.parent.transform.SetParent(null);
            GetComponent<RigManager>().travisProp = Player;
            GetComponent<RigManager>().travCurrentRig = Player.transform.GetChild(0).gameObject;
            GetComponent<RigManager>().westinProp = Player;
            GetComponent<RigManager>().wesCurrentRig = Player.transform.GetChild(0).gameObject;
            Player.GetComponent<PlayerController>().k2.gameObject.SetActive(false);
            Player.GetComponent<PlayerController>().enabled = false;
            Player.GetComponent<FlashlightSystem>().enabled = false;
            Player.GetComponent<HealthSystem>().enabled = false;
            Player.GetComponent<ShootingSystem>().enabled = false;
            Player.transform.root.position = new Vector3(0, 0, 0);
            Player.transform.position = new Vector3(-0.9f,-1.3f,3.27f);
            Player.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
            Player.GetComponent<Animator>().Rebind();
            Destroy(Player.GetComponent<Rigidbody>());
            DontDestroyOnLoad(Player.transform.root.gameObject);

        }

        public void ResetGame()
        {
            connected = false;
            PING = 0;
            sioCom.Instance.Close();
            DestroyImmediate(GameObject.Find("Player").transform.parent.gameObject);
            SceneManager.LoadScene("Lobby");
            DestroyImmediate(this.gameObject);
        }

    }
}