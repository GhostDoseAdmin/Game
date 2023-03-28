using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using System;


public class LobbyControl : MonoBehaviour
{
    private GameObject ChooseRoom;
    private GameObject ChooseBro;
    private GameObject WESTIN;
    private GameObject TRAVIS;
    private GameObject startButton;
    private string MSG;
    [HideInInspector] public bool READY = false; 
    private bool chooseBro = false;
    [HideInInspector] public bool start = false;
    [HideInInspector] public bool startOther = false; //receives 1 for ready 2 go

    public string selectedBro ="";
    public string otherBro ="";

    private string[] animations = { "Walk_Flashlight", "Pistol_Shot", "Knife_Attack", "Idle", "Running" };


    public void Awake()
    {
        if(SceneManager.GetActiveScene().name == "Lobby")
        {
            ChooseRoom = GameObject.Find("ChooseRoom");
            ChooseBro = GameObject.Find("ChooseBro");
            WESTIN = GameObject.Find("WESTIN");
            TRAVIS = GameObject.Find("TRAVIS");
            ChooseBro.SetActive(false);
            StartCoroutine(randomAnimations());
        }


    }


    void Update()
    {

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
           
            if (!start)
            {
                
                // GO TO CHOOSE BRO
                if (GetComponent<GameDriver>().ROOM_VALID)
                {
                    
                    if (!chooseBro)
                    {
                        ChooseRoom.SetActive(false);
                        ChooseBro.SetActive(true);
                        WESTIN.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = false;
                        TRAVIS.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = false;
                        GameObject.Find("InputField (TMP)").SetActive(false);
                        chooseBro = true;
                    }

                }

                //------------------------- C H O O S E     A     B R O ----------------------------------------------
                if (chooseBro)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        BroSelector();
                    }

                    if (selectedBro == "TRAVIS" || otherBro == "TRAVIS")
                    {
                        TRAVIS.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = true;
                    }
                    else { TRAVIS.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = false; }

                    if (selectedBro == "WESTIN" || otherBro == "WESTIN")
                    {
                        WESTIN.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = true;
                    }
                    else { WESTIN.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = false; }



                    //-------------------BROS CHOSEN----------------------
                    READY = false; ChooseBro.SetActive(false);
                    MSG = "CHOOSE A BRO ";
                    //Single Player
                    if (!GetComponent<GameDriver>().twoPlayer)
                    {
                        if (selectedBro.Length > 1)
                        {
                            READY = true;
                            ChooseBro.SetActive(true);
                        }
                    }
                    else
                    {
                        if (TRAVIS.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled && WESTIN.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled)
                        {
                            READY = true; ChooseBro.SetActive(true);
                        }
                        else
                        {
                            if (selectedBro.Length > 1 && otherBro.Length > 1) { MSG = "CANT BE SAME BRO "; }

                        }
                    }
                }
            }
            else //--------------------------STARTED--------------------------------
            {
                if (!startOther)
                {
                    MSG = "WAITING FOR OTHER PLAYER";
                }else
                {
                    NextScene();
                }

            }
        }
    }

    public void NextScene()
    {
        if (selectedBro == "TRAVIS") { GetComponent<GameDriver>().TRAVIS = true; }
        if (selectedBro == "WESTIN") { GetComponent<GameDriver>().TRAVIS = false; }
        SceneManager.LoadScene("SceneMain");
        
    }



    public void BroSelector()
    {
        Debug.Log("BRO SELECTOR " + selectedBro + otherBro);
        //-----------GET SELECTED BRO ------------------ !!!!! ENSURE BOTH BROS HAVE COLLIDERS
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObject = hit.collider.gameObject;
                selectedBro = GetTopParentName(clickedObject);

                Debug.Log(selectedBro);
                GetComponent<NetworkDriver>().sioCom.Instance.Emit("bro", selectedBro.ToString(), true);

            }
        
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 40;
        style.normal.textColor = Color.blue;
        Vector2 textSize = style.CalcSize(new GUIContent(MSG));
        float posX = Screen.width / 2f;
        float posY = 0;
        posY += 100;
        Rect labelRect = new Rect(posX - (textSize.x / 2f), posY - (textSize.y / 2f), textSize.x, textSize.y);
        GUI.Label(labelRect, MSG, style);
    }

    //-------------------ANIMATIONS -----------------
    IEnumerator randomAnimations()
    {
        while (true)
        {
            //PLAY RANDOM ANIMATIONS
            TRAVIS.transform.GetChild(0).gameObject.GetComponent<Animator>().Play(animations[UnityEngine.Random.Range(0, animations.Length)], 0);
            WESTIN.transform.GetChild(0).gameObject.GetComponent<Animator>().Play(animations[UnityEngine.Random.Range(0, animations.Length)], 0);
            yield return new WaitForSeconds(3f);
        }
    }
    //----------------HELPER FUNCTION----------------
    public string GetTopParentName(GameObject gameObject)
    {
        Transform parent = gameObject.transform.parent;
        while (parent != null && parent.gameObject.name != "TRAVIS" && parent.gameObject.name != "WESTIN")
        {
            gameObject = parent.gameObject;
            parent = gameObject.transform.parent;
        }
        return gameObject.transform.parent.name;
    }
}

