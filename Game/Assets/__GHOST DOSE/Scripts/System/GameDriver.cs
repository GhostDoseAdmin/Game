using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.ParticleSystem;

public class GameDriver : MonoBehaviour
{
    public bool isTRAVIS = true;//which character is the player playing
    public GameObject Player;
    public GameObject Client;
    public string ROOM;
    public bool ROOM_VALID;//they joined valid room
    public string MSG = "";
    public bool GAMESTART = false;
    public bool twoPlayer = false;
    private GameObject WESTIN;
    private GameObject TRAVIS;

    public static GameDriver instance;
    public NetworkDriver ND;

    //GHOST EFFECT LIGHT REFS
    public Light PlayerWeapLight;
    public Light PlayerFlashLight;
    public Light ClientWeapLight;
    public Light ClientFlashLight;

    public GameObject mySelectedRig;
    public GameObject theirSelectedRig;

    private static utilities util;

    void Awake()
    {

        MSG = "Welcome to GhostDose";
        ROOM = "room";

        util = new utilities();

        //ONLY ONE CAN EXIST
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { DestroyImmediate(gameObject); }


        ND = this.gameObject.AddComponent<NetworkDriver>();


        //NON LOBBY INSTANCE
        if (SceneManager.GetActiveScene().name != "Lobby" && !GetComponent<LobbyControl>().start)
        {
            Debug.Log("PRE EMPTIVE CALL");
            GetComponent<LobbyControl>().enabled = false;
            //ND = this.gameObject.AddComponent<NetworkDriver>();
            ND.NetworkSetup();
            SetupScene();
        }

       
    }

    public void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    
    //----------------GAME SCENES----------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoad");
        GetComponent<LobbyControl>().enabled = false;
        SetupScene();
        
    }


    public void SetupScene()
    {
        {

            Debug.Log("SETTING UP SCENE");
            TRAVIS = GameObject.Find("TRAVIS");
            WESTIN = GameObject.Find("WESTIN");

            Client = GameObject.Find("Client");

            //-----------------------------------CUSTOM RIGS---------------------------------
            if (mySelectedRig)
            {
                //MY TRAVIS RIGS
                if (isTRAVIS)
                {
                    if (TRAVIS.transform.GetChild(0).childCount > 0) { Debug.Log("Destroying Travis Rig "); DestroyImmediate(TRAVIS.transform.GetChild(0).GetChild(0).gameObject); }
                    Instantiate(mySelectedRig, TRAVIS.transform.GetChild(0).transform);

                    //THEIR WESTIN RIGS
                    if (twoPlayer)
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
                    if (twoPlayer)
                    {
                        if (TRAVIS.transform.GetChild(0).childCount > 0) { Debug.Log("Destroying Travis Rig "); DestroyImmediate(TRAVIS.transform.GetChild(0).GetChild(0).gameObject); }
                        Instantiate(theirSelectedRig, TRAVIS.transform.GetChild(0).transform);
                    }
                }

            }

            // Debug.Log("---------------------------------------------" + TRAVIS.transform.GetChild(0).childCount);
            //------------CHECK FOR MISSING A RIG------------    
            if (TRAVIS.transform.GetChild(0).childCount <= 0) { Debug.Log("Creating Default Rig for Travis"); Instantiate(GetComponent<RigManager>().travRigList[0], TRAVIS.transform.GetChild(0).transform); }
                if (WESTIN.transform.GetChild(0).childCount <= 0) { Debug.Log("Creating Default Rig for Westin"); Instantiate(GetComponent<RigManager>().wesRigList[0], WESTIN.transform.GetChild(0).transform); }
            


            //---------DISABLE UNUSED PLAYER------------
            if (!isTRAVIS) { //PLAYING WESTIN
                Instantiate(TRAVIS.transform.GetChild(0).transform.GetChild(0).gameObject, Client.transform); //gets TRAVIS rig and copys as client
                Client.transform.position = TRAVIS.transform.position;
                TRAVIS.SetActive(false);
                
            }
            else {  //PLAYING TRAVIS
                Instantiate(WESTIN.transform.GetChild(0).transform.GetChild(0).gameObject, Client.transform); //gets WESTIN rig and copys as client
                Client.transform.position = WESTIN.transform.position;
                WESTIN.SetActive(false);
            }

            Player = GameObject.Find("Player");

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

            if (!twoPlayer) { ND.HOST = true; }
            GAMESTART = true;

        }
    }







    //----------------SYSTEM CONSOLE-------------------------
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        Vector2 textSize = style.CalcSize(new GUIContent(MSG));
        float posX = Screen.width / 2f;
        float posY = Screen.height / 2f;
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            posY += 100;
        }
        else
        {
            posX = textSize.x / 2f; posY = textSize.y;
        }
        Rect labelRect = new Rect(posX - (textSize.x / 2f), posY - (textSize.y / 2f), textSize.x, textSize.y);
        GUI.Label(labelRect, MSG, style);
    }
}
