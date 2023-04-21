using InteractionSystem;
using UnityEngine;
using GameManager;
using Unity.VisualScripting;
using UnityEditor;

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
    public float targStrafe = 0f;
    public float targWalk = 0f;

    //-------GEAR
    private float gear_delay = 1f;//0.25
    private float gear_timer = 0.0f;//USED FOR EMITS
    public int gear = 1; //0 = cam 1=ks 2=rem
    public bool gearAim;
    public GameObject k2;
    private GameObject camera;
    private GameObject camInventory;
    private GameObject k2Inventory;
    public bool changingGear;
    public bool throwing = false;


    //NETWORKER
    public string animation;
	public bool state = false;
	public float Float;
	public Vector3 destination;
	public float speed;
	public bool running;
	public Vector3 targetRotation;
	public bool toggleFlashlight = false;//command sent from other player to turn on/off flashlight
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
	public GameObject camFlash;
    public GameObject death;

	private static utilities util;


    private void Awake()
    {

    }

    #region Start
    void Start()
	{
       // SetupRig();

	}

	public void SetupRig()
	{
        util = new utilities();

        Debug.Log("Setting up Player References");
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
		if (!GameDriver.instance.twoPlayer){ return; }

		//------------------------------------- M A I N ---------------------------------------------------
        targetPosVec = Vector3.Lerp(targetPosVec, targetPos.position, 0.1f);//0.1

        // transform.LookAt(targetPosVec);

        //---- locomotion animations ------
        if (speed>0f || Vector3.Distance(transform.position, destination)>0.1)
		{
			strafe = Mathf.Lerp(strafe, targStrafe, 0.1f);
            walk = Mathf.Lerp(walk, targWalk, 0.1f);

            anim.SetFloat("Strafe", strafe);
            anim.SetFloat("Walk", walk);
			if (running) { anim.SetBool("Running", true);  }
			else { anim.SetBool("Running", false); }
        }

		Attack();

        if (Vector3.Distance(transform.position, destination) > 0.1) { if (speed == 0) { speed = 4f; }  }//catch up to destination
        // if (speed == 0) { speed = 4f; } 
        //KEEP POS UPDATED
        if (Vector3.Distance(transform.position, destination)>2)//1.5
		{
            transform.position = new Vector3(destination.x, destination.y, destination.z);
        }
        transform.position = Vector3.Lerp(transform.position, destination, speed * 0.95f * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetRotation), 600f * Time.deltaTime); //150


    }


    #region Update
    void Update() 
	{
        //SET CURRENT LIGHT SOURCE FOR CLIENT
        if (GetComponent<ClientFlashlightSystem>().FlashLight.GetComponent<Light>().enabled) { currLight = GetComponent<ClientFlashlightSystem>().FlashLight.gameObject; }
        else if (GetComponent<ClientFlashlightSystem>().WeaponLight.enabled) { currLight = GetComponent<ClientFlashlightSystem>().WeaponLight.gameObject; }



    }
    #endregion



    public void ChangeGear(int nextGear)
	{

        gear = nextGear;

        //START OF GEARCHANGE
        anim.SetBool("GetGear", true);
        if (gear == 1){camera.SetActive(true); k2.SetActive(false); camInventory.SetActive(false); k2Inventory.SetActive(true); }
				if (gear == 2){camera.SetActive(false); k2.SetActive(true); camInventory.SetActive(true); k2Inventory.SetActive(false);  }
                
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
        //anim.Play("GetGear");



    }


	void Attack()
	{
        anim.SetBool("GetGear", false);
        if (aim)
			{

                gearAim = true;
				anim.SetBool("Pistol", true);
				newHandWeight = 1f;
				canShoot = true;

				// SHOOT
                if (triggerShoot)
                {
                    shootPoint.LookAt(targetPos);
                    anim.SetBool("Shoot", true);
                    AudioManager.instance.Play(shootSound);
                    muzzleFlash.Play();
                    Shell.Play();

					//--------------------------FLASH-------------------------------------
					 GameObject newFlash = Instantiate(camFlash);
					newFlash.transform.position = shootPoint.position;
                    newFlash.name = "CamFlashClient";
                    //---POINT FLASH IN DIRECTION OF THE SHOT
                    Quaternion newYRotation = Quaternion.Euler(0f, shootPoint.rotation.eulerAngles.y, 0f);
					newFlash.transform.rotation = newYRotation;

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
		
	}


    public void Flinch(Vector3 force)
    {
        GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
        GetComponent<Animator>().Play("Flinch", -1, 0f);
    }


	public void Flashlight(bool on)//TRIGGRED BY EMIT
	{
		
		is_Flashlight = on;

		//TOGGLE FLASHLIGHT
		if (
            (!aim && GetComponent<ClientFlashlightSystem>().FlashLight.enabled == false && on) 
            || (!aim && GetComponent<ClientFlashlightSystem>().FlashLight.enabled == true && !on)
            || (aim && GetComponent<ClientFlashlightSystem>().WeaponLight.enabled == false && on)
            || (aim && GetComponent<ClientFlashlightSystem>().WeaponLight.enabled == true && !on)
            )
        {
            AudioManager.instance.Play("FlashlightClick");
            if (aim == false)
            {
                gameObject.GetComponent<ClientFlashlightSystem>().WeaponLight.enabled = false;
                if (on)
                {
                    is_FlashlightAim = true;

                    gameObject.GetComponent<ClientFlashlightSystem>().handFlashlight.SetActive(true);
                    gameObject.GetComponent<ClientFlashlightSystem>().FlashLight.enabled = true;
                    anim.SetBool("Flashlight", true);//regular flashlight hold
                }
                if (!on)
                {
                    is_FlashlightAim = false;

                    gameObject.GetComponent<ClientFlashlightSystem>().handFlashlight.SetActive(true);
                    gameObject.GetComponent<ClientFlashlightSystem>().FlashLight.enabled = true;
                    anim.SetBool("Flashlight", false);//regular flashlight hold
                }

            }

            if (aim == true)
            {
                anim.SetBool("Flashlight", false);//regular flashlight hold
                gameObject.GetComponent<ClientFlashlightSystem>().handFlashlight.SetActive(false);
                gameObject.GetComponent<ClientFlashlightSystem>().FlashLight.enabled = false;

                if (on)
                {
                    gameObject.GetComponent<ClientFlashlightSystem>().WeaponLight.enabled = true;

                }
                if (!on)
                {
                    gameObject.GetComponent<ClientFlashlightSystem>().WeaponLight.enabled = false;
                }

            }
        }



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
        if (gear == 1)
        {
            stanceRH = rightHandTargetCam;
            stanceLH = leftHandTargetCam;
        }
        if (gear == 2)
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
		AudioManager.instance.Play(getFrom);
	}
}