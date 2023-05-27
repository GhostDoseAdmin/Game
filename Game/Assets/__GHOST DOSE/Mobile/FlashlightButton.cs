using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using GameManager;
public class FlashlightButton : MonoBehaviour
{

    public GameObject fill;
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
        if (GameDriver.instance.Player.GetComponent<FlashlightSystem>().FlashLight.GetComponent<Light>().enabled || GameDriver.instance.Player.GetComponent<FlashlightSystem>().WeaponLight.GetComponent<Light>().enabled)
        {
            fill.SetActive(true);

        }
        else { fill.SetActive(false); }
           
    }
}
