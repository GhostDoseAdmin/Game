using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switcher : Item
{
    [Header("LIGHT")]
    [Space(10)]
    [SerializeField] private Light pointLight = null;
    public GameObject Lamp; // The object for which we will include the Emission

    [Header("SWITCHER SOUND")]
    [Space(10)]
    [SerializeField] private string clickSwitch;

    private Animator animator;

    protected bool isOn;
    private bool isPointLightOn;


    private void Start()
    {
        this.animator = GetComponent<Animator>();
    }

    public override void ActivateObject(bool otherPlayer)
    {
        this.ClickSwitch();
    }

    public void ClickSwitch()
    {
        if (this.isOn)
        {
            this.isOn = false;
            AudioManager.instance.Play(clickSwitch);
            pointLight.enabled = false;
            this.animator.SetBool("Off", true);
            this.animator.SetBool("On", false);
            Lamp.GetComponent<EmissionLight>().emis = false;
        }
        else
        {
            this.isOn = true;
            AudioManager.instance.Play(clickSwitch);
            pointLight.enabled = true;
            this.animator.SetBool("On", true);
            this.animator.SetBool("Off", false);
            Lamp.GetComponent<EmissionLight>().emis = true;
        }
    }
}
