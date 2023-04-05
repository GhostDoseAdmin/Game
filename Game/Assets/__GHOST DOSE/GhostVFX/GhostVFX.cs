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
    //public RenderTexture DynamicShadowMap;


    public void Start()
    {
        GD = GameObject.Find("GameController").GetComponent<GameDriver>();
        skin = gameObject.transform.GetChild(0).gameObject;
        shader = skin.GetComponent<SkinnedMeshRenderer>().material.shader.name;
    }

    public void UpdateShaderValues()
    {
        //PlayerLight = GD.Player.GetComponent<PlayerController>().currLight;
        //ClientLight = GD.Client.GetComponent<ClientPlayerController>().currLight;


        //if (PlayerLight != null && ClientLight != null)
        {

            if (shader == "Custom/Ghost") { envLights = GameObject.FindGameObjectsWithTag("GhostLight"); }
            else { envLights = GameObject.FindGameObjectsWithTag("ShadowerLight"); }

            //GameObject.FindGameObjectsWithTag("ShadowerLight");

            Light lightSource;
            //ADD IN PLAYER LIGHT

            lightSource = PlayerLight.GetComponent<Light>();
            float spotAngle = lightSource.spotAngle;
            if (!lightSource.enabled) { spotAngle = 0; }
            ---------------------------------------If something blocking its line of site via ray cast spot angle 0
            skin.GetComponent<SkinnedMeshRenderer>().material.SetVector("_PlayerLightPosition", lightSource.transform.position);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetVector("_PlayerLightDirection", -lightSource.transform.forward);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_PlayerLightAngle", spotAngle);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_PlayerStrengthScalarLight", 20);

            //ADD IN CLIENT LIGHT
            lightSource = ClientLight.GetComponent<Light>();
            spotAngle = lightSource.spotAngle;
            if (!lightSource.enabled) { spotAngle = 0; }
            skin.GetComponent<SkinnedMeshRenderer>().material.SetVector("_ClientLightPosition", lightSource.transform.position);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetVector("_ClientLightDirection", -lightSource.transform.forward);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_ClientLightAngle", spotAngle);
            skin.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_ClientStrengthScalarLight", 20);


            //Debug.Log($"PlayerLight position: {PlayerLight.transform.position}, direction: {-PlayerLight.transform.forward}, spotAngle: {PlayerLight.GetComponent<Light>().spotAngle}");
            //Debug.Log($"ClientLight position: {ClientLight.transform.position}, direction: {-ClientLight.transform.forward}, spotAngle: {ClientLight.GetComponent<Light>().spotAngle}");

            //ADD IN ENVIRONMENT LIGHT SOURCES
            if (envLights.Length > 0)
            {
                int envLightCount = envLights.Length;
                Vector4[] lightPositions = new Vector4[envLightCount];
                Vector4[] lightDirections = new Vector4[envLightCount];
                float[] lightAngles = new float[envLightCount];
                float[] ScalarStrengths = new float[envLightCount];
                for (int i = 0; i < envLightCount; i++)
                {
                    lightSource = envLights[i].GetComponent<Light>();

                    lightPositions[i] = lightSource.transform.position;
                    lightDirections[i] = -lightSource.transform.forward;
                    lightAngles[i] = lightSource.spotAngle;
                    ScalarStrengths[i] = 30;


                }
                IsVisible(envLights);
                Debug.Log(visible);

                skin.GetComponent<SkinnedMeshRenderer>().material.SetInt("_EnvLightCount", envLightCount);
                skin.GetComponent<SkinnedMeshRenderer>().material.SetVectorArray("_LightPositions", lightPositions);
                skin.GetComponent<SkinnedMeshRenderer>().material.SetVectorArray("_LightDirections", lightDirections);
                skin.GetComponent<SkinnedMeshRenderer>().material.SetFloatArray("_LightAngles", lightAngles);
                skin.GetComponent<SkinnedMeshRenderer>().material.SetFloatArray("_StrengthScalarLight", ScalarStrengths);
            }

        }

    }

    private void IsVisible(GameObject[] lights)
    {

        //DEFAULT VISIBLIITY
        if (shader == "Custom/Ghost") { visible = false; }
        else { visible = true; }

        for (int i = 0; i < lights.Length; i++)
        {
            if (IsObjectInLightCone(lights[i].GetComponent<Light>()))
            {
                //IF there is no object blocking with raycast
                float hitHeight = 1f; // adjust the hit height as per your requirement
                Debug.DrawLine(lights[i].transform.position, new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + hitHeight, this.gameObject.transform.position.z));
                Ray ray = new Ray(lights[i].transform.position, (new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + hitHeight, this.gameObject.transform.position.z) - lights[i].transform.position).normalized);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    //Debug.Log("OBJECT HIT " + hit.collider.gameObject.name);
                    if (hit.collider.gameObject == this.gameObject)
                    {
                        if (shader == "Custom/Ghost") { visible = true; return; }
                        else { visible = false; return; }//shadower
                    }
                }
            }
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

    //CALLED BY PLAYER AIMING - DETERMINE IF CAN HIT WHILE IN LIGHT
    public bool IsObjectInLightCone(Light spotlight)
    {
        Vector3 directionToObject = (this.gameObject.transform.position - spotlight.transform.position).normalized;
        float angleToObject = Vector3.Angle(spotlight.transform.forward, directionToObject);

        return angleToObject <= spotlight.spotAngle * 0.5f;
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