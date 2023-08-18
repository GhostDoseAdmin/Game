using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemPod : MonoBehaviour
{

    public GameObject remPodTarget, remPodProj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Throw()
    {
        GameObject remProj = Instantiate(remPodProj);
        remProj.transform.position = this.transform.position;
        remProj.GetComponent<RemPodProj>().target = remPodTarget.transform.position;
    }
}
