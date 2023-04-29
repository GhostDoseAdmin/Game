using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using GameManager;
using Newtonsoft.Json;

public class K2 : MonoBehaviour
{
    public GameObject k2wave;
    private Transform shootPoint;
    public bool isClient;
    public float closestEnemyDist;
    private float pulse_timer = 0.0f;
    private float pulse_delay = 2.0f;
    private float strength;
    public GameObject closestEnemy;
    private float k2range = 10f;
    private GameObject hud;
    private void Awake()
    {
        hud =GameObject.Find("K2Hud");
    }
    void Start()
    {
        closestEnemyDist = 20f;//MAX DISTANCE
        if (transform.root.name == "CLIENT") { isClient = true; }
        else if (transform.root.name == "WESTIN" || transform.root.name == "TRAVIS") { isClient = false; }
        else { DestroyImmediate(this.gameObject); }//DEAD PLAYER

        if (!isClient) { shootPoint = GameDriver.instance.Player.GetComponent<ShootingSystem>().shootPoint; } 
        else { shootPoint = GameDriver.instance.Client.GetComponent<ClientPlayerController>().shootPoint; }
    }

    // Update is called once per frame
    void Update()
    {
        //AIMING
        if(!isClient && GameDriver.instance.Player.GetComponent<PlayerController>().gearAim)
        {
            if (Time.time > pulse_timer + pulse_delay)
            {
                fire(false);
                pulse_timer = Time.time;//cooldown
            }
        }


        //BEEP BASED ON AMOUNT OF ENEMIES
        int enemyNum = 0;
        List<GameObject> enemies = GameDriver.instance.GetComponent<DisablerControl>().enemyObjects;
        float closestDist =999f;
        strength = 0f;
        closestEnemy = null;
        foreach (GameObject obj in enemies)
        {
            if (obj.transform.GetChild(0).GetComponent<Outline>().OutlineWidth > 0.1f) { 
                enemyNum++; 
                if(Vector3.Distance(obj.transform.position, this.gameObject.transform.position)< closestDist)
                {
                    closestEnemy = obj;
                }

            }
        }
        //BEEP LEVEL BASED ON DISTANCE
        if (closestEnemy != null) { strength = Mathf.Lerp(0, 5, Mathf.InverseLerp(k2range, 2f, Vector3.Distance(this.gameObject.transform.position, closestEnemy.transform.position))); }
        //BEEEEEP
        strength = Mathf.Clamp(strength + enemyNum, 0, 5f);
        GetComponent<k2beep>().Level = (int)strength;
      
    }

    public void fire(bool otherPlayer)
    {
        GameObject newK2wave = Instantiate(k2wave);
        newK2wave.transform.position = shootPoint.position;
        Quaternion newYRotation = Quaternion.Euler(0f, shootPoint.rotation.eulerAngles.y + 90f, 90f);
        newK2wave.transform.rotation = newYRotation;
        newK2wave.GetComponent<K2Wave>().isClient = isClient;
        newK2wave.GetComponent<K2Wave>().K2Source = this.gameObject;
        newK2wave.GetComponent<K2Wave>().hud = hud;
        if (!otherPlayer) { GameDriver.instance.Player.GetComponent<PlayerController>().fireK2 = true; }//EMIT FIRE
    }
    private void OnDisable()
    {
        GetComponent<k2beep>().Level = 0;
    }
    /*private void OnDisable()
    {
        //----------TURN OFF OUTLINES
        NPCController[] enemies = FindObjectsOfType<NPCController>();
        foreach(NPCController enemy in enemies)
        {
            enemy.gameObject.transform.GetChild(0).GetComponent<Outline>().OutlineWidth = 0; 
        }
    }*/
}
