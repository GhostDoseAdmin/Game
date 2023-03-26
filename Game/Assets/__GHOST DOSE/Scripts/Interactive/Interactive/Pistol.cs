using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Item
{
    [Header("PISTOL UI")]
    [Space(10)]
    [SerializeField] public GameObject pistolUI;

    public void Start()
    {
        pistolUI.SetActive(false);
    }

    public override void ActivateObject()
    {
        this.AddWeapon();
    }

    private void AddWeapon()
    {
        WeaponParameters.instance.EnableInventoryPistol();
        this.DestroyObject(0);
        pistolUI.SetActive(true);
    }
}
