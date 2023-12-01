using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;

public class Battery : Item
{
    [Header("BATTERIES COUNT")]
    [Space(10)]
    [SerializeField] private int count;
    Vector3 startPos;
    public float respawnTime;


    public override void Awake()
    {
        //EMPTY TO REMOVE AUDIO SOURCE
    }



    private void Start()
    {
        startPos = transform.position;
    }


    public override void ActivateObject(bool otherPlayer)
    {

        //HealthSystem.kitinstance.CollectKit(this.kit);
        GameObject.Find("Player").GetComponent<ShootingSystem>().camBatteryUI.fillAmount =1;
        if (!NetworkDriver.instance.OFFLINE) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'bat','event':'pickup','pass':'none'}}"), false); }
        DestroyWithSound(false);
    }


    public void DestroyWithSound(bool otherPlayer)
    {
        if (!otherPlayer) { AudioManager.instance.Play("PickUp", null); }
        //if (!otherPlayer) { AudioManager.instance.Play("PickUp", GameObject.Find("Player").GetComponent<PlayerController>().audioSource); }
        //else { AudioManager.instance.Play("PickUp", GameObject.Find("Client").GetComponent<ClientPlayerController>().audioSource); }
        transform.position = new Vector3(transform.position.x, -9999, transform.position.z);//MIMIC DESTROY
        Invoke("Respawn", respawnTime);
    }

    void Respawn()
    {
        transform.position = startPos;
    }
}
