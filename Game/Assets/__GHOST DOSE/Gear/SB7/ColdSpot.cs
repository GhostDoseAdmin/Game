using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using Newtonsoft.Json;
using GameManager;
using static UnityEngine.GraphicsBuffer;

public class ColdSpot : Item
{
    public GameObject effect;
    public List<GameObject> locations;
    public float respawnTimer;
    private bool decay;
    public float SessionTimer = 10f;
    public int session;
    private float effectStartSize;
    public GameObject Trigger;
    private bool exposed;
    public int questionIndexYoungEvilMurderGender;


    public void Awake()
    {
        effectStartSize = effect.transform.localScale.x;
        //Debug.Log("----------------------------START SIZE " + effectStartSize);
        effect.SetActive(false);
        Trigger.GetComponent<MeshRenderer>().enabled = false;

        exposed = false;
    }

    public void Start()
    {
        Respawn(null);
    }
    public void Update()
    {
        //----FLY AWAY
        if (decay) { 
            effect.transform.localScale = Vector3.Lerp(effect.transform.localScale, effect.transform.localScale * 0.5f, Time.deltaTime * 1);
            Vector3 currPos = transform.position;            currPos.y += 0.025f;            transform.position = currPos;
            if (effect.transform.localScale.x < 0.01f) { 
                effect.SetActive(false); decay = false; exposed = false;
                if (NetworkDriver.instance.HOST) { Invoke("InvokeRespawn", respawnTimer); }
                // this.gameObject.SetActive(false); 
            }
        }
        //-----CRAZY K2 BEEP
        if(exposed)
        {
            K2[] k2s = GameObject.FindObjectsOfType<K2>();
            foreach(K2 k2 in  k2s)
            {
                if (Vector3.Distance(k2.transform.position, this.gameObject.transform.position) < 5) { k2.gameObject.GetComponent<k2beep>().Level = 5; }
            }
        }


    }


    public void Exposed(bool otherPlayer)
    {
        if (!exposed)
        {
            exposed = true;
            effect.SetActive(true);
            Invoke("Decay", SessionTimer);
            if (NetworkDriver.instance.TWOPLAYER && !otherPlayer) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'expose','event':'coldspot'}}"), false); }
        }
       
    }
    public override void ActivateObject(bool otherPlayer)
    {
        if (exposed)
        {
            GetComponentInParent<ColdSpotControl>().hasDoneSession = true;
            Trigger.transform.localScale = new Vector3(5f, 5f, 5f);
            //START UP SPIRITBOX
            GameDriver.instance.Player.GetComponent<PlayerController>().ChangeGear(true);
        }

    }

    public void Decay()
    {
        decay = true;
       
    }

    public void InvokeRespawn()    {        Respawn(null);    }
    public void Respawn(GameObject givenDestination)
    {
        int randomSpot = Random.Range(0, locations.Count);
        if (givenDestination == null) { transform.position = locations[randomSpot].transform.position; }
        else { transform.position = givenDestination.transform.position; }//FROM HOST
        Trigger.transform.localScale = new Vector3(1f, 1f, 1f);
        effect.transform.localScale = new Vector3(effectStartSize, effectStartSize, effectStartSize);
        effect.SetActive(false); decay = false; exposed = false;
        if (NetworkDriver.instance.TWOPLAYER && NetworkDriver.instance.HOST && NetworkDriver.instance.OTHERS_SCENE_READY) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{this.gameObject.name}','type':'respawn','event':'coldspot','loc':'{locations[randomSpot].name}'}}"), false); }
    }



}