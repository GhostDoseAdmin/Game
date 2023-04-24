using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("NPC DAMAGE")]
    [Space(10)]
    public int damage;
    //public float force;
    private GameObject main;
    public bool triggerHit;
    public void Start()
    {
        main = transform.root.gameObject;
    }
    private void OnCollisionEnter(Collision collision)
    {

            if (collision.gameObject.tag == "Player")
            {
                Debug.Log("---------------------------------------COLLSION----------------------------------------" );

                Vector3 oppositeForce = -main.GetComponent<NPCController>().transform.forward * main.GetComponent<NPCController>().force;
                oppositeForce.y = 0f; // Set the y component to 0
               // collision.gameObject.GetComponent<HealthSystem>().HealthDamage(main.GetComponent<NPCController>().damage, oppositeForce);




            }
        
    }


}
