using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampScreamer : Item
{
    [Header("MAIN")]
    [Space(10)]
    [SerializeField] public GameObject electricalVFX;
    [SerializeField] public Light pointlightSpot;
    private Material mat;
    public AudioSource audioLamp;

    public void Start()
    {
        pointlightSpot.enabled = true;
        electricalVFX.SetActive(false);
        mat = GetComponent<Renderer>().material;
        mat.EnableKeyword("_EMISSION");
        audioLamp.Stop();
    }

    public override void ActivateObject()
    {
        this.ScreamerLamp();
    }

    private void ScreamerLamp()
    {
        pointlightSpot.enabled = false;
        electricalVFX.SetActive(true);
        audioLamp.Play();
        mat.DisableKeyword("_EMISSION");
    }
}
