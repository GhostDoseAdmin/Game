using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using GameManager;

public class ColdSpotControl : MonoBehaviour
{

    public List<GameObject> ColdSpotsSession1;
    public List<GameObject> ColdSpotsSession2;
    public List<GameObject> ColdSpotsSession3;
    public float respawnTimer;
    public int q1, q2, q3;//locations
    public bool hasDoneSession;

    // Start is called before the first frame update
    void Start()
    {
        hasDoneSession = false;
        ChooseColdSpot();
    }

    // Update is called once per frame

    public void ChooseColdSpot()
    {
        if (NetworkDriver.instance.HOST)
        {

            q1 = Random.Range(0, ColdSpotsSession1.Count);

            for (int i = 0; i < ColdSpotsSession1.Count; i++)
            {
                if (q1 != i) { 
                    if (ColdSpotsSession1[i].gameObject != null) { ColdSpotsSession1[i].gameObject.SetActive(false); } }
                else {
                    if (ColdSpotsSession1[i].gameObject != null) { ColdSpotsSession1[i].gameObject.SetActive(true); } }
            }
            
            q2 = Random.Range(0, ColdSpotsSession2.Count);
            for (int i = 0; i < ColdSpotsSession2.Count; i++)
            {
                if (q2 != i) {
                    if (ColdSpotsSession2[i].gameObject != null) { ColdSpotsSession2[i].gameObject.SetActive(false); }} 
                else
                {
                    if (ColdSpotsSession2[i].gameObject != null) { ColdSpotsSession2[i].gameObject.SetActive(true);  }
                }

            }

            q3 = Random.Range(0, ColdSpotsSession3.Count);
            for (int i = 0; i < ColdSpotsSession3.Count; i++)
            {
                if (q3 != i)
                {
                    if (ColdSpotsSession3[i].gameObject != null) { ColdSpotsSession3[i].gameObject.SetActive(false); } } else
                {
                    if (ColdSpotsSession3[i].gameObject != null) { ColdSpotsSession3[i].gameObject.SetActive(true);  }
                }

            }

            if (GameDriver.instance.twoPlayer) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'','q1':'{q1}','q2':'{q2}','q3':'{q3}','type':'update','event':'coldspot'}}"), false); }
        }

    }

    public void ChooseColdSpotNetwork(int location1, int location2, int location3)
    {
        q1 = location1; q2 = location2; q3 = location3;

        for (int i = 0; i < ColdSpotsSession1.Count; i++)
        {
            if (q1 != i)
            {
                if (ColdSpotsSession1[i].gameObject != null) { ColdSpotsSession1[i].gameObject.SetActive(false); } } else {
                if (ColdSpotsSession1[i].gameObject != null) { ColdSpotsSession1[i].gameObject.SetActive(true); }
            }

        }

        for (int i = 0; i < ColdSpotsSession2.Count; i++)
        {
            if (q2 != i) {
                if (ColdSpotsSession2[i].gameObject != null) { ColdSpotsSession2[i].gameObject.SetActive(false); }
            } else {
                if (ColdSpotsSession2[i].gameObject != null) { ColdSpotsSession2[i].gameObject.SetActive(true); }
            }

        }


        for (int i = 0; i < ColdSpotsSession3.Count; i++)
        {
            if (q3 != i)
            {
                if (ColdSpotsSession3[i].gameObject != null) { ColdSpotsSession3[i].gameObject.SetActive(false); }
            } else
            {
                if (ColdSpotsSession3[i].gameObject != null) { ColdSpotsSession3[i].gameObject.SetActive(true); }
            }

        }
    }


    public void Respawn()
    {
        Invoke("ChooseColdSpot", respawnTimer);
    }
}
