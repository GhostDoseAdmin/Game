using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManager;
using InteractionSystem;
public class bruteNPC : MonoBehaviour
{
    AudioSource audioSource1, audioSource2;
    public Sound[] footSteps;
    private bool bruteMusic;
    private void Awake()
    {
        audioSource1 = gameObject.AddComponent<AudioSource>();
        audioSource1.spatialBlend = 1.0f;
        audioSource2 = gameObject.AddComponent<AudioSource>();
        audioSource2.spatialBlend = 1.0f;

    }

    public void Update()
    {
        if(GetComponent<NPCController>().closestPlayer!=null && !bruteMusic && Vector3.Distance(transform.position, GetComponent<NPCController>().closestPlayer.transform.position) <15) {
            bruteMusic = true;
            AudioManager.instance.Play("BruteMusic", null);
        }
    }



    public void TriggerFootstepRightBrute()
    {
        if (GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length > 0 && GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "bruteWalk")
        {
            audioSource1.clip = footSteps[Random.Range(0, footSteps.Length)].clip;
            audioSource1.volume = 2f;
            audioSource1.Play();
            FootStep();
        }

    }
    public void TriggerFootstepLeftBrute()
    {
        if (GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length > 0 && GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "bruteWalk")
        {
            audioSource2.clip = footSteps[Random.Range(0, footSteps.Length)].clip;
            audioSource2.volume = 2f;
            audioSource2.Play();
            FootStep();
        }
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
                if (light.GetComponent<GhostLight>() != null) { if (light.GetComponent<GhostLight>().canFlicker) { light.GetComponent<GhostLight>().InvokeFlicker(1f); } }
            }
        }
    }
}
