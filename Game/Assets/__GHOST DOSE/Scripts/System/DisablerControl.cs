
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

    public float closestPlayerDist;
    private float timer_delay = 1f;
    private float timer = 0.0f;
    public float disableDistance = 20;
    public float p1dist, p2dist;

    private void Awake()
    {
        //-----------------GET A LIST OF ALL ENEMIES-----------------------
        enemyObjects = new List<GameObject>();

        //GameObject[] shadowers = GameObject.FindGameObjectsWithTag("Shadower");
        NPCController[] ghosts = FindObjectsOfType<NPCController>();

        foreach (NPCController ghost in ghosts)
        {
            if (ghost.GetComponent<NPCController>() != null)
            {
                //Debug.Log("----------------------------------------------------------------" + ghost.name);
                enemyObjects.Add(ghost.gameObject);
                //ghost.SetActive(false);
            }
        }

    }


    // Update is called once per frame
    void LateUpdate()
    {


        if (NetworkDriver.instance.HOST)
        {
            if (Time.time > timer + timer_delay)
            {
                emitEnableObjects = new List<GameObject>();
                emitDisableObjects = new List<GameObject>();

                foreach (GameObject enemy in enemyObjects)
                {
                    float p1_dist = Vector3.Distance(enemy.transform.position, GameDriver.instance.Player.transform.position);
                    float p2_dist = Vector3.Distance(enemy.transform.position, GameDriver.instance.Client.transform.position);
                    if (p1_dist < p2_dist) { closestPlayerDist = p1_dist; } else { closestPlayerDist = p2_dist; }

                    //-------------DEACTIVATE----------------------
                    if (closestPlayerDist > disableDistance && enemy.GetComponent<NPCController>().target == null && enemy.activeSelf && enemy.GetComponent<Teleport>().teleport==0)
                    {
                         Debug.Log("DISABLING"); 
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
                            if (!NetworkDriver.instance.HOST) { Debug.Log("ENABLING"); }
                            /*string objName;
                            Dictionary<string, string> propsDict = new Dictionary<string, string>();
                            propsDict.Add("active", "true");
                            objName = obj.name;
                            enableObjects.Add(objName, propsDict);*/
                            obj.gameObject.SetActive(true);
                        }
                    }

                    foreach (GameObject obj in GetComponent<DisablerControl>().emitDisableObjects)
                    {
                        {
                            /*string objName;
                            Dictionary<string, string> propsDict = new Dictionary<string, string>();
                            propsDict.Add("active", "false");
                            objName = obj.name;
                            enableObjects.Add(objName, propsDict);*/
                            obj.gameObject.SetActive(false);
                        }
                    }
                   // if (GameDriver.instance.twoPlayer && enableObjects.Count>0) { NetworkDriver.instance.sioCom.Instance.Emit("disable", JsonConvert.SerializeObject(enableObjects), false); }


                }
                timer = Time.time;//cooldown
            }



        }

    }


}

