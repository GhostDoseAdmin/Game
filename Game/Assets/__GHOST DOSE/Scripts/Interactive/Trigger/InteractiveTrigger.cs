using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveTrigger : MonoBehaviour
{
    [Header("INTERACTIVE OBJECT")]
    [Space(10)]
    [SerializeField] public GameObject interactiveObject;
    [SerializeField] public GameObject interact;

    public bool allowInteraction;
    private float delay = 1f;
    private float timer = 0f;

    private void Start()
    {
        allowInteraction = false;
        interact.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(InputManager.instance.interactButton) && allowInteraction)
        {
            if (Time.time > timer + delay)
            {
                Debug.Log("-----------------------------------------INTERACTING");
                interactiveObject.GetComponent<Item>().ActivateObject(false);
                timer = Time.time;//cooldown
            }
        }


        if (interactiveObject == null)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            allowInteraction = true;
            interact.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            allowInteraction = false;
            interact.SetActive(false);
        }
    }
}
