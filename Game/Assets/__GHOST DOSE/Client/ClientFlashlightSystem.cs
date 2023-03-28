using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientFlashlightSystem : MonoBehaviour
{
    [Header("FLASHLIGHT PARAMETERS")]
    [Space(10)]
    [SerializeField] public bool hasFlashlight = false;
    [SerializeField] public GameObject handFlashlight;
    [SerializeField] public GameObject inventoryFlashlight;
    [SerializeField] public Light FlashLight = null;
    [SerializeField] public Light WeaponLight = null;
    [SerializeField] public float maxFlashlightIntensity = 1.0f;

    [Header("FLASHLIGHT SOUNDS")]
    [Space(10)]
    [SerializeField] private string pickUp;
    [SerializeField] private string flashlightClick;
    [SerializeField] private string reloadBattery;

    private bool isFlashlightOn;

    public static ClientFlashlightSystem instance;

    public bool toggleFlashlight = false;

    private void Awake()
    {

        instance = this;

        WeaponLight = FindChildObject(this.gameObject.transform, "WeaponLight").GetComponent<Light>();
        FlashLight = FindChildObject(this.gameObject.transform, "FlashLight").GetComponent<Light>();
        handFlashlight = FindChildObject(this.gameObject.transform, "Flashlight_Hand");
        inventoryFlashlight = FindChildObject(this.gameObject.transform, "Flashlight_Inventory");

        WeaponLight.enabled = false;
        FlashLight.enabled = false;
        handFlashlight.SetActive(false);

        inventoryFlashlight.SetActive(false);
        FlashLight.intensity = maxFlashlightIntensity;

        Debug.Log("CLIENT LIGHTING SET");
    }

    public GameObject FindChildObject(Transform parentTransform, string name)
    {
        if (parentTransform.gameObject.name == name)
        {
            return parentTransform.gameObject;
        }

        foreach (Transform childTransform in parentTransform)
        {
            GameObject foundObject = FindChildObject(childTransform, name);
            if (foundObject != null)
            {
                return foundObject;
            }
        }
        return null;
    }

    void Start()
    {



    }

 
    public void Flashlight()
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

        
    }
}
