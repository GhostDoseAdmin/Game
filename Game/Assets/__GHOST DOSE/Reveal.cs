//Shady
using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[ExecuteInEditMode]
public class Reveal : MonoBehaviour
{
    private SkinnedMeshRenderer renderer;
    [SerializeField] Light SpotLight1;
    [SerializeField] Light SpotLight2;
    private int materialIndex = 0;
    private string newMaterialName = "Copy";

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
                Material[] materials = renderer.sharedMaterials;
                Array.Resize(ref materials, materials.Length + 1);
                materials[materials.Length - 1] = newMaterial;
                renderer.sharedMaterials = materials;
            }
        }
    }

    void Update()
    {
        renderer = GetComponent<SkinnedMeshRenderer>();
        Material[] materials = renderer.sharedMaterials;

        materials[0].SetVector("_LightPosition", SpotLight1.transform.position);
        materials[0].SetVector("_LightDirection", -SpotLight1.transform.forward);
        materials[0].SetFloat("_LightAngle", SpotLight1.spotAngle);

        materials[1].SetVector("_LightPosition", SpotLight2.transform.position);
        materials[1].SetVector("_LightDirection", -SpotLight2.transform.forward);
        materials[1].SetFloat("_LightAngle", SpotLight2.spotAngle);
    }


}