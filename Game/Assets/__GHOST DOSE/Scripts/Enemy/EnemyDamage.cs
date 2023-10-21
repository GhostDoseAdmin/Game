using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionSystem;
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
        main = GetComponentInParent<NPCController>().gameObject;
    }
    private void OnTriggerEnter(Collider other)
    {
        
        Vector3 oppositeForce = main.GetComponent<NPCController>().transform.forward * main.GetComponent<NPCController>().force;
        oppositeForce.y = 0f; // Set the y component to 0

            if (other.gameObject.name == "Player")
            {
                //if (other.gameObject.GetComponent<PlayerController>().canFlinch)
                {
                    AudioManager.instance.Play("EnemyHit", main.GetComponent<NPCController>().audioSource);
                    other.gameObject.GetComponent<HealthSystem>().HealthDamage(main.GetComponent<NPCController>().damage, oppositeForce, false);
                }

            }
            if (other.gameObject.name == "Client")
            {
            AudioManager.instance.Play("EnemyHit", main.GetComponent<NPCController>().audioSource);
            other.gameObject.GetComponent<ClientPlayerController>().Flinch(oppositeForce, false);
            }
        
    }

}
