using System;
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

//[ExecuteInEditMode]
public class ShadowRevealV2 : MonoBehaviour
{


    public GameObject PlayerLight;
    public GameObject ClientLight;
    public GameObject EnvLight;
    public GameDriver GD;

    public void Start()
    {
        GD = GameObject.Find("GameController").GetComponent<GameDriver>();
    }

    public void Update()
    {
        PlayerLight = GD.Player.GetComponent<PlayerController>().currLight;
        ClientLight = GD.Client.GetComponent<ClientPlayerController>().currLight;
        EnvLight = GetClosestLight();

        if (PlayerLight != null && PlayerLight.GetComponent<Light>().isActiveAndEnabled)
        {
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPositionPlayer", PlayerLight.transform.position);
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirectionPlayer", -PlayerLight.transform.forward);
            GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAnglePlayer", PlayerLight.GetComponent<Light>().spotAngle);
        }
        if (ClientLight != null && ClientLight.GetComponent<Light>().isActiveAndEnabled)
        {
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPositionClient", ClientLight.transform.position);
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirectionClient", -ClientLight.transform.forward);
            GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAngleClient", ClientLight.GetComponent<Light>().spotAngle);
        }
       if (EnvLight != null && EnvLight.GetComponent<Light>().isActiveAndEnabled)
        {
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPositionEnv", EnvLight.transform.position);
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirectionEnv", -EnvLight.transform.forward);
            GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAngleEnv", EnvLight.GetComponent<Light>().spotAngle);
        }

    }

    private GameObject GetClosestLight()//used to have some light control if not reading by cone
    {
        GameObject[] shadowerLights = GameObject.FindGameObjectsWithTag("GhostLight");
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
        Debug.Log("HOME LIGHT " + closestLight.name);
        return closestLight;
    }

}


/*
 * 
     public GameObject GetEnvLight()
    {
        GameObject[] envLights = GameObject.FindGameObjectsWithTag("GhostLight");

        foreach (GameObject lightObject in envLights)
        {
            Light spotlight = lightObject.GetComponent<Light>();
            if (spotlight != null && spotlight.type == LightType.Spot)
            {

                if (IsObjectInLightCone(spotlight, gameObject))
                {
                    Debug.Log("CASTING LIGHT " + spotlight.name);
                    return(spotlight.gameObject);

                }
            }
        }
        Debug.Log("NO LIGHT FOUND");
        return null; // GetClosestLight(); 
    }

    private bool IsObjectInLightCone(Light spotlight, GameObject target)
    {
        Vector3 directionToObject = (target.transform.position - spotlight.transform.position).normalized;
        float angleToObject = Vector3.Angle(spotlight.transform.forward, directionToObject);

        return angleToObject <= spotlight.spotAngle * 0.5f;
    }
 GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPosition1", light1.transform.position);
GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirection1", -light1.transform.forward);
GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAngle1", light1.GetComponent<Light>().spotAngle);

GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPosition2", light2.transform.position);
GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirection2", -light2.transform.forward);
GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAngle2", light2.GetComponent<Light>().spotAngle);
*/