using System;
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

//[ExecuteInEditMode]
public class ShadowRevealV2 : MonoBehaviour
{


    public GameObject light1;
    public GameObject light2;
    public void Update()
    {
        GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPosition1", light1.transform.position);
        GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirection1", -light1.transform.forward);
        GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAngle1", light1.GetComponent<Light>().spotAngle);

        GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPosition2", light2.transform.position);
        GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirection2", -light2.transform.forward);
        GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAngle2", light2.GetComponent<Light>().spotAngle);

    }


}