using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ShadowDetector : MonoBehaviour
{
    // Start is called before the first frame update

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Shadower" || other.tag=="Ghost")
        {
           // Debug.Log("IN SHADOW");
            other.GetComponent<GhostVFX>().inShadow = true;
        }
            
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Shadower" || other.tag == "Ghost")
        {
            // Debug.Log("IN SHADOW");
            other.GetComponent<GhostVFX>().inShadow = false;
        }

    }

}
