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
        main = FindEnemyMain(gameObject.transform);
    }
    private void OnTriggerEnter(Collider other)
    {

        Vector3 oppositeForce = main.GetComponent<NPCController>().transform.forward * main.GetComponent<NPCController>().force;
        oppositeForce.y = 0f; // Set the y component to 0

            if (other.gameObject.name == "Player")
            {
                other.gameObject.GetComponent<HealthSystem>().HealthDamage(main.GetComponent<NPCController>().damage, oppositeForce);
            }
            if (other.gameObject.name == "Client")
            {
            AudioManager.instance.Play("EnemyHit", main.GetComponent<NPCController>().audioSource);
            other.gameObject.GetComponent<ClientPlayerController>().Flinch(oppositeForce);
            }

    }

    GameObject FindEnemyMain(Transform head)
    {
        Transform currentTransform = head;
        while (currentTransform != null)
        {
            if (currentTransform.GetComponent<NPCController>() != null)
            {
                Debug.Log("Found parent with Person component: " + currentTransform.name);
                return currentTransform.gameObject;
            }
            currentTransform = currentTransform.parent;
        }
        return null;

    }
}
