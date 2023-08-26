using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using GameManager;

public class remPodItem : Item
{
    [Header("BATTERIES COUNT")]
    [Space(10)]
    [SerializeField] private int count;
    Vector3 startPos;
    public float respawnTime;

    private void Start()
    {
        startPos = transform.position;
    }


    public override void ActivateObject(bool otherPlayer)
    {
        if (!GameDriver.instance.Player.GetComponent<PlayerController>().hasRem && !GameDriver.instance.Player.GetComponent<PlayerController>().changingGear)
        {
            //HealthSystem.kitinstance.CollectKit(this.kit);
            NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'rem','event':'pickup','pass':'none'}}"), false);
            GameDriver.instance.Player.GetComponent<PlayerController>().hasRem = true;
            GameDriver.instance.Player.GetComponent<PlayerController>().gear = 2;//changes to grid +1
            GameDriver.instance.Player.GetComponent<PlayerController>().ChangeGear(false, true);
            DestroyWithSound(false);
        }
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
