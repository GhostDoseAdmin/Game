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

    public static GameDriver instance;
    public NetworkDriver ND;

    //GHOST EFFECT LIGHT REFS
    public Light PlayerWeapLight;
    public Light PlayerFlashLight;
    public Light ClientWeapLight;
    public Light ClientFlashLight;

    private GameObject TRAVIS;
    private GameObject WESTIN;


    void Awake()
    {

        MSG = "Welcome to GhostDose";
        ROOM = "room";


        //ONLY ONE CAN EXIST
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Debug.Log("IM DYING"); DestroyImmediate(gameObject); }


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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GetComponent<LobbyControl>().enabled = false;
        SetupScene();
        Debug.Log("OnSceneLoad");
    }


    public void SetupScene()
    {
        {

            Debug.Log("SETTING UP SCENE");
            Client = GameObject.Find("Client");




            //MISSING A RIG                
            if (GameObject.Find("TRAVIS").transform.GetChild(0).transform.childCount == 0) { Debug.Log("NO RIG FOUND FOR TRAVIS"); Instantiate(Resources.Load<GameObject>("Prefabs/Rigs/Travis/TravisRigBasic"), GameObject.Find("TRAVIS").transform.GetChild(0).transform); }
            if (GameObject.Find("WESTIN").transform.GetChild(0).transform.childCount == 0) { Debug.Log("NO RIG FOUND FOR WESTIN"); Instantiate(Resources.Load<GameObject>("Prefabs/Rigs/Westin/WestinRigBasic"), GameObject.Find("WESTIN").transform.GetChild(0).transform); }


            //DISABLE MODELS
            if (!isTRAVIS) { //PLAYING WESTIN
                Instantiate(GameObject.Find("TRAVIS").transform.GetChild(0).transform.GetChild(0).gameObject, Client.transform); //gets TRAVIS rig and copys as client
                GameObject.Find("TRAVIS").SetActive(false);
                
            }
            else {  //PLAYING TRAVIS
                Instantiate(GameObject.Find("WESTIN").transform.GetChild(0).transform.GetChild(0).gameObject, Client.transform); //gets WESTIN rig and copys as client
                GameObject.Find("WESTIN").SetActive(false);
            }

            Player = GameObject.Find("Player");

            //Setup flashlights 
            Player.GetComponent<FlashlightSystem>().setupLightRefs();//find lights on heiarchy and create a ref to them
            Client.GetComponent<ClientFlashlightSystem>().setupLightRefs();
            //store light refs for ghost fx - this must happen before start where we disable lights
            PlayerWeapLight = Player.GetComponent<FlashlightSystem>().WeaponLight; 
            PlayerFlashLight = Player.GetComponent<FlashlightSystem>().FlashLight;
            ClientWeapLight = Client.GetComponent<ClientFlashlightSystem>().WeaponLight;
            ClientFlashLight = Client.GetComponent<ClientFlashlightSystem>().FlashLight;

            //----CLEAR ANIMATOR CACHE---
            Client.SetActive(false);
            Client.SetActive(true);
            Player.SetActive(false);
            Player.SetActive(true);

            Vector3 clientStart = Client.transform.position;
            Vector3 playerStart = Player.transform.position;



            //SWAP OBJECTS
            Client.transform.position = new Vector3(0f, 50f, 0f);
            Player.transform.position = clientStart;
            Client.transform.position = playerStart;

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
