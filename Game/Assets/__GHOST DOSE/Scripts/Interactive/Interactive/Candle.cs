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

            DestroyWithSound(false);
    }


    public void DestroyWithSound(bool otherPlayer)
    {

        if (!otherPlayer) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'cand','event':'pickup','pass':'none'}}"), false); }
        if (GameObject.Find("OuijaBoardManager").GetComponentInChildren<VictimControl>().candleCount < GameObject.Find("OuijaBoardManager").GetComponentInChildren<VictimControl>().maxCandles)
        {
            GameObject.Find("OuijaBoardManager").GetComponentInChildren<VictimControl>().candleCount++;

            if (GameObject.Find("OuijaBoardManager").GetComponentInChildren<VictimControl>().candleCount >= GameObject.Find("OuijaBoardManager").GetComponentInChildren<VictimControl>().maxCandles)
            {
                AudioManager.instance.Play("candle", null);
            }
            if (!otherPlayer) { AudioManager.instance.Play("PickUp", null); }

            //DESTROY
            transform.position = new Vector3(transform.position.x, -9999, transform.position.z);//MIMIC DESTROY
            Invoke("Respawn", respawnTime);

        }
    }

    void Respawn()
    {
        transform.position = startPos;
    }
}
