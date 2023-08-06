using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManager;
using NetworkSystem;

public class LevelBoundary : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {

       
        if (other.GetComponentInParent<PlayerController>()!=null)
        {
            Debug.Log("------------------------------------OUT OF BOUNDS" + other.GetComponentInParent<PlayerController>().transform.parent.name);
            if (NetworkDriver.instance.LevelManager.GetComponentInChildren<VictimControl>().ZOZO.activeSelf)
            {
                other.GetComponentInParent<PlayerController>().transform.position = NetworkDriver.instance.LevelManager.GetComponentInChildren<VictimControl>().ZOZO.transform.position + Vector3.up;
            }
            else { other.GetComponentInParent<PlayerController>().transform.position = GameDriver.instance.playerStartPos+Vector3.up; }
            
        }
    }
}
