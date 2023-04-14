using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("NPC DAMAGE")]
    [Space(10)]
    public int damage;
    public Transform player;
    public float force;
    public bool triggerHit;

    public void Start()
    {
        player = GameObject.Find("GameController").GetComponent<GameDriver>().Player.transform;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (triggerHit)
        {
            if (collision.gameObject.tag == "Player")
            {
                //Debug.Log("-------------------------------------------------------------------------------" + collision.gameObject.name);

                Vector3 oppositeForce = -transform.root.gameObject.GetComponent<NPCController>().transform.forward * force;
                oppositeForce.y = 0f; // Set the y component to 0
                collision.gameObject.GetComponent<HealthSystem>().HealthDamage(damage, oppositeForce);

                //Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();



            }
        }
    }


}
