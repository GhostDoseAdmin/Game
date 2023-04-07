using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[ExecuteInEditMode]
public class ShadowCastController : MonoBehaviour
{
    GameObject[] casts;

    // Start is called before the first frame update
    void Awake()
    {
        casts = GameObject.FindGameObjectsWithTag("Caster");
    }

    private void OnEnable()
    {
        for (int i = 0; i < casts.Length; i++)
        {
            casts[i].GetComponent<ShadowCasterV2>().UpdateResources();
        }
    }

    private void OnValidate()
    {
        if (casts!=null && casts.Length > 0)
        {
            for (int i = 0; i < casts.Length; i++)
            {
                casts[i].GetComponent<ShadowCasterV2>().UpdateResources();
            }
        }
    }

    private void Update()
    {
        casts = GameObject.FindGameObjectsWithTag("Caster");
    }
    private void OnPostRender()
    {
        Debug.Log("RENDERING ");
        casts = GameObject.FindGameObjectsWithTag("Caster");
        //Shader.SetGlobalInt("_NumShadowMaps", casts.Length);
        for (int i = 0; i < casts.Length; i++)
        {
            casts[i].GetComponent<ShadowCasterV2>().CastPostRender();
        }
        for (int i = 0; i < casts.Length; i++)
        {

            //Debug.Log("SETTING CAST" + casts[i].name);
            Shader.SetGlobalMatrix($"_ShadowMatrix{i+1}", casts[i].GetComponent<ShadowCasterV2>().MTX);
            Shader.SetGlobalTexture($"_ShadowTex{i+1}", casts[i].GetComponent<ShadowCasterV2>().depthTarget);

            //Shader.SetGlobalMatrix($"_ShadowMatrix1", casts[i].GetComponent<ShadowCasterV2>().MTX);
            //Shader.SetGlobalTexture($"_ShadowTex1", casts[i].GetComponent<ShadowCasterV2>().depthTarget);
        }
       // Shader.SetGlobalMatrix($"_ShadowMatrix1", casts[0].GetComponent<ShadowCasterV2>().MTX);
        //Shader.SetGlobalTexture($"_ShadowTex1", casts[0].GetComponent<ShadowCasterV2>().depthTarget);
        //Shader.SetGlobalMatrix($"_ShadowMatrix2", casts[1].GetComponent<ShadowCasterV2>().MTX);
        //Shader.SetGlobalTexture($"_ShadowTex2", casts[1].GetComponent<ShadowCasterV2>().depthTarget);
    }
}
