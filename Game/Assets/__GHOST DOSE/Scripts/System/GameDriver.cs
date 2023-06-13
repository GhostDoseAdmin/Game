using UnityEngine;
using UnityEngine.SceneManagement;
using NetworkSystem;
using TMPro;
using InteractionSystem;
using Newtonsoft.Json;
using UnityEngine.LowLevel;

namespace GameManager
{
    public class GameDriver : MonoBehaviour
    {
        public static GameDriver instance;
        //public string USERNAME;

        //public bool isTRAVIS = true;//which character is the player playing
        public GameObject Player;
        public GameObject Client;
        public MobileController mobileController;
        //public string ROOM;
        //public bool ROOM_VALID;//they joined valid room
        
        //public bool GAMESTART = false;
        //public bool twoPlayer = false;

        public bool infiniteAmmo, infiniteHealth;
        private GameObject WESTIN;
        private GameObject TRAVIS;
        //public GameObject loginCanvas;
        public GameObject playerUI;
        public GameObject GamePlayManager;
        public GameObject otherUserName;

        public GameObject travisBasic;
        public GameObject westinBasic;

        public GameObject myRig;
        public GameObject theirRig;

        Vector3 playerStartPos;

        //public NetworkDriver ND;

        //GHOST EFFECT LIGHT REFS
        [HideInInspector] public Light PlayerWeapLight;
        [HideInInspector] public Light PlayerFlashLight;
        [HideInInspector] public Light ClientWeapLight;
        [HideInInspector] public Light ClientFlashLight;

        //[HideInInspector] public GameObject mySelectedRig;
        //[HideInInspector] public GameObject theirSelectedRig;

        private static utilities util;

        private void Update()
        {
            if(infiniteAmmo) { if (Player != null) { Player.GetComponent<ShootingSystem>().camBatteryUI.fillAmount = 1; } }

            //---------------------------------WAITING FOR OTHER PLAYER----------------------------------
            if (Player!=null && NetworkDriver.instance.TWOPLAYER && !NetworkDriver.instance.OTHERS_SCENE_READY)
            {
                WriteGuiMsg("Waiting for other player...", 1f, false, Color.red);
                Player.transform.position = playerStartPos;
            }
            //----------------------------------OTHER USERNAME--------------------------------
            //NetworkDriver.instance.otherUSERNAME = "DEEZ NUTS";
            if (NetworkDriver.instance.otherUSERNAME.Length > 0 && Client!=null && GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), Client.GetComponentInChildren<SkinnedMeshRenderer>(false).bounds))
            {
                otherUserName.GetComponent<TextMeshProUGUI>().text = NetworkDriver.instance.otherUSERNAME;
                // Update the name tag position based on the player's position
                Vector3 worldPosition = new Vector3(Client.transform.position.x, Client.transform.position.y+1.5f, Client.transform.position.z);  
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
                otherUserName.GetComponent<RectTransform>().position = screenPosition;
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
                GameObject mainCam = null;

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

                Debug.Log("I AM READY");
                NetworkDriver.instance.SCENE_READY = true;
                NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { otherssceneready = true }), false);


            }
        }
 
        //----------------SYSTEM CONSOLE-------------------------
        public GameObject loadingIcon;
        public GameObject systemConsole;
        public void WriteGuiMsg(string msg, float timer, bool loading, Color color)
        {
            

            CancelInvoke();
            Invoke("StopMSG", timer);
            loadingIcon.SetActive(loading);
            systemConsole.GetComponent<TextMeshProUGUI>().text = msg;
            systemConsole.GetComponent<TextMeshProUGUI>().color = color;
        }

        private void StopMSG() { systemConsole.GetComponent<TextMeshProUGUI>().text = ""; loadingIcon.SetActive(false); }
    }
}
