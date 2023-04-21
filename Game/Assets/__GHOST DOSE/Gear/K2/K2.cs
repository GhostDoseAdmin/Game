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

    private float pulse_timer = 0.0f;
    private float pulse_delay = 2.0f;


    // Start is called before the first frame update
    void Start()
    {
        if (transform.root.name == "CLIENT") { isClient = true; }
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
    }

    public void fire(bool otherPlayer)
    {
        GameObject newK2wave = Instantiate(k2wave);
        newK2wave.transform.position = shootPoint.position;
        Quaternion newYRotation = Quaternion.Euler(0f, shootPoint.rotation.eulerAngles.y + 90f, 90f);
        newK2wave.transform.rotation = newYRotation;
        newK2wave.GetComponent<K2Wave>().isClient = isClient;
        if (!otherPlayer) { GameDriver.instance.Player.GetComponent<PlayerController>().fireK2 = true; }//EMIT FIRE
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