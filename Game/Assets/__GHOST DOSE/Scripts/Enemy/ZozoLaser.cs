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

    private void Update()
    {
        //if (!laserGrid)
        {
            if (Instance == null && (( !laserGrid && GetComponentInParent<ZozoControl>().laserActive) || laserGrid))
            {
                Instance = Instantiate(Prefabs[0], FirePoint.transform.position, FirePoint.transform.rotation);
                Instance.transform.parent = transform;
                LaserScript = Instance.GetComponent<Hovl_Laser>();
                Instance.GetComponent<Hovl_Laser>().LASERGRID = laserGrid;
                LaserScript2 = Instance.GetComponent<Hovl_Laser2>();
            }

            if (Instance == null && ((!laserGrid && GetComponentInParent<ZozoControl>().laserActive) || laserGrid))
            {
                if (LaserScript) LaserScript.DisablePrepare();
                if (LaserScript2) LaserScript2.DisablePrepare();
                Destroy(Instance, 1);
            }
        }
    }


}
