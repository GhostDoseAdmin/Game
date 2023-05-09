using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class KeyInventory : MonoBehaviour
{
    public static KeyInventory instance;

    [SerializeField]
    private List<string> allKeys = new List<string>();

    private void Awake()
    {
        if (!instance)
            instance = this;
    }

    public void AddKey(string newKeyPass)
    {
        this.allKeys.Add(newKeyPass);
    }
    public void RemoveKey(string newKeyPass)
    {
        this.allKeys.Remove(newKeyPass);
    }

    public string GetKeyWithPath(string keyPass)
    {
        foreach(var key in this.allKeys)
        {
            if(key == keyPass)
            {
                return key;
            }
        }

        return null;
    }

}