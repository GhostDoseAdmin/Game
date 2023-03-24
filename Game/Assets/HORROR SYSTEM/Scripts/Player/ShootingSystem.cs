using InteractionSystem;
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

    public static ShootingSystem instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        curRateFire = rateOfFire;
        ammoCountUI.text = ammoCount.ToString("0");
        ammo—lipCountUI.text = ammoClipCount.ToString("0");
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
            GameObject myBullet = Instantiate(bullet);
            ammoClipCount--;
            //ammo—lipCountUI.text = ammoClipCount.ToString("0");
            myBullet.transform.position = shootPoint.position;
            myBullet.transform.rotation = shootPoint.rotation;
            Destroy(myBullet, shootFireLifeTime);
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
