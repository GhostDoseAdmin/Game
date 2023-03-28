using TMPro;
using UnityEngine;


public class ButtonStart : MonoBehaviour
{
    private bool brosChosen =false;

    public void Clicked(string sceneName)
    {
        //CHOOSING ROOM - go button
        if (!GameObject.Find("GameController").GetComponent<GameDriver>().ROOM_VALID)
        {
            string room_text = transform.parent.Find("InputField (TMP)").GetComponent<TMP_InputField>().text;

            if (room_text.Length > 1)
            {
                GameObject.Find("GameController").GetComponent<GameDriver>().ROOM = room_text;
                GameObject.Find("GameController").GetComponent<NetworkDriver>().connected = false;
                GameObject.Find("GameController").GetComponent<NetworkDriver>().Setup();

            }
        }
        //CHOOSING BRO
        else
        {
            

        }
            
    }



}
