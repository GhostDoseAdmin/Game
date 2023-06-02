using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using GameManager;

public class MobileController : MonoBehaviour
{
    public GameObject flashlight, gear, interact;
    public GPButton aimShootBTN, flashlightBTN;
    public VariableJoystick joystick;
    public RayAimer aimer;

    private void Start()
    {
        Debug.Log("-----------------------MOBILE CONTROLLER" + gameObject.name + "NETWORK DRIVER " + NetworkDriver.instance.name);
        if (!NetworkDriver.instance.isMobile) { gameObject.SetActive(false); }
    }
    // Update is called once per frame
    void Update()
    {
        
        //AIM
        if (aimShootBTN.buttonPressed)
        { // && !GameDriver.instance.Player.GetComponent<PlayerController>().gearAim
            aimer.gameObject.SetActive(true);
            
        }
        //SHOOT
        if (aimShootBTN.buttonReleased)
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
        //Delay to allow playercontroller to trigger shoot
        CancelInvoke("ReleaseShoot");
        aimer.gameObject.SetActive(false);
    }


}
