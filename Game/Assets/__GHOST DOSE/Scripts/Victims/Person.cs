using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
public class Person : MonoBehaviour
{
    public bool isYoung;
    public bool isEvil;
    public bool isMurdered;
    public bool isGirl;

    public GameObject eyes;
    public GameObject deathWeap;
    public GameObject whiteLight;
    public GameObject darkLight;
    public GameObject horns;

    public void Start()
    {
        deathWeap.SetActive(false);
        RandomizeTraits();
       
    }
    public void RandomizeTraits()
    {
        isEvil = UnityEngine.Random.value > 0.5f;
        isMurdered = UnityEngine.Random.value > 0.5f;

        UpdateTraits();
    }

    public void UpdateTraits()
    {
        if (isEvil) {
            eyes.GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_Color", Color.red); 
        } else {
            horns.SetActive(false);
            eyes.GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_Color", Color.white); 
        }
        if (isMurdered) { deathWeap.SetActive(true); } else { deathWeap.SetActive(false); }
    }

    public void EmitTraits()
    {
        NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','isEvil':'{isEvil}','isMurdered':'{isMurdered}','isGirl':'{isGirl}'}}"), false);
    }
    public void Update()
    {
        
    }

}
