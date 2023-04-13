using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClientSidePrediction : MonoBehaviour
{
    public float speed = 1f;
    public float distanceThreshold = 0.1f;

    private Vector3 targetPosition;

    void Update()
    {
        if (!GetComponent<NPCController>().GD.ND.HOST)
        {
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance > distanceThreshold)
            {
                float timeToTarget = distance / speed;
                float newSpeed = distance / timeToTarget;
                GetComponent<NavMeshAgent>().speed = newSpeed;
                Debug.Log("-----------------------------------NEW SPEED -----------------------------" + newSpeed);
            }
        }
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
       // GetComponent<NavMeshAgent>().SetDestination(position);
    }
}
