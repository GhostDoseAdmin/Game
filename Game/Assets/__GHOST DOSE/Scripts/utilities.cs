using UnityEngine;

public class utilities : MonoBehaviour
{
   
    //----------------------FIND OBJECT IN HEIARCHY--------------------------
    public GameObject FindChildObject(Transform parentTransform, string name)
    {
        if (parentTransform.gameObject.name == name)
        {
            return parentTransform.gameObject;
        }

        foreach (Transform childTransform in parentTransform)
        {
            GameObject foundObject = FindChildObject(childTransform, name);
            if (foundObject != null)
            {
                return foundObject;
            }
        }
        return null;
    }
}
