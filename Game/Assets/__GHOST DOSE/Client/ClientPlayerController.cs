using InteractionSystem;
using UnityEngine;
using GameManager;
using Unity.VisualScripting;
using UnityEditor;
using NetworkSystem;

public class ClientPlayerController : MonoBehaviour
{
	[Header("PLAYER PARAMETRS")]
	[Space(10)]
	public float lookIKWeight;
	public float bodyWeight;
	public Transform targetPos;//determined by other player
	public float angularSpeed;
	bool isPlayerRot;
	public float luft;
    public int hp = 100;

    [Header("HAND PARAMETRS")]
	[Space(10)]
    [HideInInspector] public Transform rightHandTarget;
    [HideInInspector] public Transform rightHand;


    [HideInInspector] public Transform leftHandTarget;
    [HideInInspector] public Transform leftHand;


    [HideInInspector] public Transform rightHandTargetCam;
    [HideInInspector] public Transform rightHandTargetK2;
    [HideInInspector] public Transform rightHandTargetREM;
    [HideInInspector] public Transform leftHandTargetCam;
    [HideInInspector] public Transform leftHandTargetK2;
    [HideInInspector] public Transform leftHandTargetREM;



    float handWeight;
	public float handSpeed;

	float newHandWeight = 0f;

	[Header("INVENTORY BOOL")]
	[Space(10)]
	public bool is_Flashlight = false;
	public bool is_FlashlightAim = false;

	public bool canShoot { get; private set; }

	[Space(10)]


	[Header("PLAYER SOUND")]
	[Space(10)]
	[SerializeField] private string getFrom;

	Animator anim;

	public Vector3 targetPosVec;
	public float walk = 0f;
	public float strafe = 0f;
    //public float targStrafe = 0f;
    //public float targWalk = 0f;

    //-------GEAR
    private float gear_delay = 1f;//0.25
    private float gear_timer = 0.0f;//USED FOR EMITS
    public int gear = 1; //0 = cam 1=ks 2=rem
    public bool gearAim;
    public GameObject k2;
    public GameObject camera;
    private GameObject camInventory;
    private GameObject k2Inventory;
    private GameObject ouija;
    public bool changingGear;
    public bool throwing = false;


    //NETWORKER
    public string animation;
	public bool state = false;
	public float Float;
	public Vector3 destination;
	public float speed;
    public int dodge;
	//public bool running;
	//public Vector3 targetRotation;
	//public bool toggleFlashlight = false;//command sent from other player to turn on/off flashlight
	public bool aim = false;
	public bool triggerShoot;
    //private NetworkDriver ND;
	

    [Header("SHOOTING")]
    [Space(10)]
    [SerializeField] private string shootSound;
    [SerializeField] private string reloadSound;
    public ParticleSystem muzzleFlash;
    public ParticleSystem Shell;
    //public Transform shootPoint;
    public float distance;
    public GameObject bullet;
    public float shootFireLifeTime;
    public float force;
    public Transform shootPoint;
	public bool flashlighton =false;
    public GameObject currLight;//tracks current light source
	public GameObject camFlash, laserGridProj;
    private GameObject SB7, laserGrid;
    public GameObject death;
    public float targWalk;
    public float targStrafe;
    public bool running;
    public AudioSource audioSource;
    public AudioSource audioSourceSpeech;
    private string currentAni;
    public bool isTravis;
    public bool canFlinch;
    //public bool wlOn, flOn;//weaplight and flashlight


	private static utilities util;


    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;

        audioSourceSpeech = gameObject.AddComponent<AudioSource>();
        audioSourceSpeech.spatialBlend = 1.0f;
        canFlinch = true;
    }

    #region Start


	public void SetupRig()
	{
        util = new utilities();

        //Debug.Log("Setting up Player References");
        laserGrid = GetComponentInChildren<laserGrid>().gameObject;
        rightHandTargetCam = util.FindChildObject(this.gameObject.transform, "RHTargetCam").transform;
        rightHandTargetK2 = util.FindChildObject(this.gameObject.transform, "RHTargetK2").transform;
        rightHandTargetREM = util.FindChildObject(this.gameObject.transform, "RHTargetREM").transform;
        rightHand = util.FindChildObject(this.gameObject.transform, "mixamorig:RightHand").transform;
        leftHandTargetCam = util.FindChildObject(this.gameObject.transform, "LHTargetCam").transform;
        leftHandTargetK2 = util.FindChildObject(this.gameObject.transform, "LHTargetK2").transform;
        leftHandTargetREM = util.FindChildObject(this.gameObject.transform, "LHTargetREM").transform;
        leftHand = util.FindChildObject(this.gameObject.transform, "mixamorig:LeftHand").transform;
        k2 = util.FindChildObject(this.gameObject.transform, "K2").gameObject;
        camera = util.FindChildObject(this.gameObject.transform, "camera").gameObject;
        camInventory = util.FindChildObject(this.gameObject.transform, "CamInventory").gameObject;
        k2Inventory = util.FindChildObject(this.gameObject.transform, "K2Inventory").gameObject;
        ouija = util.FindChildObject(this.gameObject.transform, "Ouija").gameObject;
        ouija.SetActive(false);
        SB7 = util.FindChildObject(this.gameObject.transform, "SB7").gameObject;
        SB7.SetActive(false);
        camInventory.SetActive(false);


        shootPoint = util.FindChildObject(this.gameObject.transform, "ShootPoint").transform;
        muzzleFlash = util.FindChildObject(this.gameObject.transform, "MuzzleFlashEffect").GetComponent<ParticleSystem>();
        Shell = util.FindChildObject(this.gameObject.transform, "Puff").GetComponent<ParticleSystem>();
        k2.SetActive(false);
        GetComponent<ClientFlashlightSystem>().RigLights();

        //ND = GameObject.Find("GameController").GetComponent<GameDriver>().ND;

        anim = GetComponent<Animator>();
    }

        #endregion

        private void FixedUpdate()
        {
		    if (!NetworkDriver.instance.TWOPLAYER) { return; }

            //------------------------------------- M A I N ---------------------------------------------------
            targetPosVec = Vector3.Lerp(targetPosVec, targetPos.position, 0.1f);//0.1
       
            if (running) { speed = 4f; } else { speed = 2f; }
            if (strafe == 0 && walk == 0) { speed = 0f; }
        //-------------------ANIMATION----------------------------
            strafe = Mathf.Lerp(strafe, targStrafe, 0.1f);
            walk = Mathf.Lerp(walk, targWalk, 0.1f);

                anim.SetFloat("Strafe", strafe);
                anim.SetFloat("Walk", walk);
                if (NetworkDriver.instance.isMobile) { anim.SetFloat("Strafe", 0); anim.SetFloat("Walk", Mathf.Max(Mathf.Abs(walk), Mathf.Abs(strafe))); }
			    if (speed>=4f) { anim.SetBool("Running", true);  }
			    else { anim.SetBool("Running", false); }

        //----------DODGE
        if (dodge > 0) { anim.Play("dodgeRightAni");  }
        if (dodge < 0) { anim.Play("dodgeLeftAni");  }
        dodge = 0;


        //-------------------MOVEMENT---------------------------
        if (anim.GetCurrentAnimatorClipInfo(0).Length>0 && anim.GetCurrentAnimatorClipInfo(0)[0].clip.name!="React")
        {
            if (Vector3.Distance(transform.position, destination) > 2) { transform.position = new Vector3(destination.x, destination.y, destination.z); }
            float distance = Vector3.Distance(transform.position, destination);
            float timeToTravel = distance / (speed + 0.00001f);
            if (!NetworkDriver.instance.isMobile) { transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime / timeToTravel); }
            else { if (speed > 0) { transform.position = Vector3.Lerp(transform.position, destination, 0.1f); } }
        }


        //--------------------ROTATION--------------------------------------
        if (!NetworkDriver.instance.isMobile)
        {
            if (walk != 0 || strafe != 0 || is_FlashlightAim == true || gearAim == true)
            {
                Vector3 rot = transform.eulerAngles;
                transform.LookAt(targetPosVec);

                float angleBetween = Mathf.DeltaAngle(transform.eulerAngles.y, rot.y);
                if ((Mathf.Abs(angleBetween) > luft) || strafe != 0)
                {
                    isPlayerRot = true;
                }
                if (isPlayerRot == true)
                {
                    float bodyY = Mathf.LerpAngle(rot.y, transform.eulerAngles.y, Time.deltaTime * angularSpeed);
                    transform.eulerAngles = new Vector3(0, bodyY, 0);
                }
                else
                {
                    transform.eulerAngles = new Vector3(0f, rot.y, 0f);
                }
            }
            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
        }
        else { transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(targetPosVec - transform.position).eulerAngles.y, 0f); }
    }


    #region Update
    void Update() 
	{
        if (anim.GetCurrentAnimatorClipInfo(0).Length > 0) { currentAni = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name; }
        if (currentAni != "dodgeRightAni" && currentAni != "dodgeLeftAni")
        {
            Flashlight();
            Attack();
        }
        else
        {
            gearAim = false;
            anim.SetBool("Pistol", false);
        }

        //SET CURRENT LIGHT SOURCE FOR CLIENT
        if (GetComponent<ClientFlashlightSystem>().FlashLight.GetComponent<Light>().enabled) { currLight = GetComponent<ClientFlashlightSystem>().FlashLight.gameObject; }
        else if (GetComponent<ClientFlashlightSystem>().WeaponLight.enabled) { currLight = GetComponent<ClientFlashlightSystem>().WeaponLight.gameObject; }

       

    }
    #endregion

    public void TriggerCanFlinch()
    {
        canFlinch = !canFlinch;
    }

    public void ChangeGear(int nextGear)
	{

        gear = nextGear;
        //START OF GEARCHANGE
        anim.SetBool("GetGear", true);
        k2.SetActive(false); camera.SetActive(false); camInventory.SetActive(false); k2Inventory.SetActive(false); SB7.SetActive(false); laserGrid.SetActive(false);
        if (gear == 0) { SB7.SetActive(true); }
        if (gear == 1) { camera.SetActive(true); }
        if (gear == 2) { k2.SetActive(true); }
        if (gear == 4) { laserGrid.SetActive(true); }


            gearAim = false;
			throwing = false;
            //handWeight = 0f;
			anim.SetBool("Pistol", false); 
            anim.SetBool("Shoot", false);
            if (is_FlashlightAim)
            {
				anim.SetBool("Flashlight", true); 
                gameObject.GetComponent<ClientFlashlightSystem>().handFlashlight.SetActive(true);
                gameObject.GetComponent<ClientFlashlightSystem>().FlashLight.enabled = true;
                gameObject.GetComponent<ClientFlashlightSystem>().WeaponLight.enabled = false;
            }

        if (gear == 0) { AudioManager.instance.Play("sb7sweep", audioSource); }
        else { AudioManager.instance.StopPlaying("sb7sweep", audioSource); }
        //anim.Play("GetGear");



    }


	void Attack()
	{
        anim.SetBool("GetGear", false);
        if (aim || gear==0)
			{
            if (gear == 0) { SB7.SetActive(true); }
                gearAim = true;
				anim.SetBool("Pistol", true);
				newHandWeight = 1f;
				canShoot = true;

				// SHOOT
                if (triggerShoot)
                {

                    //Debug.Log("--------------------------------------------------------------------SHOOT CAM FLASH");
                    shootPoint.LookAt(targetPos);
                    anim.SetBool("Shoot", true);
                        if (gear==1)
                        {
                            //AudioManager.instance.Play("ShotCam");
                            muzzleFlash.Play();
                            Shell.Play();

                            //--------------------------FLASH-------------------------------------
                            GameObject newFlash = Instantiate(camFlash);
                            newFlash.transform.position = shootPoint.position;
                            newFlash.name = "CamFlashClient";
                            newFlash.GetComponent<CamFlash>().isClient = true;
                            //---POINT FLASH IN DIRECTION OF THE SHOT
                            Quaternion newYRotation = Quaternion.Euler(0f, shootPoint.rotation.eulerAngles.y, 0f);
                            newFlash.transform.rotation = newYRotation;
                        }
					triggerShoot = false;

                }
                else // if (Input.GetMouseButtonUp(0))
                {
                    anim.SetBool("Shoot", false);
                }

            }
			else 
			{
                gearAim = false;
				anim.SetBool("Pistol", false);
				anim.SetBool("Shoot", false);
				newHandWeight = 0f;
				canShoot = false;

			}
			handWeight = Mathf.Lerp(handWeight, newHandWeight, Time.deltaTime * handSpeed);
        if (anim.GetBool("ouija")) { handWeight = 0f; anim.SetBool("Pistol", true); gear = 1; ouija.SetActive(true); camera.SetActive(false); k2.SetActive(false); camInventory.SetActive(true); k2Inventory.SetActive(true); } else { ouija.SetActive(false); }


    }


    public void Flinch(Vector3 force)
    {
        GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
        GetComponent<Animator>().Play("Flinch", -1, 0f);
    }


	public void Flashlight()//TRIGGRED BY EMIT
	{
        if (flashlighton)
        {
            if (aim == false)//NO WEAPON
            {
                is_FlashlightAim = true;
                GetComponent<ClientFlashlightSystem>().FlashLight.enabled = true;
                GetComponent<ClientFlashlightSystem>().handFlashlight.SetActive(true);
                GetComponent<ClientFlashlightSystem>().WeaponLight.enabled = false;
                anim.SetBool("Flashlight", true);

            }
            if (aim == true)//WEAPON 
            {
                is_FlashlightAim = false;
                GetComponent<ClientFlashlightSystem>().FlashLight.enabled = false;
                GetComponent<ClientFlashlightSystem>().handFlashlight.SetActive(false);
                GetComponent<ClientFlashlightSystem>().WeaponLight.enabled = true;
                anim.SetBool("Flashlight", false);
            }
        }
        else
        {
            GetComponent<ClientFlashlightSystem>().FlashLight.enabled = false;
            GetComponent<ClientFlashlightSystem>().handFlashlight.SetActive(false);
            GetComponent<ClientFlashlightSystem>().WeaponLight.enabled = false;
            is_FlashlightAim = false;
            anim.SetBool("Flashlight", false);
        }
    }

    public void ToggleFlashlight(bool flOn)//TRIGGRED BY EMIT
    {
        if((flashlighton && !flOn) || (!flashlighton && flOn))
        {
            AudioManager.instance.Play("FlashlightClick", audioSource);
        }
        flashlighton = flOn;
    }


    void OnAnimatorIK()
    {

        if (is_FlashlightAim || gearAim)
        {
            anim.SetLookAtWeight(lookIKWeight, bodyWeight);
            anim.SetLookAtPosition(targetPosVec);
        }

        //-----------------  STANCES --------------------------------
        Transform stanceRH = null;
        Transform stanceLH = null;
        if (gear == 1 || gear == 4)
        {
            stanceRH = rightHandTargetCam;
            stanceLH = leftHandTargetCam;
        }
        if (gear == 2 || gear==0)
        {
            stanceRH = rightHandTargetK2;
            stanceLH = leftHandTargetK2;
        }
        if (gear == 3)
        {
            stanceRH = rightHandTargetREM;
            stanceLH = leftHandTargetREM;
        }
        if (stanceRH != null && !throwing)
        {
            if (gearAim)
            {
                //Debug.Log("-----------------------------GEART AIM -------------------------------------");
                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, handWeight);
                anim.SetIKPosition(AvatarIKGoal.RightHand, stanceRH.position);

                anim.SetIKRotationWeight(AvatarIKGoal.RightHand, handWeight);
                anim.SetIKRotation(AvatarIKGoal.RightHand, stanceRH.rotation);


                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, handWeight);
                anim.SetIKPosition(AvatarIKGoal.LeftHand, stanceLH.position);

                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, handWeight);
                anim.SetIKRotation(AvatarIKGoal.LeftHand, stanceLH.rotation);
            }

        }


    }


    void GetFromSoundEvent()
	{
		AudioManager.instance.Play(getFrom, audioSource);
	}
}