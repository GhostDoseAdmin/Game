//Shady
using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

//[ExecuteInEditMode]
public class Reveal : MonoBehaviour
{
    private SkinnedMeshRenderer renderer;
    Material[] materials;
    [SerializeField] Light PlayerLight;
    [SerializeField] Light ClientLight;
    private int materialIndex = 0;
    private string newMaterialName = "Copy";
    private float maxDistance = 10;

    public void Start()
    {
        // Add another ghost skin for 2nd player
        renderer = GetComponent<SkinnedMeshRenderer>();
        if (renderer != null && materialIndex >= 0 && materialIndex < renderer.sharedMaterials.Length)
        {
            Material material = renderer.sharedMaterials[materialIndex];

            // Check if new material name already exists
            bool nameExists = false;
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat.name == newMaterialName)
                {
                    nameExists = true;
                    break;
                }
            }

            if (!nameExists)
            {
                Material newMaterial = new Material(material);
                newMaterial.name = newMaterialName;
                materials = renderer.sharedMaterials;
                Array.Resize(ref materials, materials.Length + 1);
                materials[materials.Length - 1] = newMaterial;
                renderer.sharedMaterials = materials;
            }
        }
    }

    void Update()
    {
        Material[] materials = renderer.sharedMaterials;

       
        materials[0].SetVector("_LightPosition", PlayerLight.transform.position);
        materials[0].SetVector("_LightDirection", -PlayerLight.transform.forward);
        materials[0].SetFloat("_LightAngle", PlayerLight.spotAngle);
        if (!PlayerLight.isActiveAndEnabled) { materials[0].SetFloat("_MaxDistance", 0); }
        else { materials[0].SetFloat("_MaxDistance", maxDistance); }

        materials[1].SetVector("_LightPosition", ClientLight.transform.position);
        materials[1].SetVector("_LightDirection", -ClientLight.transform.forward);
        materials[1].SetFloat("_LightAngle", ClientLight.spotAngle);
        materials[1].SetFloat("_MaxDistance", 10);
        if (!ClientLight.isActiveAndEnabled) { materials[1].SetFloat("_MaxDistance", 0); }
        else { materials[1].SetFloat("_MaxDistance", maxDistance); }

    }


}