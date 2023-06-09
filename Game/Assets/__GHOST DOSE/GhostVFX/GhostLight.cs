using UnityEngine;
using System.Collections;
using InteractionSystem;
public class GhostLight : MonoBehaviour
{
    public bool show_Ghost;
    public bool show_Shadowers;

    [HideInInspector] public bool ghost;
    [HideInInspector] public bool shadower;
    [HideInInspector] public bool flicker;
    public bool canFlickers = true;
    public float strength;

    private float flickerDuration = 0.05f; // duration of each flicker in seconds
    private float minDelay = 0.1f; // minimum delay between flickers in seconds
    private float maxDelay = 0.3f; // maximum delay between flickers in seconds
    private Light lightComponent; // reference to the Light component
    private float originalSpotAngle; // original spot angle of the light
    private bool defaultFlicker;
    private AudioSource flickerAudioSource;


    private void Awake()
    {
        ghost = show_Ghost;
        shadower = !show_Shadowers;

        flickerAudioSource = gameObject.AddComponent<AudioSource>();
        flickerAudioSource.spatialBlend = 1.0f;
    }

    private void Start()
    {
        // get reference to the Light component
        lightComponent = GetComponent<Light>();

        // store the original spot angle of the light
        originalSpotAngle = lightComponent.spotAngle;

        defaultFlicker = flicker;
    }

    private IEnumerator Flicker()
    {
       // flicker = false;
        // flicker the light
        lightComponent.spotAngle = 0;

        // wait for the flicker duration
        yield return new WaitForSeconds(flickerDuration);

        // restore the original spot angle
        AudioManager.instance.Play("lightflicker", flickerAudioSource);
        lightComponent.spotAngle = originalSpotAngle;
    }

    public void InvokeFlicker(float duration)
    {
        CancelInvoke();
        flicker = true;
        Invoke("FlickerTimer", duration);
    }

    public void FlickerTimer()
    {
        flicker = defaultFlicker;
    }

    private void Update()
    {

        // check if it's time to flicker the light
        if (Random.Range(0f, 1f) < Time.deltaTime / Random.Range(minDelay, maxDelay))
        {
            // start the flicker coroutine
            if (flicker) { StartCoroutine(Flicker()); }
        }
    }

    private void OnDisable()
    {
        flicker = defaultFlicker;
    }
}
