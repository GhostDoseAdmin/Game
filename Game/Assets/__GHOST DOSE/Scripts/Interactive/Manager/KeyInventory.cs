using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class KeyInventory : MonoBehaviour
{
    public static KeyInventory instance;

    [SerializeField]
    private List<string> allKeyPass = new List<string>();

    private void Awake()
    {
        if (!instance)
            instance = this;
    }

    public void AddKey(string newKeyPass)
    {
        this.allKeyPass.Add(newKeyPass);
    }


    public string GetKeyWithPath(string keyPass)
    {
        foreach(var key in this.allKeyPass)
        {
            if(key == keyPass)
            {
                return key;
            }
        }

        return null;
    }

}