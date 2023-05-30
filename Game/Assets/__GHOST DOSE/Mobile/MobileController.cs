using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using GameManager;

public class MobileController : MonoBehaviour
{
    public GameObject joystick, flashlight, gear, interact, shoot;
    void Start()
    {
        if (!NetworkDriver.instance.isMobile)
        {
            this.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //SHOOT
        if (GameDriver.instance.Player.GetComponent<PlayerController>().mobileGearAim)
        {
            shoot.SetActive(true);
        }
        else { shoot.SetActive(false); }

        //FLAHSLIGHT
        if (GameDriver.instance.Player.GetComponent<FlashlightSystem>().FlashLight.GetComponent<Light>().enabled || GameDriver.instance.Player.GetComponent<FlashlightSystem>().WeaponLight.GetComponent<Light>().enabled)
        {
            flashlight.SetActive(true);

        }
        else { flashlight.SetActive(false); }
    }
}
