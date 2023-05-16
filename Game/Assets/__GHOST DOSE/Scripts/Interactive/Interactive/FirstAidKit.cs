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
    public float respawnTime;
    Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    public override void ActivateObject(bool otherPlayer)
    {
        AudioManager.instance.Play("GetFrom", null);
        //HealthSystem.kitinstance.CollectKit(this.kit);
        GameObject.Find("Player").GetComponent<HealthSystem>().Health += GameObject.Find("Player").GetComponent<HealthSystem>().maxHealth* healFactor; 
        NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'med','event':'pickup','pass':'none'}}"), false);
        DestroyWithSound(false);
    }

    public void DestroyWithSound(bool otherPlayer)
    {
        if (!otherPlayer) { AudioManager.instance.Play("GetFrom", null); }
        //if (!otherPlayer) { AudioManager.instance.Play("GetFrom", GameObject.Find("Player").GetComponent<PlayerController>().audioSource); }
        //else { AudioManager.instance.Play("GetFrom", GameObject.Find("Client").GetComponent<ClientPlayerController>().audioSource); }
        transform.position = new Vector3(transform.position.x, -9999, transform.position.z);//MIMIC DESTROY
        Invoke("Respawn", respawnTime);
    }


    void Respawn()
    {
        transform.position = startPos;
    }


}



