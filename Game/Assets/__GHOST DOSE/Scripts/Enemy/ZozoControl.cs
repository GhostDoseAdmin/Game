using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManager;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.AI;
using Unity.VisualScripting;
using Newtonsoft.Json;
using NetworkSystem;
using InteractionSystem;

public class ZozoControl : MonoBehaviour
{
    AudioSource audioSource1, audioSource2;
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
    private bool startCharging;
    //private int laserBlocked;
    //public bool blocked = false;
    private void Awake()
    {
        audioSource1 = gameObject.AddComponent<AudioSource>();
        audioSource1.spatialBlend = 1.0f;
        audioSource2 = gameObject.AddComponent<AudioSource>();
        audioSource2.spatialBlend = 1.0f;
       
    }
    // Start is called before the first frame update
    void Start()
    {
        targetPos = transform.position;
        canLaser = true;
        laserActive = false;

        chargeLightStart = chargeLight.transform.localPosition;
        laserChargeVFXstartScale = laserChargeVFX.transform.localScale;

        if (GetComponentInParent<VictimControl>() != null) { this.gameObject.name = "ZOZO-" + GetComponentInParent<VictimControl>().gameObject.name; this.gameObject.SetActive(false); }
    }


    private float canLaserTimer;
    public void Update()
    {
        /*if (Input.GetKeyUp(KeyCode.Escape)) 
        {
            canLaser = true;
            ChargeLaser(false);
            Debug.Log("------------TRIGGER LASER-------------------");
        }*/


        //INITIATE LASER
        if (Time.time > canLaserTimer + laserCoolDown && GetComponent<NPCController>().target!=null && NetworkDriver.instance.HOST)
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
                    if (Vector3.Distance(transform.position, new Vector3(GetComponent<NPCController>().target.position.x, transform.position.y, GetComponent<NPCController>().target.position.z)) < laserDistanceMax
                        && Vector3.Distance(transform.position, new Vector3(GetComponent<NPCController>().target.position.x, transform.position.y, GetComponent<NPCController>().target.position.z)) >= laserDistanceMin
                        )
                    {
                            ChargeLaser(false); 
                        
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
                startCharging = false;
                chargeLight.transform.localPosition = chargeLightStart;
                if(charging)
                {
                    audioSource1.volume = 10f;
                    AudioManager.instance.Play("laserorigin", audioSource1);
                   // Invoke("HasTriedLaser", 1f);
                }
                charging = false;
                chargeLight.SetActive(false);
                laserChargeVFX.SetActive(false);
                GetComponent<Animator>().SetBool("laser", true);
                laserLights.SetActive(true);
                laserActive = true;
                //shake
                if (GameObject.Find("Hit") != null) { GameObject.Find("PlayerCamera").GetComponent<Camera_Controller>().InvokeShake(0.5f, Mathf.InverseLerp(20f, 0f, Vector3.Distance(GameDriver.instance.Player.transform.position, GameObject.Find("Hit").transform.position))); }
                GameObject.Find("PlayerCamera").GetComponent<Camera_Controller>().InvokeShake(0.5f, Mathf.InverseLerp(20f, 0f, Vector3.Distance(GameDriver.instance.Player.transform.position, transform.position))); ;

                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(GetComponent<NPCController>().target.transform.position - transform.position), 50f * Time.deltaTime);
                //LASER BLOCKED
                /*if (NetworkDriver.instance.HOST && hasTriedLaser)
                {
                    RaycastHit hit;
                    Vector3 targPos = GetComponent<NPCController>().target.position + Vector3.up * 1.4f;
                    Debug.DrawLine(Head.transform.position, targPos); //1.6
                    LayerMask mask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Default"));
                    if (Physics.Linecast(Head.transform.position, targPos, out hit, mask.value))
                    {
                        //Debug.Log("----------TARGET -------------------" + hit.collider.gameObject.name);
                        if (hit.transform != GetComponent<NPCController>().target)
                        {
                            laserBlocked++;
                            if (laserBlocked > 150)
                            {
                                blocked = true;
                                StopLaser();
                                if (NetworkDriver.instance.TWOPLAYER) { NetworkDriver.instance.sioCom.Instance.Emit("laser", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','on':'false'}}"), false); }
                            }
                        }
                    }
                }*/
            }
            //CHARGING
            if (startCharging && GetComponent<Animator>().GetCurrentAnimatorClipInfo(1)[0].clip.name != "zozoLaserStart")
            {
                GetComponent<Animator>().Play("zozoLaserStart", 1, 0f);
            }

            GetComponent<Animator>().SetBool("Walk", false);
            GetComponent<Animator>().SetBool("Fighting", false);
            GetComponent<Animator>().SetBool("Run", false);
            GetComponent<Animator>().SetBool("Attack", false);
        }


    }
   /* bool hasTriedLaser;
    void HasTriedLaser()
    {
        hasTriedLaser = true;
    }*/
    public void TriggerCharge()//FROM ANIMATION, FOR EFFECTS
    {
        charging = true;
        chargeLight.SetActive(true);
        laserChargeVFX.SetActive(true);
        //laserBlocked = 0;
       // blocked = false;
       // hasTriedLaser = false;
    }

    public void ChargeLaser(bool otherPlayer)
    {
        if(canLaser || otherPlayer)
        {
            Debug.Log("CHARGING LASER");
            startCharging = true;
            if (NetworkDriver.instance.HOST && NetworkDriver.instance.TWOPLAYER) { NetworkDriver.instance.sioCom.Instance.Emit("laser", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','on':'true'}}"), false); }
            canLaser = false;
            GetComponent<Animator>().SetLayerWeight(0, 0f);
            GetComponent<Animator>().SetLayerWeight(1, 1f);
            //GetComponent<Animator>().Play("zozoLaserStart", 1, 0f);
            GetComponent<NavMeshAgent>().speed = 0;
            GetComponent<NavMeshAgent>().isStopped = true;
            GetComponent<NPCController>().zozoLaser = true;//OVERRIDE ATTACK FUNCTION
            Invoke("StopLaser", laserDuration);
        }

    }

    private void TriggerLaugh()
    {
        AudioManager.instance.Play("zozolaugh", null);
    }

    public void StopLaser()
    {
        GetComponent<Animator>().SetLayerWeight(0, 1f);
        GetComponent<Animator>().SetLayerWeight(1, 0f);
        GetComponent<NPCController>().zozoLaser = false;
        AudioManager.instance.Play("zozoLaserStop", null);
        canLaser = true;
        laserActive = false;
        laserLights.SetActive(false);
        GetComponent<Animator>().SetBool("laser", false);
        laserChargeVFX.transform.localScale = laserChargeVFXstartScale; 
        canLaserTimer = Time.time;//cooldown
       // if (blocked) { canLaserTimer =0; AudioManager.instance.Play("zozoLaserStop", null); }//reset cooldown
    }

    public void TriggerFootstepRight()
    {
        if (GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length > 0 && GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "WalkZozo")
        {
            audioSource1.clip = footSteps[Random.Range(0, footSteps.Length)].clip;
            audioSource1.volume = 2f;
            audioSource1.Play();
            FootStep();
        }

    }
    public void TriggerFootstepLeft()
    {
        if (GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length > 0 && GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "WalkZozo")
        {
            audioSource2.clip = footSteps[Random.Range(0, footSteps.Length)].clip;
            audioSource2.volume = 2f;
            audioSource2.Play();
            FootStep();
        }
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

    private void OnEnable()
    {
        Debug.Log("ACTIVING ZOZO");
    }

    //------------LASER HEAD------------------
    void OnAnimatorIK()
    {
       if (GetComponent<NPCController>().target!=null)
        {
            targetPos = Vector3.Lerp(targetPos, GetComponent<NPCController>().target.position, 1f * Time.deltaTime); //
            Animator animator = GetComponent<Animator>();
            animator.SetLookAtWeight(1);
            animator.SetLookAtPosition(targetPos);

        }


    }
}
