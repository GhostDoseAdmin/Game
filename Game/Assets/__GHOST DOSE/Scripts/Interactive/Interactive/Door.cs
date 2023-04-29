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
    [SerializeField] private bool isNeedKey;

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
            this.OpenClose();
    }

    private void CheckKeyPass()
    {
        var key = KeyInventory.instance.GetKeyWithPath(this.doorPass);
        string emitEvent;
        if (key == this.doorPass)
        {
            this.OpenClose();
            emitEvent = "openclose";
        }
        else
        {
            this.Locked();
            emitEvent = "locked";
        }

        if (GameDriver.instance.twoPlayer) { Debug.Log("----------------------SENDING OPEN DOOR"); NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'door','event':'{emitEvent}'}}"), false); }
    }

    public void OpenClose()
    {
        if (this.isOpen)
        {
            this.isOpen = false;
            this.animator.SetBool("Close", true);
            this.animator.SetBool("Open", false);
        }
        else
        {
            this.isOpen = true;
            this.animator.SetBool("Open", true);
            this.animator.SetBool("Close", false);
        }

       

    }

    public void Locked()
    {
        this.animator.SetTrigger("CantOpen");
        AudioManager.instance.Play(this.doorLockSound);
    }

    public void OpenSound()
    {
        AudioManager.instance.Play(this.doorOpenSound);
    }

    public void CloseSound()
    {
        AudioManager.instance.Play(this.doorCloseSound);
    }

    public void SpecialOpenSound()
    {
        AudioManager.instance.Play(this.doorSpecialOpenSound);
    }

    public void SpecialCloseSound()
    {
        AudioManager.instance.Play(this.doorSpecialCloseSound);
    }
}
