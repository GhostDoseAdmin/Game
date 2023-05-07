using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using GameManager;
using NetworkSystem;
using UnityEngine;
using UnityEngine.UI;

public class ShootingSystem : MonoBehaviour
{
    [Header("CAMERA PARAMETERS")]
    [Space(10)]
    public GameObject camFlash;
    public Transform shootPoint;
    public Transform targetLook;
    public float distance;
    public float force;
    public float shootFireLifeTime;
    [SerializeField] private float rateOfFire;
    [SerializeField] private PlayerController playerController;

    [Header("AMMO PARAMETERS")]
    [Space(10)]
    [SerializeField] public int ammoCount = 1;
    [SerializeField] public int ammoClipCount = 1;
    [SerializeField] public int maxAmmoClipCount = 1;

    [Header("UI")]
    [Space(10)]
    [SerializeField] private Text ammoCountUI = null;
    [SerializeField] private Text ammo—lipCountUI = null;
    public GameObject crosshairs;
    public GameObject K2;
    private Image enemyIndicatorUI;
    private Image headShotIndicatorUI;
    private Image flashLightIndicatorUI;
    private Image focusIndicatorUI;
    public Image camBatteryUI;

    [Header("PISTOL SOUNDS")]
    [Space(10)]
    [SerializeField] private string pickUp;
    [SerializeField] private string shootSound;
    [SerializeField] private string reloadSound;
    [SerializeField] private string noAmmo;

    [Header("VFX")]
    [Space(10)]
    public ParticleSystem muzzleFlash;
    public ParticleSystem Shell;
    
    float curRateFire;
    
    //bool canShoot;
    //bool AmmoShoot = true;

    private bool flash;

    //TARGET
    //private bool[] targParams;
    private bool isVisible;
    private bool isHeadshot;
    private Camera camera;
    private Aiming aiming;
    private GameObject target;

    public static ShootingSystem instance;
    private static utilities util;

    public void RigShooter()
    {
        util = new utilities();

        shootPoint = util.FindChildObject(this.gameObject.transform, "ShootPoint").transform;
        muzzleFlash = util.FindChildObject(this.gameObject.transform, "MuzzleFlashEffect").GetComponent<ParticleSystem>();
        Shell = util.FindChildObject(this.gameObject.transform, "Puff").GetComponent<ParticleSystem>();
        crosshairs = GameObject.Find("Crosshairs");
        K2 = GameObject.Find("K2Hud");
        enemyIndicatorUI = GameObject.Find("Vector-5-Image").GetComponent<Image>();
        headShotIndicatorUI = GameObject.Find("Dot").GetComponent<Image>();
        flashLightIndicatorUI = GameObject.Find("Vector-3-Image").GetComponent<Image>();
        focusIndicatorUI = GameObject.Find("Vector-4-Image").GetComponent<Image>();
        camera = transform.parent.Find("PlayerCamera").GetComponent<Camera>();
        aiming = transform.parent.Find("PlayerCamera").GetComponent<Aiming>();
    }


    void Start()
    {
        curRateFire = rateOfFire;
        //ammoCountUI.text = ammoCount.ToString("0");
        //ammo—lipCountUI.text = ammoClipCount.ToString("0");
    }

    public void Update()
    {
        //CheckShoot();
        ReloadAmmo();
       
    }

    public void Aiming(int gear)
    {
        //UPDATE AIMER
        aiming.gear = gear;

        if (gear == 1)
        {
            //RESET UI
            enemyIndicatorUI.color = Color.white;
            headShotIndicatorUI.color = Color.white;
            flashLightIndicatorUI.color = Color.white;
            focusIndicatorUI.color = Color.white;

            //TARGET PARAMS
            targetParams(20); //GetComponent<FlashlightSystem>().FlashLight.range
            if (isVisible)
            {
                if (target != null) { 
                    if(((target.GetComponent<GhostVFX>()!=null) && target.GetComponent<Teleport>()!=null && target.GetComponent<Teleport>().teleport == 0) || (target.tag=="Victim"))
                    {
                        enemyIndicatorUI.color = Color.red;
                        if (isHeadshot) { headShotIndicatorUI.color = Color.red; }
                    }


                }
                
            }
            if (gameObject.GetComponent<FlashlightSystem>().FlashLight.isActiveAndEnabled || gameObject.GetComponent<FlashlightSystem>().WeaponLight.enabled) { flashLightIndicatorUI.color = Color.yellow; }

            if (Mathf.Approximately(Mathf.Round(camera.fieldOfView * 10) / 10f, aiming.zoom))
            {
                focusIndicatorUI.color = Color.yellow;
                //Animator animator = crosshairs.GetComponent<Animator>();
                //animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, 0f);
                //animator.Update(0f);
            }

            crosshairs.GetComponent<Animator>().speed = (camera.fieldOfView - aiming.zoom) * 3;//ANIMATE FOCUS INDICATOR
        }
        if (gear == 2)
        {

        }

        //Debug.Log(isEnemy.ToString() + isVisible.ToString() + isHeadshot.ToString());
    }

    private float shootTimer;
    private float shootCoolDown = 0.7f;
    public bool Shoot()
    {
        //if (AmmoShoot)
        {
            Debug.Log("TARGET " + target);
            //---------------------CAN SHOOT------------------------------------
            //if (Mathf.Approximately(Mathf.Round(camera.fieldOfView * 10) / 10f, aiming.zoom))//if current zoom is close to target zoom
            {

                if (Time.time > shootTimer + shootCoolDown && camBatteryUI.fillAmount>0)
                {
                    GameObject victimManager = GameObject.Find("OuijaBoardManager").GetComponent<OuijaSessionControl>().OuijaSessions[GameObject.Find("OuijaBoardManager").GetComponent<OuijaSessionControl>().currentSession];
                    //AudioManager.instance.Play("ShotCam");
                    camBatteryUI.fillAmount -= 0.1f;
                    muzzleFlash.Play();
                    Shell.Play();
                    int damage = 0;
                    GetComponent<PlayerController>().emitShoot = true;
                    //if (target.tag == "Victim") { GetComponent<PlayerController>().emitShoot = false; }
                    //DO DAMAGE
                        if (target != null)
                        {
                        if ((target.GetComponent<GhostVFX>()!=null) && isVisible && target.GetComponent<Teleport>().teleport == 0)
                        {
                            damage = 40;
                            if (target.GetComponent<NPCController>().animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip != null && target.GetComponent<NPCController>().animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "agro") { damage = 20; }
                            if (isHeadshot) { damage = 100; }
                            if (damage >= target.GetComponent<NPCController>().healthEnemy) { GetComponent<PlayerController>().emitKill = true; }
                            target.GetComponent<NPCController>().TakeDamage(damage, false);
                            GetComponent<PlayerController>().shotName = target.name;
                            GetComponent<PlayerController>().shotDmg = damage;
                        }
                        if (target.tag == "Victim")
                        {
                            victimManager.GetComponent<VictimControl>().testAnswer(target);
                            //used to emit answer
                            damage = -1;
                            GetComponent<PlayerController>().shotName = target.name;
                            GetComponent<PlayerController>().shotDmg = damage;
                        }
                    }

                    //--------------FLASH-----------------
                    GameObject newFlash = Instantiate(camFlash);
                    newFlash.transform.position = shootPoint.position;
                    newFlash.name = "CamFlashPlayer";
                    //---POINT FLASH IN DIRECTION OF THE SHOT
                    Quaternion newYRotation = Quaternion.Euler(0f, shootPoint.rotation.eulerAngles.y, 0f);
                    newFlash.transform.rotation = newYRotation;

                    camera.fieldOfView = 40;//40
                    shootTimer = Time.time;//cooldown
                  
                }

            }
            



        }

        return true;
    }
    public void targetParams(float distance)
    {
        isHeadshot = false;
        isVisible = false;
        target = null;
        //RETUNRS 1 for visible 2 for headshot
        LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Enemy") | 1 << LayerMask.NameToLayer("Ghost");
        RaycastHit hit;
        Vector3 startPoint = GameObject.Find("PlayerCamera").transform.position;
        Vector3 direction = (targetLook.position -startPoint).normalized;

        Debug.DrawLine(startPoint, startPoint + direction * distance, Color.red);
        if (Physics.Raycast(startPoint, direction, out hit, distance, mask.value))
        {
           
            NPCController ghost = hit.collider.gameObject.GetComponentInParent<NPCController>();

            if (ghost != null)
            {
                if (ghost.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    //Ensure mesh can be read
                    if (ghost.GetComponent<NPCController>().healthEnemy>0 && ghost.GetComponent<Teleport>().teleport == 0)
                    {
                        if (!ghost.GetComponent<GhostVFX>().Shadower) { isVisible = !ghost.GetComponent<GhostVFX>().invisible; }
                        else { isVisible = ghost.GetComponent<GhostVFX>().visible; }
                        if (!isVisible) { Debug.Log("INVISISHOT"); }
                        if (hit.collider.gameObject.name == "mixamorig:Head") { isHeadshot = true; }
                    }
                    //VICTIMS
                    target = ghost.gameObject;
                }
            }
            //VICTIM
            if (hit.collider.gameObject.tag == "Victim")
            {
                isVisible = true;
                isHeadshot = true;
                target = hit.collider.gameObject.GetComponentInParent<Person>().gameObject;
            }

        }
    }
    GameObject FindEnemyMain(Transform head)
    {
        Transform currentTransform = head;
        while (currentTransform != null)
        {
            if (currentTransform.GetComponent<NPCController>() != null)
            {
                //Debug.Log("Found parent with Person component: " + currentTransform.name);
                return currentTransform.gameObject;
            }
            currentTransform = currentTransform.parent;
        }
        return null;

    }

 
    
    
    
    void ReloadAmmo()
    {
        if (Input.GetKeyDown(InputManager.instance.reloadPistol) && ammoCount > 0 && ammoClipCount <= 0)
        {
            ammoClipCount = ammoClipCount + maxAmmoClipCount;
            ammo—lipCountUI.text = ammoClipCount.ToString("0");

            ammoCount = ammoCount - maxAmmoClipCount;
            ammoCountUI.text = ammoCount.ToString("0");

            AudioManager.instance.Play(reloadSound, gameObject.GetComponent<PlayerController>().audioSource);
        }
    }
}
