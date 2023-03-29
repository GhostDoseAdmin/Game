﻿using System;
using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class Reveal : MonoBehaviour
{
    private SkinnedMeshRenderer renderer;
    Material[] materials;

    [SerializeField] Light PlayerLight;
    private Light PlayerWeapLight;
    private Light PlayerFlashLight;

    [SerializeField] Light ClientLight;
    private Light ClientWeapLight;
    private Light ClientFlashLight;

    private int materialIndex = 0;
    private string newMaterialName = "Copy";
    private float maxDistance = 10;
    private GameDriver GD;
    private bool lightsSetup = false;

    public void Start()
    {
        lightsSetup = false;

        //-------------------- CLONE MATERIAL FOR 2nd PLAYER --------------------------
        renderer = GetComponent<SkinnedMeshRenderer>();
        if (renderer != null && materialIndex >= 0 && materialIndex < renderer.sharedMaterials.Length)
        {
            Material material = renderer.sharedMaterials[materialIndex];

            // Check if new material name already exists
            bool nameExists = false;
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat.name == newMaterialName)
                {
                    nameExists = true;
                    break;
                }
            }

            if (!nameExists)
            {
                Material newMaterial = new Material(material);
                newMaterial.name = newMaterialName;
                materials = renderer.sharedMaterials;
                Array.Resize(ref materials, materials.Length + 1);
                materials[materials.Length - 1] = newMaterial;
                renderer.sharedMaterials = materials;
            }
        }

        

        //------------------- GET CURRENT FLASHLIGHTS ---------------------------------
        GD = GameObject.Find("GameController").GetComponent<GameDriver>();
        //StartCoroutine(GameStarted());
        //IEnumerator GameStarted()
        {
            
            //while (!GD.GAMESTART){
               // yield return new WaitForSeconds(5f);
            //}
           
            PlayerWeapLight = GD.PlayerWeapLight;
            PlayerFlashLight = GD.PlayerFlashLight;
            ClientWeapLight = GD.ClientWeapLight;
            ClientFlashLight = GD.ClientFlashLight;

            PlayerLight = PlayerFlashLight;
            ClientLight = ClientFlashLight;

            lightsSetup = true;
        }

    }

    void Update()
    {
        //if (lightsSetup)
        {
           // Debug.Log(GD.Player.GetComponent<FlashlightSystem>().WeaponLight.enabled);
            //--------------------CHOOSE LIGHT SOURCES-----------------------
             if (PlayerWeapLight.enabled) { PlayerLight = PlayerWeapLight; }
             if (PlayerFlashLight.enabled) { PlayerLight = PlayerFlashLight; }
             if (ClientWeapLight.enabled) { ClientLight = ClientWeapLight; }
             if (ClientFlashLight.enabled) { ClientLight = ClientFlashLight; }

             //-------------------RENDER MATERIALS ---------------------------
             Material[] materials = renderer.sharedMaterials;

             materials[0].SetVector("_LightPosition", PlayerLight.transform.position);
             materials[0].SetVector("_LightDirection", -PlayerLight.transform.forward);
             materials[0].SetFloat("_LightAngle", PlayerLight.spotAngle);
             if (!PlayerLight.isActiveAndEnabled) { materials[0].SetFloat("_MaxDistance", 0); }
             else { materials[0].SetFloat("_MaxDistance", maxDistance); }

             materials[1].SetVector("_LightPosition", ClientLight.transform.position);
             materials[1].SetVector("_LightDirection", -ClientLight.transform.forward);
             materials[1].SetFloat("_LightAngle", ClientLight.spotAngle);
             materials[1].SetFloat("_MaxDistance", 10);
             if (!ClientLight.isActiveAndEnabled) { materials[1].SetFloat("_MaxDistance", 0); }
             else { materials[1].SetFloat("_MaxDistance", maxDistance); }
            
        }
    }


}