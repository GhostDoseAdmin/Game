using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;

public class EndGameRegion : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            NetworkDriver.instance.EndGame();
        }
    }
}
