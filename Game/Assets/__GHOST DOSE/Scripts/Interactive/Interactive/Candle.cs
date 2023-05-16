using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;

public class Candle : Item
{
    [SerializeField] private int count;
    Vector3 startPos;
    public float respawnTime;

    private void Start()
    {
        startPos = transform.position;
    }

    public override void ActivateObject(bool otherPlayer)
    {

        //HealthSystem.kitinstance.CollectKit(this.kit);
        NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'cand','event':'pickup','pass':'none'}}"), false);
        GameObject.Find("OuijaBoardManager").GetComponentInChildren<VictimControl>().candleCount++;
        DestroyWithSound(false);
    }


    public void DestroyWithSound(bool otherPlayer)
    {
        if (!otherPlayer) { AudioManager.instance.Play("PickUp", null); }
        /*if (!otherPlayer) { AudioManager.instance.Play("PickUp", GameObject.Find("Player").GetComponent<PlayerController>().audioSource); }
        else { AudioManager.instance.Play("PickUp", GameObject.Find("Client").GetComponent<ClientPlayerController>().audioSource); }*/
        transform.position = new Vector3(transform.position.x, -9999, transform.position.z);//MIMIC DESTROY
        Invoke("Respawn", respawnTime);
        //Destroy(gameObject);
    }

    void Respawn()
    {
        transform.position = startPos;
    }
}
