using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using InteractionSystem;
public class RemPod : MonoBehaviour
{

    public GameObject remPodTarget, remPodProj;
    Vector3 target;
    //public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        //audioSource = gameObject.AddComponent<AudioSource>();
       // audioSource.spatialBlend = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(GetComponentInParent<PlayerController>() != null) {
            if (GetComponentInParent<PlayerController>().gearAim)
            {
                remPodTarget.GetComponent<MeshRenderer>().enabled = true;
            }
            else { remPodTarget.GetComponent<MeshRenderer>().enabled = true; }
        
        }
    }

    public void StartThrow()
    {
       // AudioManager.instance.Play("EMPThrow", audioSource);
        target = remPodTarget.transform.position;
        if (NetworkDriver.instance.TWOPLAYER) { 
            NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { remthrow = true}), false);
            //NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'rem':'true','x':'{target.x}','y':'{target.y}','z':'{target.z}'}}"), false);
        }
       
    }
    public void ReleaseClient(Vector3 targetClient)
    {
        GameObject remProj = Instantiate(remPodProj);
        remProj.transform.position = this.transform.position;
        remProj.GetComponent<RemPodProj>().target = targetClient;
    }
    public void Release() //Vector3 othersTarget
    {
        GameObject remProj = Instantiate(remPodProj);
        remProj.transform.position = this.transform.position;
        remProj.GetComponent<RemPodProj>().target = target;
       // if(othersTarget!=null) { remProj.GetComponent<RemPodProj>().target = othersTarget; }
       NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'remrelease':'true','x':'{target.x}','y':'{target.y}','z':'{target.z}'}}"), false); 
    }
}
