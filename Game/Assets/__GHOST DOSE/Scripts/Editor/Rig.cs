using UnityEngine;
using UnityEditor;

public class Rig : EditorWindow
{
    GameObject originSkeletonRoot;
    GameObject destSkeletonRoot;

    public string myField;
    [MenuItem("Window/Rig Model with BasicRig")]
    public static void ShowWindow()
    {
        GetWindow<Rig>("Rig Model");
    }

    void OnGUI()
    {
        GUILayout.Label("Copy Skeleton Objects", EditorStyles.boldLabel);

        EditorGUILayout.TextField("Unpack Prefabs First! Select root Hips");

        EditorGUILayout.Space();

        originSkeletonRoot = Resources.Load<GameObject>("Prefabs/Rigs/ROOTRIG");
        destSkeletonRoot = EditorGUILayout.ObjectField("Destination Skeleton Root", destSkeletonRoot, typeof(GameObject), true) as GameObject;

        EditorGUILayout.Space();

        if (GUILayout.Button("Copy Skeleton Objects"))
        {
            if (originSkeletonRoot == null || destSkeletonRoot == null)
            {
                Debug.LogError("Please select both origin and destination skeleton root objects.");
                return;
            }
            if (!destSkeletonRoot.name.Contains("mixamorig:Hips"))
            {
                Debug.LogError("Please select HIPS");
                return;
            }
            CopyObjectsOnSkeletonPart(originSkeletonRoot.transform, destSkeletonRoot.transform);
        }
    }

    void CopyObjectsOnSkeletonPart(Transform originSkeletonPart, Transform destSkeletonPart)
    {
        for (int i = 0; i < originSkeletonPart.childCount; i++)
        {
            Transform originChild = originSkeletonPart.GetChild(i);
            Transform destChild = destSkeletonPart.Find(originChild.name);

            if (destChild == null)
            {
                // Create a new object in the destination hierarchy
                GameObject newObject = new GameObject(originChild.name);
                newObject.transform.parent = destSkeletonPart;
                newObject.transform.localPosition = originChild.localPosition;
                newObject.transform.localRotation = originChild.localRotation;
                newObject.transform.localScale = originChild.localScale;

                // Copy components from the origin object to the new object
                foreach (Component component in originChild.GetComponents<Component>())
                {
                    if (component is Transform) continue; // Skip the transform component

                    UnityEditorInternal.ComponentUtility.CopyComponent(component);

                    if (component is ParticleSystem)
                    {
                        ParticleSystem newParticleSystem = newObject.AddComponent<ParticleSystem>();
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(newParticleSystem);
                    }
                    else
                    {
                        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(newObject);
                    }
                }

                // Recursively copy the objects on the child's hierarchy
                CopyObjectsOnSkeletonPart(originChild, newObject.transform);
            }
            else
            {
                // A matching object already exists in the destination hierarchy
                // Copy missing components from the origin object to the destination object
                foreach (Component originComponent in originChild.GetComponents<Component>())
                {
                    if (originComponent is Transform) continue; // Skip the transform component

                    Component destComponent = destChild.GetComponent(originComponent.GetType());

                    if (destComponent == null)
                    {
                        UnityEditorInternal.ComponentUtility.CopyComponent(originComponent);

                        if (originComponent is ParticleSystem)
                        {
                            ParticleSystem newParticleSystem = destChild.gameObject.AddComponent<ParticleSystem>();
                            UnityEditorInternal.ComponentUtility.PasteComponentValues(newParticleSystem);
                        }
                        else
                        {
                            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(destChild.gameObject);
                        }
                    }
                }

                // Recursively copy the objects on the child's hierarchy
                CopyObjectsOnSkeletonPart(originChild, destChild);
            }
        }
    }



}
