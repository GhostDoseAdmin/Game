using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : Item
{
    [Header("BATTERIES COUNT")]
    [Space(10)]
    [SerializeField] private int count;


    public override void ActivateObject()
    {
        FlashlightSystem.instance.CollectBattery(this.count);
        this.DestroyObject(0);
    }
}
