using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using System;
using Newtonsoft.Json;
using GameManager;
using NetworkSystem;
using UnityEngine.UI;
using TMPro;

public class LobbyControl : MonoBehaviour
{
    public GameObject ChooseRoom;
    public GameObject ChooseBro;
    private GameObject WESTIN;
    private GameObject TRAVIS;
    public GameObject roomField;

    private string MSG;
    public bool READY = false; 
    private bool chooseBro = false;
    public bool start = false;
    public bool startOther = false; //receives 1 for ready 2 go

    public string selectedBro ="";
    public string otherBro ="";

    private string[] animations = { "Walk_Flashlight", "Pistol_Shot", "Knife_Attack", "Idle", "Running" };

    public int otherIndex; //used to determine other persons rig choice
    public bool otherSelects; //used to toggle a selection action from the other play to update model

    private static utilities util;
    
    public void Start()
    {

 
        if(SceneManager.GetActiveScene().name == "Lobby")
        {
            util = new utilities();
            WESTIN = GameObject.Find("WESTIN");
            TRAVIS = GameObject.Find("TRAVIS");

            ChooseRoom.SetActive(false);
            ChooseBro.SetActive(false);
            StartCoroutine(randomAnimations());
        }


    }

    public bool checkingRoom = false;
    public void Clicked(string sceneName)
    {
       /* Debug.Log("CLICKING");
        //CHOOSING ROOM - go button
        if (!GameDriver.instance.ROOM_VALID)
        {
            string room_text = roomField.GetComponent<TMP_InputField>().text;

            if (room_text.Length > 1 && !checkingRoom)
            {
                GameDriver.instance.ROOM = room_text;
                NetworkDriver.instance.sioCom.Instance.Emit("join", room_text, true);
                GameDriver.instance.WriteGuiMsg("Checking Room " + GameDriver.instance.ROOM, 999f, true, Color.white);
                //NetworkDriver.instance.connected = false;
                //NetworkDriver.instance.NetworkSetup();

            }
        }
        //CHOOSING BRO
        else
        {
            if (GameDriver.instance.twoPlayer)
            {
                NetworkDriver.instance.sioCom.Instance.Emit("start", "true", true);
                GameDriver.instance.GetComponent<LobbyControl>().start = true;
                gameObject.SetActive(false);
            }
            else
            {
                GameDriver.instance.GetComponent<LobbyControl>().NextScene();
            }

        }
       */
    }
    void Update()
    {

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
           
            if (!start)
            {
                
                // GO TO CHOOSE BRO
               /* if (GameDriver.instance.GetComponent<GameDriver>().ROOM_VALID)
                {
                    
                    if (!chooseBro)
                    {
                        ChooseRoom.SetActive(false);
                        ChooseBro.SetActive(true);
                        WESTIN.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = false; 
                        TRAVIS.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled = false;
                        roomField.SetActive(false);
                        chooseBro = true;
                    }

                }
               */
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
                    if (!NetworkDriver.instance.TWOPLAYER)
                    {
                        if (selectedBro.Length > 1)
                        {
                            READY = true;
                            ChooseBro.SetActive(true);
                        }
                    }
                    //TWO PLAYER
                    else
                    {   //BOTH BROS ARE SELECTED
                        if (TRAVIS.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled && WESTIN.transform.GetChild(0).gameObject.GetComponent<Outline>().enabled)
                        {
                            READY = true; ChooseBro.SetActive(true);
                        }
                        else
                        {//EACH BRO SELECTED
                            if (selectedBro.Length > 1 && otherBro.Length > 1) { 
                                MSG = "CANT BE SAME BRO ";
                                if (startOther && otherBro.ToString() == "TRAVIS")
                                {
                                    MSG = "BRO TAKEN ";
                                }
                            }
                        }
                        //PREVENT USING OTHER PLAYER SKIN
                        if (GameDriver.instance.GetComponent<RigManager>().travCurrRig >= GameDriver.instance.GetComponent<RigManager>().travRigCap && selectedBro.ToString() == "TRAVIS") { READY = false; ChooseBro.SetActive(false); MSG = "SKIN NOT AVAILABLE TO YOU "; if (startOther) { MSG = "BRO TAKEN "; } }
                        if (GameDriver.instance.GetComponent<RigManager>().wesCurrRig >= GameDriver.instance.GetComponent<RigManager>().wesRigCap && selectedBro.ToString() == "WESTIN") { READY = false; ChooseBro.SetActive(false); MSG = "SKIN NOT AVAILABLE TO YOU "; if (startOther) { MSG = "BRO TAKEN "; } }
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
        StopCoroutine(randomAnimations());
        if (selectedBro == "TRAVIS") { NetworkDriver.instance.isTRAVIS = true;  }
        if (selectedBro == "WESTIN") { NetworkDriver.instance.isTRAVIS = false;  }
        SceneManager.LoadScene(1);
        
    }



    public void BroSelector()
    {
        //if (start) { return; }--------------------------------------------------------------------------------WORK ON THIS WANNA SEE OTHER PLAYER STILL SELECTING;-----------------------------------------------------------------
        Debug.Log("BRO SELECTOR " + selectedBro + otherBro);
        if (otherSelects) { Debug.Log("OTHER BRO SELECTED "); }
        int emitThisRig =0;

         //-----------GET SELECTED BRO ------------------ !!!!! ENSURE BOTH BROS HAVE COLLIDERS
           RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit) || otherSelects)
            {
                if (!otherSelects)
                {
                    GameObject clickedObject = hit.collider.gameObject;
                    selectedBro = GetTopParentName(clickedObject);
                }

            //-----------------------RIG SELECTOR------------------------------------
            if((selectedBro.ToString()=="TRAVIS" && !otherSelects ) || (otherBro.ToString() == "TRAVIS") && otherSelects)
            {
                if (startOther && otherBro.ToString() == "TRAVIS") { return; }//already took bro, can no longer change
                //CYCLE THROUGH RIGS ARRAY
                GameDriver.instance.GetComponent<RigManager>().travCurrRig = (GameDriver.instance.GetComponent<RigManager>().travCurrRig + 1) % GameDriver.instance.GetComponent<RigManager>().travRigCap;
                //DESTROY CURRENT RIG 
                Destroy(GameObject.Find("TRAVIS").transform.GetChild(0).gameObject.transform.GetChild(0).gameObject);
                //CREATE RIG BASED ON INDEX OF RIGS ARRAY
                if (otherSelects) {Instantiate(GameDriver.instance.GetComponent<RigManager>().travRigList[otherIndex], GameObject.Find("TRAVIS").transform.GetChild(0).transform); }
                else {Instantiate(GameDriver.instance.GetComponent<RigManager>().travRigList[GameDriver.instance.GetComponent<RigManager>().travCurrRig], GameObject.Find("TRAVIS").transform.GetChild(0).transform); }
                //DESTROY OUTLINE 
                DestroyImmediate(GameObject.Find("TRAVIS").transform.GetChild(0).gameObject.GetComponent<Outline>());
                //REFRESH ANIMATOR 
                StartCoroutine(util.ReactivateAnimator(GameObject.Find("TRAVIS").transform.GetChild(0).gameObject));
                //REBUILD OUTLINE 
                GameObject.Find("TRAVIS").transform.GetChild(0).gameObject.AddComponent<Outline>();
                GameObject.Find("TRAVIS").transform.GetChild(0).gameObject.GetComponent<Outline>().OutlineColor = Color.green;
                GameObject.Find("TRAVIS").transform.GetChild(0).gameObject.GetComponent<Outline>().OutlineWidth = 10f;

                emitThisRig = GameDriver.instance.GetComponent<RigManager>().travCurrRig;
                if (!otherSelects) { GameDriver.instance.GetComponent<GameDriver>().mySelectedRig = GameDriver.instance.GetComponent<RigManager>().travRigList[GameDriver.instance.GetComponent<RigManager>().travCurrRig]; }
                else { GameDriver.instance.GetComponent<RigManager>().travCurrRig = otherIndex; GameDriver.instance.GetComponent<GameDriver>().theirSelectedRig = GameDriver.instance.GetComponent<RigManager>().travRigList[GameDriver.instance.GetComponent<RigManager>().travCurrRig]; }

            }
            if ((selectedBro.ToString() == "WESTIN" && !otherSelects) || (otherBro.ToString() == "WESTIN") && otherSelects)
            {
                if (startOther && otherBro.ToString() == "WESTIN") { return; }//already took bro, can no longer change
                //CYCLE THROUGH RIGS ARRAY
                GameDriver.instance.GetComponent<RigManager>().wesCurrRig = (GameDriver.instance.GetComponent<RigManager>().wesCurrRig + 1) % GameDriver.instance.GetComponent<RigManager>().travRigCap;
                //DESTROY CURRENT RIG 
                Destroy(GameObject.Find("WESTIN").transform.GetChild(0).gameObject.transform.GetChild(0).gameObject);
                //CREATE RIG BASED ON INDEX OF RIGS ARRAY
                if (otherSelects) { Instantiate(GameDriver.instance.GetComponent<RigManager>().wesRigList[otherIndex], GameObject.Find("WESTIN").transform.GetChild(0).transform); }
                else { Instantiate(GameDriver.instance.GetComponent<RigManager>().wesRigList[GameDriver.instance.GetComponent<RigManager>().wesCurrRig], GameObject.Find("WESTIN").transform.GetChild(0).transform); }
                //DESTROY OUTLINE 
                DestroyImmediate(GameObject.Find("WESTIN").transform.GetChild(0).gameObject.GetComponent<Outline>());
                //REFRESH ANIMATOR 
                StartCoroutine(util.ReactivateAnimator(GameObject.Find("WESTIN").transform.GetChild(0).gameObject));
                //REBUILD OUTLINE 
                GameObject.Find("WESTIN").transform.GetChild(0).gameObject.AddComponent<Outline>();
                GameObject.Find("WESTIN").transform.GetChild(0).gameObject.GetComponent<Outline>().OutlineColor = Color.red;
                GameObject.Find("WESTIN").transform.GetChild(0).gameObject.GetComponent<Outline>().OutlineWidth = 10f;

                emitThisRig = GameDriver.instance.GetComponent<RigManager>().wesCurrRig;
                if (!otherSelects) { GameDriver.instance.GetComponent<GameDriver>().mySelectedRig = GameDriver.instance.GetComponent<RigManager>().wesRigList[GameDriver.instance.GetComponent<RigManager>().wesCurrRig]; }
                else { GameDriver.instance.GetComponent<RigManager>().wesCurrRig = otherIndex; GameDriver.instance.GetComponent<GameDriver>().theirSelectedRig = GameDriver.instance.GetComponent<RigManager>().wesRigList[GameDriver.instance.GetComponent<RigManager>().wesCurrRig]; }
            }

            //EMIT BRO CHOICE
            if (!otherSelects)
            {
                string emitting = $"{{'bro':'{selectedBro.ToString()}','index':'{emitThisRig}'}}";
                Debug.Log("EMMITING BRO " + emitting);
                GameDriver.instance.GetComponent<NetworkDriver>().sioCom.Instance.Emit("bro", JsonConvert.SerializeObject(emitting), false);
            }
        }
        otherSelects = false;
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

