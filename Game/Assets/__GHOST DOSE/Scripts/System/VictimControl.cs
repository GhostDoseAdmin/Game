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
    Vector3 domeStartSize;

    Vector3 mainStartPos;

    // Start is called before the first frame update
    void Start()
    {
        ChooseVictim();
        mainStartPos = transform.position;
        domeStartSize = effectDome.transform.localScale;
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
            if (GameDriver.instance.twoPlayer) { if (Vector3.Distance(GameDriver.instance.Client.transform.position, transform.position) > 3) { GameDriver.instance.Client.transform.position = Vector3.Lerp(GameDriver.instance.Client.transform.position, transform.position, 0.02f); } }
        }


        //START EFFECT
        if (playerOn || clientOn)
        {
            effectInner.SetActive(true);
        }
        else { effectInner.SetActive(false); }

    }

    public void ChooseVictim()
    {
        ChosenVictim = Victims[Random.Range(0, Victims.Count)];
    }

    public override void ActivateObject(bool otherPlayer)
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

    public void testAnswer(GameObject victim)
    {
        if (victim == ChosenVictim)
        {
            GameDriver.instance.WriteGuiMsg("RIGHT ANWER", 2f);
        }
        else { GameDriver.instance.WriteGuiMsg("WRONG ANWER " + victim.name, 2f); }
    }


}
