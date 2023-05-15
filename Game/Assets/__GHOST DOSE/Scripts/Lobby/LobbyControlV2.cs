using GameManager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using NetworkSystem;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.IO;
using SlimUI.ModernMenu;

public class LobbyControlV2 : MonoBehaviour
{
    public string GAMEMODE;
    public string LEVEL;
    public string otherLEVEL;

    public GameObject levelName;
    public GameObject skinName;
    public GameObject loginCanvas;
    public GameObject entryMenuCanvas;
    public GameObject lobbyMenuCanvas;
    public GameObject roomCanvas;
    public GameObject roomNameField;
    public GameObject findRoomButton;
    public GameObject screenMask;
    public GameObject otherUserName;
    public GameObject lobbyMenu;
    public GameObject Carousel;
    
    private float timer = 0f;
    private float rotDuration = 1f;
    private bool foundRoom = false;
    private bool lookingForPlayer;
    public bool READY = false;
    public bool otherREADY = false;
    public void Start()
    {
        NetworkDriver.instance.isTRAVIS = true;
    }

    public void SelectLevel(string level)
    {
        LEVEL = level;
        levelName.GetComponent<TextMeshPro>().text = level;
    }


    public void LoadLobbyCanvas(string gameMode)
    {
        //lobbyMenuCanvas.GetComponentInChildren<UIMenuManager>().SetThemeColors();
        entryMenuCanvas.SetActive(false);
        GAMEMODE = gameMode;

        //if (gameMode == "duo" || gameMode == "duorandom")
        {
            screenMask.SetActive(true);
            roomCanvas.SetActive(true);
            roomNameField.SetActive(true);
            findRoomButton.SetActive(true);
            lookingForPlayer = false;
            if (gameMode == "single") { roomNameField.GetComponent<TMP_InputField>().text = NetworkDriver.instance.USERNAME; FindRoom(); }

        }
       // else { lobbyMenuCanvas.SetActive(true); }
        
        
    }

    public void LeaveRoom()
    {
        //roomNameField.SetActive(true);
        //findRoomButton.SetActive(true);
        foundRoom = false;
        GetComponent<RigManager>().otherPlayerProp.SetActive(false);
        NetworkDriver.instance.Reconnect();
        NetworkDriver.instance.otherUSERNAME = "";
    }

    public void FindRoom()
    {
        string roomName = roomNameField.GetComponent<TMP_InputField>().text;
        if (roomName.Length > 0)
        {
            if (!lookingForPlayer)
            {
                lookingForPlayer = true;
                NetworkDriver.instance.sioCom.Instance.Emit("join", roomName, true);
                roomNameField.SetActive(false);
                findRoomButton.SetActive(false);
                GameDriver.instance.WriteGuiMsg("Joining Room " + roomName, 999f, true, Color.yellow);
            }
        }
        else { GameDriver.instance.WriteGuiMsg("Must Specify Room!" + roomName, 999f, true, Color.red); }
    }
    public void RoomFound()
    {
        roomCanvas.SetActive(false);
        screenMask.SetActive(false);
        lobbyMenuCanvas.SetActive(true);
        foundRoom = true;
    }

    public void Ready()
    {
        if (!READY)
        {
            if (LEVEL.Length > 0)
            {
                if (GAMEMODE == "single") {
                    //UPDATE RIG INFO FOR

                    if (NetworkDriver.instance.isTRAVIS) { NetworkDriver.instance.myRig = GetComponent<RigManager>().travCurrentRig.name; }
                    else { NetworkDriver.instance.myRig = GetComponent<RigManager>().wesCurrentRig.name; }

                    NetworkDriver.instance.theirRig = GetComponent<RigManager>().otherPlayerRig.name;

                    SceneManager.LoadScene(LEVEL); Debug.Log("READY"); 
                }
                //----TWO PLAYER
                else
                {
                    if (!NetworkDriver.instance.TWOPLAYER)
                    {
                        GameDriver.instance.WriteGuiMsg("Waiting for another Player", 5f, true, Color.yellow);
                    }
                    else {
                        //------same bros
                        if (NetworkDriver.instance.isTRAVIS == NetworkDriver.instance.otherIsTravis)
                        {
                            GameDriver.instance.WriteGuiMsg("Can't be the same bro!", 5f, false, Color.red);
                        }
                        else
                        {
                            //----diff levels
                            if (LEVEL == otherLEVEL)
                            {
                                //UPDATE RIG INFO FOR GAMEMANAGER
                                if (NetworkDriver.instance.isTRAVIS) { NetworkDriver.instance.myRig = GetComponent<RigManager>().travCurrentRig.name; }
                                else { NetworkDriver.instance.myRig = GetComponent<RigManager>().wesCurrentRig.name; }

                                NetworkDriver.instance.theirRig = GetComponent<RigManager>().otherPlayerRig.name;

                                READY = true; lobbyMenu.SetActive(false); NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { ready = true }), false); GameDriver.instance.WriteGuiMsg("Waiting for other player...", 999f, true, Color.white);
                            }
                            else { GameDriver.instance.WriteGuiMsg("Need same level!", 5f, true, Color.red); }
                        }
                            
                        
                       

                    } 
                }
            }
            else { GameDriver.instance.WriteGuiMsg("Must Select a Level", 5f, false, Color.yellow); }
        }
    }
    public void OtherReady()
    {
        GameDriver.instance.WriteGuiMsg("Other player is ready!", 5f, false, Color.green);
        otherREADY = true;
    }
    public void EmitSkin()
    {
        if (NetworkDriver.instance.TWOPLAYER) {
            //change to dots for json compat
            string rigName = GetComponent<RigManager>().currentRigName;
            //skinPath = skinPath.Replace("/", ".");
            NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { skin = rigName, NetworkDriver.instance.isTRAVIS }), false); 
        }
    }
    public void EmitLevel()
    {
        if (NetworkDriver.instance.TWOPLAYER && LEVEL.Length>0) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { level = LEVEL }), false); }
    }

    public void UpdateOtherRig(string rigName)
    {
        GetComponent<RigManager>().UpdatePlayerRig(rigName, NetworkDriver.instance.otherIsTravis, true);
    }
    public void UpdateOtherLevel(string level)
    {
        otherLEVEL = level;
        GameDriver.instance.WriteGuiMsg(NetworkDriver.instance.otherUSERNAME + " selected level " + otherLEVEL, 5f, false, Color.yellow);
    }
    public void Update()
    {
        //--------------NEXT SCENE-------------------
        if(READY && otherREADY) { SceneManager.LoadScene(LEVEL); Debug.Log("READY"); }

        //--------------OTHER PLAYER-----------------
        if (foundRoom)
        {
            if (NetworkDriver.instance.otherUSERNAME.Length > 0)
            {
                GetComponent<RigManager>().otherPlayerProp.SetActive(true);
                GameObject otherPlayer = null;
                otherPlayer = GetComponent<RigManager>().otherPlayerProp; 
                otherUserName.GetComponent<TextMeshPro>().text = NetworkDriver.instance.otherUSERNAME;
                Vector3 worldPosition = new Vector3(otherPlayer.transform.position.x, otherPlayer.transform.position.y + 13f, otherPlayer.transform.position.z);
                otherUserName.GetComponent<RectTransform>().position = worldPosition;
            }
        }


        //------------------------UPDATE SELECTED BRO----------------------------
        if (GameObject.Find("SkinsPanel")==null && foundRoom)
        {
            if (Time.time > timer + rotDuration)
            {
                //SELECT BRO
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit))
                    {

                        GameObject clickedObject = hit.collider.gameObject.transform.parent.gameObject;
                        if (clickedObject.name == "TRAVIS" || clickedObject.name == "WESTIN")
                        {
                            //if (clickedObject.name == "TRAVIS") { NetworkDriver.instance.isTRAVIS = true; }
                            //else { NetworkDriver.instance.isTRAVIS = false; }
                            NetworkDriver.instance.isTRAVIS = !NetworkDriver.instance.isTRAVIS;
                            if (NetworkDriver.instance.TWOPLAYER)
                            {
                                //NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { isTRAVIS = NetworkDriver.instance.isTRAVIS }), false);
                                if (NetworkDriver.instance.isTRAVIS) { GetComponent<RigManager>().currentRigName = GetComponent<RigManager>().travCurrentRig.name; }
                                else { GetComponent<RigManager>().currentRigName = GetComponent<RigManager>().wesCurrentRig.name; }
                                GetComponent<RigManager>().currentRigName = GetComponent<RigManager>().currentRigName.Replace("(Clone)", "");
                                EmitSkin();
                            }
                            StartCoroutine(RotateCarousel());
                        }


                    }
                    timer = Time.time;
                }


            }
            IEnumerator RotateCarousel()
            {
                Quaternion startRotation = Carousel.transform.rotation;
                Quaternion endRotation = startRotation * Quaternion.Euler(0f, 180f, 0f);
                Quaternion propRotations = GetComponent<RigManager>().travisProp.transform.rotation;

                float elapsedTime = 0f;

                while (elapsedTime < rotDuration)
                {
                    Carousel.transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / rotDuration);
                    elapsedTime += Time.deltaTime;

                    GetComponent<RigManager>().travisProp.transform.rotation = propRotations;
                    GetComponent<RigManager>().westinProp.transform.rotation = propRotations;
                    yield return null;
                }

                Carousel.transform.rotation = endRotation;


                yield break;
            }
        }
        
    }
}
