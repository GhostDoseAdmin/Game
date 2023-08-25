using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using GameManager;
using NetworkSystem;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class ShootingSystem : MonoBehaviour
{
    [Header("CAMERA PARAMETERS")]
    [Space(10)]
    public GameObject camFlash, remPodTarget;
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
    public GameObject planchette;
    public GameObject K2;
    private Image enemyIndicatorUI;
    private Image headShotIndicatorUI;
    private Image flashLightIndicatorUI;
    private Image focusIndicatorUI;
    public Image camBatteryUI, gridBatteryUI;

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
    public int headShotDamage = 101;
    //bool canShoot;
    //bool AmmoShoot = true;

    private bool flash;

    //TARGET
    //private bool[] targParams;
    public bool isVisible;
    public bool isHeadshot;
    private Camera camera;
    public Aiming aiming;
    public GameObject target, laserGrid;
    public RemPod remPod;
    public int Damage = 40;
    public bool shotGun = false;

    public static ShootingSystem instance;
    private static utilities util;
    //public int GEAR =1;
    private int range = 20;
    private int recoil = 40;
    public void RigShooter()
    {
        util = new utilities();
        laserGrid = GetComponentInChildren<laserGrid>().gameObject;
        laserGrid.SetActive(false);
        remPod = GetComponentInChildren<RemPod>();
        gridBatteryUI = GameObject.Find("grid_battery_ui").GetComponent<Image>();
        gridBatteryUI.transform.parent.gameObject.SetActive(false);
        camBatteryUI = GameObject.Find("cam_battery_ui").GetComponent<Image>();
        shootPoint = util.FindChildObject(this.gameObject.transform, "ShootPoint").transform;
        muzzleFlash = util.FindChildObject(this.gameObject.transform, "MuzzleFlashEffect").GetComponent<ParticleSystem>();
        Shell = util.FindChildObject(this.gameObject.transform, "Puff").GetComponent<ParticleSystem>();
        crosshairs = GameObject.Find("Crosshairs");
        planchette = GameObject.Find("planchette");
        K2 = GameObject.Find("K2Hud");
        enemyIndicatorUI = GameObject.Find("Vector-5-Image").GetComponent<Image>();
        headShotIndicatorUI = GameObject.Find("Dot").GetComponent<Image>();
        flashLightIndicatorUI = GameObject.Find("Vector-3-Image").GetComponent<Image>();
        focusIndicatorUI = GameObject.Find("Vector-4-Image").GetComponent<Image>();
        camera = transform.parent.Find("PlayerCamera").GetComponent<Camera>();
        aiming = transform.parent.Find("PlayerCamera").GetComponent<Aiming>();
        planchette.SetActive(false);


    }


    void Start()
    {
        headShotDamage = 101;
        curRateFire = rateOfFire;
        //ammoCountUI.text = ammoCount.ToString("0");
        //ammo—lipCountUI.text = ammoClipCount.ToString("0");
    }

    public void Update()
    {
        //UPDATE AIMER
        aiming.gear = GetComponent<PlayerController>().gear;

        //CheckShoot();
        ReloadAmmo();
        if (GetComponent<Animator>().GetBool("ouija"))
        {
            aiming.isOuija = true;
            crosshairs.SetActive(false);
        }
        else { aiming.isOuija = false; }
       
    }
    public void SwitchGear()
    {
        range = 20; 
        recoil = 40; //based on the zoom value of field angle
        camBatteryUI.transform.parent.gameObject.SetActive(true);
        gridBatteryUI.transform.parent.gameObject.SetActive(false);
        laserGrid.SetActive(false);
        if (GetComponent<PlayerController>().gear == 4) { camBatteryUI.transform.parent.gameObject.SetActive(false); range = 10; gridBatteryUI.transform.parent.gameObject.SetActive(true); laserGrid.SetActive(true); recoil = 75; }
    }


    public void Aiming()
    {


        if (GetComponent<PlayerController>().gear == 1 || GetComponent<PlayerController>().gear == 4)
        {
            //RESET UI
            enemyIndicatorUI.color = Color.white;
            headShotIndicatorUI.color = Color.white;
            flashLightIndicatorUI.color = Color.white;
            focusIndicatorUI.color = Color.white;

            //MOBILE TARGET PARAMS
            /* if (NetworkDriver.instance.isMobile)
             {
                 RayAimer aimer = GetComponent<PlayerController>().gamePad.aimer;
                 target = aimer.target;
                 isHeadshot = aimer.isHeadshot;
             }*/
            if (!NetworkDriver.instance.isMobile)
            {
                //TARGET PARAMS
                targetParams(range); //GetComponent<FlashlightSystem>().FlashLight.range
            }
            else { targetParamsMobile(range); }
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

            if (!NetworkDriver.instance.isMobile) { crosshairs.GetComponent<Animator>().speed = (camera.fieldOfView - aiming.zoom) * 3; }//ANIMATE FOCUS INDICATOR
        }
        //REM POD
        if (GetComponent<PlayerController>().gear == 3)
        {

            if (!GetComponent<PlayerController>().throwing)
            {
                LayerMask mask = LayerMask.GetMask("Default"); // Use LayerMask.GetMask to create a LayerMask
                RaycastHit hit;
                Vector3 startPoint = GameObject.Find("PlayerCamera").transform.position;
                Vector3 direction = (targetLook.position - startPoint).normalized;

                Debug.DrawLine(startPoint, startPoint + direction * 10, Color.red);

                if (Physics.Raycast(startPoint, direction, out hit, 10, mask))
                {
                    Vector3 newPosition;
                    //Debug.Log("--------------------------HITTING OBJECT " + hit.collider.name);
                    if (hit.collider != null)
                    {
                        // Make sure to calculate the position relative to the remPod's transform
                        //newPosition = remPod.transform.InverseTransformPoint(hit.point);
                        newPosition = hit.point;
                    }
                    else
                    {//fallback to end of ray
                        newPosition = direction * 10;

                    }
                    remPod.remPodTarget.transform.position = newPosition;

                }
            }
        }

        //Debug.Log(isEnemy.ToString() + isVisible.ToString() + isHeadshot.ToString());
    }

    private float shootTimer;
    private float shootCoolDown = 0.7f;
    public bool canShoot;
    public bool Shoot()
    {
        //if (AmmoShoot)
        {
            //Debug.Log("TARGET " + target);
            //---------------------CAN SHOOT------------------------------------
            //if (Mathf.Approximately(Mathf.Round(camera.fieldOfView * 10) / 10f, aiming.zoom))//if current zoom is close to target zoom
            if (GetComponent<PlayerController>().gear == 1) { shootCoolDown = 0.7f; }//CAMERA PROPERTIES
            if (GetComponent<PlayerController>().gear == 4) { shootCoolDown = 1f; } //LASER GRID

            
                //OUT OF BATTERY
                if(Time.time > shootTimer + shootCoolDown && ((camBatteryUI.fillAmount <= 0 && GetComponent<PlayerController>().gear == 1) || (gridBatteryUI.fillAmount <= 0 && GetComponent<PlayerController>().gear == 4)))
                {
                    string audioString;
                    if (NetworkDriver.instance.isTRAVIS) { audioString = "travbattery"; }
                    else { audioString = "wesbattery"; }
                    AudioManager.instance.Play(audioString, GameDriver.instance.Player.GetComponent<PlayerController>().audioSourceSpeech);
                    shootTimer = Time.time;//cooldown
                }


                if (Time.time > shootTimer + shootCoolDown && ((camBatteryUI.fillAmount > 0 && GetComponent<PlayerController>().gear == 1) || (gridBatteryUI.fillAmount > 0 && GetComponent<PlayerController>().gear == 4)))
                {
                    canShoot = true;
                    GameObject victimManager = GameObject.Find("OuijaBoardManager").GetComponent<OuijaSessionControl>().OuijaSessions[GameObject.Find("OuijaBoardManager").GetComponent<OuijaSessionControl>().currentSession];
                    //AudioManager.instance.Play("ShotCam");
                    if (GetComponent<PlayerController>().gear == 1) { camBatteryUI.fillAmount -= 0.1f; }
                    if (GetComponent<PlayerController>().gear == 4) { gridBatteryUI.fillAmount -= 0.2f; }
                    muzzleFlash.Play();
                    Shell.Play();
                    //DO DAMAGE
                    if (GetComponent<PlayerController>().gear != 4 && GetComponent<PlayerController>().gear != 3) {
                        if (target != null)
                        {
                            if ((target.GetComponent<GhostVFX>() != null) && target.GetComponent<Teleport>().teleport == 0) // && isVisible 
                            {
                                if (target.GetComponent<NPCController>().animEnemy.GetCurrentAnimatorClipInfo(0).Length > 0 && target.GetComponent<NPCController>().animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name == "agro") { Damage = 20; }
                                if (isHeadshot) { Damage = headShotDamage;
                                    if (NetworkDriver.instance.isMobile && !NetworkDriver.instance.TWOPLAYER) { Damage = headShotDamage * 2; }
                                }
                                //Debug.Log("---------------------------------------" + damage);
                                target.GetComponent<NPCController>().TakeDamage(Damage, false);
                            }
                            if (target != null && target.tag == "Victim")
                            {
                                victimManager.GetComponent<VictimControl>().testAnswer(target);
                                //used to emit answer
                                Damage = -1;
                            }

                        }
                        string targName = "";
                        if (target != null) { targName = target.name; }
                        if (NetworkDriver.instance.TWOPLAYER) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { shoot = true, obj = targName, dmg = Damage }), false); }
                    }

                    if(GetComponent<PlayerController>().gear == 1)
                    {
                        //--------------FLASH-----------------
                        GameObject newFlash = Instantiate(camFlash);
                        newFlash.transform.position = shootPoint.position;
                        newFlash.name = "CamFlashPlayer";
                        //---POINT FLASH IN DIRECTION OF THE SHOT
                        Quaternion newYRotation = Quaternion.Euler(0f, shootPoint.rotation.eulerAngles.y, 0f);
                        newFlash.transform.rotation = newYRotation;
                    }
                    if(GetComponent<PlayerController>().gear == 4)
                    {

                        laserGrid.GetComponent<laserGrid>().Shoot(false); //SetActive(true);
                    }

                    camera.fieldOfView = recoil;//40
                    shootTimer = Time.time;//cooldown

                }
                else { 
                    canShoot = false;


                }

            
            



        }

        return true;
    }
    public void targetParams(float distance)
    {
        //Debug.Log("----------------------------------TARGET PARAMS " );
        isHeadshot = false;
        isVisible = false;
        target = null;
        Damage = 40;
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
                        if (ghost.GetComponent<GhostVFX>().Shadower) { isVisible = true; } //ghost.GetComponent<GhostVFX>().visible;
                        //if (!isVisible) { Debug.Log("INVISISHOT"); }
                        if (hit.collider.gameObject.name == "mixamorig:Head") { isHeadshot = true; }
                    }

                    if (ghost.GetComponent<NPCController>().outline.OutlineWidth > 0f)
                    {
                        isVisible = true;
                    }
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
    public void targetParamsMobile(float distance)
    {
        isHeadshot = false;
        target = null;
        isVisible = false;
        GameObject targ = EasyTarget(distance);
        if (GetComponent<PlayerController>().gamePad.camSup.AIMMODE)
        {
            Damage = 60;
            RectTransform rect = headShotIndicatorUI.GetComponent<RectTransform>();//CENTER OF CROSSHAIR
            
            //CROSSHAIR TARGET
            if (targ != null)
            {
                Vector3 targetScreenPos = Camera.main.WorldToScreenPoint(targ.GetComponentInParent<Animator>().GetBoneTransform(HumanBodyBones.Hips).transform.position) + (Vector3.up * 0.3f);
                Vector2 targetScreenPoint = new Vector2(targetScreenPos.x, targetScreenPos.y);

                float proximity = Vector2.Distance(targetScreenPoint, new Vector2(rect.transform.position.x, rect.transform.position.y));

                if (proximity < (aiming.crosshair.GetComponent<RectTransform>().rect.width / 2)){

                    target = targ;

                        if (target.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                        {
                                if (!target.GetComponent<GhostVFX>().Shadower) { isVisible = !target.GetComponent<GhostVFX>().invisible; }
                                if (target.GetComponent<GhostVFX>().Shadower) { isVisible = true; }
                            if (target.GetComponent<NPCController>().outline.OutlineWidth > 0f){isVisible = true;}
                        }
                
                }
            }
            //RAY TARGET by HEADSHOT
            LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Enemy") | 1 << LayerMask.NameToLayer("Ghost");
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(rect.transform.position.x, rect.transform.position.y));
            Debug.DrawRay(ray.origin, ray.direction * distance, Color.green);
            if (Physics.Raycast(ray, out hit, distance, mask.value))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Default")) { return; }
                if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    if (hit.collider.gameObject.GetComponentInParent<Teleport>() != null && hit.collider.gameObject.GetComponentInParent<Teleport>().teleport != 0) { return; }
                    if (hit.collider.gameObject.GetComponentInParent<NPCController>().dead) { return; }
                    //if (!hit.collider.gameObject.GetComponentInParent<GhostVFX>().Shadower && hit.collider.gameObject.GetComponentInParent<GhostVFX>().invisible) { return; }
                }
                if (hit.collider.gameObject.name == "mixamorig:Head") { isHeadshot = true; target = hit.collider.GetComponentInParent<Animator>().gameObject; }
            }

        }
        else//FALL BACK TO EASYTARGET
        {
            Damage = 100;
            target = targ;
        }
    }

    public GameObject EasyTarget(float dist)
    {
        List<NPCController> enemies = new List<NPCController>(FindObjectsOfType<NPCController>());
        List<Person> victims = new List<Person>(FindObjectsOfType<Person>());

        List<GameObject> targets = new List<GameObject>();

        foreach (NPCController enemy in enemies)
        {
            targets.Add(enemy.gameObject);
        }

        foreach (Person victim in victims)
        {
            targets.Add(victim.gameObject);
        }

        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            float targetDistance = Vector3.Distance(transform.position, target.transform.position);
            if (targetDistance < dist)
            {
                if (targetDistance < closestDistance)
                {
                    RaycastHit hit; //if the line between player and enemy hits a default object
                    if (Physics.Linecast(shootPoint.transform.position, target.GetComponentInParent<Animator>().GetBoneTransform(HumanBodyBones.Hips).transform.position, out hit, 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Enemy") | 1 << LayerMask.NameToLayer("Ghost")))
                    {
                        if (hit.collider != null)
                        {
                            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Default")) { continue; }
                            
                            float angleThreshold = 30f; // Set the angle threshold in degrees
                            if (!GetComponent<PlayerController>().gamePad.camSup.AIMMODE) { angleThreshold = 60f; }//in platform mode
                            Vector3 targetDirection = target.transform.position - transform.position;
                            float angle = Vector3.Angle(transform.forward, targetDirection);

                            bool facingTarget = angle < angleThreshold;
                            if (facingTarget)
                            {
                                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                                {
                                    if (hit.collider.gameObject.GetComponentInParent<Teleport>() != null && hit.collider.gameObject.GetComponentInParent<Teleport>().teleport != 0) { continue; }
                                    if (hit.collider.gameObject.GetComponentInParent<NPCController>().dead) { continue; }

                                    closestTarget = target; closestDistance = targetDistance;

                                    /*if (!hit.collider.gameObject.GetComponentInParent<GhostVFX>().Shadower)//GHOST
                                    {
                                        if (!hit.collider.gameObject.GetComponentInParent<GhostVFX>().invisible) { 
                                            closestTarget = target; closestDistance = targetDistance; 
                                        }
                                        
                                    }
                                    else { closestTarget = target; closestDistance = targetDistance; }//shadower
                                    */


                                }
                                else //VICTIM
                                {
                                    if (hit.collider.gameObject.transform.position.y > transform.position.y)
                                    {
                                        closestTarget = target;
                                        closestDistance = targetDistance;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
       // Debug.Log(closestTarget.name);
        return closestTarget;
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
