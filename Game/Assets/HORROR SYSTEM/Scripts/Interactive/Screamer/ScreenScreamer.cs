using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine;

public class ScreenScreamer : Item
{
    [Header("MAIN")]
    [Space(10)]
    [SerializeField] public MeshRenderer Display;
    [SerializeField] public float timerScream;
    [SerializeField] public Light pointlightSpot = null;
    public AudioSource audioScreen;

    [Header("MATERIALS")]
    [Space(10)]
    [SerializeField] public Material OffMaterial;
    [SerializeField] public Material RenderMaterial;


    public void Start()
    {
        Display.material = OffMaterial;
        pointlightSpot.enabled = false;
        audioScreen.Stop();
    }

    public override void ActivateObject()
    {
        this.ScreamerTV();
    }

    private void ScreamerTV()
    {
        Display.material = RenderMaterial;
        pointlightSpot.enabled = true;
        audioScreen.Play();
        StartCoroutine(EndScreamer());
    }

    IEnumerator EndScreamer()
    {
        yield return new WaitForSeconds(timerScream);
        Display.material = OffMaterial;
        pointlightSpot.enabled = false;
        audioScreen.Stop();
    }
}
