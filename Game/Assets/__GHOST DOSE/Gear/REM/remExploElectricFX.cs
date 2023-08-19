using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class remExploElectricFX : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
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
