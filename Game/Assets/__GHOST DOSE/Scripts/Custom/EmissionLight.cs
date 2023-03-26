using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionLight : MonoBehaviour
{
    private Material mat;
    public bool emis;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        if (emis)
        {
            mat.EnableKeyword("_EMISSION");
        }
        else mat.DisableKeyword("_EMISSION");
    }
}
