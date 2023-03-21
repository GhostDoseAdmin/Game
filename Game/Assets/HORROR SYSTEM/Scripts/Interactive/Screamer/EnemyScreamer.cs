using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScreamer : Item
{
    [Header("MAIN")]
    [Space(10)]
    [SerializeField] public float timerScream;

    [Header("SOUND SCREAMER")]
    [Space(10)]
    [SerializeField] private string screamerSound;

    void Start()
    {
        this.gameObject.SetActive(false);
    }

    public override void ActivateObject()
    {
        this.ScreamerGhost();
    }

    public void ScreamerGhost()
    {
        this.gameObject.SetActive(true);
        AudioManager.instance.Play(screamerSound);

        StartCoroutine(EndScreamer());
    }

    IEnumerator EndScreamer()
    {
        yield return new WaitForSeconds(timerScream);
        this.gameObject.SetActive(false);
    }
}