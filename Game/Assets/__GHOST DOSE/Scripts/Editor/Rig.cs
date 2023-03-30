using UnityEngine;
using UnityEditor;
using System;
using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer;

public class Rig : EditorWindow
{
    GameObject originSkeletonRoot;
    GameObject destSkeletonRoot;
    GameObject myModel;

    [HideInInspector] GameObject currentRig;
    private bool isTravis;
    private string prefabName = "";

    [MenuItem("Window/Rig Model with BasicRig")]
    public static void ShowWindow()
    {
        GetWindow<Rig>("Rig Model");
    }

    void OnGUI()
    {
        isTravis = EditorGUILayout.Toggle("Is Travis Rig?", isTravis);


        EditorGUILayout.Space();

        originSkeletonRoot = Resources.Load<GameObject>("Prefabs/Rigs/ROOTRIG");
        destSkeletonRoot = EditorGUILayout.ObjectField("Destination Skeleton Root", destSkeletonRoot, typeof(GameObject), true) as GameObject;
        

        EditorGUILayout.Space();

        if (GUILayout.Button("Copy Skeleton Objects"))
        {
            myModel = destSkeletonRoot.transform.root.gameObject;

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

            //--------LOOK FOR EXISTING RIG
            if (isTravis)
            {
                if (GameObject.Find("TRAVIS").transform.GetChild(0).childCount > 0)
                {
                    currentRig = GameObject.Find("TRAVIS").transform.GetChild(0).GetChild(0).gameObject;
                }
            }
            else
            {
                if (GameObject.Find("WESTIN").transform.GetChild(0).childCount > 0)
                {
                    currentRig = GameObject.Find("WESTIN").transform.GetChild(0).GetChild(0).gameObject;
                }
            }

            GUILayout.Label("Rig Model", EditorStyles.boldLabel);


            //----CHECK IF CURRRENT RIG EXISTS
            if (currentRig != null)
            {
                bool deleteObject = EditorUtility.DisplayDialog("Warning", "Do you want to delete the current Rig?", "Delete", "Cancel");
                if (deleteObject)
                {
                    DestroyImmediate(currentRig);
                }
            }
            else
            {
                Debug.Log("No previous rig");
            }
            //DEEP COPY
            CopyObjectsOnSkeletonPart(originSkeletonRoot.transform, destSkeletonRoot.transform);
            //CHANGE PARENT to CORRECT PLAYER
            if (isTravis) {
                myModel.transform.position = GameObject.Find("TRAVIS").transform.GetChild(0).transform.position;
                myModel.transform.SetParent(GameObject.Find("TRAVIS").transform.GetChild(0)); 
            }
            else {
                myModel.transform.position = GameObject.Find("WESTIN").transform.GetChild(0).transform.position;
                myModel.transform.SetParent(GameObject.Find("WESTIN").transform.GetChild(0));
            }
            destSkeletonRoot = null;
            Debug.Log("Copy Completed");
        }


        prefabName = EditorGUILayout.TextField("Name of Rig", prefabName);

        if (GUILayout.Button("Save Object"))
        {
            if (myModel == null)
            {
                Debug.LogError("Object is null, cannot save.");
                return;
            }


            string path;
            if(isTravis) { path = "Assets/Resources/Prefabs/Rigs/Travis/" + prefabName +".prefab"; }
            else { path = "Assets/Resources/Prefabs/Rigs/Westin"; }
            //Store new object as prefab
            GameObject myNewPrefabRig = PrefabUtility.SaveAsPrefabAssetAndConnect(myModel, path, InteractionMode.UserAction);
            //get prfab
            GameObject gameControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/__GHOST DOSE/GameController.prefab");
            // Get the current length of the array
            int currentLength = gameControllerPrefab.GetComponent<RigManager>().travRigList.Length;
            //Create extra spot on array
            Array.Resize(ref gameControllerPrefab.GetComponent<RigManager>().travRigList, gameControllerPrefab.GetComponent<RigManager>().travRigList.Length + 1);
            // Add the new GameObject at the end of the array
            gameControllerPrefab.GetComponent<RigManager>().travRigList[currentLength] = myNewPrefabRig;
            // Save the changes to the prefab
            AssetDatabase.SaveAssets();
            Debug.Log("Rig Saved!");
            // Unload the prefab from memory
            //PrefabUtility.UnloadPrefabContents(gameControllerPrefab);

            Debug.Log("Object saved successfully.");
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
