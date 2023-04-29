using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour
{
    public bool isYoung;
    public bool isEvil;
    public bool murdered;

    public GameObject eyes;
    public GameObject deathWeap;

    public void Start()
    {
        deathWeap.SetActive(false);
        RandomizeTraits();
       
    }
    public void RandomizeTraits()
    {
        isEvil = UnityEngine.Random.value > 0.5f;
        murdered = UnityEngine.Random.value > 0.5f;

        UpdateTraits();
    }

    public void UpdateTraits()
    {
        if (isEvil) { eyes.GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_Color", Color.red); } else { eyes.GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_Color", Color.white); }
        if (murdered) { deathWeap.SetActive(true); } else { deathWeap.SetActive(false); }
    }

    public void Update()
    {
        
    }
}
