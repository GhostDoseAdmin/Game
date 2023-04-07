using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]
public class ShadowDetector : MonoBehaviour
{

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Shadower" || other.tag=="Ghost")
        {
            // Debug.Log("IN SHADOW");
            if (other.gameObject.GetComponent<GhostVFX>() != null) { other.gameObject.GetComponent<GhostVFX>().inShadow = true; }
        }
            
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Shadower" || other.tag == "Ghost")
        {
            // Debug.Log("IN SHADOW");
            if (other.gameObject.GetComponent<GhostVFX>() != null)
            {
                other.gameObject.GetComponent<GhostVFX>().inShadow = false;
            }
        }

    }

}
