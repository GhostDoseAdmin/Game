using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using GameManager;

public class MobileController : MonoBehaviour
{
    public GameObject flashlight, gear, interact;
    public GPButton  flashlightBTN;
    public VariableJoystick joystick, joystickAim;
    public RayAimer aimer;
    public Camera_Supplyer camSup;

    private void Start()
    {
        camSup = Camera.main.GetComponent<Camera_Supplyer>();
        if (!NetworkDriver.instance.isMobile) { gameObject.SetActive(false); }
    }
    // Update is called once per frame
    void Update()
    {

        //AIM
        if (!GameDriver.instance.Player.GetComponent<PlayerController>().gearAim && aimer.gameObject.activeSelf) { aimer.gameObject.SetActive(false); }
        if (joystickAim.GetComponent<GPButton>().buttonPressed)
        { // && !GameDriver.instance.Player.GetComponent<PlayerController>().gearAim
            aimer.gameObject.SetActive(true);
        }

        //SHOOT
        if (joystickAim.GetComponent<GPButton>().buttonReleased)
        {
            aimer.shrink = false;
            Invoke("ReleaseShoot",0.3f);
        }


        //FLAHSLIGHT
        if (GameDriver.instance.Player.GetComponent<FlashlightSystem>().FlashLight.GetComponent<Light>().enabled || GameDriver.instance.Player.GetComponent<FlashlightSystem>().WeaponLight.GetComponent<Light>().enabled)
        {
            flashlight.SetActive(true);

        }
        else { flashlight.SetActive(false); }
    }

    void ReleaseShoot()
    {
        //Delay to allow playercontroller to get target info
        CancelInvoke("ReleaseShoot");
        aimer.gameObject.SetActive(false);
    }


}
