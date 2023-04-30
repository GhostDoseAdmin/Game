using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public AudioSource audioSource;
    public void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
    }
    public virtual void ActivateObject(bool otherPlayer)
    {
        //Starting an action
    }

    protected void DestroyObject(float destroyTimer)
    {
        Destroy(this.gameObject, destroyTimer);
    }
}
