using GameManager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using NetworkSystem;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.IO;

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

    public GameObject Carousel;
    
    private float timer = 0f;
    private float rotDuration = 1f;
    private bool foundRoom = false;
    private bool lookingForPlayer;
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
        NetworkDriver.instance.Reconnect();
    }

    public void FindRoom()
    {
        string roomName = roomNameField.GetComponent<TMP_InputField>().text;
        if (!lookingForPlayer) { 
            lookingForPlayer = true; 
            NetworkDriver.instance.sioCom.Instance.Emit("join", roomName, true);
            roomNameField.SetActive(false);
            findRoomButton.SetActive(false);
            GameDriver.instance.WriteGuiMsg("Joining Room " + roomName, 999f, true, Color.yellow);
        }
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

        if (GAMEMODE == "single") { SceneManager.LoadScene(LEVEL); Debug.Log("READY"); }
        else
        {
            if (!NetworkDriver.instance.TWOPLAYER)
            {
                GameDriver.instance.WriteGuiMsg("Waiting for another Player", 5f, true, Color.yellow);
            }
            else { SceneManager.LoadScene(LEVEL); Debug.Log("READY"); }
        }
    }

    public void EmitSkin()
    {
        if (NetworkDriver.instance.TWOPLAYER) {
            //change to dots for json compat
            string skinPath = GetComponent<RigManager>().currentRigPath;
            skinPath = skinPath.Replace("/", ".");
            NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { skin = skinPath }), false); 
        }
    }
    public void EmitLevel()
    {
        if (NetworkDriver.instance.TWOPLAYER) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { level = LEVEL }), false); }
    }

    public void UpdateOtherRig(string rigName)
    {
        //string path = rigPath;
        //path = path.Replace(".", "/");
        if (NetworkDriver.instance.isTRAVIS && !NetworkDriver.instance.otherIsTravis) {
            GetComponent<RigManager>().UpdatePlayerRig(null, Resources.Load<GameObject>(rigName), false, true);
                }
        if (!NetworkDriver.instance.isTRAVIS && NetworkDriver.instance.otherIsTravis)
        {
            GetComponent<RigManager>().UpdatePlayerRig(null, Resources.Load<GameObject>(rigName), false, true);
        }
    }
    public void UpdateOtherLevel(string level)
    {
        otherLEVEL = level;
    }
    public void Update()
    {

        //--------------NAMETAG-----------------
        if (NetworkDriver.instance.otherUSERNAME.Length > 0)
        {
            GameObject otherPlayer = null;
            if (NetworkDriver.instance.otherIsTravis) { otherPlayer = GetComponent<RigManager>().travisProp; } else { otherPlayer = GetComponent<RigManager>().westinProp; }
            otherUserName.GetComponent<TextMeshPro>().text =  NetworkDriver.instance.otherUSERNAME;
            Vector3 worldPosition = new Vector3(otherPlayer.transform.position.x, otherPlayer.transform.position.y + 13f, otherPlayer.transform.position.z);
            otherUserName.GetComponent<RectTransform>().position = worldPosition;
        }


        //------------------------SELECT BRO----------------------------
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
                            if (clickedObject.name == "TRAVIS") { NetworkDriver.instance.isTRAVIS = true; }
                            else { NetworkDriver.instance.isTRAVIS = false; }
                            NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { isTRAVIS = NetworkDriver.instance.isTRAVIS }), false);
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
