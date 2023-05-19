using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;

public class EndGameRegion : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        NetworkDriver.instance.EndGame();

    }
}
