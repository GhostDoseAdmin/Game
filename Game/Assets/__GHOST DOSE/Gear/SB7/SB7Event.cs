using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using Newtonsoft.Json;
using GameManager;
using static UnityEngine.GraphicsBuffer;

public class SB7Event : Item
{
    public GameObject effect;
    private bool decay;
    public float SessionTimer = 10f;
    public int session;
    private float effectStartSize;
    private Vector3 startPosition;
    public GameObject Trigger;

    public void Awake()
    {
        effectStartSize = effect.transform.localScale.x;
        //Debug.Log("----------------------------START SIZE " + effectStartSize);
        effect.SetActive(false);
        Trigger.GetComponent<MeshRenderer>().enabled = false;
        startPosition = transform.position;
    }
    public void Update()
    {
        if (decay) { 
            effect.transform.localScale = Vector3.Lerp(effect.transform.localScale, effect.transform.localScale * 0.5f, Time.deltaTime * 1);
            Vector3 currPos = transform.position;            currPos.y += 0.025f;            transform.position = currPos;
            if (effect.transform.localScale.x < 0.1f) { 
                effect.SetActive(false); decay = false; 
                GameObject.Find("ColdSpotManager").GetComponent<ColdSpotControl>().Respawn(); 
                this.gameObject.SetActive(false); }
        }

    }
    public override void ActivateObject(bool otherPlayer)
    {
        Trigger.transform.localScale = new Vector3(5f, 5f, 5f);
        //START UP SPIRITBOX
        GameDriver.instance.Player.GetComponent<PlayerController>().ChangeGear(true);
    }


    public void Exposed()
    {
        effect.SetActive(true);
        Invoke("Decay", SessionTimer);
    }
    public void Decay()
    {
        decay = true;
    }

    private void OnDisable()
    {
        transform.position = startPosition;
        Trigger.transform.localScale = new Vector3(1f, 1f, 1f);
        effect.transform.localScale = new Vector3(effectStartSize, effectStartSize, effectStartSize); 
    }

}