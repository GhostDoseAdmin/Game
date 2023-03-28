using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonStart : MonoBehaviour
{

    public void Clicked(string sceneName)
    {
        Debug.Log("CLICKING");
        //CHOOSING ROOM - go button
        if (!GameObject.Find("GameController").GetComponent<GameDriver>().ROOM_VALID)
        {
            string room_text = GameObject.Find("InputField (TMP)").GetComponent<TMP_InputField>().text;

            if (room_text.Length > 1)
            {
                GameObject.Find("GameController").GetComponent<GameDriver>().ROOM = room_text;
                GameObject.Find("GameController").GetComponent<NetworkDriver>().connected = false;
                GameObject.Find("GameController").GetComponent<NetworkDriver>().NetworkSetup();

            }
        }
        //CHOOSING BRO
        else
        {
            if (GameObject.Find("GameController").GetComponent<GameDriver>().twoPlayer)
            {
                GameObject.Find("GameController").GetComponent<NetworkDriver>().sioCom.Instance.Emit("start", "true", true);
                GameObject.Find("GameController").GetComponent<LobbyControl>().start = true;
                gameObject.SetActive(false);
            }
            else {
                GameObject.Find("GameController").GetComponent<LobbyControl>().NextScene();
            }

            

        }
            
    }



}
