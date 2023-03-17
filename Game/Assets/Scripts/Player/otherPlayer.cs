using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class otherPlayer : MonoBehaviour
{
    private float gravity = -5.0f;
    void Start()
    {
        
    }

    // Update is called once per frame


    private void FixedUpdate()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.up * gravity, ForceMode.Acceleration);//gravity
    }
}
