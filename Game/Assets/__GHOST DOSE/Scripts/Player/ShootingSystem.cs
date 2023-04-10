using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
//using System.Numerics;
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
    private GameObject crosshairs;
    private Image enemyIndicatorUI;
    private Image headShotIndicatorUI;
    private Image flashLightIndicatorUI;
    private Image focusIndicatorUI;

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
    
    bool canShoot;
    bool AmmoShoot = true;

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

    public void CollectCartridges(int ammo)
    {
        ammoCount = ammoCount + ammo;
        ammoCountUI.text = ammoCount.ToString("0");
        AudioManager.instance.Play(pickUp);
    }

    public void Update()
    {
        CheckShoot();
        ReloadAmmo();
       
    }

    public void CheckShoot()
    {
        if (!playerController.canShoot)
            return;

        //shootPoint.LookAt(targetLook);

        if (curRateFire > 0)
        {
            curRateFire -= Time.deltaTime * 10;
            this.canShoot = false;
        }
        else
        {
            curRateFire = rateOfFire;
            this.canShoot = true;
        }
    }

    public void Aiming()
    {
        //RESET UI
        enemyIndicatorUI.color = UnityEngine.Color.white;
        headShotIndicatorUI.color = UnityEngine.Color.white;
        flashLightIndicatorUI.color = UnityEngine.Color.white;
        focusIndicatorUI.color = UnityEngine.Color.white;

        //TARGET PARAMS
        targetParams(20);


        if (isVisible)
        {
            if (target!=null) { enemyIndicatorUI.color = Color.red; }
            if (isHeadshot) { headShotIndicatorUI.color = Color.red; }
        }
        if (gameObject.GetComponent<FlashlightSystem>().FlashLight.isActiveAndEnabled || gameObject.GetComponent<FlashlightSystem>().WeaponLight.enabled) { flashLightIndicatorUI.color = UnityEngine.Color.yellow; }

        if (Mathf.Approximately(Mathf.Round(camera.fieldOfView * 10) / 10f, aiming.zoom)) { 
            focusIndicatorUI.color = UnityEngine.Color.yellow;
            //Animator animator = crosshairs.GetComponent<Animator>();
            //animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, 0f);
            //animator.Update(0f);
        }
        
        crosshairs.GetComponent<Animator>().speed = (camera.fieldOfView - aiming.zoom) * 3;//ANIMATE FOCUS INDICATOR
        //Debug.Log(isEnemy.ToString() + isVisible.ToString() + isHeadshot.ToString());
    }

    public bool Shoot()
    {
        //if (AmmoShoot)
        {
            //if (!this.canShoot)
            // return;



            //Debug.Log(isEnemy.ToString() + isVisible.ToString() + isHeadshot.ToString());
            //Debug.Log(camera.fieldOfView);


            //CAN SHOOT
            if (Mathf.Approximately(Mathf.Round(camera.fieldOfView * 10) / 10f, aiming.zoom))
            {


                AudioManager.instance.Play(shootSound);
                muzzleFlash.Play();
                Shell.Play();
                RaycastHit hit;
                if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, distance))
                {
                    if (hit.transform.GetComponent<Rigidbody>())
                    {
                        hit.transform.GetComponent<Rigidbody>().AddForceAtPosition(shootPoint.forward * force, hit.point);
                    }
                }

                if (isVisible && target!=null)
                {
                    target.GetComponent<NPCController>().TakeDamage(40);
                    if (isHeadshot) { target.GetComponent<NPCController>().TakeDamage(100); }
                    target.GetComponent<NPCController>().visible=15;
                }

                //--------------FLASH-----------------
               GameObject newFlash = Instantiate(camFlash);
                newFlash.transform.position = shootPoint.position;
                //---POINT FLASH IN DIRECTION OF THE SHOT
                Quaternion newYRotation = Quaternion.Euler(0f, shootPoint.rotation.eulerAngles.y, 0f);
                newFlash.transform.rotation = newYRotation;



                //EMIT SHOOT
                if (GetComponent<PlayerController>().GD.twoPlayer) { GetComponent<PlayerController>().GD.ND.sioCom.Instance.Emit("shoot", JsonConvert.SerializeObject($"{{'weapon':'camera'}}"), false); }


                camera.fieldOfView = 40;//40
            }
            



        }

        /*if (ammoClipCount <= 0)
        {
            AmmoShoot = false;

            if (!this.canShoot)
                return;
            AudioManager.instance.Play(noAmmo);
        }
        else
        {
            AmmoShoot = true;
        }*/

        return true;
    }
    public void targetParams(float distance)
    {
        isHeadshot = false;
        isVisible = false;
        //RETUNRS 1 for visible 2 for headshot
        LayerMask mask = 1 << LayerMask.NameToLayer("Default");
        RaycastHit hit;
        Vector3 startPoint = GameObject.Find("PlayerCamera").transform.position;
        Vector3 direction = (targetLook.position -startPoint).normalized;

        Debug.DrawLine(startPoint, startPoint + direction * distance, UnityEngine.Color.red);
        if (Physics.Raycast(startPoint, direction, out hit, distance, mask.value))
        {
            string ghostType = hit.collider.gameObject.transform.root.tag;
            //Debug.Log(hit.collider.gameObject.name);
            if (ghostType == "Ghost" || ghostType == "Shadower")
            {
                //Ensure mesh can be read
                //if (hit.collider.gameObject.transform.root.GetComponent<GhostVFX>() != null)
                {
                    isVisible = hit.collider.gameObject.transform.root.GetComponent<GhostVFX>().visible;
                    if (!isVisible) { Debug.Log("INVISISHOT"); }
                    if (hit.collider.gameObject.name == "mixamorig:Head") { isHeadshot = true;  }
                }
                target = hit.collider.gameObject.transform.root.gameObject;
            }
        }
    }
    

    
    
    
    
    void ReloadAmmo()
    {
        if (Input.GetKeyDown(InputManager.instance.reloadPistol) && ammoCount > 0 && ammoClipCount <= 0)
        {
            ammoClipCount = ammoClipCount + maxAmmoClipCount;
            ammo—lipCountUI.text = ammoClipCount.ToString("0");

            ammoCount = ammoCount - maxAmmoClipCount;
            ammoCountUI.text = ammoCount.ToString("0");

            AudioManager.instance.Play(reloadSound);
        }
    }
}
