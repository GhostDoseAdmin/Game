using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ShadowCastController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void OnPostRender()
    {
        GameObject[] casts = GameObject.FindGameObjectsWithTag("Caster");
        Matrix4x4[] shadowMatrices = new Matrix4x4[casts.Length];

        Shader.SetGlobalInt("_NumShadowMaps", casts.Length);
        for (int i = 0; i < casts.Length; i++)
        {
            string propertyName = "_ShadowMatrix" + i.ToString();
            Shader.SetGlobalMatrix(propertyName, shadowMatrices[i]);
        }
        for (int i = 0; i < casts.Length; i++)
        {
            string textureName = string.Format("_ShadowTextures[{0}]", i);
            Shader.SetGlobalTexture(textureName, casts[i].GetComponent<ShadowCasterV2>().depthTarget);
        }

    }
}
