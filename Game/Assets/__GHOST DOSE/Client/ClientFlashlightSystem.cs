using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameManager;
public class ClientFlashlightSystem : MonoBehaviour
{
    [Header("FLASHLIGHT PARAMETERS")]
    [Space(10)]
    [SerializeField] public bool hasFlashlight = false;
    [SerializeField] public GameObject handFlashlight;
    [SerializeField] public GameObject inventoryFlashlight;
    [SerializeField] public Light FlashLight = null;
    [SerializeField] public Light WeaponLight = null;
    //[SerializeField] public float maxFlashlightIntensity = 1.0f;

    [Header("FLASHLIGHT SOUNDS")]
    [Space(10)]
    [SerializeField] private string pickUp;
    [SerializeField] private string flashlightClick;
    [SerializeField] private string reloadBattery;

    private bool isFlashlightOn;

    public static ClientFlashlightSystem instance;

    public bool toggleFlashlight = false;

    public float weapLightAngle = 45;
    private float flashLightAngle = 52;
    public float weapLightIntensity =5;
    private float flashLightIntensity =5;


    private static utilities util;
    private void Awake()
    {

        instance = this;

    }

 
    public void RigLights()
    {
        util = new utilities();

        WeaponLight = util.FindChildObject(this.gameObject.transform, "WeaponLight").GetComponent<Light>();
        FlashLight = util.FindChildObject(this.gameObject.transform, "FlashLight").GetComponent<Light>();
        handFlashlight = util.FindChildObject(this.gameObject.transform, "Flashlight_Hand");
        inventoryFlashlight = util.FindChildObject(this.gameObject.transform, "Flashlight_Inventory");

        FlashLight.spotAngle = flashLightAngle;
        FlashLight.intensity = flashLightIntensity;
        WeaponLight.spotAngle = weapLightAngle;
        WeaponLight.intensity = weapLightIntensity;

        GetComponent<ClientPlayerController>().currLight = FlashLight.gameObject;
       
        //GHOST LIGHT REFERENCES
        GameDriver.instance.ClientWeapLight = WeaponLight;
        GameDriver.instance.ClientFlashLight = FlashLight;

        Debug.Log("LIGHTS SETUP");
    }

    public void Start()
    {

        WeaponLight.enabled = false;
        FlashLight.enabled = false;
        handFlashlight.SetActive(false);
        inventoryFlashlight.SetActive(false);
        //FlashLight.intensity = maxFlashlightIntensity;
    }
    /*public void Flashlight()
    {
                if (FlashLight.enabled == false)
                {
                    isFlashlightOn = true;

                    handFlashlight.SetActive(true);
                    inventoryFlashlight.SetActive(false);

                    AudioManager.instance.Play(this.flashlightClick);
                    FlashLight.enabled = true;
                   // GameObject.Find("ClientSpot_Light_Flashlight").GetComponent<Light>().enabled = true;

                }
                else
                {
                    isFlashlightOn = false;
                    //Debug.Log(" CLIENT FLASH LIGHT IS OFF ");
                    handFlashlight.SetActive(false);
                    inventoryFlashlight.SetActive(true);
                    AudioManager.instance.Play(this.flashlightClick);
                    FlashLight.enabled = false;
                    //GameObject.Find("ClientSpot_Light_Flashlight").GetComponent<Light>().enabled = true;
                }

            if (isFlashlightOn)
            {

                if (FlashLight.intensity <= 0)
                {
                    FlashLight.intensity = 0;
                    isFlashlightOn = false;
                }
            }

        
    }*/
}
