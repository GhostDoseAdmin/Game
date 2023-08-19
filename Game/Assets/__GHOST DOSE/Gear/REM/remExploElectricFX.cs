using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionSystem;
public class remExploElectricFX : MonoBehaviour
{
    //public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        //audioSource = gameObject.AddComponent<AudioSource>();
        //audioSource.spatialBlend = 1.0f;
        //AudioManager.instance.Play("EMPHit", audioSource);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 3f, Time.deltaTime * 1);
        if (transform.localScale.x < -100f)
        {
            Destroy(this.gameObject);
        }
    }
}
