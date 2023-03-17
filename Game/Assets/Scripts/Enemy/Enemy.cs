using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private float xMin = -90f;
    private float xMax = 90f;
    private float zMin = -90f;
    private float zMax = 90f;

    private float x, z;

    public GameObject otherObject;
    private Rigidbody rb;


    void Start()
    {
         StartCoroutine(SetDestination());

        GameObject[] instances = GameObject.FindGameObjectsWithTag("sync");

        foreach (GameObject instance in instances)
        {
            if (instance != gameObject && instance.name == gameObject.name)
            {
                Physics.IgnoreCollision(instance.GetComponent<Collider>(), GetComponent<Collider>());
            }
        }

    }

    public void Update()
    {
        
    }


    IEnumerator SetDestination()
    {
        while (true)
        {
            yield return new WaitForSeconds(11.0f);
            Debug.Log("TRYING MOVE");
            x = Random.Range(xMin, xMax);
            z = Random.Range(zMin, zMax);
            Vector3 destination = new Vector3(x, 0f, z);
            GetComponent<MovementNetworker>().moveEmit(this.gameObject, destination, true);
        }
    }
}