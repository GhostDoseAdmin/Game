using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameManager;
using NetworkSystem;

public class ButtonStart : MonoBehaviour
{

    public void Clicked(string sceneName)
    {
        Debug.Log("CLICKING");
        //CHOOSING ROOM - go button
        if (!GameDriver.instance.ROOM_VALID)
        {
            string room_text = GameObject.Find("InputField (TMP)").GetComponent<TMP_InputField>().text;

            if (room_text.Length > 1)
            {
                GameDriver.instance.ROOM = room_text;
                NetworkDriver.instance.connected = false;
                NetworkDriver.instance.NetworkSetup();

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
            else {
                GameDriver.instance.GetComponent<LobbyControl>().NextScene();
            }

            

        }
            
    }



}
