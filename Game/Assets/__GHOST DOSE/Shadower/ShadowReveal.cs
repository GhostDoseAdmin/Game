using System;
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

//[ExecuteInEditMode]
public class ShadowReveal : MonoBehaviour
{


    private GameObject shadowerLight;


    void Update()
    {
        // Vector4 lightPos = Shader.GetGlobalVector("_LightPosition");
        //lightPos.y = 50f;
        //Shader.SetGlobalVector("_LightPosition", lightPos);
        GameObject exposingLight = GetClosestShadowerLight();
        if (exposingLight.GetComponent<Light>().isActiveAndEnabled)
        {
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPosition", exposingLight.transform.position);
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirection", -exposingLight.transform.forward);
            GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAngle", exposingLight.GetComponent<Light>().spotAngle);
        }
        

       //GetComponent<SkinnedMeshRenderer>().materials = materials;
    }
    private GameObject GetClosestShadowerLight()
    {
        GameObject[] shadowerLights = GameObject.FindGameObjectsWithTag("ShadowerLight");
        GameObject closestLight = null;
        float closestDistance = Mathf.Infinity;
        foreach (GameObject shadowerLight in shadowerLights)
        {
            float distance = Vector3.Distance(transform.position, shadowerLight.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLight = shadowerLight;
            }
        }
        return closestLight;
    }

}