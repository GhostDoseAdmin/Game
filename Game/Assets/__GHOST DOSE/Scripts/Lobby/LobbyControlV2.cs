using GameManager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using NetworkSystem;
using UnityEngine.SceneManagement;

public class LobbyControlV2 : MonoBehaviour
{
    public string GAMEMODE;
    public string LEVEL;

    public GameObject levelName;
    public GameObject skinName;
    public GameObject loginCanvas;
    public GameObject entryMenuCanvas;
    public GameObject lobbyMenuCanvas;
    public GameObject roomCanvas;
    public GameObject roomNameField;
    public GameObject findRoomButton;
    public GameObject screenMask;

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
            if (gameMode == "single") { roomNameField.GetComponent<TMP_InputField>().text = NetworkDriver.instance.USERNAME; FindRoom(); }

        }
       // else { lobbyMenuCanvas.SetActive(true); }
        
        
    }

    public void FindRoom()
    {
        string roomName = roomNameField.GetComponent<TMP_InputField>().text;
        if (!lookingForPlayer) { 
            lookingForPlayer = true; 
            NetworkDriver.instance.sioCom.Instance.Emit("join", roomName, true);
            roomNameField.SetActive(false);
            findRoomButton.SetActive(false);
            GameDriver.instance.WriteGuiMsg("Joining Room " + roomName, 5f, true, Color.yellow);
        }
    }
    public void RoomFound()
    {
        screenMask.SetActive(false);
        lobbyMenuCanvas.SetActive(true);
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

    public void Update()
    {






        //------------------------SELECT BRO----------------------------
        if(GameObject.Find("SkinsPanel")==null && foundRoom)
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
