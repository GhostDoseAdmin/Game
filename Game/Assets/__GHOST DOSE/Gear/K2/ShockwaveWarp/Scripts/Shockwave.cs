using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Allows a shockwave effect to be created to attached GameObjects.
public class Shockwave : MonoBehaviour
{
    public float startOffsetValue;
    public float endOffsetValue;
    public float amplitude;

    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    private void Awake()
    {
        RecursiveGetMeshRenderers(transform);
    }

    void RecursiveGetMeshRenderers(Transform rendererTransform)
    {
        MeshRenderer renderer = rendererTransform.GetComponent<MeshRenderer>();

        if(renderer != null)
        {
            meshRenderers.Add(renderer);
        }

        for (int i = 0; i < rendererTransform.childCount; i++)
        {
            RecursiveGetMeshRenderers(rendererTransform.GetChild(i));
        }
    }

    public void NewShockwave(Vector3 startPoint, float duration)
    {
        NewShockwave(startPoint, duration, amplitude);
    }


    public void NewShockwave(Vector3 startPoint, float duration, float amplitude)
    {
        StopAllCoroutines();

        for (int i = 0; i < meshRenderers.Count; i++)
        {
            meshRenderers[i].material.SetFloat("_SineOffset", startOffsetValue);

            meshRenderers[i].material.SetFloat("_SineAmp", 0);

            StartCoroutine(NewShockwaveRoutine(meshRenderers[i], startPoint, duration, amplitude));
        }
    }

    IEnumerator NewShockwaveRoutine(MeshRenderer meshRenderer, Vector3 startPoint, float duration, float amplitude)
    {
        Vector3 vector = startPoint - meshRenderer.transform.position;

        Vector3 point = meshRenderer.transform.InverseTransformPoint(meshRenderer.transform.position + vector);

        meshRenderer.material.SetFloat("_SineAmp", amplitude);
        meshRenderer.material.SetFloat("_SineOffset", startOffsetValue);
        // Get a reference to the dome mesh
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;
        meshRenderer.material.SetVector("_Origin", new Vector4(mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z, 0));
        //meshRenderer.material.SetVector("_Origin", new Vector4(point.x, point.y, point.z, 0));

        float startTime = Time.time;


        while (Time.time - startTime <= duration)
        {
            meshRenderer.material.SetFloat("_SineOffset", Mathf.Lerp(startOffsetValue, endOffsetValue, (Time.time - startTime) / duration));           

            yield return new WaitForEndOfFrame();
        }

        meshRenderer.material.SetFloat("_SineOffset", endOffsetValue);

        meshRenderer.material.SetFloat("_SineAmp", 0);      
    }
}
