using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShootingSystem : MonoBehaviour
{
    [Header("CAMERA PARAMETERS")]
    [Space(10)]
    public GameObject bullet;
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
    private bool[] targParams;
    private bool isEnemy;
    private bool isVisible;
    private bool isHeadshot;
    private Camera camera;
    private Aiming aiming;

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
        targParams = targetParams(20);
             isEnemy = targParams[0];
             isVisible = targParams[1];
             isHeadshot = targParams[2];
        
        if (isVisible)
        {
            if (isEnemy) { enemyIndicatorUI.color = UnityEngine.Color.red; }
            if (isHeadshot) { headShotIndicatorUI.color = UnityEngine.Color.red; }
        }
        if (gameObject.GetComponent<FlashlightSystem>().FlashLight.isActiveAndEnabled || gameObject.GetComponent<FlashlightSystem>().WeaponLight.enabled) { flashLightIndicatorUI.color = UnityEngine.Color.yellow; }

        if (Mathf.Approximately(Mathf.Round(camera.fieldOfView * 10) / 10f, aiming.zoom)) { 
            focusIndicatorUI.color = UnityEngine.Color.yellow;
            focusIndicatorUI.transform.localRotation = UnityEngine.Quaternion.Euler(0f, 0f, 90f);
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
                if (isVisible)
                {
                    GameObject myBullet = Instantiate(bullet);
                    //ammoClipCount--;
                    //ammo—lipCountUI.text = ammoClipCount.ToString("0");
                    myBullet.transform.position = shootPoint.position;
                    myBullet.transform.rotation = shootPoint.rotation;
                    Destroy(myBullet, shootFireLifeTime);
                }

                Flash();



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
    public bool[] targetParams(float distance)
    {
        //shootPoint.LookAt(targetLook);
        //Debug.Log("TARGET PARAMS");
        bool[] targParams = new bool[3];//[visible][head]
        targParams[0] = false;//enemy?
        targParams[1] = false;//visible?
        targParams[2] = false;//head?

        //RETUNRS 1 for visible 2 for headshot
        LayerMask mask = ~(1 << LayerMask.NameToLayer("ShadowReceiver")) & ~(1 << LayerMask.NameToLayer("ShadowBox"));
        RaycastHit hit;
        UnityEngine.Vector3 startPoint = GameObject.Find("PlayerCamera").transform.position;
        UnityEngine.Vector3 direction = (targetLook.position -startPoint).normalized;

        Debug.DrawLine(startPoint, startPoint + direction * distance, UnityEngine.Color.red);
        if (Physics.Raycast(startPoint, direction, out hit, distance, mask.value))
        {
            string ghostType = hit.collider.gameObject.transform.root.tag;
            Debug.Log(hit.collider.gameObject.name);
            if (ghostType == "Ghost" || ghostType == "Shadower")
            {
                targParams[0] = true;
                //Ensure mesh can be read
                //if (hit.collider.gameObject.transform.root.GetComponent<GhostVFX>() != null)
                {
                    targParams[1] = hit.collider.gameObject.transform.root.GetComponent<GhostVFX>().visible;
                    if (!targParams[1]) { Debug.Log("INVISISHOT"); }
                    if (hit.collider.gameObject.name == "mixamorig:Head") { targParams[2] = true;  }
                }
            }
        }
        return targParams;//no target
    }
    
    private void Flash()
    {

        //--------------------------FLASH-------------------------------------
        bool weapLightState = GetComponent<FlashlightSystem>().WeaponLight.enabled;
        GetComponent<FlashlightSystem>().WeaponLight.enabled = true;
        GetComponent<FlashlightSystem>().WeaponLight.spotAngle = 125;
        GetComponent<FlashlightSystem>().WeaponLight.intensity = 15;
        StartCoroutine(stopFlash());
        flash = true;
        IEnumerator stopFlash()
        {
            yield return new WaitForSeconds(0.2f);
            GetComponent<FlashlightSystem>().WeaponLight.spotAngle = GetComponent<FlashlightSystem>().WeaponLight.spotAngle = GetComponent<FlashlightSystem>().weapLightAngle;
            GetComponent<FlashlightSystem>().WeaponLight.intensity = GetComponent<FlashlightSystem>().WeaponLight.intensity = GetComponent<FlashlightSystem>().weapLightIntensity;
            GetComponent<FlashlightSystem>().WeaponLight.enabled = weapLightState;//was weaplight on previously
            flash = false;
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
