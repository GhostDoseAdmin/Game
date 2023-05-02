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
    public GameObject zozoEffectEnd;
    public GameObject zozoEffectMid;
    public GameObject zozoDummy;
    Vector3 domeStartSize;
    Vector3 zozoSpawnStartPos;
    Vector3 zozoSpawnStartSize;
    Vector3 zozoDummyStartPos;
    Vector3 zozoEffectMidStartSize;
    Vector3 zozoEffectEndStartSize;
    private bool zozo;
    private bool zozoRise;
    private float zozoAlpha;
    private bool zozoEnd;

    Vector3 mainStartPos;

    // Start is called before the first frame update
    void Start()
    {
        ChooseVictim();
        mainStartPos = main.transform.position;
        domeStartSize = effectDome.transform.localScale;
        zozoSpawnStartSize = zozoSpawn.transform.localScale;
        zozoSpawnStartPos = zozoSpawn.transform.position;
        zozoDummyStartPos = zozoDummy.transform.position;
        zozoEffectMidStartSize = zozoEffectMid.transform.localScale;
        zozoEffectEndStartSize = zozoEffectEnd.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {

        //START EFFECT
        if (playerOn || clientOn)
        {
            effectInner.SetActive(true);
        }
        else { effectInner.SetActive(false); }


       
        
        if (startCircle)
        {
            main.transform.Rotate(0f, 5f * Time.deltaTime, 0f);

            //ELEVATE DEAD
            if(main.transform.position.y < mainStartPos.y+3)
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


       //ZOZO SPAWN
        if(zozo && !zozoEnd)
        {
            if (effectDome.transform.localScale.x > 0.01) { effectDome.transform.localScale = Vector3.Lerp(effectDome.transform.localScale, effectDome.transform.localScale * 0.0007f, Time.deltaTime * 1); }
            //ARISE GATE
            if (zozoSpawn.transform.position.y < zozoSpawnStartPos.y + 6.5)
            {
                Vector3 currPos = zozoSpawn.transform.position;
                currPos.y += 0.007f;
                zozoSpawn.transform.position = currPos;
            }
            else
            {   //EXPAND
                if (zozoSpawn.transform.localScale.x < 30) { zozoSpawn.transform.localScale = Vector3.Lerp(zozoSpawn.transform.localScale, zozoSpawn.transform.localScale * 1.05f, Time.deltaTime * 1); }
                    //END FX
                    else {
                    if (zozoEffectEnd.transform.localScale.x < 100) { zozoEffectEnd.transform.localScale = Vector3.Lerp(zozoEffectEnd.transform.localScale, zozoEffectEnd.transform.localScale * 2f, Time.deltaTime * 1); }
                }
                //PUSH AWAY PLAYER
                if (Vector3.Distance(GameDriver.instance.Player.transform.position, transform.position) < 3)
                {
                    GameDriver.instance.Player.transform.position = Vector3.Lerp(GameDriver.instance.Player.transform.position,
                        GameDriver.instance.Player.transform.position + (GameDriver.instance.Player.transform.position - transform.position).normalized * 3, 4f * Time.deltaTime);
                }

            }
            //WHITE RAYS MID EFFECT
           if (zozoEffectMid.activeSelf == true) { if (zozoEffectMid.transform.localScale.x < 3) { zozoEffectMid.transform.localScale = Vector3.Lerp(zozoEffectMid.transform.localScale, zozoEffectMid.transform.localScale * 1.5f, Time.deltaTime * 1); } }
            GameObject.Find("PlayerCamera").GetComponent<Camera_Controller>().InvokeShake(2f, Mathf.InverseLerp(20f, 0f, Vector3.Distance(GameDriver.instance.Player.transform.position,transform.position)));
        }


        if (zozoRise)
        {   
            //ARISE
            if (zozoDummy.transform.position.y < zozoDummyStartPos.y + 8)
            {
                Vector3 currPos = zozoDummy.transform.position;                currPos.y += 0.007f;                zozoDummy.transform.position = currPos;                zozoAlpha = 0.2f;
                currPos = main.transform.position; currPos.y += 0.005f; main.transform.position = currPos;
                main.transform.Rotate(0f, 5f * Time.deltaTime, 0f);

            }
            else {//DONE ARISING
                foreach (GameObject victim in Victims) { victim.GetComponent<Animator>().SetBool("falling", true); }
                Vector3 currPos = main.transform.position; currPos.y -= 0.007f; main.transform.position = currPos;//DROP VICTIMS
                zozoAlpha += 0.0005f;
                if (zozoEffectMid.activeSelf == true) { if (zozoAlpha > 0) { zozoAlpha -= 0.0008f; } }
                zozoDummy.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[0].SetFloat("_EMFAlpha", zozoAlpha);
                zozoDummy.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[1].SetFloat("_EMFAlpha", zozoAlpha);

            }
        }
        //ZOZO ENTRACE END
        if(zozoEnd)
        {
            effectDome.transform.localScale = Vector3.Lerp(effectDome.transform.localScale, effectDome.transform.localScale * 0.6f, Time.deltaTime * 1);
            zozoSpawn.transform.localScale = Vector3.Lerp(zozoSpawn.transform.localScale, zozoSpawn.transform.localScale * 0.6f, Time.deltaTime * 1);
            zozoEffectMid.transform.localScale = Vector3.Lerp(zozoEffectMid.transform.localScale, zozoEffectMid.transform.localScale * 0.6f, Time.deltaTime * 1);
            zozoEffectEnd.transform.localScale = Vector3.Lerp(zozoEffectEnd.transform.localScale, zozoEffectEnd.transform.localScale * 0.8f, Time.deltaTime * 1);
            clientOn = false; playerOn = false;
            AudioManager.instance.StopPlaying("demoncircle", null);

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
        if(startCircle && main.transform.position.y >= mainStartPos.y + 3)
        {
            if (victim == ChosenVictim)
            {

                GameDriver.instance.WriteGuiMsg("RIGHT ANWER", 2f);
            }
            else { SummonZozo(); GameDriver.instance.WriteGuiMsg("WRONG ANWER", 2f); }

           
        }
    }

    public void SummonZozo()
    {
        zozo = true;
        startCircle = false;
        AudioManager.instance.Play("enterzozomusic", null);
        Invoke("SpawnInitialEffect", 15f);
        Invoke("SpawnMidEffect", 39f);
        Invoke("CreateZozo", 45f);//CLIMAX
      

    }
    public void SpawnInitialEffect()
    {
        zozoRise = true;
    }
    public void SpawnMidEffect()
    {
        zozoEffectMid.SetActive(true);
    }

    public void CreateZozo()
    {
        zozoEffectEnd.SetActive(true);
        Invoke("ZoZoHasArrived", 10f);
    }

    public void ZoZoHasArrived()
    {
        zozoEnd = true;
        Invoke("RefreshSpawner", 10f);
    }
    public void RefreshSpawner()
    {
        zozo = false;
        startCircle = false;
        zozoRise = false;
        zozoEnd = false;
        effectInner.SetActive(false);
        zozoEffectMid.SetActive(false);
        zozoEffectEnd.SetActive(false);
        trigger.SetActive(true);
        foreach (GameObject victim in Victims) { victim.GetComponent<Animator>().SetBool("falling", false); }
        main.transform.position = mainStartPos;
        zozoDummy.transform.position = zozoDummyStartPos;
        zozoSpawn.transform.position = zozoSpawnStartPos;
        zozoSpawn.transform.localScale = zozoSpawnStartSize;
        zozoEffectMid.transform.localScale = zozoEffectMidStartSize;
        zozoEffectEnd.transform.localScale = zozoEffectEndStartSize;
        effectDome.transform.localScale = domeStartSize;

        ChooseVictim();

    }
}
