using InteractionSystem;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.InputSystem.LowLevel;
using NetworkSystem;

public class PlayerController : MonoBehaviour
{
	[Header("PLAYER PARAMETRS")]
	[Space(10)]
	public float lookIKWeight;
	public float bodyWeight;
	public Transform targetPos;
	public float angularSpeed;
	bool isPlayerRot;
	public float luft;
	public Camera_Controller cameraController;

    //STANCES
    [HideInInspector] public Transform leftHand;
    [HideInInspector] public Transform rightHand;

    [HideInInspector] public Transform rightHandTargetCam;
    [HideInInspector] public Transform rightHandTargetK2;
    [HideInInspector] public Transform rightHandTargetREM;
    [HideInInspector] public Transform leftHandTargetCam;
    [HideInInspector] public Transform leftHandTargetK2;
    [HideInInspector] public Transform leftHandTargetREM;


    float handWeight;
	public float handSpeed;

	float newHandWeight = 0f;

	[Header("INVENTORY")]
	[Space(10)]
	public bool is_Flashlight = false;
	public bool is_FlashlightAim = false;

	//-------GEAR
     private float gear_delay = 1f;//0.25
    private float gear_timer = 0.0f;//USED FOR EMITS
    public int gear = 1; //0 = cam 1=ks 2=rem
    public bool gearAim;
	private GameObject k2;
	private GameObject camera;
    private GameObject camInventory;
    private GameObject k2Inventory;
	private GameObject SB7;
    public bool changingGear;
	public bool throwing = false;
	public bool emitGear;
	public float shotDmg;
	public string shotName="";
	public bool emitShoot;
    [Space(10)]

	[Header("STAMINA UI")]
	[Space(10)]
	[SerializeField] public Image staminaLevel = null;
	[Space(10)]
	public float restoringStamina;
	public float reducedStamina;

	[Header("PLAYER SOUND")]
	[Space(10)]
	[SerializeField] private string getFrom;


    Animator anim;

	Vector3 targetPosVec;
	float walk = 0f;
	float strafe = 0f;
	//public bool emitPos;
    //public GameDriver GD;
    private float emit_timer = 0.0f;//USED FOR EMITS
    private float emit_delay = 0.33f;//0.25
	public float speed;
	public float prevSpeed;
	private string prevEmit;
	public bool emitDamage;
	public Vector3 damageForce;
	public string currentAni="";
	public bool fireK2;
	public bool emitFlashlight;
	public bool emitKill;
	public bool sb7;
	private string currPos;
	private string prevPos;
	public bool emitPos;
	//private GameObject playerCam;
    private static utilities util;

	public GameObject currLight;
	public AudioSource audioSource;
    #region Start


    public void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
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
        SB7 = util.FindChildObject(this.gameObject.transform, "SB7").gameObject;
        SB7.SetActive(false);
        camInventory.SetActive(false);
		//playerCam = GameObject.Find("PlayerCamera");

        //GetComponent<WeaponParameters>().RigWeapons();
        GetComponent<ShootingSystem>().RigShooter();
        GetComponent<FlashlightSystem>().RigLights();

		k2.SetActive(false);
        fireK2 = false;
    }



    void Start()
	{
       
        //GetComponent<WeaponParameters>().EnableInventoryPistol();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

        anim = GetComponent<Animator>();
    }
    #endregion

    #region Update

    void Update() 
	{
        //gearAim = true;
        //anim.SetBool("Pistol", true);
        //handWeight = 1f;
        //gear = 2;
        currentAni = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
		if (currentAni != "React")
		{
			Locomotion();
			Running();
			ChangeGear(false);
			Throwing();
			GearAim();
			CheckFlashlight();

		}
		else //FLINCH
		{
			//anim.SetBool("Pistol", true);
			//anim.SetBool("Pistol", false);
			//anim.Play("Idle", 0, 0f);
			//anim.SetBool("Flashlight", true);
			gearAim = false;
            anim.SetBool("Pistol", false);

        }



        if (Input.GetKeyUp(KeyCode.Escape))
        {
			if (!Cursor.visible)
			{
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
			else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }



        }

		//----------------------------------  E M I T          P L A Y E R             A C T I O N S -----------------------------------------------
        Ray ray = GameObject.Find("PlayerCamera").GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 crosshairPos = ray.origin + ray.direction * 10f;///USE TO EMIT targPos

        if (Time.time > emit_timer + emit_delay)
            {
				//--------------- DAMAGE EMIT-----------------
				/*string dmgString = "";
				if (emitDamage){
					dmgString = $",'dmg':'{emitDamage}','fx':'{damageForce.x.ToString("F2")}','fy':'{damageForce.y.ToString("F2")}','fz':'{damageForce.z.ToString("F2")}'";
					emitDamage = false;
				}*/
				//--------------- FLASHLIGHT EMIT-----------------
				string flashLightString = "";
				if (emitFlashlight){
				bool anyFlashlight = false;
				if (GetComponent<FlashlightSystem>().FlashLight.GetComponent<Light>().enabled || GetComponent<FlashlightSystem>().WeaponLight.GetComponent<Light>().enabled) { anyFlashlight = true; }
				flashLightString = $",'fl':'{anyFlashlight}'";
                emitFlashlight = false;
				}
				//--------------- K2 EMIT-----------------
				string k2String ="";
				if (fireK2){
                k2String = $",'k2':''";
                fireK2 = false;
				}
				//--------------- GEAR EMIT-----------------
				string gearString ="";
				if (emitGear){
                gearString = $",'gear':'{gear}'";
                emitGear = false;
				}
				//--------------- AIM EMIT-----------------
				string aimString = "";
				if (Input.GetMouseButton(1)){
					aimString = $",'aim':''";
				}
				//--------------- WALK EMIT-----------------
				string walkString = "";
				if (walk != 0){
				walkString = $",'w':'{walk.ToString("F1")}'";
                }
				//--------------- STRAFE EMIT-----------------
			    string strafeString = "";
				if (strafe != 0){
                strafeString = $",'s':'{strafe.ToString("F1")}'";
				}
				//--------------- RUN EMIT-----------------
				string runString ="";
				if (Input.GetKey(InputManager.instance.running)){
					runString = $",'r':''";
				}
            //--------------- POSITION EMIT-----------------
				string posString = "";
				currPos = transform.position.x.ToString("F2") + transform.position.z.ToString("F2");
				if (currPos != prevPos || emitPos) { posString = $",'x':'{transform.position.x.ToString("F2")}','y':'{transform.position.y.ToString("F2")}','z':'{transform.position.z.ToString("F2")}'"; }
                prevPos = currPos; emitPos = false;
                /*string posString ="";
				if(emitPos || anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="React"){
                posString = $",'x':'{transform.position.x.ToString("F2")}','y':'{transform.position.y.ToString("F2")}','z':'{transform.position.z.ToString("F2")}'";
				emitPos = false;
                }*/

            //--------------- CAMSHOT EMIT-----------------
                string shotString = "";
				if (emitShoot)
				{
					shotString = $",'shoot':'{shotName}'";
					if (shotName.Length > 1)					{
						shotString = $",'shoot':'{shotName}','sdmg':'{shotDmg}'";
					}
					shotName = "";
					emitShoot = false;
				}

				//--------------- KILL EMIT-----------------
				string killString = "";
				if (emitKill){
					killString = $",'kill':''";
					emitKill = false;
				}
            //--------------- E M I T   S T R I N G ----------------------
            string actions = $"{{'flintensity':'{gameObject.GetComponent<FlashlightSystem>().FlashLight.intensity}','ax':'{crosshairPos.x.ToString("F0")}','ay':'{crosshairPos.y.ToString("F0")}','az':'{crosshairPos.z.ToString("F0")}'{flashLightString}{k2String}{gearString}{aimString}{walkString}{strafeString}{runString}{posString}{shotString}{killString}}}";
			//Debug.Log("------------------------------------------SENDING STRING " + actions);

			
			if (actions != prevEmit) { NetworkDriver.instance.sioCom.Instance.Emit("player_action", JsonConvert.SerializeObject(actions), false); prevEmit = actions; }
			
			
			emit_timer = Time.time;//cooldown
			}

		//CHOOSE LIGHT SOURCE
        if (GetComponent<FlashlightSystem>().FlashLight.GetComponent<Light>().enabled) {  currLight = GetComponent<FlashlightSystem>().FlashLight.gameObject;  }
        else if (GetComponent<FlashlightSystem>().WeaponLight.GetComponent<Light>().enabled) {  currLight = GetComponent<FlashlightSystem>().WeaponLight.gameObject;  }
    }
    #endregion


    #region Locomotion
    void Locomotion()
	{
		targetPosVec = targetPos.position;

        walk = Input.GetAxis("Vertical");
        strafe =Input.GetAxis("Horizontal");

        anim.SetFloat("Strafe", strafe); 
		anim.SetFloat("Walk", walk);
        //Debug.Log("SPEED---------------------------------------------------" + strafe + walk);

        if (walk != 0 || strafe != 0 || is_FlashlightAim == true || gearAim == true || CameraType.FPS == cameraController.cameraType)
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

		//------------------S P E E D -------------------------
		//if (currentAni != "React")
		{
			if (currentAni == "Idle" || currentAni == "Idle_Flashlight" || currentAni == "Idle_Pistol") { speed = 0; }
			else if (currentAni == "Running") { speed = 4f; }
			else { speed = 2f; }//walk


			Vector3 movement = new Vector3(strafe, 0.0f, walk);
			movement = movement.normalized;
			transform.Translate(movement * speed * Time.deltaTime);
		}
    }

	private void Running()
	{
		if (Input.GetKey(InputManager.instance.running) && walk != 0)        {			anim.SetBool("Running", true);        }		
		else if (Input.GetKeyUp(InputManager.instance.running))		{			anim.SetBool("Running", false);        }

		if (gearAim == true)        {			anim.SetBool("Running", false);        }
	}
	#endregion



	//--------------------THROWING (REM POD)-----------------------
	void Throwing()
	{
		if(throwing)
		{
            anim.SetBool("Throw", false);
            gearAim = true;
            anim.SetBool("Pistol", true);
            newHandWeight = 1f;

        }
	}
    public void ThrowRemPod()	{		Debug.Log("--------------------------------THROWING ---------------------------------"); }//ANIMATION EVENT
    public void EndThrow()	{		throwing = false; }//ANIMATION EVENT

   
	public void ChangeGear(bool triggerSb7)
	{
		//END OF GEAR CHANGE
		//if (Mathf.Abs(Time.time - gear_timer) < 0.001f)
		//if (!changeGearThisFrame)
		{

            if (Time.time > gear_timer)
			{
				changingGear = false;
				//START OF GEARCHANGE
				if ((Input.GetKeyDown(InputManager.instance.gear)) || (triggerSb7 && !sb7) || (triggerSb7 && sb7))
				{

					emitGear = true;
					anim.SetBool("GetGear", true);
                    SB7.SetActive(false);
                    gear += 1;
					if (gear > 2) { gear = 1; }
					//-----E
					if (triggerSb7)
					{
						if (sb7)
						{ //PUT AWAY sb7
							sb7 = false;
							gear = 1;

						}
						else
						{ //TAKE OUT sb7
							sb7 = true;
							gear = 0;
							camera.SetActive(false); k2.SetActive(false); camInventory.SetActive(false); k2Inventory.SetActive(false); SB7.SetActive(true);
						}
					}
					//------Q
					else { sb7 = false; }

					if (gear == 1) { camera.SetActive(true); k2.SetActive(false); camInventory.SetActive(false); k2Inventory.SetActive(true); }
					if (gear == 2) { camera.SetActive(false); k2.SetActive(true); camInventory.SetActive(true); k2Inventory.SetActive(false); }

                    if (gear == 0) { AudioManager.instance.Play("sb7sweep", audioSource); }
                    else { AudioManager.instance.StopPlaying("sb7sweep", audioSource); }

                    gear_timer = Time.time + gear_delay;//cooldown
				}
			}
			else
			{ //DURING GEAR CHANGE
				changingGear = true;
				gearAim = false;
				throwing = false;
				//handWeight = 0f;
				anim.SetBool("Pistol", false);
				anim.SetBool("Shoot", false);
				if (is_FlashlightAim)
				{
					anim.SetBool("Flashlight", true);
					gameObject.GetComponent<FlashlightSystem>().handFlashlight.SetActive(true);
					gameObject.GetComponent<FlashlightSystem>().FlashLight.enabled = true;
					gameObject.GetComponent<FlashlightSystem>().WeaponLight.enabled = false;
				}
				//anim.SetBool("GetGear", false);

			}
		}
    }
    private void LateUpdate()
    {
        anim.SetBool("GetGear", false);
    }

    void GearAim()
	{

        if (!changingGear)
		{
            if (gear == 3 || gear==0) //SPIRIT BOX AND REM
            {
				gearAim = true;
                anim.SetBool("Pistol", true);
				newHandWeight = 1f;
            }
            //if (gear == 1 || gear == 2)
            {
				if (Input.GetMouseButton(1)) //AIMING
				{
                    if (!gearAim) { if (gear == 1) { AudioManager.instance.Play("camfocus", audioSource); } }

                    if (is_FlashlightAim)
					{
						anim.SetBool("Flashlight", false);
						if (gear == 1)
						{
							gameObject.GetComponent<FlashlightSystem>().handFlashlight.SetActive(false);
							gameObject.GetComponent<FlashlightSystem>().FlashLight.enabled = false;
							gameObject.GetComponent<FlashlightSystem>().WeaponLight.enabled = true;
						}


					}
					else
					{
						gameObject.GetComponent<FlashlightSystem>().WeaponLight.enabled = false; 
						gameObject.GetComponent<FlashlightSystem>().handFlashlight.SetActive(false);
						gameObject.GetComponent<FlashlightSystem>().FlashLight.enabled = false;
					}

					gearAim = true;
					anim.SetBool("Pistol", true);
					newHandWeight = 1f;

					GetComponent<ShootingSystem>().Aiming(gear);

					//-------------------------------SHOOTING -----------------------------------
					if (Input.GetMouseButtonDown(0))
					{
                        if (gear == 1) { anim.SetBool("Shoot", true); GetComponent<ShootingSystem>().Shoot(); AudioManager.instance.StopPlaying("camfocus", audioSource); }
                        if (gear == 3) { anim.SetBool("Throw", true); throwing = true; }

					}
					else if (Input.GetMouseButtonUp(0))
					{
						anim.SetBool("Shoot", false);
                        //anim.SetBool("Throw", false);
                    }
				}
				else if (Input.GetMouseButtonUp(1))
				{
					gearAim = false;
					anim.SetBool("Pistol", false);
					anim.SetBool("Shoot", false);
					//anim.SetBool("Throw", false);

					if (gear != 3) { newHandWeight = 0f; }

					if (is_FlashlightAim)
					{
						anim.SetBool("Flashlight", true);
						gameObject.GetComponent<FlashlightSystem>().handFlashlight.SetActive(true);
						gameObject.GetComponent<FlashlightSystem>().FlashLight.enabled = true;
						gameObject.GetComponent<FlashlightSystem>().WeaponLight.enabled = false;
					}
				}
				handWeight = Mathf.Lerp(handWeight, newHandWeight, Time.deltaTime * handSpeed);
			}
		}
	}


	#region Flashlight
	private void CheckFlashlight()
    {
		if (Input.GetKeyDown(InputManager.instance.flashlightSwitch) && is_FlashlightAim == false)
		{
			//if (gameObject.GetComponent<FlashlightSystem>().hasFlashlight == true)
            {
				is_Flashlight = true;
				is_FlashlightAim = true;
				anim.SetBool("Flashlight", true);
                
            }
		}
		else if (Input.GetKeyDown(InputManager.instance.flashlightSwitch) && is_FlashlightAim == true)
		{
			is_Flashlight = false;
			is_FlashlightAim = false;
			anim.SetBool("Flashlight", false);
        }
	}



	#endregion

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

