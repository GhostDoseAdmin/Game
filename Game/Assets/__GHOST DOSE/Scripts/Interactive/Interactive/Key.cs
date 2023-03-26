using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Item
{
    [Header("KEY PASSWORD")] //Key password (must match the door password)
    [Space(10)]
    [SerializeField] private string keyPass;

    [Header("KEY SOUND")]
    [Space(10)]
    [SerializeField] private string pickUpKey;

    public override void ActivateObject()
    {
        this.AddKey();
    }

    private void AddKey()
    {
        KeyInventory.instance.AddKey(keyPass);
        AudioManager.instance.Play(pickUpKey);
        this.DestroyObject(0);
    }

    public string GetKeyPass()
    {
        return this.keyPass;
    }
}