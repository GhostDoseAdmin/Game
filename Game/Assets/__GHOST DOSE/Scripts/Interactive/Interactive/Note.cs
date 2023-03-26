using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Note : Item
{
    [Header("NOTE UI")]
    [Space(10)]
    [SerializeField] private Image noteImage;

    [Header("NOTE SOUNDS")]
    [Space(10)]
    [SerializeField] private string noteShowSound;
    [SerializeField] private string noteCloseSound;

    protected bool isOpenNote;


    private void Start()
    {
        noteImage.enabled = false;
    }

    public override void ActivateObject()
    {
        this.ShowCloseNote();
    }

    public void ShowCloseNote()
    {
        if (this.isOpenNote)
        {
            this.isOpenNote = false;
            noteImage.enabled = false;
            AudioManager.instance.Play(noteCloseSound);
            DisableManager.instance.DisablePlayer(false);
        }
        else
        {
            this.isOpenNote = true;
            noteImage.enabled = true;
            AudioManager.instance.Play(noteShowSound);
            DisableManager.instance.DisablePlayer(true);
         }
    }
}
