using UnityEngine;
using UnityEditor;

public class CopySkeletonObjects : EditorWindow
{
    GameObject originSkeletonRoot;
    GameObject destSkeletonRoot;
    [HideInInspector] GameObject newBody;
    [HideInInspector] GameObject player;
    public string myField;
    [MenuItem("Window/Copy Skeleton Objects")]
    public static void ShowWindow()
    {
        GetWindow<CopySkeletonObjects>("Copy Skeleton Objects");
    }

    void OnGUI()
    {
        GUILayout.Label("Copy Skeleton Objects", EditorStyles.boldLabel);
        
        EditorGUILayout.TextField("Unpack Prefabs First! Select root Hips");

        EditorGUILayout.Space();

        originSkeletonRoot = EditorGUILayout.ObjectField("Origin Skeleton Root", originSkeletonRoot, typeof(GameObject), true) as GameObject;
        destSkeletonRoot = EditorGUILayout.ObjectField("Destination Skeleton Root", destSkeletonRoot, typeof(GameObject), true) as GameObject;

        EditorGUILayout.Space();

        if (GUILayout.Button("Copy Skeleton Objects"))
        {
            if (originSkeletonRoot == null || destSkeletonRoot == null)
            {
                Debug.LogError("Please select both origin and destination skeleton root objects.");
                return;
            }

            //UPACK
            //PrefabUtility.UnpackPrefabInstance(originSkeletonRoot.transform.root.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            //PrefabUtility.UnpackPrefabInstance(destSkeletonRoot.transform.root.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            //newBody = destSkeletonRoot.transform.root.gameObject;//travisBones
            //player = originSkeletonRoot.transform.root.gameObject.transform.GetChild(0).gameObject;

            //newBody.transform.SetParent(player.transform);//assumes player is child of TRAVIS
            
            
            /*for (int i = 0; i < player.transform.childCount; i++)
            {
                Transform child = player.transform.GetChild(i);
                if (child.name == "Body")
                {
                    GameObject.DestroyImmediate(child.gameObject);
                    newBody.name = "Player";
                    break;
                }
            }*/



            CopyObjectsOnSkeletonPart(originSkeletonRoot.transform, destSkeletonRoot.transform);
            //destSkeletonRoot.transform.transform.SetParent(player.transform);
            originSkeletonRoot.SetActive(false);

            //GameObject.DestroyImmediate(originSkeletonRoot);

            /*COPY PLAYER COMPONENTS 
            Component[] components = player.GetComponents<Component>();
            foreach (Component component in components)
            {
                // Ignore Transform and GameObject components, which are automatically added to every GameObject
                if (!(component is Transform) && !(component is GameObject))
                {
                    // Add the component to the target object
                    UnityEditorInternal.ComponentUtility.CopyComponent(component);
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(newBody);
                }
            }

            */

            Debug.Log("Objects and components copied successfully.");
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
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(newObject);
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
                        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(destChild.gameObject);
                    }
                }

                // Recursively copy the objects on the child's hierarchy
                CopyObjectsOnSkeletonPart(originChild, destChild);
            }
        }
    }
}
