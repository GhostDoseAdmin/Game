using System;
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

//[ExecuteInEditMode]
public class ShadowReveal : MonoBehaviour
{


    private GameObject shadowerLight;
    GameObject exposingLight;
    private float action_timer = 0.0f;
    private float action_delay = 0.33f;//0.25

    void Update()
    {
        {
            GameObject[] shadowerLights = GameObject.FindGameObjectsWithTag("ShadowerLight");

            foreach (GameObject lightObject in shadowerLights)
            {
                Light spotlight = lightObject.GetComponent<Light>();
                if (spotlight != null && spotlight.type == LightType.Spot)
                {
                    ///MAKE LIGHT EQUAL TO PLAYERS LIGHT
                    GameObject playerLight = GetClosesFlashlight();
                    if (playerLight != null && playerLight.GetComponent<Light>().isActiveAndEnabled)
                    {
                        if (Time.time > action_timer + action_delay)
                        {
                             exposingLight = playerLight;
                            Debug.Log("PLAYER LIGHT");
                        }
                    }

                    if (IsObjectInLightCone(spotlight, gameObject))
                    {
                            exposingLight = spotlight.gameObject;
                            Debug.Log("SPT LIGHT");

                            action_timer = Time.time;//cooldown
                    }
                }
            }
            if (exposingLight != null)
            {
                GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPosition", exposingLight.transform.position);
                GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirection", -exposingLight.transform.forward);
                GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAngle", exposingLight.GetComponent<Light>().spotAngle);
            }
        }
        // Vector4 lightPos = Shader.GetGlobalVector("_LightPosition");
        //lightPos.y = 50f;
        //Shader.SetGlobalVector("_LightPosition", lightPos);
        /*GameObject exposingLight = GetClosestShadowerLight();
        if (exposingLight.GetComponent<Light>().isActiveAndEnabled)
        {
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPosition", exposingLight.transform.position);
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirection", -exposingLight.transform.forward);
            GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAngle", exposingLight.GetComponent<Light>().spotAngle);
        }*/


        //GetComponent<SkinnedMeshRenderer>().materials = materials;
    }
    private GameObject GetClosesFlashlight()
    {
        GameObject[] shadowerLights = GameObject.FindGameObjectsWithTag("Flashlight");
        GameObject closestLight = null;
        float closestDistance = Mathf.Infinity;
        foreach (GameObject shadowerLight in shadowerLights)
        {
            float distance = Vector3.Distance(transform.position, shadowerLight.transform.position);
            if (shadowerLight.GetComponent<Light>().intensity > 5) { distance = distance * 0.5f; }//higher intesity lights keep the control
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLight = shadowerLight;
            }
        }
        return closestLight;
    }

    private bool IsObjectInLightCone(Light spotlight, GameObject target)
    {
        Vector3 directionToObject = (target.transform.position - spotlight.transform.position).normalized;
        float angleToObject = Vector3.Angle(spotlight.transform.forward, directionToObject);

        return angleToObject <= spotlight.spotAngle * 0.5f;
    }

}