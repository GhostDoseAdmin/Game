using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using InteractionSystem;
public class Battery : Item
{
    [Header("BATTERIES COUNT")]
    [Space(10)]
    [SerializeField] private int count;


    public override void ActivateObject(bool otherPlayer)
    {

        //HealthSystem.kitinstance.CollectKit(this.kit);
        GameObject.Find("Player").GetComponent<HealthSystem>().Health += GameObject.Find("Player").GetComponent<ShootingSystem>().camBatteryUI.fillAmount =1;
        NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'bat','event':'pickup','pass':'none'}}"), false);
        DestroyWithSound(false);
    }


    public void DestroyWithSound(bool otherPlayer)
    {
        if (!otherPlayer) { AudioManager.instance.Play("PickUp", GameObject.Find("Player").GetComponent<PlayerController>().audioSource); }
        else { AudioManager.instance.Play("PickUp", GameObject.Find("Client").GetComponent<ClientPlayerController>().audioSource); }
        Destroy(gameObject);
    }
}
