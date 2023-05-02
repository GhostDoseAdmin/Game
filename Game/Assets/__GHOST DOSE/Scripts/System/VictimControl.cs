using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManager;
using UnityEngine.UIElements;
using InteractionSystem;

public class VictimControl : Item
{
    public List<GameObject> Victims;
    private bool targetIsMurdered;
    private bool targetIsEvil;
    private bool targetIsYoung;
    public GameObject ChosenVictim;
    public GameObject trigger;
    private bool startCircle;
    public GameObject main;
    public GameObject effectInner;
    public GameObject effectDome;//DOME
    public GameObject zozoSpawn;
    Vector3 domeStartSize;
    Vector3 zozoSpawnStartPos;
    Vector3 zozoSpawnStartSize;
    private bool zozo;


    Vector3 mainStartPos;

    // Start is called before the first frame update
    void Start()
    {
        ChooseVictim();
        mainStartPos = transform.position;
        domeStartSize = effectDome.transform.localScale;
        zozoSpawnStartSize = zozoSpawn.transform.localPosition;
        zozoSpawnStartPos = zozoSpawn.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject victim in Victims)
        {
           // victim.GetComponent<Animator>().Play("React");
        }
        
        if (startCircle)
        {
            main.transform.Rotate(0f, 5f * Time.deltaTime, 0f);

            //ELEVATE DEAD
            if(main.transform.position.y < mainStartPos.y+1)
            {
                Vector3 currPos = main.transform.position;
                currPos.y += 0.01f;
                main.transform.position = currPos;//descend

                //EXPAND DOME
                if (effectDome.transform.localScale.x < 1) { effectDome.transform.localScale = Vector3.Lerp(effectDome.transform.localScale, effectDome.transform.localScale * 3f, Time.deltaTime * 1); }
            }
            //KEEP IN CIRCLE
            if (Vector3.Distance(GameDriver.instance.Player.transform.position, transform.position) > 3) { GameDriver.instance.Player.transform.position = Vector3.Lerp(GameDriver.instance.Player.transform.position, transform.position, 0.02f); }
            //if (GameDriver.instance.twoPlayer) { if (Vector3.Distance(GameDriver.instance.Client.transform.position, transform.position) > 3) { GameDriver.instance.Client.transform.position = Vector3.Lerp(GameDriver.instance.Client.transform.position, transform.position, 0.02f); } }
        }


        //START EFFECT
        if (playerOn || clientOn)
        {
            effectInner.SetActive(true);
        }
        else { effectInner.SetActive(false); }

        //ZOZO SPAWN
        if(zozo)
        {
            if (effectDome.transform.localScale.x > 0.01) { effectDome.transform.localScale = Vector3.Lerp(effectDome.transform.localScale, effectDome.transform.localScale * 0.0007f, Time.deltaTime * 1); }
            //ARISE
            if (zozoSpawn.transform.position.y < zozoSpawnStartPos.y + 6)
            {
                Vector3 currPos = zozoSpawn.transform.position;
                currPos.y += 0.01f;
                zozoSpawn.transform.position = currPos;
            }
            else
            {   //EXPAND
                if (zozoSpawn.transform.localScale.x < 20) { zozoSpawn.transform.localScale = Vector3.Lerp(zozoSpawn.transform.localScale, zozoSpawn.transform.localScale * 1.1f, Time.deltaTime * 1); }
                //PUSH AWAY PLAYER
                if (Vector3.Distance(GameDriver.instance.Player.transform.position, transform.position) < 10)
                {
                    GameDriver.instance.Player.transform.position = Vector3.Lerp(GameDriver.instance.Player.transform.position,
                        GameDriver.instance.Player.transform.position + (GameDriver.instance.Player.transform.position - transform.position).normalized * 3, 1f * Time.deltaTime);
                }

            }


            GameObject.Find("PlayerCamera").GetComponent<Camera_Controller>().InvokeShake(2f, Mathf.InverseLerp(20f, 0f, Vector3.Distance(GameDriver.instance.Player.transform.position,transform.position)));
        }





    }

    public void ChooseVictim()
    {
        ChosenVictim = Victims[Random.Range(0, Victims.Count)];
    }

    public override void ActivateObject(bool otherPlayer)
    {
        if (!zozo)
        {
            if ((playerOn && clientOn && GameDriver.instance.twoPlayer) || (!GameDriver.instance.twoPlayer && playerOn))
            {
                startCircle = true;
                AudioManager.instance.Play("demoncircle", null);
                GameDriver.instance.WriteGuiMsg("Beware to summon ZOZO", 5f);
                trigger.SetActive(false);
            }
            else { GameDriver.instance.WriteGuiMsg("Both Players must be present!", 5f); }
        }


    }

    public void testAnswer(GameObject victim)
    {
        if(startCircle)
        {
            if (victim == ChosenVictim)
            {

                GameDriver.instance.WriteGuiMsg("RIGHT ANWER", 2f);
            }
            else { SummonZozo(); GameDriver.instance.WriteGuiMsg("WRONG ANWER", 2f); }

            ChooseVictim();
        }
    }

    public void SummonZozo()
    {
        zozo = true;
        startCircle = false;
        AudioManager.instance.Play("enterzozomusic", null);
    }
}
