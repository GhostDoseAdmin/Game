using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionSystem;

public class bruteExplosion : MonoBehaviour
{

    public GameObject main;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 5f, Time.deltaTime * 1);
        if (transform.localScale.x > 12)
        {
            Destroy(this.gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (main != null)
        {
            Vector3 oppositeForce = main.GetComponent<NPCController>().transform.forward * (main.GetComponent<NPCController>().force );
            oppositeForce.y = 0f; // Set the y component to 0

            if (other.gameObject.name == "Player")
            {
                if (other.gameObject.GetComponent<PlayerController>().canFlinch)
                {
                    AudioManager.instance.Play("EnemyHit", main.GetComponent<NPCController>().audioSource);
                    other.gameObject.GetComponent<HealthSystem>().HealthDamage(main.GetComponent<NPCController>().damage, oppositeForce);
                }

            }
            if (other.gameObject.name == "Client")
            {
                AudioManager.instance.Play("EnemyHit", main.GetComponent<NPCController>().audioSource);
                other.gameObject.GetComponent<ClientPlayerController>().Flinch(oppositeForce);
            }
        }
    }


}
