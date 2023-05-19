using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using TMPro;

public class EndGameControl : MonoBehaviour
{
    private bool hasRetrievedLeaderboard = false;

    public GameObject elapsedTime, highScore, unlockSkinsText, unlockSkinsPanel;
    void Start()
    {
        NetworkDriver.instance.LEVELINDEX = 1;
        NetworkDriver.instance.timeElapsed = 10;
        NetworkDriver.instance.SPEEDSCORE = 90;

        elapsedTime.GetComponent<TextMeshPro>().text = "TIME: " + NetworkDriver.instance.timeElapsed.ToString("F2") ;
        highScore.GetComponent<TextMeshPro>().text = "Highscore: " + NetworkDriver.instance.SPEEDSCORE.ToString("F2");

        //New highscore
        if (NetworkDriver.instance.timeElapsed < NetworkDriver.instance.SPEEDSCORE) { 
            bool isNewHighScore =  NetworkDriver.instance.GetComponent<RigManager>().UnlockSkins(unlockSkinsPanel, NetworkDriver.instance.SPEEDSCORE, NetworkDriver.instance.timeElapsed);
            if(isNewHighScore)
            {
                highScore.GetComponent<TextMeshPro>().text = "NEW HIGH SCORE!! " + NetworkDriver.instance.timeElapsed.ToString("F2");
                unlockSkinsText.SetActive(true);
            }
            
        }

    }

    // Update is called once per frame
    void Update()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }



    public void RetrieveLeaderboard()
    {
        if(hasRetrievedLeaderboard)
        {
            hasRetrievedLeaderboard = false;
        }
    }

    public void BackToLobby()
    {
        NetworkDriver.instance.ResetGame();

    }
}
