using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManager;
using NetworkSystem;

public class InteractiveTrigger : MonoBehaviour
{
    [Header("INTERACTIVE OBJECT")]
    [Space(10)]
    [SerializeField] public GameObject interactiveObject;
    [SerializeField] public GameObject interact;

    public bool allowInteraction;
    private float delay = 1f;
    private float timer = 0f;

    private void Awake()
    {
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }
    private void Start()
    {
        allowInteraction = false;
        interact.SetActive(false);
    }

    private void Update()
    {
        if ((Input.GetKeyDown(InputManager.instance.interactButton) && !NetworkDriver.instance.isMobile)  && allowInteraction)
        {
            if (Time.time > timer + delay)
            {
                interactiveObject.GetComponent<Item>().ActivateObject(false);
                timer = Time.time;//cooldown
            }
        }
        if (NetworkDriver.instance.isMobile)
        {
            if(GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.interactBTN.buttonReleased && allowInteraction)
            {
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
        if (other.gameObject.name =="Player")
        {
            //Debug.Log("--------------ALLWING INTERACTION OBJECT" + interactiveObject.name);
            allowInteraction = true;
            interact.SetActive(true);
            interactiveObject.GetComponent<Item>().playerOn = true;
            if (NetworkDriver.instance.isMobile) { other.gameObject.GetComponent<PlayerController>().gamePad.canPickUp = true; }
        }

        if (other.gameObject.name == "Client") { interactiveObject.GetComponent<Item>().clientOn = true; }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            allowInteraction = false;
            interact.SetActive(false);
            interactiveObject.GetComponent<Item>().playerOn = false;
            if (GameDriver.instance.Player.GetComponent<PlayerController>().sb7) { GameDriver.instance.Player.GetComponent<PlayerController>().ChangeGear(true); }
        }
        if (other.gameObject.name == "Client") { interactiveObject.GetComponent<Item>().clientOn = false; }
    }


}
