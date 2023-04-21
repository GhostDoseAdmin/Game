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
           
            if (other.gameObject.transform.root.GetComponent<GhostVFX>() != null) { other.gameObject.transform.root.GetComponent<GhostVFX>().inShadow = true;  }
        }
            
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Shadower" || other.tag == "Ghost")
        {
            // Debug.Log("IN SHADOW");
            if (other.gameObject.transform.root.GetComponent<GhostVFX>() != null)
            {
                other.gameObject.transform.root.GetComponent<GhostVFX>().inShadow = false;
            }
        }

    }

}
