using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManager;

public class ZozoControl : MonoBehaviour
{
    AudioSource audioSourceFootStepRight, audioSourceFootStepLeft;
    public Sound[] footSteps;
    // Start is called before the first frame update
    void Start()
    {
        audioSourceFootStepRight = gameObject.AddComponent<AudioSource>();
        audioSourceFootStepRight.spatialBlend = 1.0f;
        audioSourceFootStepLeft = gameObject.AddComponent<AudioSource>();
        audioSourceFootStepLeft.spatialBlend = 1.0f;
    }

    public void TriggerFootstepRight()
    {
        audioSourceFootStepRight.clip = footSteps[Random.Range(0, footSteps.Length)].clip;
        audioSourceFootStepRight.volume = 2f;
        audioSourceFootStepRight.Play();
        FootStep();

    }
    public void TriggerFootstepLeft()
    {
        audioSourceFootStepLeft.clip = footSteps[Random.Range(0, footSteps.Length)].clip;
        audioSourceFootStepLeft.volume = 2f;
        audioSourceFootStepLeft.Play();
        FootStep();

    }

    private void FootStep()
    {
        GameObject.Find("PlayerCamera").GetComponent<Camera_Controller>().InvokeShake(0.5f, Mathf.InverseLerp(20f, 0f, Vector3.Distance(GameDriver.instance.Player.transform.position, transform.position)));
        Light[] allLights = GameObject.FindObjectsOfType<Light>(); // Find all the Light components in the scene

        foreach (Light light in allLights)
        {
            float distanceToLight = Vector3.Distance(transform.position, light.transform.position); // Calculate the distance to the light

            if (distanceToLight <= 20f) // If the light is within range, call the InvokeFlicker method
            {
                if (light.GetComponent<GhostLight>() != null) { light.GetComponent<GhostLight>().InvokeFlicker(1f); }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
