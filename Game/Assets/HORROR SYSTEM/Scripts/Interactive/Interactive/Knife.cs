using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : Item
{
    public override void ActivateObject()
    {
        this.AddWeapon();
    }

    private void AddWeapon()
    {
        WeaponParameters.instance.EnableInventoryKnife();
        this.DestroyObject(0);
    }
}
