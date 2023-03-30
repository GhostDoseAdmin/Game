using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class RigManager : MonoBehaviour
{
    public GameObject[] wesRigList;
    public GameObject[] travRigList;
    [SerializeField] public int travCurrRig = 0; // INDEX of rig array 
    [SerializeField] public int wesCurrRig = 0;
    [SerializeField] public int travRigCap;
    [SerializeField] public int wesRigCap;

    // Start is called before the first frame update
    void Start()
    {
        //myProperty = PlayerPrefs.GetInt("MyProperty", myProperty);

        if (travRigCap > travRigList.Length) { travRigCap = travRigList.Length; Debug.LogWarning("RIG CAP OUT OF INDEX"); }
        if (wesRigCap > wesRigList.Length) { wesRigCap = wesRigList.Length; Debug.LogWarning("RIG CAP OUT OF INDEX"); }
        //PlayerPrefs.SetInt("travRigCap", travRigCap);
        //PlayerPrefs.Save();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
