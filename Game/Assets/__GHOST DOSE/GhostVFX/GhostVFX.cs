﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

//[ExecuteInEditMode]
public class GhostVFX : MonoBehaviour
{


    public GameObject PlayerLight;
    public GameObject ClientLight;
    private GameObject skin;
    GameObject[] envLights;
    private GameObject EnvLight;
    private GameDriver GD;
    public List<Light> LightSources;
    private string shader;
    public bool visible; //USD IN CONJUNCTION WITH PLAY AIM RAYCAST
    public bool inShadow;
    //public RenderTexture DynamicShadowMap;


    public void Start()
    {
        GD = GameObject.Find("GameController").GetComponent<GameDriver>();
        skin = gameObject.transform.GetChild(0).gameObject;
        shader = skin.GetComponent<SkinnedMeshRenderer>().material.shader.name;
    }

    public void UpdateShaderValues()
    {
       PlayerLight = GD.Player.GetComponent<PlayerController>().currLight;
       ClientLight = GD.Client.GetComponent<ClientPlayerController>().currLight;


        //if (PlayerLight != null && ClientLight != null)
        {

            if (shader == "Custom/Ghost") { envLights = GameObject.FindGameObjectsWithTag("GhostLight"); }
            else { envLights = GameObject.FindGameObjectsWithTag("ShadowerLight"); }

            //DEFAULT VISIBLIITY
            if (shader == "Custom/Ghost") { visible = false; }
            else { visible = true; }



            Light lightSource;

            //ADD IN ENVIRONMENT LIGHT SOURCES
            if (envLights.Length > 0)
            {
                int envLightCount = envLights.Length;
                Vector4[] lightPositions = new Vector4[envLightCount];
                Vector4[] lightDirections = new Vector4[envLightCount];
                float[] lightAngles = new float[envLightCount];
                float[] ScalarStrengths = new float[envLightCount];
                float[] lightRanges = new float[envLightCount];

                for (int i = 0; i < envLightCount; i++)
                {
                    lightSource = envLights[i].GetComponent<Light>();

                    lightPositions[i] = lightSource.transform.position;
                    lightDirections[i] = -lightSource.transform.forward;
                    lightAngles[i] = lightSource.spotAngle+5;
                    ScalarStrengths[i] = 50;//30
                    lightRanges[i] = lightSource.range;//30


                }
                IsVisible(envLights);
                skin.GetComponent<SkinnedMeshRenderer>().material.SetInt("_EnvLightCount", envLightCount);
                skin.GetComponent<SkinnedMeshRenderer>().material.SetVectorArray("_LightPositions", lightPositions);
                skin.GetComponent<SkinnedMeshRenderer>().material.SetVectorArray("_LightDirections", lightDirections);
                skin.GetComponent<SkinnedMeshRenderer>().material.SetFloatArray("_LightAngles", lightAngles);
                skin.GetComponent<SkinnedMeshRenderer>().material.SetFloatArray("_StrengthScalarLight", ScalarStrengths);
                skin.GetComponent<SkinnedMeshRenderer>().material.SetFloatArray("_LightRanges", lightRanges);



            }

            //SAHDOW OVERRIDES ENVIRONMENT LIGHTS
            if (inShadow) {
                if (shader == "Custom/Ghost") { visible = false; }else { visible = true; }
            }


            //ADD IN PLAYER LIGHT
            lightSource = PlayerLight.GetComponent<Light>();
            float spotAngle = lightSource.spotAngle;
            if (!lightSource.enabled) { spotAngle = 0; }
            else { //flashlight enabled
                if (!InLineOfSightArea(lightSource, true)) { spotAngle = 0; }
                //FLASHLIGHT OVERRIDES SHADOW AND ENVIORNMENT
                else if (IsObjectInLightCone(lightSource, true))//directly under light source
                {
                    if (shader == "Custom/Ghost") { visible = true; } else { visible = false; } 
                } 
            } 
            skin.GetComponent<SkinnedMeshRenderer>().material.SetVector("_PlayerLightPosition", lightSource.transform.position);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetVector("_PlayerLightDirection", -lightSource.transform.forward);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_PlayerLightAngle", spotAngle);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_PlayerStrengthScalarLight", 20);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_PlayerLightRange", lightSource.range);

            //ADD IN CLIENT LIGHT
            lightSource = ClientLight.GetComponent<Light>();
            spotAngle = lightSource.spotAngle;
            if (!lightSource.enabled) { spotAngle = 0; }
            else
            {
                if (!InLineOfSightArea(lightSource, true)) { spotAngle = 0; }
                //FLASHLIGHT OVERRIDES SHADOW AND ENVIORNMENT
                else if(IsObjectInLightCone(lightSource, true))//directly under light source
                {
                    if (shader == "Custom/Ghost") { visible = true; } else { visible = false; }
                }
            }
            skin.GetComponent<SkinnedMeshRenderer>().material.SetVector("_ClientLightPosition", lightSource.transform.position);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetVector("_ClientLightDirection", -lightSource.transform.forward);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_ClientLightAngle", spotAngle);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_ClientStrengthScalarLight", 20);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_ClientLightRange", lightSource.range);

            //testShadow();
            //Debug.Log(visible);
            //SHADOW
            //inShadow = false;

        }

    }



    private void IsVisible(GameObject[] lights)
    {
        Light closestLight = null;
        float closestDistance = Mathf.Infinity;
        for (int i = 0; i < lights.Length; i++)
        {
            if (IsObjectInLightCone(lights[i].GetComponent<Light>(), false))
            {
                float distance = Vector3.Distance(transform.position, lights[i].transform.position);
                if (distance < closestDistance && InLineOfSightArea(lights[i].GetComponent<Light>(), true))
                {
                    closestLight = lights[i].GetComponent<Light>();
                    closestDistance = distance;
                }
            }
        }
        if (closestLight != null)
        {
           // Debug.Log("SETTING VISIBLITY " + closestLight.name);
            if (shader == "Custom/Ghost") { visible = true; }
            else { visible = false; } // shadower
        }
    }

    private bool InLineOfSightArea(Light light, bool ignoreShadow)
    {
        LayerMask mask = ~(1 << LayerMask.NameToLayer("EnemyHead"));

        if (ignoreShadow)
        {   //LAYERS TO IGNORE
            int ShadowReceiver = LayerMask.NameToLayer("ShadowReceiver");
            int ShadowBox = LayerMask.NameToLayer("ShadowBox");
            mask = ~(1 << ShadowReceiver) & ~(1 << ShadowBox) & ~(1 << LayerMask.NameToLayer("EnemyHead"));
        }


        float hitHeight = 1.5f; // adjust the hit height as per your requirement
        Vector3 targPos = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + hitHeight, this.gameObject.transform.position.z);
        Ray ray = new Ray(light.transform.position, (targPos - light.transform.position).normalized);
        float distance = Vector3.Distance(light.transform.position, targPos);
        Vector3 endPoint = ray.GetPoint(distance);
        Debug.DrawLine(light.transform.position, endPoint, UnityEngine.Color.blue);

        // Perform the raycast, excluding the specified layers
        RaycastHit hit;
        //if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask.value))
        if(Physics.Linecast(light.transform.position, endPoint, out hit, mask.value))
        {
           
            //Debug.Log("OBJECT AREA HIT " + light.name + hit.collider.gameObject.name);
            if (hit.collider.gameObject == this.gameObject)
            {
                return true;
            }
            //Debug.DrawLine(light.transform.position, targPos, UnityEngine.Color.red);
        }
        
        return false;
    }


    private bool InLineOfSightPoint(Light light, bool ignoreShadow)
    {
        LayerMask mask = ~(1 << LayerMask.NameToLayer("EnemyHead"));

        if (ignoreShadow)
        {   //LAYERS TO IGNORE
            int ShadowReceiver = LayerMask.NameToLayer("ShadowReceiver");
            int ShadowBox = LayerMask.NameToLayer("ShadowBox");
            mask = ~(1 << ShadowReceiver) & ~(1 << ShadowBox) & ~(1 << LayerMask.NameToLayer("EnemyHead"));
        }

        Vector3 direction = light.gameObject.transform.forward;

        Debug.DrawLine(light.transform.position, light.transform.position + light.transform.forward * 10f, UnityEngine.Color.green, 0.5f);
        Ray ray = new Ray(light.transform.position, direction);

        // Perform the raycast, excluding the specified layers
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask.value))
        {
            //Debug.Log("OBJECT POINT HIT " + hit.collider.gameObject.name);
            if (hit.collider.gameObject == this.gameObject)
            {
                return true;
            }
        }

        return false;
    }


    //CALLED BY PLAYER AIMING - DETERMINE IF CAN HIT WHILE IN LIGHT
    public bool IsObjectInLightCone(Light spotlight, bool isPlayer)
    {
        float adjustedSpotAngle = spotlight.spotAngle;
        float hitHeight = 0; 

        if (!isPlayer) { adjustedSpotAngle = spotlight.spotAngle * 1.1f;
            hitHeight = 1.5f;
        }
        float distanceToObject = Vector3.Distance(this.gameObject.transform.position + Vector3.up * hitHeight, spotlight.transform.position);
        bool inRange = distanceToObject <= spotlight.range;
        Vector3 directionToObject = (this.gameObject.transform.position + Vector3.up * hitHeight - spotlight.transform.position).normalized;
        float angleToObject = Vector3.Angle(spotlight.transform.forward, directionToObject);
        bool inCone = angleToObject <= adjustedSpotAngle * 0.5f;
        if (isPlayer && this.gameObject.tag == "Shadower") { return inCone; }//fl effect on shadowers have no range
        return inRange && inCone;
    }



}



