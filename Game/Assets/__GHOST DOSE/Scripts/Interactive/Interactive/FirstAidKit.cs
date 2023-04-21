using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;

public class FirstAidKit : Item
{
    [Header("KITS COUNT")]
    [Space(10)]
    //[SerializeField] private int kit;
    public float healFactor;

    public override void ActivateObject(bool otherPlayer)
    {
        //HealthSystem.kitinstance.CollectKit(this.kit);
        GameObject.Find("Player").GetComponent<HealthSystem>().Health += GameObject.Find("Player").GetComponent<HealthSystem>().maxHealth* healFactor; 
        NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'med','event':'pickup','pass':'none'}}"), false);
        this.DestroyObject(0);
    }
}
