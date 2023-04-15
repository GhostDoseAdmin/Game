using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagDollDeath : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<GhostVFX>().Fade(false,1f,0);
    }
    public void StartDeath()
    {
        
    }
    public void EndDeath()
    {
       Destroy(gameObject);
    }
}
