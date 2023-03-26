using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstAidKit : Item
{
    [Header("KITS COUNT")]
    [Space(10)]
    [SerializeField] private int kit;


    public override void ActivateObject()
    {
        HealthSystem.kitinstance.CollectKit(this.kit);
        this.DestroyObject(0);
    }
}
