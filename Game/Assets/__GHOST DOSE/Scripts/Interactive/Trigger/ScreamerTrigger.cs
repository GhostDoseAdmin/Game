using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreamerTrigger : MonoBehaviour
{
    [Header("SCREAMER OBJECT")]
    [Space(10)]
    [SerializeField] public GameObject screamerObject;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            screamerObject.GetComponent<Item>().ActivateObject(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameObject.GetComponent<Collider>().enabled = false;
        }
    }
}
