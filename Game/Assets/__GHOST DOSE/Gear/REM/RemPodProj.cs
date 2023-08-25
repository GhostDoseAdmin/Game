using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionSystem;
public class RemPodProj : MonoBehaviour
{
    public Vector3 target; // The target destination
    public float travelTime = 2f; // Adjust this value for the desired travel time

    private Vector3 initialPosition;
    private float currentTime = 0f;
    private float archHeight = 1f;

    public GameObject remExplo, remPod;
    public AudioSource audioSource;
    private void Start()
    {
        initialPosition = transform.position;
        travelTime = Vector3.Distance(initialPosition, target)*0.1f;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        AudioManager.instance.Play("EMPThrow", audioSource);
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        // Calculate the lerp parameter based on time
        float t = currentTime / travelTime;

        // Calculate the y position using a quadratic function for the arch
        float yOffset = -4 * archHeight * (t - 0.5f) * (t - 0.5f) + archHeight;

        // Calculate the new position using lerp
        Vector3 newPosition = Vector3.Lerp(initialPosition, target, t);

        // Apply the y offset
        newPosition.y += yOffset;

        // Set the new position
        transform.position = newPosition;

        // If the travel is done, reset the time
        if (currentTime >= travelTime)
        {
            GameObject remExploInst = Instantiate(remExplo);
            remPod.GetComponent<RemPod>().remExploInstance = remExploInst;
            remExploInst.transform.position = transform.position;
            Destroy(gameObject);
            //transform.position = target.position;
            //currentTime = 0f;
        }
    }
}
