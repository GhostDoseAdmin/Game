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

    private string MSG;
    public bool READY = false; 
    private bool chooseBro = false;
    public bool start = false;
    public bool startOther = false; //receives 1 for ready 2 go

    public string selectedBro ="";
    public string otherBro ="";

    private string[] animations = { "Walk_Flashlight", "Pistol_Shot", "Knife_Attack", "Idle", "Running" };
    private string[] travisRigs = { "Prefabs/Rigs/Travis/TravisRigBasic", "Prefabs/Rigs/EnemyRig", "Prefabs/Rigs/BasicDudeRig" };
    private int travisRigIndex = 0;
    private string[] westinRigs = { "Prefabs/Rigs/Westin/WestinRigBasic", "Prefabs/Rigs/EnemyRig", "Prefabs/Rigs/BasicDudeRig" };
    private int westinRigIndex = 0;
    private static utilities util;

    public void Awake()
    {
        if(SceneManager.GetActiveScene().name == "Lobby")
        {
            util = new utilities();
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
                        WESTIN.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = false; 
                        TRAVIS.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = false;
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
                        TRAVIS.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = true;
                    }
                    else { TRAVIS.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = false; }

                    if (selectedBro == "WESTIN" || otherBro == "WESTIN")
                    {
                        WESTIN.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = true;
                    }
                    else { WESTIN.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = false; }



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
                        if (TRAVIS.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled && WESTIN.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled)
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
        if (selectedBro == "TRAVIS") { GetComponent<GameDriver>().isTRAVIS = true; GetComponent<GameDriver>().selectedRig = travisRigs[travisRigIndex]; }
        if (selectedBro == "WESTIN") { GetComponent<GameDriver>().isTRAVIS = false; GetComponent<GameDriver>().selectedRig = westinRigs[westinRigIndex]; }
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

            //-----------------------RIG SELECTOR------------------------------------
            if(selectedBro.ToString()=="TRAVIS")
            {
                //CYCLE THROUGH RIGS ARRAY
                travisRigIndex = (travisRigIndex + 1) % travisRigs.Length;
                //DESTROY CURRENT RIG 
                Destroy(GameObject.Find("TRAVIS").transform.GetChild(0).gameObject.transform.GetChild(0).gameObject);
                //CREATE RIG BASED ON INDEX OF RIGS ARRAY
                GameObject newRig = Instantiate(Resources.Load<GameObject>(travisRigs[travisRigIndex]), GameObject.Find("TRAVIS").transform.GetChild(0).transform);
                //DESTROY OUTLINE 
                DestroyImmediate(GameObject.Find("TRAVIS").transform.GetChild(0).gameObject.GetComponent<Outline>());
                //REFRESH ANIMATOR 
                StartCoroutine(util.ReactivateAnimator(GameObject.Find("TRAVIS").transform.GetChild(0).gameObject));
                //REBUILD OUTLINE 
                GameObject.Find("TRAVIS").transform.GetChild(0).gameObject.AddComponent<Outline>();
                GameObject.Find("TRAVIS").transform.GetChild(0).gameObject.GetComponent<Outline>().OutlineColor = Color.green;
                GameObject.Find("TRAVIS").transform.GetChild(0).gameObject.GetComponent<Outline>().OutlineWidth = 10f;
            }
            if (selectedBro.ToString() == "WESTIN")
            {
                //CYCLE THROUGH RIGS ARRAY
                westinRigIndex = (westinRigIndex + 1) % westinRigs.Length;
                //DESTROY CURRENT RIG 
                Destroy(GameObject.Find("WESTIN").transform.GetChild(0).gameObject.transform.GetChild(0).gameObject);
                //CREATE RIG BASED ON INDEX OF RIGS ARRAY
                GameObject newRig = Instantiate(Resources.Load<GameObject>(westinRigs[westinRigIndex]), GameObject.Find("WESTIN").transform.GetChild(0).transform);
                //DESTROY OUTLINE 
                DestroyImmediate(GameObject.Find("WESTIN").transform.GetChild(0).gameObject.GetComponent<Outline>());
                //REFRESH ANIMATOR 
                StartCoroutine(util.ReactivateAnimator(GameObject.Find("WESTIN").transform.GetChild(0).gameObject));
                //REBUILD OUTLINE 
                GameObject.Find("WESTIN").transform.GetChild(0).gameObject.AddComponent<Outline>();
                GameObject.Find("WESTIN").transform.GetChild(0).gameObject.GetComponent<Outline>().OutlineColor = Color.red;
                GameObject.Find("WESTIN").transform.GetChild(0).gameObject.GetComponent<Outline>().OutlineWidth = 10f;
            }

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

