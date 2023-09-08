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

    public bool Shadower;//SET BY NPC CONTROLLER
    [HideInInspector] public GameObject PlayerLight;
    [HideInInspector] public GameObject ClientLight;
    private GameObject skin;
    Light[] envLights;
    [HideInInspector] public List<Light> LightSources;
    public List<float> originalMaxAlpha = new List<float>(); //affects total alpha
    public List<float> currentMaxAlpha = new List<float>();
    public bool visible; //USD IN CONJUNCTION WITH PLAY AIM RAYCAST
    public bool inShadow;
    public bool invisible;
    public GameObject HEAD;
    //public bool death;
    private bool visibilitySet;
    private bool playedSound;

    //ENVIRONMENT LIGHTS
    int envLightCount;
    Vector4[] lightPositions;
    Vector4[] lightDirections;
    float[] lightAngles;
    float[] ScalarStrengths;
    float[] lightRanges;

    private int updateCall;

    public bool camflashplayer, camflashclient;
    public void Awake()
    {
        Shadower = GetComponent<NPCController>().Shadower;
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
                if (Shadower) { 
                    if (gameObject.tag != "ZOZO") { 
                        gameObject.tag = "Shadower"; HEAD.tag = "Shadower";
                    } 
                    material.SetInt("_Shadower", 1); 
                }
            }
        }
        //------FIND ENVIRONMENT LIGHTS---------------
        envLights = new Light[0];
        GhostLight[] ghostLights = FindObjectsOfType<GhostLight>();
        List<Light> lights = new List<Light>();
        foreach (GhostLight ghostLight in ghostLights)
        {
            if(Vector3.Distance(ghostLight.gameObject.transform.position, this.gameObject.transform.position)<80)
            {
                if ((ghostLight.ghost || ghostLight.shadower) && (ghostLight.gameObject.name!="WeaponLight" && ghostLight.gameObject.name != "FlashLight"))
                {
                    Light light = ghostLight.GetComponent<Light>();
                    if (light != null)
                    {
                        lights.Add(light);
                        //Debug.Log(light.gameObject.name);
                    }
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
                if (!GetComponent<NPCController>().ZOZO) { ScalarStrengths[i] = lightSource.GetComponent<GhostLight>().strength; }//50
                else { ScalarStrengths[i] = 5; }
                lightRanges[i] = lightSource.range;//30
            }
            //Debug.Log("COUNT " + envLights.Length);
            foreach (Material material in skin.GetComponent<SkinnedMeshRenderer>().materials)
            {
                //Debug.Log("---------------------LIGHT POSITIONS " + lightPositions[0] + " " + material.name +" "+ envLights.Length);
                material.SetInt("_EnvLightCount", envLightCount);
                material.SetVectorArray("_LightPositions", lightPositions);
                material.SetVectorArray("_LightDirections", lightDirections);
                //material.SetFloatArray("_LightAngles", lightAngles);
                material.SetFloatArray("_StrengthScalarLight", ScalarStrengths);
                material.SetFloatArray("_LightRanges", lightRanges);
                //Debug.Log("--LIGHTS--" + "count " + envLightCount + "pos " + lightPositions[0] + "dir " + lightDirections[0] + "angle " + lightAngles[0] + "strength " + ScalarStrengths[0] + "range " + lightRanges[0]);
            }
        }

        //Debug.Log("--------------------------------------------- LIGHT COUNT " + envLightCount);
    }

    public void UpdateShaderValues()
    {
        updateCall++;
        //if (updateCall > 2)
        if ((updateCall > 1 && GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), skin.GetComponent<SkinnedMeshRenderer>().bounds)) || (GetComponent<Teleport>().teleport!=0))//
        {
            updateCall = 0;
            if (Application.isPlaying)
            {
                camflashplayer = false; camflashclient = false;
                PlayerLight = GameDriver.instance.Player.GetComponent<PlayerController>().currLight;
                ClientLight = GameDriver.instance.Client.GetComponent<ClientPlayerController>().currLight;

                if (GameObject.Find("CamFlashPlayer") != null) { PlayerLight = GameObject.Find("CamFlashPlayer"); camflashplayer = true; }
                if (GameObject.Find("CamFlashClient") != null) { ClientLight = GameObject.Find("CamFlashClient"); camflashclient = true; }
            }
            visibilitySet = false;
            //-------------------DEATH LIGHT UP ENEMY-----------------------------
            // if (death) { if (gameObject.tag != "Shadower") { PlayerLight = GetComponent<EnemyDeath>().light; } else { inShadow = true; }  }
            //if (PlayerLight != null && ClientLight != null)
            {
                Light lightSource;
                //UPDATE ENVIRONMENT LIGHT SOURCES
                if (envLights.Length > 0)
                {
                    for (int i = 0; i < envLightCount; i++)
                    {
                        //Debug.Log(envLights[i].gameObject.name);
                        lightSource = envLights[i].GetComponent<Light>();
                        lightAngles[i] = lightSource.spotAngle + 5;
                    }
                    IsVisible(envLights);
                    foreach (Material material in skin.GetComponent<SkinnedMeshRenderer>().materials)
                    {
                        //material.SetInt("_EnvLightCount", envLightCount);
                        //material.SetVectorArray("_LightPositions", lightPositions);
                        //material.SetVectorArray("_LightDirections", lightDirections);
                        material.SetFloatArray("_LightAngles", lightAngles);
                        //material.SetFloatArray("_StrengthScalarLight", ScalarStrengths);
                        //material.SetFloatArray("_LightRanges", lightRanges);
                        //Debug.Log("--LIGHTS--" + "count " + envLightCount + "pos " + lightPositions[0] + "dir " + lightDirections[0] + "angle " + lightAngles[0] + "strength " + ScalarStrengths[0] + "range " + lightRanges[0]);
                    }


                }
                //SHADOW OVERRIDES ENVIRONMENT LIGHTS
                if (inShadow)
                {
                    visibilitySet = true;
                    if (!Shadower) { visible = false; } else { visible = true; }
                }


                //ADD IN PLAYER LIGHT
                lightSource = PlayerLight.GetComponent<Light>();
                float spotAngle = lightSource.spotAngle;
                if (!lightSource.enabled) { spotAngle = 0; }
                else
                { //flashlight enabled
                    if (!InLineOfSight(lightSource, true, false)) { spotAngle = 0; }
                    //FLASHLIGHT OVERRIDES SHADOW AND ENVIORNMENT
                    else if (IsObjectInLightCone(lightSource, true))//directly under light source
                    {
                        if (NetworkDriver.instance.HOST) { if (GetComponent<NPCController>() != null) { GetComponent<NPCController>().alertLevelPlayer += 10; } } //4
                        if (!invisible) { TriggerWanderSound(); }
                        if (!Shadower) { visible = true; visibilitySet = true; } else { visible = false; visibilitySet = true; }
                        GetComponentInChildren<Outline>().OutlineWidth = 4;
                    }
                }
                foreach (Material material in skin.GetComponent<SkinnedMeshRenderer>().materials)
                {
                    material.SetVector("_PlayerLightPosition", lightSource.transform.position);
                    material.SetVector("_PlayerLightDirection", -lightSource.transform.forward);
                    material.SetFloat("_PlayerLightAngle", spotAngle);
                    material.SetFloat("_PlayerStrengthScalarLight", 40);//20
                    if (camflashplayer) { material.SetFloat("_PlayerStrengthScalarLight", 100); }
                    //if (!GetComponent<NPCController>().ZOZO) { material.SetFloat("_PlayerStrengthScalarLight", 20); }
                    //else { material.SetFloat("_PlayerStrengthScalarLight", 5); }
                    material.SetFloat("_PlayerLightRange", lightSource.range);
                }
                //FLICKER
                //if (GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length > 0 && GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "agro" && PlayerLight.GetComponent<GhostLight>() != null) { PlayerLight.GetComponent<GhostLight>().InvokeFlicker(1f); }
                //ADD IN CLIENT LIGHT
                lightSource = ClientLight.GetComponent<Light>();
                spotAngle = lightSource.spotAngle;
                if (!lightSource.enabled) { spotAngle = 0; }
                else
                {
                    if (!InLineOfSight(lightSource, true, false)) { spotAngle = 0; }
                    //FLASHLIGHT OVERRIDES SHADOW AND ENVIORNMENT
                    else if (IsObjectInLightCone(lightSource, true))//directly under light source
                    {
                        if (NetworkDriver.instance.HOST) { if (GetComponent<NPCController>() != null) { GetComponent<NPCController>().alertLevelClient += 10; } }//4
                        if (!Shadower) { visible = true; visibilitySet = true; } else { visible = false; visibilitySet = true; }
                        GetComponentInChildren<Outline>().OutlineWidth = 4;
                    }
                }
                foreach (Material material in skin.GetComponent<SkinnedMeshRenderer>().materials)
                {
                    material.SetVector("_ClientLightPosition", lightSource.transform.position);
                    material.SetVector("_ClientLightDirection", -lightSource.transform.forward);
                    material.SetFloat("_ClientLightAngle", spotAngle);
                    material.SetFloat("_ClientStrengthScalarLight", 40);//20
                    if (camflashclient) { material.SetFloat("_ClientStrengthScalarLight", 100); }
                    //if (!GetComponent<NPCController>().ZOZO) { material.SetFloat("_ClientStrengthScalarLight", 20); }
                    //else { material.SetFloat("_ClientStrengthScalarLight", 5); }
                    material.SetFloat("_ClientLightRange", lightSource.range);
                }
                //FLICKER
                //if (GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length > 0 && GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "agro" && ClientLight.GetComponent<GhostLight>() != null) { ClientLight.GetComponent<GhostLight>().InvokeFlicker(1f); }

                //--------------VISIBLE IF CLOSE ----------------
                if (Vector3.Distance(PlayerLight.gameObject.transform.position, this.transform.position) < 2 || Vector3.Distance(ClientLight.gameObject.transform.position, this.transform.position) < 2)
                {
                    if (!Shadower) { visible = true; visibilitySet = true; }//if (GetComponent<Teleport>().teleport == 0) { }
                }

                //-------------SET TOTAL ALPHA--------------------------------
                //if (death) { visible = true; visibilitySet = true; Fade(true, 5f, 1); }
                invisible = false;
                {
                    float fadeinfactor = 1;
                    if(camflashplayer || camflashclient) { fadeinfactor = 2; }//accelerate visiblity from camflashes
                    if (visible) { 
                        if (GetComponent<ZozoControl>() == null) { 
                            Fade(true, fadeinfactor * 0.5f); //inverse relationship for shadowers
                        } 
                        else {//ZOZO FADE
                            fadeinfactor = 0.5f;
                            if (!NetworkDriver.instance.TWOPLAYER) { fadeinfactor *=2f; }//single player zozo easier to kill
                            Fade(true, fadeinfactor * 2f); //higher value, fades OUT faster, easier to kill
                        }  
                    }
                    else { Fade(false, 1f); }//fadeout
                }// DEATH FADE
                 //else { Fade(false, 10f, 0); }
                for (int i = 0; i < skin.GetComponent<SkinnedMeshRenderer>().materials.Length; i++)
                {

                    if (skin.GetComponent<SkinnedMeshRenderer>().materials[i].shader.name == "Custom/Ghost")
                    {

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
                    if (!Shadower) { visible = false; }
                    else { visible = true; }
                }

                //--------------OUTLINE VISABILITY----------------------
                if (gameObject.transform.GetChild(0).GetComponent<Outline>() != null) { if (gameObject.transform.GetChild(0).GetComponent<Outline>().OutlineWidth > 0.1f) { invisible = false; } }



            }

        }
    }
    public void Fade(bool fadeIn, float speed)
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
                   // Debug.Log(currentMaxAlpha[i]);
                    //if (currentMaxAlpha[i] > 0.01) { currentMaxAlpha[i] = Mathf.Lerp(currentMaxAlpha[i], currentMaxAlpha[i] * 0.5f * fadeOutLimit, Time.deltaTime * speed); }
                    if (currentMaxAlpha[i] > 0.01) { currentMaxAlpha[i] = Mathf.Lerp(currentMaxAlpha[i], 0, Time.deltaTime * speed); }
                    if (currentMaxAlpha[i] < 0.2f) //|| (GetComponent<ZozoControl>()!=null && currentMaxAlpha[i] < 0.2f)) //if diff in alpha less than 0.1
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
        if (lights != null)
        {
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

                if (InLineOfSight(closestLight.GetComponent<Light>(), false, true))
                {
                    visibilitySet = true;
                    if (!Shadower) { visible = true; }
                    else { visible = false; } // shadower 
                }
            }
        }
        

    }

    private bool InLineOfSight(Light light, bool ignoreShadow, bool env)
    {

        LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Enemy");
        if (!ignoreShadow){mask |= 1 << LayerMask.NameToLayer("ShadowBox"); }// INCLUDE SHADOW LAYER

        float hitHeight = 0f; // adjust the hit height 1.3
        Vector3 targPos = new Vector3(HEAD.transform.position.x, HEAD.transform.position.y + hitHeight, HEAD.transform.position.z);
        Ray ray = new Ray(light.transform.position, (targPos - light.transform.position).normalized);
        float distance = Vector3.Distance(light.transform.position, targPos);
        //Vector3 endPoint = ray.GetPoint(distance);
        //Debug.DrawLine(light.transform.position, endPoint, UnityEngine.Color.blue);

        // Perform the raycast, excluding the specified layers
        RaycastHit[] hits = Physics.RaycastAll(ray, distance, mask);
        bool targetHit = false;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Enemy")) { targetHit = false; break; }//environment obstruction
            if (hit.collider.GetComponentInParent<NPCController>().gameObject == this.gameObject)
            {
                targetHit = true;
                if(GetComponent<ZozoControl>() != null) { GetComponent<ZozoControl>().ZOZOFlinch(false, env); if (camflashplayer || camflashclient) { GetComponent<ZozoControl>().ZOZOFlinch(true, env); } }
                //FLICKER
                if (GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length > 0 && GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "agro" && light.GetComponent<GhostLight>() != null) { if (light.GetComponent<GhostLight>().canFlicker) { light.GetComponent<GhostLight>().InvokeFlicker(1f); } }
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
        if (isPlayer && Shadower) { return inCone; }//fl effect on shadowers have no range

        //if (spotlight == null || this.gameObject == null  || directionToObject==null) { Debug.Log("-------------------------------------------------------------NULL"); }
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




