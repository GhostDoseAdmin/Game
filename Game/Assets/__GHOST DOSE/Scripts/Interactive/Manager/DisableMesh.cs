using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableMesh : MonoBehaviour
{
    public GameObject[] triggerObjects;

    protected bool isNotDisableMesh;

    void Start()
    {
        triggerObjects = GameObject.FindGameObjectsWithTag("TriggerMesh");
        foreach (GameObject triggerObj in triggerObjects)
        {
            triggerObj.GetComponent<Renderer>().enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(InputManager.instance.meshRender))
        {
            InputDisable();
        }
    }

    void InputDisable()
    {
        if (this.isNotDisableMesh)
        {
            this.isNotDisableMesh = false;

            triggerObjects = GameObject.FindGameObjectsWithTag("TriggerMesh");

            foreach (GameObject triggerObj in triggerObjects)
            {
                triggerObj.GetComponent<Renderer>().enabled = false;
            }
        }
        else
        {
            this.isNotDisableMesh = true;
            triggerObjects = GameObject.FindGameObjectsWithTag("TriggerMesh");

            foreach (GameObject triggerObj in triggerObjects)
            {
                triggerObj.GetComponent<Renderer>().enabled = true;
            }
        }
    }
}