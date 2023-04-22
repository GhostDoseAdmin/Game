
using System.Collections.Generic;
using UnityEngine;
using GameManager;
using NetworkSystem;
using Newtonsoft.Json;

public class DisablerControl : MonoBehaviour
{
    public List<GameObject> enemyObjects;
    GameObject Player;
    GameObject Client;
    private float closestPlayerDist;


    private void Awake()
    {
        //-----------------GET A LIST OF ALL ENEMIES-----------------------
        enemyObjects = new List<GameObject>();

        //GameObject[] shadowers = GameObject.FindGameObjectsWithTag("Shadower");
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        /*foreach (GameObject shadower in shadowers)
        {
            if (shadower.GetComponent<NPCController>() != null)
            {
                enemyObjects.Add(shadower);
            }
        }*/
        foreach (GameObject ghost in ghosts)
        {
            if (ghost.GetComponent<NPCController>() != null)
            {
                Debug.Log("----------------------------------------------------------------" + ghost.name);
                enemyObjects.Add(ghost);
            }
        }

    }

    void Start()
    {
        Player = GameDriver.instance.Player;
        Client = GameDriver.instance.Client;
    }

    // Update is called once per frame
    void Update()
    {

        if (NetworkDriver.instance.HOST)
        {
            foreach (GameObject enemy in enemyObjects)
            {
                float p1_dist = Vector3.Distance(enemy.gameObject.transform.position, Player.transform.position);
                float p2_dist = Vector3.Distance(enemy.gameObject.transform.position, Client.transform.position);
                if (p1_dist < p2_dist) { closestPlayerDist = p1_dist; } else { closestPlayerDist = p2_dist; }

                //-------------DEACTIVATE----------------------
                if (closestPlayerDist > 20 && enemy.GetComponent<NPCController>().target == null && enemy.activeSelf )
                {

                    enemy.gameObject.SetActive(false);
                    if (GameDriver.instance.twoPlayer) { NetworkDriver.instance.sioCom.Instance.Emit("disable", JsonConvert.SerializeObject($"{{'obj':'{enemy.name}','active':'false'}}"), false); }
                }
                //-------------REACTIVATE------------------
                if (closestPlayerDist <= 20 && !enemy.activeSelf)
                {
                    if (!enemy.gameObject.activeSelf)
                    {
                        if (GameDriver.instance.twoPlayer) { NetworkDriver.instance.sioCom.Instance.Emit("disable", JsonConvert.SerializeObject($"{{'obj':'{enemy.name}','active':'true'}}"), false); }
                        enemy.gameObject.SetActive(true);

                    }

                }

            }

        }

    }


}

