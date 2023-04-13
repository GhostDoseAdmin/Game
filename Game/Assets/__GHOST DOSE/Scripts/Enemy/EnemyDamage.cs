using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("NPC DAMAGE")]
    [Space(10)]
    public int damage;
    public Transform player;

    public void Start()
    {
        player = GameObject.Find("GameController").GetComponent<GameDriver>().Player.transform;
    }
    private void OnTriggerEnter(Collider other)
    {
       // Debug.Log("-----------------------HIT");
        if (other.tag == "Player")
        {
           // other.gameObject.GetComponent<HealthSystem>().HealthDamage(damage);

            Rigidbody rb = player.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 oppositeForce = -(other.transform.position - transform.position).normalized * 1000f;
                rb.AddForce(oppositeForce);
            }


        }
    }
}
