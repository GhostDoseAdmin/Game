using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeddyAlarm : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {

            if (other.gameObject.name == "Player" || other.gameObject.name == "Client")
            {
            NPCController[] teddys = null;
            teddys = FindObjectsOfType<NPCController>();

            foreach (NPCController teddy in teddys)
            {
              if(teddy.teddy)
                {
                    if (Vector3.Distance(transform.position, teddy.transform.position) < 10) { 
                        teddy.canAttack = true;
                    }
                }
                
            }

        }
        
    }
}
