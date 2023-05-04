using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using GameManager;
using InteractionSystem;
using NetworkSystem;

//[ExecuteInEditMode]
public class GhostVFX : MonoBehaviour
{

    [HideInInspector] public bool Shadower;
    [HideInInspector] public GameObject PlayerLight;
    [HideInInspector] public GameObject ClientLight;
    private GameObject skin;
    Light[] envLights;
    //private GameDriver GD;
    [HideInInspector] public List<Light> LightSources;
    List<float> originalMaxAlpha = new List<float>(); //affects total alpha
    List<float> currentMaxAlpha = new List<float>();
    public bool visible; //USD IN CONJUNCTION WITH PLAY AIM RAYCAST
    public bool inShadow;
    public bool invisible;
    //public int invisibleCounter;
    public GameObject HEAD;
    public bool death;
    private bool visibilitySet;
    //private float sound_timer = 0f;
    //private float sound_delay = 5f;
    private bool playedSound;

    //ENVIRONMENT LIGHTS
    int envLightCount;
    Vector4[] lightPositions;
    Vector4[] lightDirections;
    float[] lightAngles;
    float[] ScalarStrengths;
    float[] lightRanges;

    //public RenderTexture DynamicShadowMap;


    public void Start()
    {
       // GD = GameObject.Find("GameController").GetComponent<GameDriver>();
        skin = gameObject.transform.GetChild(0).gameObject;

        //-----STORE ORIGINAL ALPHA SETTINGS
        for (int i = 0; i < skin.GetComponent<SkinnedMeshRenderer>().materials.Length; i++)
        {
            if (skin.GetComponent<SkinnedMeshRenderer>().materials[i].shader.name == "Custom/Ghost" || skin.GetComponent<SkinnedMeshRenderer>().materials[i].shader.name == "Custom/GhostAlphaCutoff")
            {
                Material material = skin.GetComponent<SkinnedMeshRenderer>().materials[i];
                float alphaValue = material.GetFloat("_Alpha");
                originalMaxAlpha.Add(alphaValue);
                currentMaxAlpha.Add(alphaValue);
                if (Shadower) { gameObject.tag = "Shadower"; HEAD.tag = "Shadower"; material.SetInt("_Shadower", 1); }
            }
        }
        //------FIND ENVIRONMENT LIGHTS---------------
        envLights = new Light[0];
        GhostLight[] ghostLights = FindObjectsOfType<GhostLight>();
        List<Light> lights = new List<Light>();
        foreach (GhostLight ghostLight in ghostLights)
        {
            if ((ghostLight.ghost && this.gameObject.tag == "Ghost") || (ghostLight.shadower && this.gameObject.tag == "Shadower"))
            {
                Light light = ghostLight.GetComponent<Light>();
                if (light != null)
                {
                    lights.Add(light);
                }
            }
        }
        envLights = lights.ToArray();

        //-------SETUP LIGHTS-------------
        Light lightSource;
        if (envLights.Length > 0)
        {
            envLightCount = envLights.Length;
            lightPositions = new Vector4[envLightCount];
            lightDirections = new Vector4[envLightCount];
            lightAngles = new float[envLightCount];
            ScalarStrengths = new float[envLightCount];
            lightRanges = new float[envLightCount];

            for (int i = 0; i < envLightCount; i++)
            {
                lightSource = envLights[i].GetComponent<Light>();

                lightPositions[i] = lightSource.transform.position;
                lightDirections[i] = -lightSource.transform.forward;
                lightAngles[i] = lightSource.spotAngle + 5;
                ScalarStrengths[i] = lightSource.GetComponent<GhostLight>().strength;//50
                lightRanges[i] = lightSource.range;//30
            }
        }
        //Debug.Log("--------------------------------------------- LIGHT COUNT " + envLightCount);
    }

    public void UpdateShaderValues()
    {
        if (Application.isPlaying)
        {
            PlayerLight = GameDriver.instance.Player.GetComponent<PlayerController>().currLight;
            ClientLight = GameDriver.instance.Client.GetComponent<ClientPlayerController>().currLight;

            if (GameObject.Find("CamFlashPlayer") != null) { PlayerLight = GameObject.Find("CamFlashPlayer"); }
            if (GameObject.Find("CamFlashClient") != null) { ClientLight = GameObject.Find("CamFlashClient"); }
        }
        visibilitySet = false;
        //-------------------DEATH LIGHT UP ENEMY-----------------------------
        if (death) { if (gameObject.tag != "Shadower") { PlayerLight = GetComponent<EnemyDeath>().light; } else { inShadow = true; }  }
        //if (PlayerLight != null && ClientLight != null)
        {
            Light lightSource;
            //UPDATE ENVIRONMENT LIGHT SOURCES
            if (envLights.Length > 0)
            {
                for (int i = 0; i < envLightCount; i++)
                {
                    lightSource = envLights[i].GetComponent<Light>();
                    lightAngles[i] = lightSource.spotAngle+5;
                }
                IsVisible(envLights);
                foreach (Material material in skin.GetComponent<SkinnedMeshRenderer>().materials)
                {
                    material.SetInt("_EnvLightCount", envLightCount);
                    material.SetVectorArray("_LightPositions", lightPositions);
                    material.SetVectorArray("_LightDirections", lightDirections);
                    material.SetFloatArray("_LightAngles", lightAngles);
                    material.SetFloatArray("_StrengthScalarLight", ScalarStrengths);
                    material.SetFloatArray("_LightRanges", lightRanges);
                    //Debug.Log("--LIGHTS--" + "count " + envLightCount + "pos " + lightPositions[0] + "dir " + lightDirections[0] + "angle " + lightAngles[0] + "strength " + ScalarStrengths[0] + "range " + lightRanges[0]);
                }
            }

            //SHADOW OVERRIDES ENVIRONMENT LIGHTS
            if (inShadow) {
                visibilitySet = true;
                if (this.gameObject.tag == "Ghost") { visible = false;  } else { visible = true; }
            }


            //ADD IN PLAYER LIGHT
            lightSource = PlayerLight.GetComponent<Light>();
            float spotAngle = lightSource.spotAngle;
            if (!lightSource.enabled) { spotAngle = 0; }
            else { //flashlight enabled
                if (!InLineOfSight(lightSource, true)) { spotAngle = 0; }
                //FLASHLIGHT OVERRIDES SHADOW AND ENVIORNMENT
                else if (IsObjectInLightCone(lightSource, true))//directly under light source
                {
                    if (NetworkDriver.instance.HOST) { if (GetComponent<NPCController>() != null) { GetComponent<NPCController>().alertLevelPlayer += 2; } }
                    if(!invisible) { TriggerWanderSound(); }
                    if (this.gameObject.tag == "Ghost") { visible = true; visibilitySet = true; } else { visible = false; visibilitySet = true; }
                } 
            }
            foreach (Material material in skin.GetComponent<SkinnedMeshRenderer>().materials)
            {
                material.SetVector("_PlayerLightPosition", lightSource.transform.position);
                material.SetVector("_PlayerLightDirection", -lightSource.transform.forward);
                material.SetFloat("_PlayerLightAngle", spotAngle);
                material.SetFloat("_PlayerStrengthScalarLight", 20);
                material.SetFloat("_PlayerLightRange", lightSource.range);
            }
            //ADD IN CLIENT LIGHT
            lightSource = ClientLight.GetComponent<Light>();
            spotAngle = lightSource.spotAngle;
            if (!lightSource.enabled) { spotAngle = 0; }
            else
            {
                if (!InLineOfSight(lightSource,true)) { spotAngle = 0; }
                //FLASHLIGHT OVERRIDES SHADOW AND ENVIORNMENT
                else if(IsObjectInLightCone(lightSource, true))//directly under light source
                {
                    if (NetworkDriver.instance.HOST) { if (GetComponent<NPCController>() != null) { GetComponent<NPCController>().alertLevelClient += 2; } }
                    if (this.gameObject.tag == "Ghost") { visible = true; visibilitySet = true; } else { visible = false; visibilitySet = true; }
                }
            }
            foreach (Material material in skin.GetComponent<SkinnedMeshRenderer>().materials)
            {
                material.SetVector("_ClientLightPosition", lightSource.transform.position);
                material.SetVector("_ClientLightDirection", -lightSource.transform.forward);
                material.SetFloat("_ClientLightAngle", spotAngle);
                material.SetFloat("_ClientStrengthScalarLight", 20);
                material.SetFloat("_ClientLightRange", lightSource.range);
            }

            //--------------VISIBLE IF CLOSE ----------------
             if(Vector3.Distance(PlayerLight.gameObject.transform.position, this.transform.position)<2 || Vector3.Distance(ClientLight.gameObject.transform.position, this.transform.position) < 2)
            {
                if (this.gameObject.tag == "Ghost") { visible = true; visibilitySet = true; }//if (GetComponent<Teleport>().teleport == 0) { }
            }

            //-------------SET TOTAL ALPHA--------------------------------
            if (death) { visible = true; visibilitySet = true; Fade(true, 0.5f, 1); }
            invisible = false;
            if (!death)
            {
                if (visible) { Fade(true, 0.5f, 1); }
                else { Fade(false, 1f, 1); }//fadeout
            }// DEATH FADE
            else { Fade(false, 1f, 0); }
            for (int i = 0; i < skin.GetComponent<SkinnedMeshRenderer>().materials.Length; i++)
            {
                if (skin.GetComponent<SkinnedMeshRenderer>().materials[i].shader.name == "Custom/Ghost") { 
                    
                    skin.GetComponent<SkinnedMeshRenderer>().materials[i].SetFloat("_Alpha", currentMaxAlpha[i]); 
                }
                else if (skin.GetComponent<SkinnedMeshRenderer>().materials[i].shader.name == "Custom/GhostAlphaCutoff")
                {
                    skin.GetComponent<SkinnedMeshRenderer>().materials[i].SetFloat("_Emission", currentMaxAlpha[i]); 
                }
            }

            //DEFAULT VISIBLIITY
            if (!visibilitySet)
            {
                //Debug.Log("---------------------DEFAULT----------------------------");
                if (this.gameObject.tag == "Ghost") { visible = false; }
                else { visible = true; }
            }

            //--------------OUTLINE VISABILITY----------------------
            if (gameObject.transform.GetChild(0).GetComponent<Outline>() != null) { if (gameObject.transform.GetChild(0).GetComponent<Outline>().OutlineWidth > 0.1f) { invisible = false; } }



        }

    }

    public void Fade(bool fadeIn, float speed, int fadeOutLimit)
    {
        for (int i = 0; i < skin.GetComponent<SkinnedMeshRenderer>().materials.Length; i++)
        {
            if (skin.GetComponent<SkinnedMeshRenderer>().materials[i].shader.name == "Custom/Ghost" || skin.GetComponent<SkinnedMeshRenderer>().materials[i].shader.name == "Custom/GhostAlphaCutoff")
            {
                if (fadeIn)
                {
                    if (currentMaxAlpha[i] < originalMaxAlpha[i]) { currentMaxAlpha[i] = Mathf.Lerp(currentMaxAlpha[i], originalMaxAlpha[i], Time.deltaTime * speed); }
                    // if (Mathf.Abs(currentMaxAlpha[i] - (originalMaxAlpha[i] * 0.5f)) < 0.1f) { fadeDone = true; }
                }
                else//FADE OUT
                {
                    if (currentMaxAlpha[i] > 0.01) { currentMaxAlpha[i] = Mathf.Lerp(currentMaxAlpha[i], currentMaxAlpha[i] * 0.5f * fadeOutLimit, Time.deltaTime * speed); }
                    if (Mathf.Abs(currentMaxAlpha[i] - 0.1f) < 0.1f)
                    {
                        invisible = true; playedSound = false;
                    }
                }
            }
        }   

       // Debug.Log("------------------------------------------ FADE --------------------------------" + fadeDone);
      //  return fadeDone;
    }


    private void IsVisible(Light[] lights)
    {
        Light closestLight = null;
        float closestDistance = Mathf.Infinity;
        for (int i = 0; i < lights.Length; i++)
        {
            if (IsObjectInLightCone(lights[i].GetComponent<Light>(), false))
            { 
                float distance = Vector3.Distance(transform.position, lights[i].transform.position);
                if (distance < closestDistance)
                {
                    closestLight = lights[i].GetComponent<Light>();
                    closestDistance = distance;
                }
            }
        }
        if (closestLight != null)
        {
            
            if (InLineOfSight(closestLight.GetComponent<Light>(), false))
            {
                visibilitySet = true;
                if (this.gameObject.tag == "Ghost") { visible = true; }
                else { visible = false; } // shadower 
            }
            else
            {
               // if (this.gameObject.tag == "Ghost") { visible = false; }
               // else { visible = true; Debug.Log(closestLight + " NOT IN LINE OF SIGHT "); } // shadower
            }

        }
    }

    private bool InLineOfSight(Light light, bool ignoreShadow)
    {
        LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Enemy");
        if (!ignoreShadow){mask |= 1 << LayerMask.NameToLayer("ShadowBox"); }// INCLUDE SHADOW LAYER

        float hitHeight = 0f; // adjust the hit height 1.3
        Vector3 targPos = new Vector3(HEAD.transform.position.x, HEAD.transform.position.y + hitHeight, HEAD.transform.position.z);
        Ray ray = new Ray(light.transform.position, (targPos - light.transform.position).normalized);
        float distance = Vector3.Distance(light.transform.position, targPos);
        Vector3 endPoint = ray.GetPoint(distance);
        Debug.DrawLine(light.transform.position, endPoint, UnityEngine.Color.blue);

        // Perform the raycast, excluding the specified layers
        RaycastHit[] hits = Physics.RaycastAll(ray, distance, mask);
        bool targetHit = false;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Enemy")) { targetHit = false; break; }//environment obstruction
            if (hit.collider.GetComponentInParent<NPCController>().gameObject == this.gameObject)
            {
                targetHit = true;
                //FLICKER
                if (GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip != null && GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "agro" && light.GetComponent<GhostLight>()!=null) { light.GetComponent<GhostLight>().InvokeFlicker(1f); }
                break;
            }
        }
        return targetHit;
    }


    //CALLED BY PLAYER AIMING - DETERMINE IF CAN HIT WHILE IN LIGHT
    public bool IsObjectInLightCone(Light spotlight, bool isPlayer)
    {
        float adjustedSpotAngle = spotlight.spotAngle;
        float hitHeight = 1f; 

        if (!isPlayer) { adjustedSpotAngle = spotlight.spotAngle * 1.1f;
            hitHeight = 1.5f;
        }
        float distanceToObject = Vector3.Distance(this.gameObject.transform.position + Vector3.up * hitHeight, spotlight.transform.position);
        bool inRange = distanceToObject <= spotlight.range;
        Vector3 directionToObject = (this.gameObject.transform.position + Vector3.up * hitHeight - spotlight.transform.position).normalized;
       // Debug.DrawLine(spotlight.transform.position, this.gameObject.transform.position + Vector3.up * hitHeight, UnityEngine.Color.red);
        float angleToObject = Vector3.Angle(spotlight.transform.forward, directionToObject);
        bool inCone = angleToObject <= adjustedSpotAngle * 0.5f;
        //if (isPlayer && spotlight.gameObject.name == "player_light") { Debug.Log("IN CONE? " + inCone); }
        if (isPlayer && this.gameObject.tag == "Shadower") { return inCone; }//fl effect on shadowers have no range
        return inRange && inCone;
    }

    private float sound_timer;
    private float sound_delay = 30f;
    public void TriggerWanderSound()
    {
        //if(NetworkDriver.instance.HOST)

        if (Time.time > sound_timer + sound_delay)
        {
            if (GetComponent<Teleport>() != null && GetComponent<Teleport>().teleport == 0)
            {
                if (!playedSound && GetComponent<NPCController>() != null && !GetComponent<NPCController>().agro)
                {
                    int i;
                    i = UnityEngine.Random.Range(1, 5);
                    if (!invisible) { AudioManager.instance.Play("Wander" + i.ToString(), null); }
                    playedSound = true;
                    sound_timer = Time.time;//cooldown
                }
            }
        }
        
    }


}




