using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class utilities : MonoBehaviour
{
    //-----------------------RE FRESH ANIMATOR--------------------------
    // takes the object running the animator and disables-re enables it 
    public IEnumerator ReactivateAnimator(GameObject animatingObject)
    {
        //Debug.Log("Deactivating Animator");
        animatingObject.SetActive(false);
        yield return new WaitForSeconds(0.001f);
        //Debug.Log("Reactivating Animator");
        animatingObject.SetActive(true);

    }

    //----------------------FIND OBJECT IN HEIARCHY--------------------------
    public GameObject FindChildObject(Transform parentTransform, string name)
    {
        if (parentTransform.gameObject.name == name)
        {
            return parentTransform.gameObject;
        }

        foreach (Transform childTransform in parentTransform)
        {
            GameObject foundObject = FindChildObject(childTransform, name);
            if (foundObject != null)
            {
                return foundObject;
            }
        }
        return null;
    }


}
