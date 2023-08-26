using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

public class ZozoLaser : MonoBehaviour
{
    public GameObject FirePoint;
    //public Camera Cam;
    public float MaxLength;
    public GameObject[] Prefabs;

    private GameObject Instance;
    private Hovl_Laser LaserScript;
    private Hovl_Laser2 LaserScript2;
    public bool laserGrid = false;
    public laserGrid laserGridOrigin;

    private void Update()
    {
        //----------------------------SHOOT LASER-------------------------------------
        if (laserGrid)
        {
            if (Instance == null)
            {
                Instance = Instantiate(Prefabs[0], FirePoint.transform.position, FirePoint.transform.rotation);
                Instance.transform.parent = transform;
                Instance.GetComponent<Hovl_Laser>().LASERGRID = laserGrid;
                //LaserScript2 = Instance.GetComponent<Hovl_Laser2>();
            }
        }
        else //-------------ZOZO/SHADOWER LASER------------------------
        {
            if (Instance == null && GetComponentInParent<ZozoControl>().laserActive)
            {
                Instance = Instantiate(Prefabs[0], FirePoint.transform.position, FirePoint.transform.rotation);
                Instance.transform.parent = transform;
                LaserScript = Instance.GetComponent<Hovl_Laser>();



            }
            //-----STOP LASER
            if (!GetComponentInParent<ZozoControl>().laserActive)
            {
                if (LaserScript) LaserScript.DisablePrepare();
                Destroy(Instance, 1);
            }

        }

    }


}
