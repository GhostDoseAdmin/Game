using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlashlightSystem : MonoBehaviour
{
    [Header("FLASHLIGHT PARAMETERS")]
    [Space(10)]
    [SerializeField] public bool hasFlashlight = false;
    [HideInInspector] public GameObject handFlashlight;
    [SerializeField] public GameObject inventoryFlashlight;
    public Light FlashLight = null;
     public Light WeaponLight = null;
    //[SerializeField] public float maxFlashlightIntensity = 1.0f;

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
    public bool isFlashlightOn;


    public float weapLightAngle = 45;
    private float flashLightAngle = 52;
    public float weapLightIntensity = 5;//20
    private float flashLightIntensity = 5;


    public static FlashlightSystem instance;

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


        GetComponent<PlayerController>().currLight =FlashLight.gameObject; 

        //Debug.Log("Player Lights Setup");

    }


    void Start()
    {

        WeaponLight.enabled = false;
        FlashLight.enabled = false;
        handFlashlight.SetActive(false);

        inventoryFlashlight.SetActive(false);
        //FlashLight.intensity = maxFlashlightIntensity;
        batteryCountUI.text = batteryCount.ToString("0");
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
        //ReloadBattery();
    }

    void Flashlight()
    {
        if (hasFlashlight)
        {
            //---------TOGGLE FLASHLGITH ON AND OFF---------------------------------
            if (Input.GetKeyDown(InputManager.instance.flashlightSwitch) && !showOnce)
            {
                if (FlashLight.enabled == false)
                {
                    isFlashlightOn = true;

                    handFlashlight.SetActive(true);
                    inventoryFlashlight.SetActive(false);

                    AudioManager.instance.Play(this.flashlightClick);
                    FlashLight.enabled = true;

                    GetComponent<PlayerController>().emitFlashlight = true;

                }
                else
                {
                    isFlashlightOn = false;

                    handFlashlight.SetActive(false);
                    inventoryFlashlight.SetActive(true);
                    AudioManager.instance.Play(this.flashlightClick);
                    FlashLight.enabled = false;

                    GetComponent<PlayerController>().emitFlashlight = true;
                }
            }

            if (isFlashlightOn)
            {
                //if (FlashLight.intensity <= maxFlashlightIntensity && FlashLight.intensity > 0)
                {
                    //FlashLight.intensity -= (0.007f * Time.deltaTime) * maxFlashlightIntensity;
                    //batteryLevel.fillAmount -= 0.007f * Time.deltaTime;
                }

                //if (FlashLight.intensity >= maxFlashlightIntensity)
                {
                    //FlashLight.intensity = maxFlashlightIntensity;
                }

                if (FlashLight.intensity <= 0)
                {
                    FlashLight.intensity = 0;
                    isFlashlightOn = false;
                }
            }

            if (FlashLight.enabled == false)
            {
                {
                   // FlashLight.intensity += (0.05f * Time.deltaTime) * maxFlashlightIntensity;
                    batteryLevel.fillAmount += 0.05f * Time.deltaTime;
                }

                //if (FlashLight.intensity >= maxFlashlightIntensity)
                {
                    //FlashLight.intensity = maxFlashlightIntensity;
                }
            }
        }
    }

    /*void ReloadBattery()
    {
        if (Input.GetKey(InputManager.instance.reloadBattery) && batteryCount > 0 && FlashLight.intensity < maxFlashlightIntensity)
        {
            shouldUpdate = false;
            replaceBatteryTimer -= Time.deltaTime;
            radialIndicator.enabled = true;
            radialIndicator.fillAmount = replaceBatteryTimer;

            if (replaceBatteryTimer <= 0)
            {
                batteryCount--;
                batteryCountUI.text = batteryCount.ToString("0");
                FlashLight.intensity += maxFlashlightIntensity;
                AudioManager.instance.Play(reloadBattery);
                batteryLevel.fillAmount = maxFlashlightIntensity;
                replaceBatteryTimer = maxReplaceBatteryTimer;
                radialIndicator.fillAmount = maxReplaceBatteryTimer;
                radialIndicator.enabled = false;

                if (gameObject.GetComponent<PlayerController>().is_Flashlight == true)
                {
                    gameObject.GetComponent<PlayerController>().is_FlashlightAim = true;
                }
                else
                {
                    gameObject.GetComponent<PlayerController>().is_FlashlightAim = false;
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

            if (gameObject.GetComponent<PlayerController>().is_Flashlight == true)
            {
                gameObject.GetComponent<PlayerController>().is_FlashlightAim = true;
            }
            else
            {
                gameObject.GetComponent<PlayerController>().is_FlashlightAim = false;
            }
        }
    }
    */

}
