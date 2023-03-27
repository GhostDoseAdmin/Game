using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDriver : MonoBehaviour
{
    public string BRO;//which character is the player playing
    public GameObject Player;
    public GameObject Client;

    // Start is called before the first frame update
    void Awake()
    {
        BRO = "Westin";
        SetPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetPlayer()
    {
        {
            Player = GameObject.Find("Player");
            Client = GameObject.Find("Client");
            //Debug.Log("PLAYERS SET " + Player);
        }
    }
}
