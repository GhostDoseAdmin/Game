using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportVFX : MonoBehaviour
{
    public GameObject glowOrb;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (glowOrb.transform.localScale.x > 0.05) { glowOrb.transform.localScale = Vector3.Lerp(glowOrb.transform.localScale, glowOrb.transform.localScale * 0.2f, Time.deltaTime * 1); }
        else { DestroyImmediate(this.gameObject); }
    }
}
