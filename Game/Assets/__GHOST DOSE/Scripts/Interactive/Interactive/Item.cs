using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public virtual void ActivateObject(bool otherPlayer)
    {
        //Starting an action
    }

    protected void DestroyObject(float destroyTimer)
    {
        Destroy(this.gameObject, destroyTimer);
    }
}