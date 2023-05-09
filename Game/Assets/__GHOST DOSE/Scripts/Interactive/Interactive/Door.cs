using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using GameManager;

public class Door : Item
{
    [Header("NEED KEY")] //Do I need a key?
    [Space(10)]
    [SerializeField] public bool isNeedKey;

    [Header("DOOR PASSWORD")] //The password for the door (must match the password of the key)
    [Space(10)]
    [SerializeField] private string doorPass;

    [Header("DOOR SOUNDS")]
    [Space(10)]
    [SerializeField] private string doorOpenSound;
    [SerializeField] private string doorCloseSound;
    [SerializeField] private string doorLockSound;

    [Space(10)]
    [SerializeField] private string doorSpecialOpenSound;
    [SerializeField] private string doorSpecialCloseSound;

    protected bool isOpen;

    private Animator animator;

    private void Start()
    {
        this.animator = GetComponent<Animator>();
    }

    public override void ActivateObject(bool otherPlayer)
    {
       
        if (this.isNeedKey)
            this.CheckKeyPass();
        else
            this.OpenClose(false);
    }

    private void CheckKeyPass()
    {
        var key = KeyInventory.instance.GetKeyWithPath(this.doorPass);
        if (key == this.doorPass)
        {
            if (this.isNeedKey) { KeyInventory.instance.RemoveKey(KeyInventory.instance.GetKeyWithPath(this.doorPass)); }
            this.OpenClose(false);
        }
        else
        {
            this.Locked(false);
        }
        
        
    }

    public void OpenClose(bool otherPlayer)
    {
        if (this.isOpen)
        {
           
            this.isOpen = false;
            this.animator.SetBool("Close", true);
            this.animator.SetBool("Open", false);
        }
        else
        {
            isNeedKey = false;
            this.isOpen = true;
            this.animator.SetBool("Open", true);
            this.animator.SetBool("Close", false);
        }

        if (GameDriver.instance.twoPlayer && !otherPlayer) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'door','event':'openclose'}}"), false); }

    }

    public void Locked(bool otherPlayer)
    {
        this.animator.SetTrigger("CantOpen");
        AudioManager.instance.Play(this.doorLockSound, audioSource);

        if (GameDriver.instance.twoPlayer && !otherPlayer) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'door','event':'locked'}}"), false); }
    }

    public void OpenSound()
    {
        AudioManager.instance.Play(this.doorOpenSound, audioSource);
    }

    public void CloseSound()
    {
        AudioManager.instance.Play(this.doorCloseSound, audioSource);
    }

    public void SpecialOpenSound()
    {
        AudioManager.instance.Play(this.doorSpecialOpenSound, audioSource);
    }

    public void SpecialCloseSound()
    {
        AudioManager.instance.Play(this.doorSpecialCloseSound, audioSource);
    }
}
