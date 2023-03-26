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

    [Header("BATTERY PARAMETERS")]
    [Space(10)]
    [SerializeField] public int batteryCount = 1;
    [SerializeField] private float replaceBatteryTimer = 1.0f;
    [SerializeField] private float maxReplaceBatteryTimer = 1.0f;

    [Header("FLASHLIGHT SOUNDS")]
    [Space(10)]
    [SerializeField] private string pickUp;
    [SerializeField] private string flashlightClick;
    [SerializeField] private string reloadBattery;

    private bool shouldUpdate = false;
    private bool showOnce = false;
    private bool isFlashlightOn;

    public static FlashlightSystem FLS;
    public static ClientFlashlightSystem instance;

    public bool toggleFlashlight = false;

    private void Awake()
    {
        if (instance != null) 
        { 
            Destroy(gameObject); 
        }
        else 
        { 
            instance = this;
        }

    }

    void Start()
    {


        WeaponLight = FLS.FindChildObject(this.gameObject.transform, "WeaponLight").GetComponent<Light>();
        FlashLight = FLS.FindChildObject(this.gameObject.transform, "FlashLight").GetComponent<Light>();
        handFlashlight = FLS.FindChildObject(this.gameObject.transform, "Flashlight_Hand");

        WeaponLight.enabled = false;
        FlashLight.enabled = false;
        handFlashlight.SetActive(false);

        inventoryFlashlight.SetActive(false);
        FlashLight.intensity = maxFlashlightIntensity;
    }

    void Update()
    {
        Flashlight();
        ReloadBattery();
    }

    void Flashlight()
    {
        //Debug.Log("FLASHLIGHT RUNNING ");
        if (hasFlashlight)
        {
            //Debug.Log(" HAS A FLASHLIGHT");
            if (toggleFlashlight && !showOnce)
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
                toggleFlashlight = false;
            }

            if (isFlashlightOn)
            {
                /*if (FlashLight.intensity <= maxFlashlightIntensity && FlashLight.intensity > 0)
                {
                    FlashLight.intensity -= (0.007f * Time.deltaTime) * maxFlashlightIntensity;
                    //batteryLevel.fillAmount -= 0.007f * Time.deltaTime;
                }*/

               /* if (FlashLight.intensity >= maxFlashlightIntensity)
                {
                    FlashLight.intensity = maxFlashlightIntensity;
                }*/

                if (FlashLight.intensity <= 0)
                {
                    FlashLight.intensity = 0;
                    isFlashlightOn = false;
                }
            }

           /* if (flashlightSpot.enabled == false)
            {
                {
                    flashlightSpot.intensity += (0.05f * Time.deltaTime) * maxFlashlightIntensity;
                    batteryLevel.fillAmount += 0.05f * Time.deltaTime;
                }

                if (flashlightSpot.intensity >= maxFlashlightIntensity)
                {
                    flashlightSpot.intensity = maxFlashlightIntensity;
                }
            }*/
        }
    }

    void ReloadBattery()
    {
        /*if (Input.GetKey(InputManager.instance.reloadBattery) && batteryCount > 0 && flashlightSpot.intensity < maxFlashlightIntensity)
        {
            shouldUpdate = false;
            replaceBatteryTimer -= Time.deltaTime;
            radialIndicator.enabled = true;
            radialIndicator.fillAmount = replaceBatteryTimer;

            if (replaceBatteryTimer <= 0)
            {
                batteryCount--;
                batteryCountUI.text = batteryCount.ToString("0");
                flashlightSpot.intensity += maxFlashlightIntensity;
                AudioManager.instance.Play(reloadBattery);
                batteryLevel.fillAmount = maxFlashlightIntensity;
                replaceBatteryTimer = maxReplaceBatteryTimer;
                radialIndicator.fillAmount = maxReplaceBatteryTimer;
                radialIndicator.enabled = false;

                if (gameObject.GetComponent<ClientPlayerController>().is_Flashlight == true)
                {
                    gameObject.GetComponent<ClientPlayerController>().is_FlashlightAim = true;
                }
                else
                {
                    gameObject.GetComponent<ClientPlayerController>().is_FlashlightAim = false;
                }

                if (batteryCount < 1)
                {
                    batteryUI.enabled = false;
                }
            }
        }
        else
        {
            if (shouldUpdate)
            {
                replaceBatteryTimer += Time.deltaTime;
                radialIndicator.fillAmount = replaceBatteryTimer;

                if (replaceBatteryTimer >= maxReplaceBatteryTimer)
                {
                    replaceBatteryTimer = maxReplaceBatteryTimer;
                    radialIndicator.fillAmount = maxReplaceBatteryTimer;
                    radialIndicator.enabled = false;
                    shouldUpdate = false;
                }
            }
        }*/

        /*if (Input.GetKeyUp(InputManager.instance.reloadBattery))
        {
            shouldUpdate = true;

            if (gameObject.GetComponent<ClientPlayerController>().is_Flashlight == true)
            {
                gameObject.GetComponent<ClientPlayerController>().is_FlashlightAim = true;
            }
            else
            {
                gameObject.GetComponent<ClientPlayerController>().is_FlashlightAim = false;
            }
        }*/
    }

   /* void EnableFlashlight()
    {
        handFlashlight.SetActive(true);
        inventoryFlashlight.SetActive(false);
    }

    void DisableFlashlight()
    {
        handFlashlight.SetActive(false);
        inventoryFlashlight.SetActive(true);
    }

    void FlashlightClickSound()
    {
        AudioManager.instance.Play(this.flashlightClick);

        if (FlashLight.enabled == false)
        {
            FlashLight.enabled = true;
        }
        else
        {
            FlashLight.enabled = false;
        }
    }
   */
}
