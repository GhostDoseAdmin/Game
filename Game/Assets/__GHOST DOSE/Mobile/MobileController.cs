using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using GameManager;

public class MobileController : MonoBehaviour
{
    public GameObject flashlight, gear, interact;
    public GPButton  flashlightBTN, interactBTN, gearBTN;
    public VariableJoystick joystick, joystickAim;
    //public RayAimer aimer;
    public Camera_Supplyer camSup;
    private PlayerController Player;
    private ShootingSystem SS;
    public bool canPickUp;
    private void Start()
    {
        Player = GameDriver.instance.Player.GetComponent<PlayerController>();
        SS = Player.GetComponent<ShootingSystem>();
        camSup = Camera.main.GetComponent<Camera_Supplyer>(); //!!!!!!!!!!!!!!!!!!!!!!!!MOUSE CONTROLS
        if (!NetworkDriver.instance.isMobile) { gameObject.SetActive(false); }
    }
    // Update is called once per frame
    void Update()
    {

        //AIM
        if (joystickAim.GetComponent<GPButton>().buttonPressed)
        {
            //FindValidTarget(10);

            //if (!aimer.AIMING) { aimer.EnableAimer(); }
        }

        if(canPickUp)
        {
            interactBTN.gameObject.SetActive(true);
        }
        else { interactBTN.gameObject.SetActive(false); }

        //FLAHSLIGHT
        if (GameDriver.instance.Player.GetComponent<FlashlightSystem>().FlashLight.GetComponent<Light>().enabled || GameDriver.instance.Player.GetComponent<FlashlightSystem>().WeaponLight.GetComponent<Light>().enabled)
        {
            flashlight.SetActive(true);

        }
        else { flashlight.SetActive(false); }
    }


    private void LateUpdate()
    {
        canPickUp = false;
    }

}
