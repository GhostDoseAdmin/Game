using UnityEngine;
using UnityEditor;

public class ReferenceSwitcherEditor : EditorWindow
{
    private GameObject parentObject;
    private GameObject child1;
    private GameObject child2;

    [MenuItem("Window/Reference Switcher")]
    public static void ShowWindow()
    {
        GetWindow<ReferenceSwitcherEditor>("Reference Switcher");
    }

    private void OnGUI()
    {
        GUILayout.Label("Reference Switcher", EditorStyles.boldLabel);

        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);
        child1 = (GameObject)EditorGUILayout.ObjectField("Child 1", child1, typeof(GameObject), true);
        child2 = (GameObject)EditorGUILayout.ObjectField("Child 2", child2, typeof(GameObject), true);

        if (GUILayout.Button("Switch References"))
        {
            if (parentObject == null || child1 == null || child2 == null)
            {
                Debug.LogError("All fields must be assigned.");
            }
            else
            {
                SwitchReferences();
            }
        }
    }

    private void SwitchReferences()
    {
        if (parentObject == null || child1 == null || child2 == null)
        {
            Debug.LogError("All fields must be assigned.");
            return;
        }

        Component[] parentComponents = parentObject.GetComponents<Component>();

        foreach (Component component in parentComponents)
        {
            SerializedObject serializedComponent = new SerializedObject(component);
            SerializedProperty property = serializedComponent.GetIterator();

            while (property.NextVisible(true))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference &&
                    (property.objectReferenceValue is GameObject || property.objectReferenceValue is Transform))
                {
                    Transform refTransform = null;

                    if (property.objectReferenceValue is GameObject)
                    {
                        refTransform = ((GameObject)property.objectReferenceValue).transform;
                    }
                    else if (property.objectReferenceValue is Transform)
                    {
                        refTransform = (Transform)property.objectReferenceValue;
                    }

                    if (refTransform != null && refTransform.IsChildOf(child1.transform))
                    {
                        string path = GetRelativePath(child1.transform, refTransform);
                        Transform newReference = child2.transform.Find(path);
                        if (newReference)
                        {
                            if (property.objectReferenceValue is GameObject)
                            {
                                property.objectReferenceValue = newReference.gameObject;
                            }
                            else if (property.objectReferenceValue is Transform)
                            {
                                property.objectReferenceValue = newReference;
                            }
                            serializedComponent.ApplyModifiedProperties();
                        }
                    }
                }
            }
        }
    }

    private string GetRelativePath(Transform root, Transform target)
    {
        System.Text.StringBuilder path = new System.Text.StringBuilder();
        while (target != root)
        {
            if (path.Length > 0)
            {
                path.Insert(0, "/");
            }
            path.Insert(0, target.name);
            target = target.parent;
        }
        return path.ToString();
    }
}
