using UnityEngine;
using UnityEngine.SceneManagement;
using NetworkSystem;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering.VirtualTexturing;
using TMPro;
using InteractionSystem;
namespace GameManager
{
    public class GameDriver : MonoBehaviour
    {
        public static GameDriver instance;
        //public string USERNAME;

        //public bool isTRAVIS = true;//which character is the player playing
        public GameObject Player;
        public GameObject Client;
        //public string ROOM;
        //public bool ROOM_VALID;//they joined valid room
        
        public bool GAMESTART = false;
        //public bool twoPlayer = false;

        public bool infiniteAmmo;
        private GameObject WESTIN;
        private GameObject TRAVIS;
        //public GameObject loginCanvas;
        public GameObject mainCam;
        public GameObject playerUI;
        public GameObject GamePlayManager;
        public GameObject otherUserName;

        Vector3 playerStartPos;

        //public NetworkDriver ND;

        //GHOST EFFECT LIGHT REFS
        [HideInInspector] public Light PlayerWeapLight;
        [HideInInspector] public Light PlayerFlashLight;
        [HideInInspector] public Light ClientWeapLight;
        [HideInInspector] public Light ClientFlashLight;

        [HideInInspector] public GameObject mySelectedRig;
        [HideInInspector] public GameObject theirSelectedRig;

        private static utilities util;

        private void Update()
        {
            if(infiniteAmmo) { if (Player != null) { Player.GetComponent<ShootingSystem>().camBatteryUI.fillAmount = 1; } }

            //---------------------------------WAITING FOR OTHER PLAYER----------------------------------
            if (Player!=null && NetworkDriver.instance.TWOPLAYER && GAMESTART && !NetworkDriver.instance.otherPlayerLoaded){
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
           // if (NetworkDriver.instance == null) {  GameObject.Find("NetworkManager").GetComponent<NetworkDriver>().Awake(); Debug.Log("---------------RUNNING AWAKE"); }

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
            else { mainCam.SetActive(false); playerUI.SetActive(false); AudioManager.instance.Play("lobbymusic", null); }


        }
        public void SetupScene()//ON AWAKE OR SCENE LOAD
        {
            {

                //Debug.Log("SETTING UP SCENE");
                TRAVIS = GameObject.Find("TRAVIS");
                WESTIN = GameObject.Find("WESTIN");

                Client = GameObject.Find("Client");

                //-----------------------------------CUSTOM RIGS---------------------------------
                if (mySelectedRig)
                {
                    //MY TRAVIS RIGS
                    if (NetworkDriver.instance.isTRAVIS)
                    {
                        if (TRAVIS.transform.GetChild(0).childCount > 0) { Debug.Log("Destroying Travis Rig "); DestroyImmediate(TRAVIS.transform.GetChild(0).GetChild(0).gameObject); }
                        Instantiate(mySelectedRig, TRAVIS.transform.GetChild(0).transform);

                        //THEIR WESTIN RIGS
                        if (NetworkDriver.instance.TWOPLAYER)
                        {
                            if (WESTIN.transform.GetChild(0).childCount > 0) { Debug.Log("Destroying Westin Rig "); DestroyImmediate(WESTIN.transform.GetChild(0).GetChild(0).gameObject); }
                            Instantiate(theirSelectedRig, WESTIN.transform.GetChild(0).transform);
                        }
                    }
                    //MY WESTIN RIGS
                    else
                    {
                        if (WESTIN.transform.GetChild(0).childCount > 0) { Debug.Log("Destroying Westin Rig "); DestroyImmediate(WESTIN.transform.GetChild(0).GetChild(0).gameObject); }
                        Instantiate(mySelectedRig, WESTIN.transform.GetChild(0).transform);

                        //THEIR TRAVIS RIGS
                        if (NetworkDriver.instance.TWOPLAYER)
                        {
                            if (TRAVIS.transform.GetChild(0).childCount > 0) { Debug.Log("Destroying Travis Rig "); DestroyImmediate(TRAVIS.transform.GetChild(0).GetChild(0).gameObject); }
                            Instantiate(theirSelectedRig, TRAVIS.transform.GetChild(0).transform);
                        }
                    }
                }

                if (mySelectedRig == null) { if (NetworkDriver.instance.isTRAVIS) { mySelectedRig = GetComponent<RigManager>().travRigList[0]; } else { mySelectedRig = GetComponent<RigManager>().wesRigList[0]; } }
                if (theirSelectedRig == null) { if (NetworkDriver.instance.isTRAVIS) { theirSelectedRig = GetComponent<RigManager>().wesRigList[0];  } else { theirSelectedRig = GetComponent<RigManager>().travRigList[0]; } }

                //------------CHECK FOR MISSING A RIG------------    
                if (TRAVIS.transform.GetChild(0).childCount <= 0) {Instantiate(GetComponent<RigManager>().travRigList[0], TRAVIS.transform.GetChild(0).transform); }
                if (WESTIN.transform.GetChild(0).childCount <= 0) { Instantiate(GetComponent<RigManager>().wesRigList[0], WESTIN.transform.GetChild(0).transform); }



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
                mainCam.SetActive(true);
                mainCam.transform.SetParent(Player.transform.parent);
                //mainCam.transform.SetAsFirstSibling();
                mainCam.GetComponent<Camera_Controller>().player = Player.transform;
                mainCam.GetComponent<Aiming>().player = Player;
               //----CLEAR ANIMATOR CACHE---
               StartCoroutine(util.ReactivateAnimator(Client));
                StartCoroutine(util.ReactivateAnimator(Player));

                Player.GetComponent<PlayerController>().SetupRig();
                Client.GetComponent<ClientPlayerController>().SetupRig();

                //RIG GHOST VFX
                PlayerWeapLight = Player.GetComponent<FlashlightSystem>().WeaponLight;
                PlayerFlashLight = Player.GetComponent<FlashlightSystem>().FlashLight;
                ClientWeapLight = Client.GetComponent<ClientFlashlightSystem>().WeaponLight;
                ClientFlashLight = Client.GetComponent<ClientFlashlightSystem>().FlashLight;

                GAMESTART = true;

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
