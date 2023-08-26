using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionSystem;

public class bruteExplosion : MonoBehaviour
{

    public GameObject main;
    public bool death = false;
    //AudioSource audioSource1;
    // Start is called before the first frame update
    private void Awake()
    {
        //audioSource1 = gameObject.AddComponent<AudioSource>();
        //audioSource1.spatialBlend = 1.0f;
    }
    void Start()
    {
        AudioManager.instance.Play("BruteSmash", null);
    }

    // Update is called once per frame
    void Update()
    {
        if (death) { transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 2f, Time.deltaTime * 1); }
        else { transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 5f, Time.deltaTime * 1); }
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
                    //AudioManager.instance.Play("EnemyHit", main.GetComponent<NPCController>().audioSource);
                    other.gameObject.GetComponent<HealthSystem>().HealthDamage(main.GetComponent<NPCController>().damage, oppositeForce);
                }

            }
            if (other.gameObject.name == "Client")
            {
               // AudioManager.instance.Play("EnemyHit", main.GetComponent<NPCController>().audioSource);
                other.gameObject.GetComponent<ClientPlayerController>().Flinch(oppositeForce);
            }
        }
    }


}
