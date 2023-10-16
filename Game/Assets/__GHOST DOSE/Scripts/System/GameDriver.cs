using UnityEngine;
using UnityEngine.SceneManagement;
using NetworkSystem;
using TMPro;
using InteractionSystem;
using Newtonsoft.Json;
using UnityEngine.LowLevel;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using System.Drawing;
using System.Collections.Generic;
using UnityEditor.Rendering;

namespace GameManager
{
    public class GameDriver : MonoBehaviour
    {
        public static GameDriver instance;
        //public string USERNAME;

        //public bool isTRAVIS = true;//which character is the player playing
        public GameObject Player;
        public GameObject Client;//<--------------------------------THIS IS ACTUALLY OTHER PLAYER!!!, nothing to do with client/server relationship
        public MobileController mobileController;
        public List<GameObject> victimInfoTraits;
        //public string ROOM;
        //public bool ROOM_VALID;//they joined valid room

        //public bool GAMESTART = false;
        //public bool twoPlayer = false;

        public bool infiniteAmmo, infiniteHealth, cineCamActive;
        private GameObject WESTIN;
        private GameObject TRAVIS;
        //public GameObject loginCanvas;
        public GameObject playerUI;
        //public GameObject GamePlayManager;
        public GameObject otherUserName,otherKills, mapPingUI, otherKeyUI;

        public GameObject travisBasic;
        public GameObject westinBasic;

        public GameObject myRig;
        public GameObject theirRig;

        public GameObject reviveIndicator;
        public Image DemonScreamerUI;
        public Vector3 playerStartPos;
        public TextMeshProUGUI TimeElapsedUI, killcountUI;
        public TextMeshProUGUI QuitBtnText;
        public GameObject quitUI, infoUI, thisLevel, thisUser;
            
        public Image zozoHealthUI;
        public int KILLS = 0;
        public int OTHER_KILLS = 0;
        //public NetworkDriver ND;

        //GHOST EFFECT LIGHT REFS
        [HideInInspector] public Light PlayerWeapLight;
        [HideInInspector] public Light PlayerFlashLight;
        [HideInInspector] public Light ClientWeapLight;
        [HideInInspector] public Light ClientFlashLight;

        public GameObject DeathCam, cineCam;
        public GameObject mainCam = null;

        private float timer = 0f;
        private float interval = 30f;
        //[HideInInspector] public GameObject mySelectedRig;
        //[HideInInspector] public GameObject theirSelectedRig;
        public GameObject gearuicam, gearuik2, gearuiemp, gearuilaser, candleUI;
        
        private static utilities util;
        public float lostGameDebugCounter = 0;

        UnityEngine.Color arrow_col;

        private void Update()
        {

            //INFO MENU
            if (Input.GetKeyUp(KeyCode.I))
            {
                InfoButton();
            }
            //QUIT/MENU - CURSOR
            if (SceneManager.GetActiveScene().name != "Lobby")
            {
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    TryQuitBtn();
                }
                //if (quitUI.activeSelf) { WriteGuiMsg("Are you sure you want to quit?", 0.1f, false, Color.yellow); }
            }
            //else
            {
                //GameObject.Find("Quit_btn").SetActive(false);
            }
            //GAME TIMER
            if (NetworkDriver.instance.GAMESTARTED)
            {
                // Calculate the elapsed time in minutes and seconds
                float elapsedSeconds = Time.time - NetworkDriver.instance.startTime;
                int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
                int seconds = Mathf.FloorToInt(elapsedSeconds % 60f);

                // Format the time as "mm:ss"
                string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);

                // Update the TextMeshProUGUI element
                TimeElapsedUI.text = formattedTime;
                killcountUI.text = "CAPTURES: " + KILLS.ToString();
                if (NetworkDriver.instance.TWOPLAYER) { 
                    otherKills.GetComponent<TextMeshProUGUI>().text =OTHER_KILLS.ToString();

                    //-----------------SYNC UP KILLS--------------------------------
                    // Increment the timer by the time passed since the last frame.
                    timer += Time.deltaTime;

                    // Check if the timer has reached the desired interval (3 seconds).
                    if (timer >= interval)
                    {
                        // Call your function here.
                        NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'amount':'{KILLS}','event':'captures','type':''}}"), false);

                        // Reset the timer.
                        timer = 0f;
                    }

                }
            }
            //CAMERA TOGGLE - CHECK HEALTHSYSTEM for CAM TOGGLE
            if (SceneManager.GetActiveScene().name != "Lobby") { if (!mainCam.activeSelf && !DeathCam.activeSelf) { mainCam.SetActive(true); } }

            //LOST GAME DEBUG
            if (NetworkDriver.instance.TWOPLAYER && NetworkDriver.instance.lostGame)
            {
                lostGameDebugCounter += Time.deltaTime;
                WriteGuiMsg("Investigation Failed", 5f, false, UnityEngine.Color.red);
                if (lostGameDebugCounter >= 5) { LostGame(); }
            }

            //END GAME
            if (NetworkDriver.instance.GAMESTARTED)
            {
                if (NetworkDriver.instance.TWOPLAYER)
                {
                    if (Player!=null && Client!=null && Player.GetComponent<HealthSystem>().Health<=0 && Client.GetComponent<ClientPlayerController>().hp<=0 && !NetworkDriver.instance.lostGame)
                    {
                        NetworkDriver.instance.lostGame = true;
                        WriteGuiMsg("Investigation Failed", 5f, false, UnityEngine.Color.red);
                        //Invoke("LostGame", 5f);
                       
                       // NetworkDriver.instance.EndGame();
                    }
                }
                else
                {
                    if (Player!=null && Player.GetComponent<HealthSystem>().Health <= 0 && !NetworkDriver.instance.lostGame)
                    {
                        Debug.Log("------------------------------------DEAD");
                        NetworkDriver.instance.lostGame = true;
                        WriteGuiMsg("Investigation Failed", 5f, false, UnityEngine.Color.red);
                        Invoke("LostGame", 5f);
                    }
                }
            }


            //DEMON SCREAMER
            if (SceneManager.GetActiveScene().name != "Lobby")
            {
                if (DemonScreamerUI.gameObject.activeSelf)
                {
                    UnityEngine.Color color = DemonScreamerUI.color;
                    color.a -= 0.5f * Time.deltaTime;
                    DemonScreamerUI.color = color;
                }
            }
            if(infiniteAmmo) { if (Player != null) { Player.GetComponent<ShootingSystem>().camBatteryUI.fillAmount = 1; } }

            //---------------------------------WAITING FOR OTHER PLAYER----------------------------------
            if (Player!=null && NetworkDriver.instance.TWOPLAYER && !NetworkDriver.instance.OTHERS_SCENE_READY)
            {
                WriteGuiMsg("Waiting for other player...", 1f, false, UnityEngine.Color.red);
                Player.transform.position = playerStartPos;
            }

            //---------------VICTIM INFO PANEL------------------
            info_timer += Time.deltaTime;

            //---------------PING ARROW/ OTHERKEYUI-----------------------
            if (NetworkDriver.instance.TWOPLAYER && SceneManager.GetActiveScene().name != "Lobby")
            {

                //------MAP PING INDICATOR-------
                ping_timer += Time.deltaTime;
                if (Player.GetComponent<PlayerController>().gearAim && ping_timer >= 3f)
                {
                    //draw button
                    if (NetworkDriver.instance.isMobile) { Player.GetComponent<PlayerController>().gamePad.pingBTN.gameObject.SetActive(true); }
                    //SHOW BUTTON
                    if ((!NetworkDriver.instance.isMobile && Input.GetKeyUp(KeyCode.V)) || (NetworkDriver.instance.isMobile && Player.GetComponent<PlayerController>().gamePad.pingBTN.GetComponent<GPButton>().buttonReleased))
                    {
                        pingArrow(Player.GetComponent<PlayerController>().targetPos.position, false);
                        NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { pingMap = 1 }), false);

                        //NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { pingMap = 1, x = transform.position.x, y = transform.position.y, z = transform.position.z }), false);
                        ping_timer = 0f;

                        //PING ARROW
                        UnityEngine.Color color = mapPingUI.GetComponent<Image>().color;
                        color.a = 1;
                        mapPingUI.GetComponent<Image>().color = color;
                    }
                }
                else { if (NetworkDriver.instance.isMobile) { Player.GetComponent<PlayerController>().gamePad.pingBTN.gameObject.SetActive(false); } }

                if (SceneManager.GetActiveScene().name != "Lobby" && mainCam.activeSelf)
                {
                    UnityEngine.Color color = mapPingUI.GetComponent<Image>().color;

                    //CHOOSING VICTIM
                    if (NetworkDriver.instance.LevelManager.GetComponentInChildren<VictimControl>().otherPlayerChoice != null)
                    {
                        //Debug.Log("----------------------------------ARROW LOCATION " + pingLoc);
                        color.a = 1;
                        pingLoc = NetworkDriver.instance.LevelManager.GetComponentInChildren<VictimControl>().otherPlayerChoice.transform.position + (Vector3.up * 1.5f);
                    }
                    else
                    {
                        //PING ARROW FADE
                        color.a -= 0.1f * Time.deltaTime;
                    }
                    mapPingUI.GetComponent<Image>().color = color;
                    Camera mainCamera = Camera.main;
                    // Calculate the viewport position of the point

                    //--------------PING ARROW---------------
                    Vector3 viewportPos = mainCamera.WorldToViewportPoint(pingLoc);
                    // Check if the point is within the camera's viewport
                    bool isWithinViewport = viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1 && viewportPos.z > 0;
                    if (isWithinViewport)
                    {
                        Vector3 screenPosition = mainCamera.WorldToScreenPoint(pingLoc);
                        mapPingUI.GetComponent<RectTransform>().position = screenPosition;
                    }

                    //------------OTHER PLAYER UI----------
                    viewportPos = mainCamera.WorldToViewportPoint(Client.transform.position);
                    // Check if the point is within the camera's viewport
                    isWithinViewport = viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1 && viewportPos.z > 0;
                    if (isWithinViewport)
                    {
                        // The point is within the camera's frustum
                        Vector3 screenPosition = mainCamera.WorldToScreenPoint(Client.transform.position);

                        if (otherKeyUI.activeSelf) { otherKeyUI.GetComponent<RectTransform>().position = screenPosition; }//---------OTHER KEY UI---------
                        otherUserName.GetComponent<TextMeshProUGUI>().text = NetworkDriver.instance.otherUSERNAME;
                        otherUserName.GetComponent<RectTransform>().position = screenPosition;
                        otherKills.GetComponent<RectTransform>().position = screenPosition;

                    }
                    else
                    {
                        otherKeyUI.GetComponent<RectTransform>().position = new Vector3(-999, -999, -999);
                    }
                }
                
            }

            //------------------------------CINECAM-----------------------------------
            if(cineCamActive)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    if (!cineCam.activeSelf) { 
                        cineCam.SetActive(true);
                        mainCam.GetComponent<Camera>().rect = new Rect(0.76f, -0.26f, 0.61f, 0.49f);
                        GetComponentInChildren<Canvas>().enabled = false;
                    }
                    else { 
                        cineCam.SetActive(false);
                        mainCam.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
                        GetComponentInChildren<Canvas>().enabled = true;
                    }
                }
                if (Input.GetKeyDown(KeyCode.L))
                {
                    cineCam.GetComponent<cineCam>().lockPos = !cineCam.GetComponent<cineCam>().lockPos;
                }
            }    


        }//<________________________________END UPDATED________________________________________>
        public float info_timer = 0;


        public Vector3 pingLoc;
        private float ping_timer = 0f;
        public void pingArrow(Vector3 location, bool otherPlayer)
        {
            //DETERMINE LOCATION OF ARROW BASEDO ON INTERSECTION
            LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Ghost");
            Vector3 startPoint;
            if (!otherPlayer) { startPoint = Player.transform.position + Vector3.up; }
            else { startPoint = Client.transform.position + Vector3.up; ; }
            Vector3 endPoint = location;
            // Calculate the direction vector from start to end points.
            Vector3 direction = endPoint - startPoint;
            // Create a ray from the startPoint in the calculated direction.
            Ray ray = new Ray(startPoint, direction);
            // Create a RaycastHit variable to store information about the hit point.
            RaycastHit hit;
            // Perform the raycast and check for an intersection on the default layer (Layer 0).
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                // An intersection occurred on the default layer.
                pingLoc = hit.point;
                // You can use intersectionPoint for further calculations or actions.
                Debug.Log("Intersection Point: " + pingLoc);
            }

            //PING ARROW FADE
            UnityEngine.Color color = mapPingUI.GetComponent<Image>().color;
            if (!otherPlayer) { color = UnityEngine.Color.yellow; } else { color = arrow_col; }
            color.a = 1;
            mapPingUI.GetComponent<Image>().color = color;
        }


        void Start()
        {
            
            if (SceneManager.GetActiveScene().name != "Lobby")
            {
                //QUIT BUTTON
                if (NetworkDriver.instance.isMobile) { QuitBtnText.text = "MENU"; }
                else { QuitBtnText.text = "MENU (ESC)"; }

                //PING ARROW
                arrow_col = mapPingUI.GetComponent<Image>().color;
                UnityEngine.Color color = mapPingUI.GetComponent<Image>().color;
                color.a =0;
                mapPingUI.GetComponent<Image>().color = color;
            }
        }
        void Awake()
        {

                //CREATE RIG FROM LOBYS
                if (NetworkDriver.instance)
            {
                GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Rigs");
                //MY RIG
                string rigName = NetworkDriver.instance.myRig.Replace("(Clone)", "");
                for (int i = 0; i < prefabs.Length; i++)
                {
                    if (prefabs[i].name == rigName)
                    {
                        myRig = prefabs[i];
                        break;
                    }
                }
                //THEIR RIG
                rigName = NetworkDriver.instance.theirRig.Replace("(Clone)", "");
                for (int i = 0; i < prefabs.Length; i++)
                {
                    if (prefabs[i].name == rigName)
                    {
                        theirRig = prefabs[i];
                        break;
                    }
                }
            }

            util = new utilities();

            //ONLY ONE CAN EXIST
            if (instance == null) { instance = this; } //DontDestroyOnLoad(gameObject);
            else { DestroyImmediate(gameObject); }                                          // 


            //this.gameObject.AddComponent<NetworkDriver>();
            //NetworkDriver.instance.NetworkSetup();

            //NON LOBBY INSTANCE
            if (SceneManager.GetActiveScene().name != "Lobby")
            {
                Debug.Log("NON LOBY LOAD");
                SetupScene();
            }
            //LOBBY
            else { if (playerUI != null) { playerUI.SetActive(false); } AudioManager.instance.Play("lobbymusic", null); }


        }
        public void SetupScene()//ON AWAKE OR SCENE LOAD
        {
            {
                NetworkDriver.instance.LevelManager = GameObject.Find("LevelManager");
                //Debug.Log("SETTING UP SCENE");
                TRAVIS = GameObject.Find("TRAVIS");
                WESTIN = GameObject.Find("WESTIN");

                Client = GameObject.Find("Client");

                Debug.Log("-----------------CREATING RIG  " + NetworkDriver.instance.myRig);

                //-----------------------------------CUSTOM RIGS---------------------------------
                if (myRig)
                {
                    //MY TRAVIS RIGS
                    if (NetworkDriver.instance.isTRAVIS)
                    {
                        if (TRAVIS.transform.GetChild(0).childCount > 0) { Debug.Log("Destroying Travis Rig "); DestroyImmediate(TRAVIS.transform.GetChild(0).GetChild(0).gameObject); }
                        Instantiate(myRig, TRAVIS.transform.GetChild(0).transform);

                        //THEIR WESTIN RIGS
                        if (NetworkDriver.instance.TWOPLAYER)
                        {
                            if (WESTIN.transform.GetChild(0).childCount > 0) { Debug.Log("Destroying Westin Rig "); DestroyImmediate(WESTIN.transform.GetChild(0).GetChild(0).gameObject); }
                            Instantiate(theirRig, WESTIN.transform.GetChild(0).transform);
                        }
                    }
                    //MY WESTIN RIGS
                    else
                    {
                        if (WESTIN.transform.GetChild(0).childCount > 0) { Debug.Log("Destroying Westin Rig "); DestroyImmediate(WESTIN.transform.GetChild(0).GetChild(0).gameObject); }
                        Instantiate(myRig, WESTIN.transform.GetChild(0).transform);

                        //THEIR TRAVIS RIGS
                        if (NetworkDriver.instance.TWOPLAYER)
                        {
                            if (TRAVIS.transform.GetChild(0).childCount > 0) { Debug.Log("Destroying Travis Rig "); DestroyImmediate(TRAVIS.transform.GetChild(0).GetChild(0).gameObject); }
                            Instantiate(theirRig, TRAVIS.transform.GetChild(0).transform);
                        }
                    }
                }

                if (myRig == null) { if (NetworkDriver.instance.isTRAVIS) { myRig = travisBasic; } else { myRig = westinBasic; } }
                if (theirRig == null) { if (NetworkDriver.instance.isTRAVIS) { theirRig = westinBasic;  } else { theirRig = travisBasic; } }

                //------------CHECK FOR MISSING A RIG------------    
                if (TRAVIS.transform.GetChild(0).childCount <= 0) {Instantiate(travisBasic, TRAVIS.transform.GetChild(0).transform); }
                if (WESTIN.transform.GetChild(0).childCount <= 0) { Instantiate(westinBasic, WESTIN.transform.GetChild(0).transform); }

                

                //---------DISABLE UNUSED PLAYER------------
                if (!NetworkDriver.instance.isTRAVIS)
                { //PLAYING WESTIN
                    Instantiate(TRAVIS.transform.GetChild(0).transform.GetChild(0).gameObject, Client.transform); //gets TRAVIS rig and copys as client
                    Client.transform.position = TRAVIS.transform.position;
                    TRAVIS.SetActive(false);

                }
                else
                {  //PLAYING TRAVIS
                    Instantiate(WESTIN.transform.GetChild(0).transform.GetChild(0).gameObject, Client.transform); //gets WESTIN rig and copys as client
                    Client.transform.position = WESTIN.transform.position;
                    WESTIN.SetActive(false);
                }

                Player = GameObject.Find("Player");
                playerStartPos = Player.transform.position;
                //SETUP CAMERA
                playerUI.SetActive(true);

                mainCam = GameObject.Find("PlayerCamera"); 
                mainCam.GetComponent<Camera_Controller>().player = Player.transform;
                mainCam.GetComponent<Aiming>().player = Player;

                /*if (!NetworkDriver.instance.isMobile) { 
                    mainCam = GameObject.Find("PlayerCamera"); GameObject.Find("PlayerCameraMobile").SetActive(false);
                    mainCam.GetComponent<Camera_Controller>().player = Player.transform;
                    mainCam.GetComponent<Aiming>().player = Player;
                }
                else { 
                    mainCam = GameObject.Find("PlayerCameraMobile"); GameObject.Find("PlayerCamera").SetActive(false);
                    mainCam.GetComponent<MobileCam>().player = Player.transform;
                }*/

                mainCam.transform.SetParent(Player.transform.parent);
               //----CLEAR ANIMATOR CACHE---
               StartCoroutine(util.ReactivateAnimator(Client));
                StartCoroutine(util.ReactivateAnimator(Player));

                Player.GetComponent<PlayerController>().SetupRig();
                if (infiniteHealth) { Player.GetComponent<HealthSystem>().maxHealth = 99999; Player.GetComponent<HealthSystem>().Health = 99999; }
                Client.GetComponent<ClientPlayerController>().SetupRig();

                //RIG GHOST VFX
                //PlayerWeapLight = Player.GetComponent<FlashlightSystem>().WeaponLight;
                //PlayerFlashLight = Player.GetComponent<FlashlightSystem>().FlashLight;
                //ClientWeapLight = Client.GetComponent<ClientFlashlightSystem>().WeaponLight;
                //ClientFlashLight = Client.GetComponent<ClientFlashlightSystem>().FlashLight;

                if(NetworkDriver.instance.isTRAVIS)
                {
                    Player.GetComponent<PlayerController>().isTravis = true;
                    Client.GetComponent<ClientPlayerController>().isTravis = false;
                }
                else
                {
                    Player.GetComponent<PlayerController>().isTravis = false;
                    Client.GetComponent<ClientPlayerController>().isTravis = true;
                }
                KILLS = 0;

               

                Debug.Log("I AM READY");
                NetworkDriver.instance.SCENE_READY = true;
                //NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { otherssceneready = true }), false);


            }
        }

        public void LostGame()
        {
            Debug.Log("-------------------------------------------------END GAME");
            NetworkDriver.instance.EndGame();
        }
        public void DemonColdSpotScreamer(bool otherPlayer)
        {

            if (!otherPlayer)
            {
                Player.GetComponent<PlayerController>().emitScreamer = true;
                DemonScreamerUI.gameObject.SetActive(true);
                UnityEngine.Color color = DemonScreamerUI.color;
                color.a = 1f;
                DemonScreamerUI.color = color;
                Invoke("disableDemonScreamer", 3f);
            }
            else { Client.GetComponent<ClientPlayerController>().Flinch(Vector3.zero, true); }
            
            AudioManager.instance.Play("demonscream", null);

            if (NetworkDriver.instance.HOST)
            {
                foreach (GameObject enemy in GetComponent<DisablerControl>().enemyObjects)
                {
                    if (!otherPlayer)
                    {
                        if (Vector3.Distance(Player.transform.position, enemy.transform.position) < 7) { enemy.GetComponent<NPCController>().alertLevelPlayer = 500; };
                    }
                    else { if (Vector3.Distance(Client.transform.position, enemy.transform.position) < 7) { enemy.GetComponent<NPCController>().alertLevelClient = 500; }; }
                }
            }
        }

        private void disableDemonScreamer()
        {
            DemonScreamerUI.gameObject.SetActive(false);
        }

        //----------------SYSTEM CONSOLE-------------------------
        public GameObject loadingIcon;
        public GameObject systemConsole;
        public void WriteGuiMsg(string msg, float timer, bool loading, UnityEngine.Color color)
        {
            

            CancelInvoke();
            Invoke("StopMSG", timer);
            loadingIcon.SetActive(loading);
            systemConsole.GetComponent<TextMeshProUGUI>().text = msg;
            systemConsole.GetComponent<TextMeshProUGUI>().color = color;
        }

        public void InfoButton()
        {
            if (infoUI.activeSelf)
            {
                if (!NetworkDriver.instance.isMobile)
                {
                    UnityEngine.Cursor.visible = false;
                    UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                }
                infoUI.SetActive(false);
                return;
            }
            else
            {
                if (!NetworkDriver.instance.isMobile)
                {
                    UnityEngine.Cursor.visible = true;
                    UnityEngine.Cursor.lockState = CursorLockMode.None;
                }
                infoUI.SetActive(true);
                return;
            }
        }
        public void TryQuitBtn()
        {
            //Debug.Log("TRY QUIT");
            if (quitUI.activeSelf) {
                if (!NetworkDriver.instance.isMobile)
                {
                    UnityEngine.Cursor.visible = false;
                    UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                }
                quitUI.SetActive(false); 
                return; 
            }
            else {
                if (!NetworkDriver.instance.isMobile)
                {
                    UnityEngine.Cursor.visible = true;
                    UnityEngine.Cursor.lockState = CursorLockMode.None;
                }
                quitUI.SetActive(true); 
                return; 
            }
        }
        public void QuitBtn()
        {
            NetworkDriver.instance.ResetGame();
        }
        private void StopMSG() { systemConsole.GetComponent<TextMeshProUGUI>().text = ""; loadingIcon.SetActive(false); }
    }
}
