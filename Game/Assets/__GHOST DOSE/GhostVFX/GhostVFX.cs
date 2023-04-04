using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class GhostVFX : MonoBehaviour
{


    public GameObject PlayerLight;
    public GameObject ClientLight;
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
        shader = GetComponent<SkinnedMeshRenderer>().material.shader.name;
    }

    public void Update()
    {
        //PlayerLight = GD.Player.GetComponent<PlayerController>().currLight;
        //ClientLight = GD.Client.GetComponent<ClientPlayerController>().currLight;


        //if (PlayerLight != null && ClientLight != null)
        {

            if (shader == "Custom/Ghost") { envLights = GameObject.FindGameObjectsWithTag("GhostLight"); }
            else { envLights = GameObject.FindGameObjectsWithTag("ShadowerLight"); }

            //GameObject.FindGameObjectsWithTag("ShadowerLight");

            int lightCount = envLights.Length + 2;

            Vector4[] lightPositions = new Vector4[lightCount];
            Vector4[] lightDirections = new Vector4[lightCount];
            float[] lightAngles = new float[lightCount];
            float[] ScalarStrengths = new float[lightCount];

            Light lightSource;
            //ADD IN PLAYER LIGHT

            lightSource = PlayerLight.GetComponent<Light>();
            lightPositions[0] = lightSource.transform.position;
            lightDirections[0] = -lightSource.transform.forward;
            lightAngles[0] = lightSource.spotAngle;
            ScalarStrengths[0] = 20;

            //ADD IN CLIENT LIGHT
            lightSource = ClientLight.GetComponent<Light>();
            lightPositions[1] = lightSource.transform.position;
            lightDirections[1] = -lightSource.transform.forward;
            lightAngles[1] = lightSource.spotAngle;
            ScalarStrengths[1] = 20;


            //Debug.Log($"PlayerLight position: {PlayerLight.transform.position}, direction: {-PlayerLight.transform.forward}, spotAngle: {PlayerLight.GetComponent<Light>().spotAngle}");
            //Debug.Log($"ClientLight position: {ClientLight.transform.position}, direction: {-ClientLight.transform.forward}, spotAngle: {ClientLight.GetComponent<Light>().spotAngle}");

            //ADD IN ENVIRONMENT LIGHT SOURCES
            for (int i = 0; i < lightCount - 2; i++)
            {
                lightSource = envLights[i].GetComponent<Light>();
                
                lightPositions[i + 2] = lightSource.transform.position;
                lightDirections[i + 2] = -lightSource.transform.forward;
                lightAngles[i + 2] = lightSource.spotAngle;
                float spotAngleMin = 10;
                float spotAngleMax = 55;
                float strengthMin = 500;
                float strengthMax = 30;
                float clampedSpotAngle = Mathf.Clamp(lightSource.spotAngle, spotAngleMin, spotAngleMax);
                float t = (clampedSpotAngle - spotAngleMin) / (spotAngleMax - spotAngleMin);

                ScalarStrengths[i + 2] = 30;//30           Mathf.Lerp(strengthMin, strengthMax, t);
                //ScalarStrengths[i + 2] = (1 - (lightSource.spotAngle / 180)) * maxStrength; ;// (1 - (lightSource.spotAngle / 180)) * maxStrength;

            }
            IsVisible(envLights);

            Debug.Log(visible);
            // Set the data to the shader
           // Shader.SetGlobalTexture("_ShadowMap", DynamicShadowMap);
            GetComponent<SkinnedMeshRenderer>().material.SetInt("_LightCount", lightCount);
            GetComponent<SkinnedMeshRenderer>().material.SetVectorArray("_LightPositions", lightPositions);
            GetComponent<SkinnedMeshRenderer>().material.SetVectorArray("_LightDirections", lightDirections);
            GetComponent<SkinnedMeshRenderer>().material.SetFloatArray("_LightAngles", lightAngles);
            GetComponent<SkinnedMeshRenderer>().material.SetFloatArray("_StrengthScalarLight", ScalarStrengths);
           


            //Shader.SetGlobalTexture("_ShadowMap", yourShadowMapTexture);

            /* //DEBUGGING SHADER OUTPUT
            Debug.Log($"Light count: {lightCount}");
            Debug.Log($"Light positions: {string.Join(", ", lightPositions.Select(pos => pos.ToString()))}");
            Debug.Log($"Light directions: {string.Join(", ", lightDirections.Select(dir => dir.ToString()))}");
            Debug.Log($"Light angles: {string.Join(", ", lightAngles.Select(angle => angle.ToString()))}");
            Debug.Log($"Scalar strengths: {string.Join(", ", ScalarStrengths.Select(str => str.ToString()))}");
            */

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
                if (shader == "Custom/Ghost") { visible = true; return; }
                else { visible = false; return; }//shadower
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