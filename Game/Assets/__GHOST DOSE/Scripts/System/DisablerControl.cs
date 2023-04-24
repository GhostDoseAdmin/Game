
using System.Collections.Generic;
using UnityEngine;
using GameManager;
using NetworkSystem;
using Newtonsoft.Json;

public class DisablerControl : MonoBehaviour
{
    public List<GameObject> enemyObjects;
    private List<GameObject> emitEnableObjects;
    private List<GameObject> emitDisableObjects;

    GameObject Player;
    GameObject Client;
    private float closestPlayerDist;
    private float timer_delay = 1f;
    private float timer = 0.0f;
    private float disableDistance = 20;

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
                //Debug.Log("----------------------------------------------------------------" + ghost.name);
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
            if (Time.time > timer + timer_delay)
            {
                emitEnableObjects = new List<GameObject>();
                emitDisableObjects = new List<GameObject>();

                foreach (GameObject enemy in enemyObjects)
                {
                    float p1_dist = Vector3.Distance(enemy.gameObject.transform.position, Player.transform.position);
                    float p2_dist = Vector3.Distance(enemy.gameObject.transform.position, Client.transform.position);
                    if (p1_dist < p2_dist) { closestPlayerDist = p1_dist; } else { closestPlayerDist = p2_dist; }

                    //-------------DEACTIVATE----------------------
                    if (closestPlayerDist > disableDistance && enemy.GetComponent<NPCController>().target == null && enemy.activeSelf && enemy.GetComponent<Teleport>().teleport==0)
                    {
                        emitDisableObjects.Add(enemy);
                    }
                    //-------------REACTIVATE------------------
                    if (closestPlayerDist <= disableDistance && !enemy.activeSelf)
                    {
                        if (!enemy.gameObject.activeSelf)
                        {
                            emitEnableObjects.Add(enemy);
                        }

                    }
                    //------------------------------BULK EMIT--------------------------------------
                    Dictionary<string, Dictionary<string, string>> enableObjects = new Dictionary<string, Dictionary<string, string>>();
                    foreach (GameObject obj in GetComponent<DisablerControl>().emitEnableObjects)
                    {
                        {
                            string objName;
                            Dictionary<string, string> propsDict = new Dictionary<string, string>();
                            propsDict.Add("active", "true");
                            objName = obj.name;
                            enableObjects.Add(objName, propsDict);
                            obj.gameObject.SetActive(true);
                        }
                    }

                    foreach (GameObject obj in GetComponent<DisablerControl>().emitDisableObjects)
                    {
                        {
                            string objName;
                            Dictionary<string, string> propsDict = new Dictionary<string, string>();
                            propsDict.Add("active", "false");
                            objName = obj.name;
                            enableObjects.Add(objName, propsDict);
                            obj.gameObject.SetActive(false);
                        }
                    }
                    if (GameDriver.instance.twoPlayer && enableObjects.Count>0) { NetworkDriver.instance.sioCom.Instance.Emit("disable", JsonConvert.SerializeObject(enableObjects), false); }


                }
                timer = Time.time;//cooldown
            }



        }

    }


}
