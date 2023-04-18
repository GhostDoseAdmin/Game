using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : Item
{
    [Header("FLASHLIGHT UI")]
    [Space(10)]
    [SerializeField] public GameObject flashlightUI;

    public void Start()
    {
        flashlightUI.SetActive(false);
    }

    public override void ActivateObject(bool otherPlayer)
    {
        this.AddFlashlight();
    }

    private void AddFlashlight()
    {
        FlashlightSystem.instance.EnableInventory();
        this.DestroyObject(0);
        flashlightUI.SetActive(true);
    }
}
