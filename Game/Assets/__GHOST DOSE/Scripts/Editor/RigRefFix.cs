using UnityEngine;
using UnityEditor;

public class ReplaceReferences : EditorWindow
{
    GameObject parentObject;
    GameObject firstChild;
    GameObject secondChild;

    [MenuItem("Window/Replace References")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceReferences>("Replace References");
    }

    void OnGUI()
    {
        GUILayout.Label("Replace References", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        parentObject = EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true) as GameObject;
        firstChild = EditorGUILayout.ObjectField("First Child", firstChild, typeof(GameObject), true) as GameObject;
        secondChild = EditorGUILayout.ObjectField("Second Child", secondChild, typeof(GameObject), true) as GameObject;

        EditorGUILayout.Space();

        if (GUILayout.Button("Replace References"))
        {
            if (parentObject == null || firstChild == null || secondChild == null)
            {
                Debug.LogError("Please select the parent object, first child, and second child.");
                return;
            }

            ReplaceComponentReferences(parentObject.transform, firstChild.transform, secondChild.transform);

            Debug.Log("References replaced successfully.");
        }
    }

    void ReplaceComponentReferences(Transform parentTransform, Transform firstChildTransform, Transform secondChildTransform)
    {
        // Loop through all the components on the parent object
        Component[] components = parentTransform.GetComponents<Component>();
        foreach (Component component in components)
        {
            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty serializedProperty = serializedObject.GetIterator();

            while (serializedProperty.Next(true))
            {
                if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference)
                {
                    // Check if the object reference points to the first child object
                    Object objectRef = serializedProperty.objectReferenceValue;
                    if (objectRef != null && objectRef.Equals(firstChildTransform.gameObject))
                    {
                        // Replace the object reference with the second child object
                        serializedProperty.objectReferenceValue = secondChildTransform.gameObject;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }

        // Recursively call this function on all the children of the parent object
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform childTransform = parentTransform.GetChild(i);
            ReplaceComponentReferences(childTransform, firstChildTransform, secondChildTransform);
        }
    }
}
