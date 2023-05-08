using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using GameManager;
public class OuijaSessionControl : MonoBehaviour
{
    public List<GameObject> OuijaSessions;
    public List<GameObject> MainTunnelExitDoors;
    public GameObject GhostDoorVFX;
    public GameObject coldSpotManager;
    public int currentSession;

    // Start is called before the first frame update
    void Start()
    {
        currentSession = 0;

        foreach(GameObject door in MainTunnelExitDoors)
        {
            GameObject newVFX = Instantiate(GhostDoorVFX, door.transform);
            newVFX.transform.SetParent(door.transform);
            newVFX.transform.SetAsFirstSibling();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextSession()
    {
        OuijaSessions[currentSession].gameObject.SetActive(false);
        currentSession++;
        coldSpotManager.GetComponent<ColdSpotControl>().NextSession();
        if (OuijaSessions[currentSession] != null)
        {
            OuijaSessions[currentSession].gameObject.SetActive(true);
            if (NetworkDriver.instance.HOST) { OuijaSessions[currentSession].GetComponent<VictimControl>().RandomVictim(null); }
        }
    }
}
