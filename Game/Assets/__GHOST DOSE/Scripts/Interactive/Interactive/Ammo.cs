using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : Item
{
    [Header("CARTRIDGES COUNT")]
    [Space(10)]
    [SerializeField] private int countCartridges;

    public override void ActivateObject(bool otherPlayer)
    {
        ShootingSystem.instance.CollectCartridges(this.countCartridges);
        this.DestroyObject(0);
    }
}
