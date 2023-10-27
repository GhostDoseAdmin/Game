using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using TMPro;
using JetBrains.Annotations;

public class EndGameControl : MonoBehaviour
{
    private bool hasRetrievedLeaderboard = false;
    private string levelData;
    public GameObject elapsedTime, highScore, unlockSkinsText, unlockSkinsPanel, leaderBoardPanel, missionFailed;
    void Start()
    {
        //test
        // NetworkDriver.instance.LEVELINDEX = 1;
        // NetworkDriver.instance.timeElapsed = 50;
        // NetworkDriver.instance.GetComponent<RigManager>().leveldata[NetworkDriver.instance.LEVELINDEX] = 999;
        levelData = "level" + NetworkDriver.instance.LEVELINDEX + "speed";

        if (NetworkDriver.instance.lostGame)
        {
            missionFailed.SetActive(true);
            if (NetworkDriver.instance.endGameDisconnect != null) { missionFailed.GetComponent<TextMeshPro>().text = NetworkDriver.instance.endGameDisconnect; }
            highScore.SetActive(false);
            elapsedTime.SetActive(false);
        }
        else
        {
            float PREVSPEEDSCORE = NetworkDriver.instance.GetComponent<RigManager>().leveldata[NetworkDriver.instance.LEVELINDEX];
            if (PREVSPEEDSCORE == -1) { PREVSPEEDSCORE = 9999999; }//NO DATA, SET HIGH FOR SKIN UNLOCKS

            Debug.Log("LEVEL SCORE " + PREVSPEEDSCORE);
            elapsedTime.GetComponent<TextMeshPro>().text = "TIME: " + NetworkDriver.instance.timeElapsed.ToString("F2") + " s";
            highScore.GetComponent<TextMeshPro>().text = "Highscore: " + PREVSPEEDSCORE.ToString("F2") + " s";

            //New highscore
            if (NetworkDriver.instance.timeElapsed < PREVSPEEDSCORE)
            {

                NetworkDriver.instance.sioCom.Instance.Emit("set_level_speed", JsonConvert.SerializeObject(new { username = NetworkDriver.instance.USERNAME, level = levelData, speed = NetworkDriver.instance.timeElapsed.ToString("F2") }), false);
                bool isNewHighScore = NetworkDriver.instance.GetComponent<RigManager>().UnlockSkins(unlockSkinsPanel, PREVSPEEDSCORE, NetworkDriver.instance.timeElapsed);
                if (isNewHighScore)
                {
                    highScore.GetComponent<TextMeshPro>().text = "NEW HIGH SCORE!! " + NetworkDriver.instance.timeElapsed.ToString("F2");
                    unlockSkinsText.SetActive(true);
                }

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
        //LEADERBOARD POPULATES IN NETWORK DRIVER!!!
        if(!hasRetrievedLeaderboard)
        {
            NetworkDriver.instance.getLeaderboard = true;
            leaderBoardPanel.SetActive(true);
            NetworkDriver.instance.sioCom.Instance.Emit("get_leaderboard", levelData , true);
            hasRetrievedLeaderboard = true;
        }
    }

    public void BackToLobby()
    {
        NetworkDriver.instance.ResetGame();

    }
}
