using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class KeyInventory : MonoBehaviour
{
    public static KeyInventory instance;
    public GameObject keyui;

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
        checkKeys();
    }
    public void RemoveKey(string newKeyPass)
    {
        Debug.Log("REMOVING KEY FROM LIST WITH PASS " + newKeyPass);
        this.allKeys.Remove(newKeyPass);
        checkKeys();
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

    private void checkKeys()
    {
        if (allKeys.Count > 0) { keyui.SetActive(true); }
        else { keyui.SetActive(false); }
    }
}