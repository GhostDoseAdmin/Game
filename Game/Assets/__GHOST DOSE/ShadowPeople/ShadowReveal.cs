using System;
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

[ExecuteInEditMode]
public class ShadowReveal : MonoBehaviour
{
 
    void Update()
    {
        //-------------------RENDER MATERIALS ---------------------------

            GameObject[] shadowerLights = GameObject.FindGameObjectsWithTag("ShadowerLight");
            foreach (GameObject shadowerLight in shadowerLights)
            {
                SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
                Material material = renderer.material;
                material.SetVector("_LightPosition", shadowerLight.transform.position);
                material.SetVector("_LightDirection", -shadowerLight.transform.forward);
                material.SetFloat("_LightAngle", shadowerLight.GetComponent<Light>().spotAngle);
            }
        
    }


}