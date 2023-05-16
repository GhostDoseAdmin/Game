using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

public class Zap : MonoBehaviour
{
    public GameObject FirePoint;
    //public Camera Cam;
    public float MaxLength;
    public GameObject[] Prefabs;

    private GameObject Instance;
    private Hovl_Laser LaserScript;
    private Hovl_Laser2 LaserScript2;


    private void Update()
    {
        if (Instance == null && GetComponentInParent<NPCController>().zapActive)
        {
            Instance = Instantiate(Prefabs[0], FirePoint.transform.position, FirePoint.transform.rotation);
            Instance.transform.parent = transform;
            LaserScript = Instance.GetComponent<Hovl_Laser>();
            LaserScript2 = Instance.GetComponent<Hovl_Laser2>();
        }

        if (Instance != null && !GetComponentInParent<NPCController>().zapActive)
        {
            if (LaserScript) LaserScript.DisablePrepare();
            if (LaserScript2) LaserScript2.DisablePrepare();
            Destroy(Instance, 1);
        }
    }



}
