using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]
public class ConeDetect : MonoBehaviour
{
    public Light spotlight;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(IsObjectInLightCone(spotlight));
    }


    bool IsObjectInLightCone(Light spotlight)
    {
        GameObject target = this.gameObject;
        // Calculate the direction vector from the light to the target object
        Vector3 directionToObject = (target.transform.position - spotlight.transform.position).normalized;

        // Calculate the angle between the light's forward direction and the direction to the object
        float angleToObject = Vector3.Angle(spotlight.transform.forward, directionToObject);

        // Check if the angle is within the cone of light
        if (angleToObject <= spotlight.spotAngle * 0.5f)
        {
            return true;
        }

        return false;
    }
}
