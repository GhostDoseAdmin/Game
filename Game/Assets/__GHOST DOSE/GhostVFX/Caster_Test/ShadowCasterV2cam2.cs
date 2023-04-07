﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

[ExecuteInEditMode]
public class ShadowCasterV2cam2 : MonoBehaviour
{
    public int targetSize = 512;
    public float shadowBias = 0.005f;
    public int num;


    public Camera cam;
    public RenderTexture depthTarget;
    private List<GhostVFX> ghostVFXObjects;
    public Matrix4x4 MTX;

    private void OnEnable()
    {
        UpdateResources();
    }

    private void OnValidate()
    {
        UpdateResources();
    }

    public void Update()
    {
        GetComponent<Camera>().depth = -1000;
    }
    public void UpdateResources()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
            cam.depth = -1000;
        }

        if (depthTarget == null || depthTarget.width != targetSize)
        {
            int sz = Mathf.Max(targetSize, 16);
            depthTarget = new RenderTexture(sz, sz, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            depthTarget.wrapMode = TextureWrapMode.Clamp;
            depthTarget.filterMode = FilterMode.Bilinear;
            depthTarget.autoGenerateMips = false;
            depthTarget.useMipMap = false;
            cam.targetTexture = depthTarget;
        }
    }

    public void OnPostRender()
    {
        var bias = new Matrix4x4() {
            m00 = 0.5f, m01 = 0,    m02 = 0,    m03 = 0.5f,
            m10 = 0,    m11 = 0.5f, m12 = 0,    m13 = 0.5f,
            m20 = 0,    m21 = 0,    m22 = 0.5f, m23 = 0.5f,
            m30 = 0,    m31 = 0,    m32 = 0,    m33 = 1,
        };
        
        Matrix4x4 view = cam.worldToCameraMatrix;
        Matrix4x4 proj = cam.projectionMatrix;
        Matrix4x4 mtx = bias * proj * view;
        MTX = mtx;

        //int index = 1;
        Shader.SetGlobalMatrix($"_ShadowMatrix2", MTX);
        Shader.SetGlobalTexture($"_ShadowTex2", depthTarget);

        //Shader.SetGlobalMatrix("_ShadowMatrix1", MTX);
        //Shader.SetGlobalTexture("_ShadowTex1", depthTarget);
        //Shader.SetGlobalMatrix("_ShadowMatrix2", MTX);
        //Shader.SetGlobalTexture("_ShadowTex2", depthTarget);



        ghostVFXObjects = new List<GhostVFX>();

        GameObject[] shadowers = GameObject.FindGameObjectsWithTag("Shadower");
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject shadower in shadowers)
        {
            GhostVFX ghostVFX = shadower.GetComponent<GhostVFX>();
            if (ghostVFX != null)
            {
                ghostVFXObjects.Add(ghostVFX);
            }
        }

        foreach (GameObject ghost in ghosts)
        {
            GhostVFX ghostVFX = ghost.GetComponent<GhostVFX>();
            if (ghostVFX != null)
            {
                ghostVFXObjects.Add(ghostVFX);
            }
        }

        foreach (GhostVFX ghostVFX in ghostVFXObjects)
        {
            Debug.Log("--------------------------------UPDATING SHADER INFO-----------------------------------");
            ghostVFX.UpdateShaderValues();
        }

    }
}