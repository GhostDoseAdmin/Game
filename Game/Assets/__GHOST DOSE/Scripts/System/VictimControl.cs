using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManager;
using UnityEngine.UIElements;

public class VictimControl : Item
{
    public List<GameObject> Victims;
    private bool targetIsMurdered;
    private bool targetIsEvil;
    private bool targetIsYoung;
    public GameObject ChosenVictim;
    public GameObject trigger;
    private bool startCircle;


    // Start is called before the first frame update
    void Start()
    {
        ChooseVictim();
    }

    // Update is called once per frame
    void Update()
    {
        if(startCircle)
        {
            transform.Rotate(0f, 5f * Time.deltaTime, 0f);
            //KEEP IN CIRCLE
            if (Vector3.Distance(GameDriver.instance.Player.transform.position, transform.position) > 3) { GameDriver.instance.Player.transform.position = Vector3.Lerp(GameDriver.instance.Player.transform.position, transform.position, 0.02f); }
            if (GameDriver.instance.twoPlayer) { if (Vector3.Distance(GameDriver.instance.Client.transform.position, transform.position) > 3) { GameDriver.instance.Client.transform.position = Vector3.Lerp(GameDriver.instance.Client.transform.position, transform.position, 0.02f); } }
        }


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
