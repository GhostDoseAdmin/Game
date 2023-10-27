using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;
using InteractionSystem;
public class Hovl_Laser : MonoBehaviour
{
    public int damageOverTime = 30;

    public GameObject HitEffect;
    public float HitOffset = 0;
    public bool useLaserRotation = false;

    public float MaxLength;
    private LineRenderer Laser;

    public float MainTextureLength = 1f;
    public float NoiseTextureLength = 1f;
    private Vector4 Length = new Vector4(1,1,1,1);
    //private Vector4 LaserSpeed = new Vector4(0, 0, 0, 0); {DISABLED AFTER UPDATE}
    //private Vector4 LaserStartSpeed; {DISABLED AFTER UPDATE}
    //One activation per shoot
    private bool LaserSaver = false;
    private bool UpdateSaver = false;

    private ParticleSystem[] Effects;
    private ParticleSystem[] Hit;
    private float collideTimer;
    private float collideDelay = 2f;//1.5f
    private AudioSource laserSound;
    public bool LASERGRID = false;
    private float laserGridOpacity = 0.5f;
    void Start ()
    {
        //Get LineRender and ParticleSystem components from current prefab;  
        Laser = GetComponent<LineRenderer>();
        Effects = GetComponentsInChildren<ParticleSystem>();
        Hit = HitEffect.GetComponentsInChildren<ParticleSystem>();
        laserSound = HitEffect.gameObject.AddComponent<AudioSource>();
        laserSound.spatialBlend = 1.0f;
        laserSound.volume = 10f;
        if (!LASERGRID) { AudioManager.instance.Play("zozolasersound", laserSound); }
        else {//LASERGRID
            GetComponent<LineRenderer>().materials[0].SetFloat("_Opacity", laserGridOpacity);
            foreach (ParticleSystem ps in Hit)
            {
                ParticleSystem.MainModule mainModule = ps.main;
                mainModule.startSize = new ParticleSystem.MinMaxCurve(0.025f);
            }
            StartCoroutine(DestroyAfterDelay());

        }
    }

    //DESTROY LASERGRID
    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        GetComponentInParent<ZozoLaser>().laserGridOrigin.flash.SetActive(false);
        Destroy(GetComponentInParent<ZozoLaser>().gameObject);
    }
    void Update()
    {

        Laser.material.SetTextureScale("_MainTex", new Vector2(Length[0], Length[1]));                    
        Laser.material.SetTextureScale("_Noise", new Vector2(Length[2], Length[3]));


        //FADE LASER GRID LASER
        if(LASERGRID)
        {
            laserGridOpacity -= 0.05f;
            GetComponent<LineRenderer>().materials[0].SetFloat("_Opacity", laserGridOpacity);
        }


        //To set LineRender position
        if (Laser != null && UpdateSaver == false)
        {
            Laser.SetPosition(0, transform.position);
        

        {
            RaycastHit hit; //DELETE THIS IF YOU WANT USE LASERS IN 2D
           //ADD THIS IF YOU WANNT TO USE LASERS IN 2D: RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward, MaxLength);       
            LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Player");
                if (LASERGRID) { mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Enemy"); }
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, MaxLength, mask))//CHANGE THIS IF YOU WANT TO USE LASERRS IN 2D: if (hit.collider != null)
            {
                //End laser position if collides with object
                Laser.SetPosition(1, hit.point);

                    HitEffect.transform.position = hit.point + hit.normal * HitOffset;
                if (useLaserRotation)
                    HitEffect.transform.rotation = transform.rotation;
                else
                    HitEffect.transform.LookAt(hit.point + hit.normal);

                foreach (var AllPs in Effects)
                {
                    if (!AllPs.isPlaying) AllPs.Play();
                }
                //Texture tiling
                Length[0] = MainTextureLength * (Vector3.Distance(transform.position, hit.point));
                Length[2] = NoiseTextureLength * (Vector3.Distance(transform.position, hit.point));



                    //-----------------LASER GRID----------------------------------
                    if (LASERGRID)
                    {
                        NPCController target = hit.collider.gameObject.GetComponentInParent<NPCController>();
                        if (target != null) { GetComponentInParent<ZozoLaser>().laserGridOrigin.AddEnemyToEmitList(target); }
                    }

                    Vector3 oppositeForce = GetComponentInParent<NPCController>().transform.forward * GetComponentInParent<NPCController>().laserForce;
                    oppositeForce.y = 0f; // Set the y component to 0

                    if (hit.collider.gameObject.name == "Player" && Time.time > collideTimer + collideDelay)
                    {


                        //if (hit.collider.gameObject.name == "Player")
                        {
                            hit.collider.gameObject.GetComponent<HealthSystem>().HealthDamage(GetComponentInParent<NPCController>().laserDamage, oppositeForce, true);
                        }
                            collideTimer = Time.time;//cooldown
                    }

                    if (hit.collider.gameObject.name == "Client")
                    {
                        //AudioManager.instance.Play("EnemyHit", GetComponentInParent<NPCController>().audioSource);
                        hit.collider.gameObject.GetComponent<ClientPlayerController>().Flinch(oppositeForce, true);//FLINCH DOESNT NEED FORCE AS PLAY IS MOVED BY THAT PLAYER ANYWAY UPDATED POS
                    }
                }
            else
            {
                //End laser position if doesn't collide with object
                var EndPos = transform.position + transform.forward * MaxLength;
                Laser.SetPosition(1, EndPos);
                HitEffect.transform.position = EndPos;
                foreach (var AllPs in Hit)
                {
                    if (AllPs.isPlaying) AllPs.Stop();
                }
                //Texture tiling
                Length[0] = MainTextureLength * (Vector3.Distance(transform.position, EndPos));
                Length[2] = NoiseTextureLength * (Vector3.Distance(transform.position, EndPos));
                //LaserSpeed[0] = (LaserStartSpeed[0] * 4) / (Vector3.Distance(transform.position, EndPos)); {DISABLED AFTER UPDATE}
                //LaserSpeed[2] = (LaserStartSpeed[2] * 4) / (Vector3.Distance(transform.position, EndPos)); {DISABLED AFTER UPDATE}
            }
                
            }
            //Insurance against the appearance of a laser in the center of coordinates!
            if (Laser.enabled == false && LaserSaver == false)
            {
                LaserSaver = true;
                Laser.enabled = true;
            }
        }  
    }

    public void DisablePrepare()
    {
        if (Laser != null)
        {
            Laser.enabled = false;
        }
        UpdateSaver = true;
        //Effects can = null in multiply shooting
        if (Effects != null)
        {
            foreach (var AllPs in Effects)
            {
                if (AllPs.isPlaying) AllPs.Stop();
            }
        }
    }
}
