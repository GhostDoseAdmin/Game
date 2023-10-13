using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManager;
using UnityEngine.UIElements;
using InteractionSystem;
using Newtonsoft.Json;
using NetworkSystem;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using TMPro;

public class VictimControl : Item
{
    public List<GameObject> Victims;
    public List<GameObject> candles;
    public int candleCount;
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
    public GameObject heaventVFX;
    public GameObject Pentagram;
    public GameObject prefabZozoDeathExplo, electricityDeath;

    //public GameObject pentagramLight;
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
    private bool zozoFXendOn;
    private bool setSpiritsFree;
    public GameObject ZOZO;
    
    private bool canDestroyZozo;
    Vector3 ZOZOstartPos;
    Quaternion ZOZOstartRot;

    Vector3 mainStartPos;
    private float zozoMusicVol;
    private bool fadeMusicOut;
    public bool canTest;
    public float zozoTimer;
    public int maxCandles = 6;
    private bool canStopPlayer = true;
    private bool canStopOther = true;

    public bool TEST = true;
    // Start is called before the first frame update
    void Start()
    {

        RandomVictim(null);
        mainStartPos = main.transform.position;
        domeStartSize = effectDome.transform.localScale;
        zozoSpawnStartSize = zozoSpawn.transform.localScale;
        zozoSpawnStartPos = zozoSpawn.transform.position;
        zozoDummyStartPos = zozoDummy.transform.position;
        zozoEffectMidStartSize = zozoEffectMid.transform.localScale;
        zozoEffectEndStartSize = zozoEffectEnd.transform.localScale;
        ZOZOstartPos = ZOZO.transform.position;
        ZOZOstartRot = ZOZO.transform.rotation;

    }
    private void LateUpdate()
    {
        /*
        if (startCircle)
        {
            //OUJIA ANIMATIONS
            GameDriver.instance.Player.GetComponent<Animator>().SetBool("ouija", true);
            if (NetworkDriver.instance.TWOPLAYER) { GameDriver.instance.Client.GetComponent<Animator>().SetBool("ouija", true); }
        }
        else { 
            GameDriver.instance.Player.GetComponent<Animator>().SetBool("ouija", false);
            if (NetworkDriver.instance.TWOPLAYER) { GameDriver.instance.Client.GetComponent<Animator>().SetBool("ouija", false); }
        }
        */
    }
    // Update is called once per frame
    void Update()
    {
        //CANDLES
        if (TEST) { candleCount = 12; }//--------------------------TEST
        if (NetworkDriver.instance.TWOPLAYER) { maxCandles = 12; }
        else { maxCandles = 6; }
        GameDriver.instance.candleUI.GetComponent<TextMeshProUGUI>().text = candleCount.ToString() + "/" + maxCandles.ToString();
        if (candleCount> maxCandles) { candleCount = maxCandles; }
        float pentaCandles = candleCount;
        if (NetworkDriver.instance.TWOPLAYER) { pentaCandles = candleCount * 0.5f; }
        for (int i = 0; i < pentaCandles; i++) {
           candles[i].SetActive(true);
        }

        //START EFFECT
        if (playerOn || clientOn)
        {
            effectInner.SetActive(true);
        }
        else { effectInner.SetActive(false); }

        if (startCircle)
        {

            if (GameDriver.instance.Player != null) { GameDriver.instance.Player.GetComponent<ShootingSystem>().camBatteryUI.fillAmount = 1; }

            main.transform.Rotate(0f, 5f * Time.deltaTime, 0f);

            //FORCE AIM MOBILE
            if (NetworkDriver.instance.isMobile)
            {
                GameDriver.instance.Player.GetComponent<ShootingSystem>().aiming.GetComponent<Aiming>().aim = true;
                GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.camSup.AIMMODE = true;
                GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.camSup.cameraController.desiredDistance = 3f;
                GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.camSup.yAngleLimitMin = 80f;
                GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.camSup.yAngleLimitMax = 110f;
                GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.camSup.forceCharacterDirection = true;
                GameDriver.instance.Player.GetComponent<PlayerController>().gearAim = true;
            }

            //ELEVATE DEAD
            if (main.transform.position.y < mainStartPos.y + 3)
            {
                canTest = false;
                Vector3 currPos = main.transform.position;
                currPos.y += 0.5f * Time.deltaTime;
                main.transform.position = currPos;//descend

                //EXPAND DOME
                if (effectDome.transform.localScale.x < 1) { effectDome.transform.localScale = Vector3.Lerp(effectDome.transform.localScale, effectDome.transform.localScale * 3f, Time.deltaTime * 1); }
            }
            else { canTest = true; }
            //KEEP IN CIRCLE
            if (ZOZO.activeSelf == false)
            {
                if (Vector3.Distance(GameDriver.instance.Player.transform.position, transform.position) > 3) { GameDriver.instance.Player.transform.position = Vector3.Lerp(GameDriver.instance.Player.transform.position, transform.position, 0.06f); }
            }
        }
        else { GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.camSup.forceCharacterDirection = false; }


       //ZOZO SPAWN
        if(zozo && !zozoEnd)
        {
            //if (effectDome.transform.localScale.x > 0.01) { effectDome.transform.localScale = Vector3.Lerp(effectDome.transform.localScale, effectDome.transform.localScale * 0.0007f, Time.deltaTime * 1); }
            //Expand Dome
            if (effectDome.transform.localScale.x < 12) { effectDome.transform.localScale = Vector3.Lerp(effectDome.transform.localScale, effectDome.transform.localScale * 3f, Time.deltaTime * 1); }
            //KEEP IN DOME
            if (ZOZO.activeSelf == false)
            {
                if (Vector3.Distance(GameDriver.instance.Player.transform.position, ZOZO.transform.position) > 12)
                {
                    GameDriver.instance.Player.transform.position = Vector3.Lerp(GameDriver.instance.Player.transform.position, transform.position, 0.03f);

                }
                if (Vector3.Distance(GameDriver.instance.Player.transform.position, transform.position) > 14) { GameDriver.instance.Player.transform.position = transform.position; }
            }
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
                //PUSH AWAY PLAYER
                if(ZOZO.activeSelf == false)
                {
                    if (Vector3.Distance(GameDriver.instance.Player.transform.position, transform.position) < 3)
                    {
                        GameDriver.instance.Player.transform.position = Vector3.Lerp(GameDriver.instance.Player.transform.position,
                                GameDriver.instance.Player.transform.position + (GameDriver.instance.Player.transform.position - transform.position).normalized * 3, 4f * Time.deltaTime);
                    }
                }
            }
            //WHITE RAYS MID EFFECT
           if (zozoEffectMid.activeSelf == true) { if (zozoEffectMid.transform.localScale.x < 20) { zozoEffectMid.transform.localScale = Vector3.Lerp(zozoEffectMid.transform.localScale, zozoEffectMid.transform.localScale * 1.5f, Time.deltaTime * 1); } }
            GameObject.Find("PlayerCamera").GetComponent<Camera_Controller>().InvokeShake(2f, Mathf.InverseLerp(20f, 0f, Vector3.Distance(GameDriver.instance.Player.transform.position,transform.position)));
        }
            //EXPLOSIONG
            if (zozoFXendOn)
            {
                if (zozoEffectEnd.transform.localScale.x < 100) { zozoEffectEnd.transform.localScale = Vector3.Lerp(zozoEffectEnd.transform.localScale, zozoEffectEnd.transform.localScale * 2f, Time.deltaTime * 1); }
            }

            if (zozoRise)
            {   
                //ARISE
                if (zozoDummy.transform.position.y < zozoDummyStartPos.y + 8)
                {
                    zozoDummy.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[0].SetFloat("_EMFAlpha", 0.2f); zozoAlpha = 0.2f;
                    zozoDummy.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[0].SetFloat("_Alpha", 0);
                    Vector3 currPos = zozoDummy.transform.position;               
                    currPos.y += 7f * Time.deltaTime;                
                    zozoDummy.transform.position = currPos;                
                    currPos = main.transform.position; 
                    currPos.y += 5f * Time.deltaTime; 
                    main.transform.position = currPos;
                    main.transform.Rotate(0f, 5f * Time.deltaTime, 0f);
                }
                else {//DONE ARISING
                    foreach (GameObject victim in Victims) { 
                        victim.GetComponent<Animator>().SetBool("falling", true);
                        if (main.transform.position.y < mainStartPos.y + 3f) { victim.GetComponent<Person>().darkLight.SetActive(true); }
                    }
                    Vector3 currPos = main.transform.position; currPos.y -= 0.007f; main.transform.position = currPos;//DROP VICTIMS
                    zozoAlpha += 0.0005f;
                    if (zozoEffectMid.activeSelf == true) { if (zozoAlpha > 0) { zozoAlpha -= 0.0006f; } }
                    zozoDummy.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[0].SetFloat("_EMFAlpha", zozoAlpha);
                    //zozoDummy.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[0].SetFloat("_Alpha", zozoAlpha);
                }
            }
        //ZOZO ENTRACE END
        if(zozoEnd)
        {

            if (zozoMusicVol < 0.6) { zozoMusicVol += 0.001f; AudioManager.instance.UpdateVolume("zozomusicloop", null, zozoMusicVol); }

            //effectDome.transform.localScale = Vector3.Lerp(effectDome.transform.localScale, effectDome.transform.localScale * 0.6f, Time.deltaTime * 1);
            zozoSpawn.transform.localScale = Vector3.Lerp(zozoSpawn.transform.localScale, zozoSpawn.transform.localScale * 0.6f, Time.deltaTime * 1);
            zozoEffectMid.transform.localScale = Vector3.Lerp(zozoEffectMid.transform.localScale, zozoEffectMid.transform.localScale * 0.6f, Time.deltaTime * 1);
            zozoEffectEnd.transform.localScale = Vector3.Lerp(zozoEffectEnd.transform.localScale, zozoEffectEnd.transform.localScale * 0.7f, Time.deltaTime * 1);
            clientOn = false; playerOn = false;
            AudioManager.instance.StopPlaying("creepywhisper", null);

        }

        //---------------------------SET FREE-----------------------------------
        if(setSpiritsFree)
        {
            main.transform.Rotate(0f, 10f * Time.deltaTime, 0f);
            if (main.transform.position.y < mainStartPos.y + 20)
            {
                if (effectDome.transform.localScale.x > 0.01) { effectDome.transform.localScale = Vector3.Lerp(effectDome.transform.localScale, effectDome.transform.localScale * 0.0007f, Time.deltaTime * 1); }
                if (heaventVFX.transform.localScale.x < 2) { heaventVFX.transform.localScale = Vector3.Lerp(heaventVFX.transform.localScale, heaventVFX.transform.localScale * 1.5f, Time.deltaTime * 1); }
                heaventVFX.GetComponentInChildren<Light>().intensity += 0.01f;
                Vector3 currPos = main.transform.position;
                currPos.y += 0.01f;
                main.transform.position = currPos;//descend
                foreach (GameObject victim in Victims) {
                    victim.GetComponent<Person>().whiteLight.SetActive(true);
                    victim.GetComponent<Animator>().SetBool("flying", true);
                   //if (main.transform.position.y > mainStartPos.y + 4)
                    {
                        Quaternion targetRotation = Quaternion.Euler(-90.0f, victim.transform.rotation.eulerAngles.y, victim.transform.rotation.eulerAngles.z);
                        victim.transform.rotation = Quaternion.Lerp(victim.transform.rotation, targetRotation, 0.7f * Time.deltaTime);
                    }
                }
            }
            else //FADE OUT
            {
                if (heaventVFX.transform.localScale.x > 0) { heaventVFX.transform.localScale = Vector3.Lerp(heaventVFX.transform.localScale, heaventVFX.transform.localScale * 0.05f, Time.deltaTime * 1); }
                heaventVFX.GetComponentInChildren<Light>().intensity -= 0.2f;
            }
            effectInner.SetActive(false);
        }
        //ZOZO FIGHT
        if(ZOZO.activeSelf == true && zozo)
        {
            //KEEP IN DOME
            Vector3 ZOZOpos2d = new Vector3(ZOZO.transform.position.x, GameDriver.instance.Player.transform.position.y, ZOZO.transform.position.z);
            if (!fadeMusicOut)
            {
                //if (!canStop)
                {
                    Vector3 oppositeForce = ZOZO.transform.forward * 300f;
                    oppositeForce.y = 0f; // Set the y component to 0
                    Debug.Log(Vector3.Distance(GameDriver.instance.Player.transform.position, ZOZOpos2d));
                    if (Vector3.Distance(GameDriver.instance.Player.transform.position, ZOZOpos2d) > 15)
                    {
                        if(canStopPlayer)
                        {
                            canStopPlayer = false;
                            Invoke("ResetCanStopPlayer", 1f);
                            GameDriver.instance.Player.GetComponent<HealthSystem>().HealthDamage(10, -oppositeForce, false);
                        }
                    }
                    if (Vector3.Distance(GameDriver.instance.Client.transform.position, ZOZOpos2d) > 15)
                    {
                        if (canStopOther)
                        {
                            canStopOther = false;
                            Invoke("ResetCanStopOther", 1f);
                            GameDriver.instance.Client.GetComponent<ClientPlayerController>().Flinch(-oppositeForce, false);
                        }
                    }
                }
                //if (Vector3.Distance(GameDriver.instance.Player.transform.position, ZOZO.transform.position) > 12) { GameDriver.instance.Player.transform.position = Vector3.Lerp(GameDriver.instance.Player.transform.position, ZOZOpos2d, 0.02f); }
                //if (Vector3.Distance(GameDriver.instance.Player.transform.position, ZOZO.transform.position) > 14) { GameDriver.instance.Player.transform.position = ZOZO.transform.position; }

            }
            //if (Vector3.Distance(GameDriver.instance.Player.transform.position, ZOZO.transform.position) > 12) { GameDriver.instance.Player.GetComponent<Rigidbody>().AddForce((ZOZOpos2d - GameDriver.instance.Player.transform.position).normalized * 1000, ForceMode.Acceleration); }
            effectDome.transform.position = ZOZO.transform.position;
        }
        if (!zozo){if (effectDome.transform.localScale.x > 0.01) { effectDome.transform.localScale = Vector3.Lerp(effectDome.transform.localScale, effectDome.transform.localScale * 0.0007f, Time.deltaTime * 1); }        }

        //DESTROY ZOZO
        if (canDestroyZozo)
        {
            //if (ZOZO.GetComponent<Teleport>().teleport == 2)
            if(NetworkDriver.instance.HOST && ZOZO.GetComponent<ZozoControl>().HP<=0)
            {
                //ZOZO.GetComponent<ZozoControl>().HP = -9999999;
                ZOZO.GetComponent<ZozoControl>().DEAD = true;
                canDestroyZozo = false;
               // if (NetworkDriver.instance.HOST) { 
                    Invoke("DestroyZozo1", 0.01f);
                    if (NetworkDriver.instance.TWOPLAYER) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'destroy','event':'zozo'}}"), false); } 
               // }
            }
        }

        //ZOZO LOOP MUSIC
        if(fadeMusicOut)
        {
            if (zozoMusicVol > 0) { zozoMusicVol -= 0.001f; AudioManager.instance.UpdateVolume("zozomusicloop", null, zozoMusicVol); }
            if(zozoMusicVol <= 0) { AudioManager.instance.StopPlaying("zozomusicloop", null); fadeMusicOut = false; }
        }

    }

    public void RandomVictim(GameObject otherPlayerVictim)
    {
        ChosenVictim = Victims[Random.Range(0, Victims.Count)];
        if(otherPlayerVictim != null) { ChosenVictim = otherPlayerVictim; Debug.Log("RANDOMIZING VICTIM FROM HOST "); } //GameDriver.instance.WriteGuiMsg("RANDOM VICTIM FROM HOST" + otherPlayerVictim.name, 10f);
       // GameDriver.instance.WriteGuiMsg("RANDOM VICTIM " + ChosenVictim.name, 10f, false);
        if (NetworkDriver.instance.HOST && NetworkDriver.instance.TWOPLAYER && NetworkDriver.instance.OTHERS_SCENE_READY) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{ChosenVictim.name}','type':'update','event':'randomvictim'}}"), false); }

    }
    //CANSTOP
    public void ResetCanStopPlayer()
    {
        canStopPlayer = true;
    }
    public void ResetCanStopOther()
    {
        canStopOther = true;
    }

    //START CIRCLE
    public override void ActivateObject(bool otherPlayer)
    {
        if (!zozo)
        {
            if (candleCount >= maxCandles)
            {
                if (GameDriver.instance.Player.GetComponent<HealthSystem>().Health <= 0){ playerOn = true;}
                if (GameDriver.instance.Client.GetComponent<ClientPlayerController>().hp <= 0) { clientOn = true; }

                if ((playerOn && clientOn && NetworkDriver.instance.TWOPLAYER) || (!NetworkDriver.instance.TWOPLAYER && playerOn))
                {
                    ActivateCircle(false);

                }
                else { GameDriver.instance.WriteGuiMsg("Both Players must be present!", 5f, false, Color.yellow); }
            }
            else { GameDriver.instance.WriteGuiMsg("Need more candles!", 5f, false, Color.yellow); }
        }
    }

    public void ActivateCircle(bool otherPlayer)
    {
        startCircle = true;
        AudioManager.instance.Play("creepywhisper", null);
        GameDriver.instance.WriteGuiMsg("Beware: Don't summon ZOZO", 5f, false, Color.yellow);
        Invoke("ShootPrompt",5f);
        trigger.SetActive(false);
        if (NetworkDriver.instance.TWOPLAYER && !otherPlayer) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'update','event':'startcircle'}}"), false); }
        GameDriver.instance.Player.GetComponent<Animator>().SetBool("ouija", true);
        GameDriver.instance.Player.GetComponent<ShootingSystem>().planchette.SetActive(true);
        GameDriver.instance.Player.GetComponent<ShootingSystem>().crosshairs.SetActive(false);
        if (NetworkDriver.instance.TWOPLAYER) { GameDriver.instance.Client.GetComponent<Animator>().SetBool("ouija", true); }
    }
    void ShootPrompt()
    {
        GameDriver.instance.WriteGuiMsg("Shoot the correct lost soul", 10f, false, Color.yellow);
    }

    public GameObject playerChoice, otherPlayerChoice;
    public void testAnswer(GameObject victim, bool otherPlayer)
    {
        //SetSpiritsFree(); return;
         GameDriver.instance.WriteGuiMsg("VICTIM " + victim.name, 5f, false, Color.red);

        bool canChoose = false;

        if (NetworkDriver.instance.TWOPLAYER)
        {
            if (!otherPlayer) { playerChoice = victim; }
            else { otherPlayerChoice = victim; }

            if (playerChoice != null && otherPlayerChoice != null)//both players have chosen a victim
            {
                if (playerChoice != otherPlayerChoice) { GameDriver.instance.WriteGuiMsg("Both players must choose the same soul!", 5f, false, Color.red); }
                else { canChoose = true; GameDriver.instance.WriteGuiMsg("", 0.01f, false, Color.white); }
            }
        }
        else { canChoose = true; }//SINGLE PLAYER
 
        if (canChoose && startCircle && main.transform.position.y >= mainStartPos.y + 3)
        {
            playerChoice = null; otherPlayerChoice = null;
            if (victim == ChosenVictim)
            {
             SetSpiritsFree();// GameDriver.instance.WriteGuiMsg("RIGHT ANWER" + victim.name, 10f, false);
                if (NetworkDriver.instance.TWOPLAYER) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'update','event':'setfree'}}"), false); }

            }
            else {
              SummonZozo();// GameDriver.instance.WriteGuiMsg("WRONG ANWER " + victim.name +"SUPPOSED TO BE " + ChosenVictim.name, 10f, false);
                if (NetworkDriver.instance.TWOPLAYER) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'update','event':'summon'}}"), false); }
            }
        }
    }
    public void revertGear()
    {
        if(NetworkDriver.instance.isMobile)
        {
            GameDriver.instance.Player.GetComponent<ShootingSystem>().aiming.GetComponent<Aiming>().aim = false;
            GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.camSup.AIMMODE = false;
            GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.camSup.forceCharacterDirection = false;
        }

        //GameDriver.instance.Player.GetComponent<ShootingSystem>().aiming.zoom = GameDriver.instance.Player.GetComponent<ShootingSystem>().aiming.startZoom;
        GameDriver.instance.Player.GetComponent<ShootingSystem>().planchette.SetActive(false);
        //GameDriver.instance.Player.GetComponent<ShootingSystem>().crosshairs.SetActive(true);
        GameDriver.instance.Player.GetComponent<Animator>().SetBool("ouija", false);
        GameDriver.instance.Player.GetComponent<PlayerController>().camera.SetActive(true);
        if (NetworkDriver.instance.TWOPLAYER) { GameDriver.instance.Client.GetComponent<Animator>().SetBool("ouija", false); GameDriver.instance.Client.GetComponent<ClientPlayerController>().camera.SetActive(true); }
    }

    public void SetSpiritsFree()
    {
        revertGear();
        Pentagram.GetComponent<MeshRenderer>().materials[0].SetColor("_Color", Color.blue);
        //pentagramLight.GetComponent<Light>().color = Color.blue;
        AudioManager.instance.StopPlaying("creepywhisper", null);
        AudioManager.instance.Play("heavenmusic", null);
        heaventVFX.SetActive(true);
        startCircle = false;
        setSpiritsFree = true;
        //UNLOCK MAIN TUNNEL DOOR
        GetComponentInParent<OuijaSessionControl>().MainTunnelExitDoors[GetComponentInParent<OuijaSessionControl>().currentSession].GetComponent<Door>().isNeedKey = false;
        GetComponentInParent<OuijaSessionControl>().MainTunnelExitDoors[GetComponentInParent<OuijaSessionControl>().currentSession].transform.GetChild(0).gameObject.SetActive(false);//TURN OFF EFFECT
       //Invoke("NextSession",120f);
       Invoke("EndGame",30f);
    }
    public void EndGame()
    {
        NetworkDriver.instance.EndGame();
    }
    /*public void NextSession()
    {
        GameObject.Find("OuijaBoardManager").GetComponent<OuijaSessionControl>().NextSession();
    }*/
    public void SummonZozo()
    {
        revertGear();
        zozoDummy.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[0].SetFloat("_Alpha", 0f);
        zozoDummy.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[0].SetFloat("_EMFAlpha", 0.3f);
        for (int i = 0; i < candles.Count; i++) { candles[i].SetActive(false); }
        //candleCount = 0;
        if (candleCount > 0) { candleCount -= 3; }
        if (candleCount > 0)
        {
            if (NetworkDriver.instance.TWOPLAYER) { candleCount -= 3; }
        }
        zozo = true;
        startCircle = false;
        AudioManager.instance.Play("enterzozomusic", null);
        if (!TEST)
        {
            Invoke("SpawnInitialEffect", 15f);
            Invoke("SpawnMidEffect", 39f);
            Invoke("Climax", 50f);//CLIMAX
        }

        if (TEST) { Invoke("SpawnZOZO", 1f); }//-----------------TEST------------------------
        zozoDummy.SetActive(true);



    }
    public void SpawnInitialEffect()
    {
        zozoRise = true;
    }
    public void SpawnMidEffect()
    {
       
        zozoEffectMid.SetActive(true);
    }

    public void Climax()
    {
        zozoMusicVol = 0;
        AudioManager.instance.Play("zozomusicloop", null);
        AudioManager.instance.Play("zozolaugh", null);
        AudioManager.instance.UpdateVolume("zozomusicloop", null, zozoMusicVol);
        zozoFXendOn = true;
        zozoEffectEnd.SetActive(true);
        if (!TEST) { Invoke("SpawnZOZO", 10f); }
    }

    public void SpawnZOZO()
    {
        ZOZO.GetComponent<ZozoControl>().HP = ZOZO.GetComponent<ZozoControl>().HP = ZOZO.GetComponent<ZozoControl>().HPMAX;
        ZOZO.GetComponent<ZozoControl>().DEAD = false;
        ZOZO.SetActive(true);
       
        //ZOZO.GetComponent<ZozoControl>().canLaser = true;
        //ZOZO.GetComponent<ZozoControl>().ChargeLaser();
        zozoDummy.SetActive(false);
        zozoEnd = true;
        zozoFXendOn = false;
        GameDriver.instance.WriteGuiMsg("Keep ZOZO in the light!", 60f, false, Color.yellow);
        Invoke("CanDestroyZozo", zozoTimer);//-------------HOW LONG ZOZO ALIVE

    }
    public void CanDestroyZozo()
    {
        canDestroyZozo = true;
    }

    public void DestroyZozo1()
    {
        GameObject explosion = Instantiate(prefabZozoDeathExplo, ZOZO.transform.position, ZOZO.transform.rotation);
        explosion.GetComponent<bruteExplosion>().main = ZOZO;
        explosion.GetComponent<bruteExplosion>().death = true;

        GameObject electricDeath = Instantiate(electricityDeath, ZOZO.transform.position, ZOZO.transform.rotation);
        electricDeath.transform.position = transform.position + (Vector3.up * 3);

        AudioManager.instance.Play("EMPHit", null);
        AudioManager.instance.Play("zozolaugh", null);

        fadeMusicOut = true;
        //AudioManager.instance.StopPlaying("zozomusicloop", null);
        ZOZO.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = true;
        ZOZO.transform.GetChild(0).GetComponent<Outline>().OutlineWidth = 0;
        ZOZO.GetComponent<Teleport>().teleport = 0;
        ZOZO.GetComponent<NPCController>().target = null;
        ZOZO.GetComponent<Animator>().enabled = true;
        ZOZO.GetComponent<NavMeshAgent>().enabled = true;
        ZOZO.GetComponent<NPCController>().enabled = true;
        //ZOZO.GetComponent<Teleport>().canTeleport = true;

        ZOZO.transform.position = ZOZOstartPos;
        ZOZO.transform.rotation = ZOZOstartRot;

        ZOZO.SetActive(false);
        Invoke("RefreshBoard", 1f);
    }

    public void RefreshBoard()
    {
        zozo = false;
        startCircle = false;
        zozoRise = false;
        zozoEnd = false;
        effectInner.SetActive(false);
        zozoEffectMid.SetActive(false);
        zozoEffectEnd.SetActive(false);
        trigger.SetActive(true);
        foreach (GameObject victim in Victims) { victim.GetComponent<Animator>().SetBool("falling", false); victim.GetComponent<Person>().darkLight.SetActive(false); }
        main.transform.position = mainStartPos;
        zozoDummy.transform.position = zozoDummyStartPos;
        zozoSpawn.transform.position = zozoSpawnStartPos;
        zozoSpawn.transform.localScale = zozoSpawnStartSize;
        zozoEffectMid.transform.localScale = zozoEffectMidStartSize;
        zozoEffectEnd.transform.localScale = zozoEffectEndStartSize;
        effectDome.transform.localScale = domeStartSize;

        //RandomVictim(null);

    }
}
