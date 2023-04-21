using UnityEngine;
using System.Collections;

public class GhostLight : MonoBehaviour
{
    public bool show_Ghost;
    public bool show_Shadowers;

    [HideInInspector] public bool ghost;
    [HideInInspector] public bool shadower;
    public bool flicker;
    public float strength;

    public float flickerDuration = 0.1f; // duration of each flicker in seconds
    public float minDelay = 0.1f; // minimum delay between flickers in seconds
    public float maxDelay = 0.5f; // maximum delay between flickers in seconds
    private Light lightComponent; // reference to the Light component
    private float originalSpotAngle; // original spot angle of the light


    private void Start()
    {
        // get reference to the Light component
        lightComponent = GetComponent<Light>();

        // store the original spot angle of the light
        originalSpotAngle = lightComponent.spotAngle;
    }

    private IEnumerator Flicker()
    {
        // flicker the light
        lightComponent.spotAngle = 0;

        // wait for the flicker duration
        yield return new WaitForSeconds(flickerDuration);

        // restore the original spot angle
        lightComponent.spotAngle = originalSpotAngle;
    }

    private void Update()
    {
        ghost = show_Ghost;
        shadower = !show_Shadowers;

        // check if it's time to flicker the light
        if (Random.Range(0f, 1f) < Time.deltaTime / Random.Range(minDelay, maxDelay))
        {
            // start the flicker coroutine
            if (flicker) { StartCoroutine(Flicker()); }
        }
    }
}
