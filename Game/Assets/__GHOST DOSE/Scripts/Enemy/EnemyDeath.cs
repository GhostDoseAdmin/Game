using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using InteractionSystem;

public class EnemyDeath : MonoBehaviour
{

    public GameObject effect;
    public GameObject effect_Shadower;
    public GameObject explo;
    public GameObject explo_Shadower;
    public bool Shadower;
    private bool end;
    public AudioSource audioSource;
    public bool brute = false;
    public GameObject main;

    // Start is called before the first frame update
    void Start()
    {

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        AudioManager.instance.Play("EnemyDeath", audioSource);
        if (brute) { AudioManager.instance.Play("BruteDeath", audioSource); }
        Invoke("Explode", 2f);
        end = false;

        if (!brute)
        {
            transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[0].SetFloat("_Alpha", 0.5f);
            transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[0].SetFloat("_EMFAlpha", 0.5f);
            if (Shadower) { transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().materials[0].SetFloat("_Shadower", 1f); }
            if (Shadower) { effect_Shadower.SetActive(true); }
            else { effect.SetActive(true); }
        }
    }

    public void Update()
    {

        //GetComponent<GhostVFX>().Fade(false, 5f, 0);

        if (!end) { effect.transform.localScale = Vector3.Lerp(effect.transform.localScale, effect.transform.localScale * 1.5f, Time.deltaTime * 1); }
        else {
            if (!brute) { explo.transform.localScale = Vector3.Lerp(explo.transform.localScale, effect.transform.localScale * 0.001f, Time.deltaTime * 1); } //shrink explo
        }
    }

    private void Explode()
    {
        if (!brute)
        {
            if (Shadower) { explo_Shadower.SetActive(true); }
            else { explo.SetActive(true); }
        }
        else {
            GameObject explosion = Instantiate(explo, transform.position, transform.rotation);
            explosion.GetComponent<bruteExplosion>().main = main;
            explosion.transform.localScale = transform.localScale;
        }

        AudioManager.instance.Play("EnemyExplode", audioSource);
        Invoke("Finish", 1f);
        Invoke("StopMesh", 0.5f);

    }
    private void StopMesh()
    {
        transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
        GetComponent<Animator>().enabled = false;
    }
    private void Finish()
    {
        AudioManager.instance.Play("EnemyExplode", audioSource);
        effect.SetActive(false);
        end = true;
        Destroy(gameObject, 0.5f);

    }

}