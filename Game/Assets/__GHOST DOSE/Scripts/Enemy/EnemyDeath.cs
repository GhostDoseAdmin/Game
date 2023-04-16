using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyDeath : MonoBehaviour
{

    public GameObject effect;
    public GameObject explo;
    public GameObject light;
    private bool end;


    public Vector3 targetScale = Vector3.one * 2f;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("Explode", 2f);
        end = false;
    }

    public void Update()
    {

        //GetComponent<GhostVFX>().Fade(false, 5f, 0);

        if (!end) { effect.transform.localScale = Vector3.Lerp(effect.transform.localScale, effect.transform.localScale * 1.5f, Time.deltaTime * 1); }
        else { explo.transform.localScale = Vector3.Lerp(explo.transform.localScale, effect.transform.localScale * 0.001f, Time.deltaTime * 1); }//shrink explo
    }

    private void Explode()
    {
        explo.SetActive(true);
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
 
        effect.SetActive(false);
        end = true;
        Destroy(gameObject, 0.5f);

    }

}