using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
public class RemPod : MonoBehaviour
{

    public GameObject remPodTarget, remPodProj;
    Vector3 target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartThrow()
    {
        target = remPodTarget.transform.position;
        if (NetworkDriver.instance.TWOPLAYER) { 
            NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { remthrow = true}), false);
            //NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'rem':'true','x':'{target.x}','y':'{target.y}','z':'{target.z}'}}"), false);
        }
       
    }

    public void Release() //Vector3 othersTarget
    {
        GameObject remProj = Instantiate(remPodProj);
        remProj.transform.position = this.transform.position;
        remProj.GetComponent<RemPodProj>().target = target;
       // if(othersTarget!=null) { remProj.GetComponent<RemPodProj>().target = othersTarget; }
       // if (othersTarget == null) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'remrelease':'true','x':'{target.x}','y':'{target.y}','z':'{target.z}'}}"), false); }
    }
}
