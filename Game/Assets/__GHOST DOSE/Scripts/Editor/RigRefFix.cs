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

            ReplaceComponentReferences(parentObject, firstChild, secondChild);

            Debug.Log("References replaced successfully.");
        }
    }

    void ReplaceComponentReferences(GameObject parentObject, GameObject firstChild, GameObject secondChild)
    {
        // Get all components on the parent object
        Component[] components = parentObject.GetComponents<Component>();

        // Loop through each component and its serialized properties
        foreach (Component component in components)
        {
            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty serializedProperty = serializedObject.GetIterator();

            while (serializedProperty.Next(true))
            {
                if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference)
                {
                    Object objectRef = serializedProperty.objectReferenceValue;

                    // Check if the object reference points to the first child object or any of its descendants
                    if (objectRef != null && IsDescendantOf(objectRef, firstChild))
                    {
                        // Traverse the hierarchy of the first child object and find the corresponding object in the second child object
                        GameObject firstChildObject = (objectRef as Component).gameObject;
                        GameObject secondChildObject = FindEquivalentObject(firstChildObject, firstChild, secondChild);

                        if (secondChildObject != null)
                        {
                            // Replace the object reference with a reference to the corresponding object in the second child object
                            serializedProperty.objectReferenceValue = secondChildObject;
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
        }
    }

    bool IsDescendantOf(Object objectRef, GameObject parent)
    {
        // Check if the object reference is a child or descendant of the specified parent object
        Transform transform = (objectRef as Component)?.transform;

        while (transform != null)
        {
            if (transform.gameObject == parent)
            {
                return true;
            }

            transform = transform.parent;
        }

        return false;
    }

    GameObject FindEquivalentObject(GameObject firstChildObject, GameObject firstChild, GameObject secondChild)
    {
        // Traverse the hierarchy of the first child object and find the corresponding object in the second child object
        Transform transform = firstChildObject.transform;
        GameObject secondChildObject = secondChild;

        while (transform != firstChild.transform && transform != null)
        {
            Transform childTransform = transform.parent.Find(transform.name);

            if (childTransform == null)
            {
                return null;
            }

            secondChildObject = secondChild.transform.Find(childTransform.name)?.gameObject;


            if (secondChildObject == null)
            {
                return null;
            }
            transform = childTransform;
        }

        return secondChildObject;
    }
}
