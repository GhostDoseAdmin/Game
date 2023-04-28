using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColdSpotControl : MonoBehaviour
{

    public List<GameObject> ColdSpotsSession1;
    public List<GameObject> ColdSpotsSession2;
    public List<GameObject> ColdSpotsSession3;
    public float respawnTimer;

    // Start is called before the first frame update
    void Awake()
    {
        ChooseColdSpot();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChooseColdSpot()
    {
        int randNum = Random.Range(0, ColdSpotsSession1.Count);
        
        for(int i=0; i< ColdSpotsSession1.Count; i++)
        {
            if (randNum != i) { ColdSpotsSession1[i].gameObject.SetActive(false); } else { ColdSpotsSession1[i].gameObject.SetActive(true); }
            
        }
        randNum = Random.Range(0, ColdSpotsSession2.Count);
        for (int i = 0; i < ColdSpotsSession2.Count; i++)
        {
            if (randNum != i) { ColdSpotsSession2[i].gameObject.SetActive(false); } else { ColdSpotsSession1[i].gameObject.SetActive(true); }

        }

        randNum = Random.Range(0, ColdSpotsSession3.Count);
        for (int i = 0; i < ColdSpotsSession3.Count; i++)
        {
            if (randNum != i) { ColdSpotsSession3[i].gameObject.SetActive(false); } else { ColdSpotsSession1[i].gameObject.SetActive(true); }

        }

    }

    public void Respawn()
    {
        Invoke("ChooseColdSpot", respawnTimer);
    }
}
