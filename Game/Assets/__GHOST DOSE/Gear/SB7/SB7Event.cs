using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using Newtonsoft.Json;
using GameManager;

public class SB7Event : Item
{
    public GameObject effect;
    private bool decay;
    private bool relocate;
    public float decayTimer = 10f;
    public void Update()
    {
        if (decay) { 
            effect.transform.localScale = Vector3.Lerp(effect.transform.localScale, effect.transform.localScale * 0.5f, Time.deltaTime * 1);
            if (effect.transform.localScale.x < 0.1f) { effect.SetActive(false); decay = false; relocate = true; }
        }

        if(relocate)
        {

        }
    }
    public override void ActivateObject(bool otherPlayer)
    {
        //player.GetComponent<PlayerController>().sb7 = true;
        GameDriver.instance.Player.GetComponent<PlayerController>().ChangeGear(true);
        //if (GameDriver.instance.twoPlayer) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'sb7','event':'on'}}"), false); }
    }


    public void Exposed()
    {
        effect.SetActive(true);
        Invoke("Decay", decayTimer);
    }
    public void Decay()
    {
        decay = true;
    }


}