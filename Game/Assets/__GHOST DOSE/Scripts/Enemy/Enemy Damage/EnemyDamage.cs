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
        player =  GameObject.Find("Player").transform;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            player.GetComponent<HealthSystem>().HealthDamage(damage);
        }
    }
}
