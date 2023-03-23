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
    [SerializeField] public Light flashlightSpot = null;
    [SerializeField] public Light flashlightSpotPistol = null;
    [SerializeField] public float maxFlashlightIntensity = 1.0f;

    [Header("BATTERY PARAMETERS")]
    [Space(10)]
    [SerializeField] public int batteryCount = 1;
    [SerializeField] private float replaceBatteryTimer = 1.0f;
    [SerializeField] private float maxReplaceBatteryTimer = 1.0f;

    [Header("FLASHLIGHT UI")]
    [Space(10)]
    [SerializeField] private Image batteryLevel = null;
    [SerializeField] private Image batteryUI = null;
    [SerializeField] private Text batteryCountUI = null;
    [SerializeField] private Image radialIndicator = null;

    [Header("FLASHLIGHT SOUNDS")]
    [Space(10)]
    [SerializeField] private string pickUp;
    [SerializeField] private string flashlightClick;
    [SerializeField] private string reloadBattery;

    private bool shouldUpdate = false;
    private bool showOnce = false;
    private bool isFlashlightOn;

    public static ClientFlashlightSystem instance;
    public bool flEmit = false;

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
        handFlashlight.SetActive(false);
        inventoryFlashlight.SetActive(false);
        flashlightSpot.intensity = maxFlashlightIntensity;
        flashlightSpotPistol.enabled = false;
        //batteryCountUI.text = batteryCount.ToString("0");
    }

    public void EnableInventory()
    {
        hasFlashlight = true;
        inventoryFlashlight.SetActive(true);
        AudioManager.instance.Play(pickUp);
    }

    public void CollectBattery(int batteries)
    {
        batteryCount = batteryCount + batteries;
        batteryCountUI.text = batteryCount.ToString("0");
        batteryUI.enabled = true;
        AudioManager.instance.Play(pickUp);
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
            if (flEmit && !showOnce)
            {
               // Debug.Log(" FLASH LIGHT TRIGGERED ");
                if (flashlightSpot.enabled == false)
                {
                    //Debug.Log(" CLIENT FLASH LIGHT IS ON ");
                    isFlashlightOn = true;

                    handFlashlight.SetActive(true);
                    inventoryFlashlight.SetActive(false);

                    AudioManager.instance.Play(this.flashlightClick);
                    flashlightSpot.enabled = true;
                   // GameObject.Find("ClientSpot_Light_Flashlight").GetComponent<Light>().enabled = true;

                }
                else
                {
                    isFlashlightOn = false;
                    Debug.Log(" CLIENT FLASH LIGHT IS OFF ");
                    handFlashlight.SetActive(false);
                    inventoryFlashlight.SetActive(true);
                    AudioManager.instance.Play(this.flashlightClick);
                    flashlightSpot.enabled = false;
                    //GameObject.Find("ClientSpot_Light_Flashlight").GetComponent<Light>().enabled = true;
                }
                flEmit = false;
            }

            if (isFlashlightOn)
            {
                if (flashlightSpot.intensity <= maxFlashlightIntensity && flashlightSpot.intensity > 0)
                {
                    flashlightSpot.intensity -= (0.007f * Time.deltaTime) * maxFlashlightIntensity;
                    //batteryLevel.fillAmount -= 0.007f * Time.deltaTime;
                }

                if (flashlightSpot.intensity >= maxFlashlightIntensity)
                {
                    flashlightSpot.intensity = maxFlashlightIntensity;
                }

                if (flashlightSpot.intensity <= 0)
                {
                    flashlightSpot.intensity = 0;
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
        if (Input.GetKey(InputManager.instance.reloadBattery) && batteryCount > 0 && flashlightSpot.intensity < maxFlashlightIntensity)
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
        }

        if (Input.GetKeyUp(InputManager.instance.reloadBattery))
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
        }
    }

    void EnableFlashlight()
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

        if (flashlightSpot.enabled == false)
        {
            flashlightSpot.enabled = true;
        }
        else
        {
            flashlightSpot.enabled = false;
        }
    }
}
