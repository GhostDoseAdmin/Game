﻿using UnityEngine;

//[ExecuteInEditMode]
public class GhostVFX : MonoBehaviour
{


    public GameObject PlayerLight;
    public GameObject ClientLight;
    private GameObject EnvLight;
    private GameDriver GD;
    private string shader;

    public void Start()
    {
        GD = GameObject.Find("GameController").GetComponent<GameDriver>();
        shader = GetComponent<SkinnedMeshRenderer>().material.shader.name;
    }

    public void Update()
    {
        PlayerLight = GD.Player.GetComponent<PlayerController>().currLight;
        ClientLight = GD.Client.GetComponent<ClientPlayerController>().currLight;
        EnvLight = ClosestEnvLight();

        if (PlayerLight != null) 
        {
            //DISABLLING LIGHT SOURCE STOPS VISIBILITY RENDERING, MUST CHANGE PROPS INSTEAD
            float spotAngle = PlayerLight.GetComponent<Light>().spotAngle;
            if (!PlayerLight.GetComponent<Light>().enabled) { spotAngle = 1; } 
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPositionPlayer", PlayerLight.transform.position);
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirectionPlayer", -PlayerLight.transform.forward);
            GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAnglePlayer", spotAngle);
            
        }
        if (ClientLight != null)
        {
            float spotAngle = ClientLight.GetComponent<Light>().spotAngle;
            if (!ClientLight.GetComponent<Light>().enabled) { spotAngle = 1; }
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPositionClient", ClientLight.transform.position);
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirectionClient", -ClientLight.transform.forward);
            GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAngleClient", spotAngle);
        }
       if (EnvLight != null)
        {
            float spotAngle = EnvLight.GetComponent<Light>().spotAngle;
            if (!EnvLight.GetComponent<Light>().enabled) { spotAngle = 1; }
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightPositionEnv", EnvLight.transform.position);
            GetComponent<SkinnedMeshRenderer>().material.SetVector("_LightDirectionEnv", -EnvLight.transform.forward);
            GetComponent<SkinnedMeshRenderer>().material.SetFloat("_LightAngleEnv", spotAngle);
        }

    }

    private GameObject ClosestEnvLight()//used to have some light control if not reading by cone
    {
        GameObject[] lights;
        if (shader == "Custom/Ghost") {  lights = GameObject.FindGameObjectsWithTag("GhostLight");  }
        else { lights = GameObject.FindGameObjectsWithTag("ShadowerLight"); }
        if (lights != null)
        {
            GameObject closestLight = null;
            float closestDistance = Mathf.Infinity;
            foreach (GameObject light in lights)
            {
                float distance = Vector3.Distance(transform.position, light.transform.position);
                if (light.GetComponent<Light>().intensity > 5) { distance = distance * 0.5f; }//higher intesity lights keep the control
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestLight = light;
                }
            }
            //Debug.Log("HOME LIGHT " + closestLight.name);
            return closestLight;
        }
        else { Debug.Log("No ghost lights found!"); return null; }
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