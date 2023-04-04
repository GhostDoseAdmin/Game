using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignTexture : MonoBehaviour
{
    public Material planeMaterial;
    public RenderTexture renderTexture;

    void Update()
    {
        planeMaterial = GetComponent<Renderer>().material;
        planeMaterial.mainTexture = renderTexture;
    }
}
