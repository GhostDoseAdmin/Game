using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;
using InteractionSystem;
public class RemPod : MonoBehaviour
{

    public GameObject remPodTarget, remPodProj, remProjInstance, remExploInstance, remPodSkin;
    Vector3 target;
    private bool startThrow;
    //private bool canThrow;
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
        //-----------------PLAYER--------------------
        //HIDE target
        if(GetComponentInParent<PlayerController>() != null) {
            if (GetComponentInParent<PlayerController>().gear == 3)
            {
                //HIDE target
                if (GetComponentInParent<PlayerController>().gearAim && remProjInstance==null && remExploInstance==null && startThrow==false)
                {
                    remPodTarget.GetComponent<MeshRenderer>().enabled = true;
                }
                else { remPodTarget.GetComponent<MeshRenderer>().enabled = false; }
                //HIDE REMPOD
                if (remProjInstance != null || remExploInstance != null)
                {
                    remPodSkin.SetActive(false);
                }
                else { remPodSkin.SetActive(true); }
            }
            else { remPodSkin.SetActive(false); remPodTarget.GetComponent<MeshRenderer>().enabled = false; startThrow = false; }
        }

        //-----------------CLIENT--------------------
        //HIDE target
        if (GetComponentInParent<ClientPlayerController>() != null)
        {
            remPodTarget.GetComponent<MeshRenderer>().enabled = false;
            if (GetComponentInParent<ClientPlayerController>().gear == 3)
            {
                //HIDE REMPOD
                if (remProjInstance != null || remExploInstance != null)
                {
                    remPodSkin.SetActive(false);
                }
                else { remPodSkin.SetActive(true); }
            }
            else { remPodSkin.SetActive(false); startThrow = false; }

        }

    }

    public void StartThrow()
    {
        // AudioManager.instance.Play("EMPThrow", audioSource);
        if (!startThrow)
        {
            //canThrow = false;
            //Invoke("ResetCanThrow",5f);
            startThrow = true;
            target = remPodTarget.transform.position;
            if (NetworkDriver.instance.TWOPLAYER)
            {
                NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { remthrow = true }), false);
                //NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'rem':'true','x':'{target.x}','y':'{target.y}','z':'{target.z}'}}"), false);
            }
        }

       
    }
   /* public void ResetCanThrow()
    {
        canThrow = true;
    }*/

    public void ReleaseClient(Vector3 targetClient)
    {
        GameObject remProj = Instantiate(remPodProj);
        remProjInstance = remProj;
        remProj.GetComponent<RemPodProj>().isClients = true;
        remProj.GetComponent<RemPodProj>().remPod = this.gameObject;
        remProj.transform.position = this.transform.position;
        remProj.GetComponent<RemPodProj>().target = targetClient;
        startThrow = false;
    }
    public void Release() //Vector3 othersTarget
    {
        GameObject remProj = Instantiate(remPodProj);
        remProjInstance = remProj;
        remProj.GetComponent<RemPodProj>().remPod = this.gameObject;
        remProj.transform.position = this.transform.position;
        remProj.GetComponent<RemPodProj>().target = target;
        startThrow = false;
        // if(othersTarget!=null) { remProj.GetComponent<RemPodProj>().target = othersTarget; }
        NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'remrelease':'true','x':'{target.x}','y':'{target.y}','z':'{target.z}'}}"), false); 
    }


}
