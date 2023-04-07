using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootingSystem : MonoBehaviour
{
    [Header("PISTOL PARAMETERS")]
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

    [Header("AMMO UI")]
    [Space(10)]
    [SerializeField] private Text ammoCountUI = null;
    [SerializeField] private Text ammo—lipCountUI = null;

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

    Vector3 startPos;
    Vector3 startRot;

    private bool flash;

    public static ShootingSystem instance;

    private static utilities util;

    public void RigShooter()
    {
        util = new utilities();

        shootPoint = util.FindChildObject(this.gameObject.transform, "ShootPoint").transform;
        muzzleFlash = util.FindChildObject(this.gameObject.transform, "MuzzleFlashEffect").GetComponent<ParticleSystem>();
        Shell = util.FindChildObject(this.gameObject.transform, "Puff").GetComponent<ParticleSystem>();
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

        shootPoint.LookAt(targetLook);

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

    public bool Shoot()
    {
        //if (AmmoShoot)
        {
            //if (!this.canShoot)
            // return;

            //DETERMINE TARG PARAMS
            bool[] targParams = targetParams(100);
            bool isEnemy = targParams[0];
            bool isVisible = targParams[1];
            bool isHeadshot = targParams[2];

            //Debug.Log(isEnemy.ToString() + isVisible.ToString() + isHeadshot.ToString());

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
           GameObject.Find("GameController").GetComponent<GameDriver>().ND.sioCom.Instance.Emit("shoot", JsonConvert.SerializeObject($"{{'weapon':'camera'}}"), false);
           
           



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
    private bool[] targetParams(float distance)
    {
        bool[] targParams = new bool[3];//[visible][head]
        targParams[0] = false;//enemy?
        targParams[1] = false;//visible?
        targParams[2] = false;//head?

        //RETUNRS 1 for visible 2 for headshot
        LayerMask mask = ~(1 << LayerMask.NameToLayer("ShadowReceiver")) & ~(1 << LayerMask.NameToLayer("ShadowBox"));
        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, distance, mask.value))
        {
            string ghostType = hit.collider.gameObject.transform.root.tag;

            if (ghostType == "Ghost" || ghostType == "Shadower")
            {
                targParams[0] = true;
                //Ensure mesh can be read
                //if (hit.collider.gameObject.transform.root.GetComponent<GhostVFX>() != null)
                {
                    targParams[1] = hit.collider.gameObject.transform.root.GetComponent<GhostVFX>().visible;
                    if (!targParams[1]) { Debug.Log("INVISISHOT"); }
                    if (hit.collider.gameObject.name == "mixamorig:Head") { targParams[2] = true; Debug.Log("HEADSHOT"); }
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
