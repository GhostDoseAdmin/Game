using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDriver : MonoBehaviour
{
    public bool TRAVIS = true;//which character is the player playing
    [HideInInspector] public GameObject Player;
    [HideInInspector] public GameObject Client;
    [HideInInspector] public string ROOM;
    [HideInInspector] public bool ROOM_VALID;//they joined valid room
    [HideInInspector] public string MSG = "";
    public bool GAMESTART = false;
    public bool twoPlayer = false;
    public bool FORCE_HOST;

    public static GameDriver instance;
    [HideInInspector] public NetworkDriver ND;

    // Start is called before the first frame update
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
            ND.NetworkSetup();
            SetupScene();
            GAMESTART = true;
        }


       
    }

    public void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (FORCE_HOST) { ND.HOST = true; }
    }

    public GameObject duplicateObject;
    // Called when a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GetComponent<LobbyControl>().enabled = false;
        SetupScene();
        Debug.Log("OnSceneLoad");
    }


    public void SetupScene()
    {
        {
            Debug.Log("SCENE SETUP");
            //DISABLE MODELS
            if (!TRAVIS) { 
                GameObject.Find("TRAVIS").SetActive(false);
                GameObject.Find("CLIENTWES").SetActive(false);
            }
            else { 
                GameObject.Find("WESTIN").SetActive(false);
                GameObject.Find("CLIENTTRAV").SetActive(false);
            }

            Player = GameObject.Find("Player");
            Client = GameObject.Find("Client");

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
