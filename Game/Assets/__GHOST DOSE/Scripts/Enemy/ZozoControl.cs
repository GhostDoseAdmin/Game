using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManager;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.AI;
using Unity.VisualScripting;
using Newtonsoft.Json;
using NetworkSystem;

public class ZozoControl : MonoBehaviour
{
    AudioSource audioSourceFootStepRight, audioSourceFootStepLeft;
    public Sound[] footSteps;
    public GameObject Head;
    public GameObject laserChargeVFX;
    Vector3 laserChargeVFXstartScale;
    public GameObject chargeLight;
    public GameObject chargeEndTransform;
    Vector3 chargeLightStart;
    public GameObject laserLights;
    private Vector3 targetPos;
    [HideInInspector]public bool canLaser;
    public float laserDuration;
    public float laserDistanceMin, laserDistanceMax;
    [HideInInspector] public bool laserActive;
    public float laserForce;
    public int laserDamage;
    public float laserCoolDown;
    private bool charging;

    // Start is called before the first frame update
    void Start()
    {
        targetPos = transform.position;
        canLaser = true;
        laserActive = false;
        audioSourceFootStepRight = gameObject.AddComponent<AudioSource>();
        audioSourceFootStepRight.spatialBlend = 1.0f;
        audioSourceFootStepLeft = gameObject.AddComponent<AudioSource>();
        audioSourceFootStepLeft.spatialBlend = 1.0f;
        chargeLightStart = chargeLight.transform.localPosition;
        laserChargeVFXstartScale = laserChargeVFX.transform.localScale;
    }


    private float canLaserTimer;
    public void Update()
    {
        //INITIATE LASER
        if (Time.time > canLaserTimer + laserCoolDown && GetComponent<NPCController>().target!=null)
        {
            RaycastHit hit;
            Vector3 targPos = GetComponent<NPCController>().target.position + Vector3.up * 1.4f;
            Debug.DrawLine(Head.transform.position, targPos); //1.6
            LayerMask mask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Default"));
            if (Physics.Linecast(Head.transform.position, targPos, out hit, mask.value))
            {
                //Debug.Log("----------TARGET -------------------" + hit.collider.gameObject.name);
                if (hit.transform == GetComponent<NPCController>().target)
                {
                    if (canLaser && Vector3.Distance(transform.position, new Vector3(GetComponent<NPCController>().target.position.x, transform.position.y, GetComponent<NPCController>().target.position.z)) < laserDistanceMax
                        && Vector3.Distance(transform.position, new Vector3(GetComponent<NPCController>().target.position.x, transform.position.y, GetComponent<NPCController>().target.position.z)) >= laserDistanceMin
                        )
                    {
                        if (NetworkDriver.instance.HOST) { ChargeLaser(); }
                    }
                }
            }


        }
        if(GetComponent<NPCController>().zozoLaser)
        {
            //CHARGING
            if (charging)
            {
                if (Vector3.Distance(chargeEndTransform.transform.position, chargeLight.transform.position) < 0.1f) { laserChargeVFX.transform.localScale = Vector3.Lerp(laserChargeVFX.transform.localScale, laserChargeVFX.transform.localScale * 2f, Time.deltaTime * 1); } 
                else { chargeLight.transform.position += (chargeEndTransform.transform.position - chargeLight.transform.position).normalized * 0.1f; }
            }
            //SHOOTING
            if (GetComponent<Animator>().GetCurrentAnimatorClipInfo(1)[0].clip.name == "ZozoLaserAni")
            {
                chargeLight.transform.localPosition = chargeLightStart;
                charging = false;
                chargeLight.SetActive(false);
                laserChargeVFX.SetActive(false);
                GetComponent<Animator>().SetBool("laser", true);
                laserLights.SetActive(true);
                laserActive = true;
                //shake
                if (GameObject.Find("Hit") != null) { GameObject.Find("PlayerCamera").GetComponent<Camera_Controller>().InvokeShake(0.5f, Mathf.InverseLerp(20f, 0f, Vector3.Distance(GameDriver.instance.Player.transform.position, GameObject.Find("Hit").transform.position))); }
                GameObject.Find("PlayerCamera").GetComponent<Camera_Controller>().InvokeShake(0.5f, Mathf.InverseLerp(20f, 0f, Vector3.Distance(GameDriver.instance.Player.transform.position, transform.position))); ;
            }
           
            GetComponent<Animator>().SetBool("Walk", false);
            GetComponent<Animator>().SetBool("Fighting", false);
            GetComponent<Animator>().SetBool("Run", false);
            GetComponent<Animator>().SetBool("Attack", false);
        }


    }
    public void TriggerCharge()//FROM ANIMATION, FOR EFFECTS
    {
        charging = true;
        chargeLight.SetActive(true);
        laserChargeVFX.SetActive(true);
    }

    public void ChargeLaser()
    {
        if (NetworkDriver.instance.HOST) { NetworkDriver.instance.sioCom.Instance.Emit("laser", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}'}}"), false); }
        canLaser = false;
        GetComponent<Animator>().SetLayerWeight(0, 0f);
        GetComponent<Animator>().SetLayerWeight(1, 1f);
        GetComponent<Animator>().Play("zozoLaserStart", 1, 0f);
        GetComponent<NavMeshAgent>().speed = 0;
        GetComponent<NavMeshAgent>().isStopped = true;
        GetComponent<NPCController>().zozoLaser = true;//OVERRIDE ATTACK FUNCTION
        Invoke("StopLaser", laserDuration);
    }


    private void StopLaser()
    {
        GetComponent<Animator>().SetLayerWeight(0, 1f);
        GetComponent<Animator>().SetLayerWeight(1, 0f);
        GetComponent<NPCController>().zozoLaser = false;
        canLaser = true;
        laserActive = false;
        laserLights.SetActive(false);
        GetComponent<Animator>().SetBool("laser", false);
        laserChargeVFX.transform.localScale = laserChargeVFXstartScale; 
        canLaserTimer = Time.time;//cooldown
    }

    public void TriggerFootstepRight()
    {
        audioSourceFootStepRight.clip = footSteps[Random.Range(0, footSteps.Length)].clip;
        audioSourceFootStepRight.volume = 2f;
        audioSourceFootStepRight.Play();
        FootStep();

    }
    public void TriggerFootstepLeft()
    {
        audioSourceFootStepLeft.clip = footSteps[Random.Range(0, footSteps.Length)].clip;
        audioSourceFootStepLeft.volume = 2f;
        audioSourceFootStepLeft.Play();
        FootStep();

    }

    private void FootStep()
    {
        GameObject.Find("PlayerCamera").GetComponent<Camera_Controller>().InvokeShake(0.5f, Mathf.InverseLerp(20f, 0f, Vector3.Distance(GameDriver.instance.Player.transform.position, transform.position)));
        Light[] allLights = GameObject.FindObjectsOfType<Light>(); // Find all the Light components in the scene

        foreach (Light light in allLights)
        {
            float distanceToLight = Vector3.Distance(transform.position, light.transform.position); // Calculate the distance to the light

            if (distanceToLight <= 20f) // If the light is within range, call the InvokeFlicker method
            {
                if (light.GetComponent<GhostLight>() != null) { light.GetComponent<GhostLight>().InvokeFlicker(1f); }
            }
        }
    }



    //------------LASER HEAD------------------
    void OnAnimatorIK()
    {

        targetPos = Vector3.Lerp(targetPos, new Vector3(GetComponent<NPCController>().target.transform.position.x, GetComponent<NPCController>().target.transform.position.y + 1f, GetComponent<NPCController>().target.transform.position.z), 1f * Time.deltaTime);
        Animator animator = GetComponent<Animator>();
        animator.SetLookAtWeight(1);
        animator.SetLookAtPosition(targetPos);


    }
}
