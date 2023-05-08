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
    public List<GameObject> currentSession;
    public float respawnTimer;
    public int q1, q2, q3;//locations
    public bool hasDoneSession;

    // Start is called before the first frame update
    void Start()
    {
        currentSession = ColdSpotsSession1;
        hasDoneSession = false;
    }
    public void NextSession()
    {
        if (currentSession == ColdSpotsSession2) { currentSession = ColdSpotsSession3; }
        if (currentSession == ColdSpotsSession1) { currentSession = ColdSpotsSession2; }
    }
    // Update is called once per frame

}
