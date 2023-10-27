using InteractionSystem;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using NetworkSystem;
using GameManager;
using UnityEngine.AI;

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
     private float gear_delay = 0.5f;//0.25
    private float gear_timer = 0.0f;//USED FOR EMITS
    public bool hasGrid, hasRem = false;
    public int gear = 1; //0 = SB7 1=cam 2=k2 3=rem
    public bool gearAim;
	public GameObject k2;
	public GameObject camera;
    private GameObject camInventory;
    private GameObject k2Inventory;
	private GameObject SB7;
	private GameObject ouija;
    public bool changingGear;
	public bool throwing = false;
	public bool emitGear;
	
	//public float shotDmg;
	//public string shotName="";
	//public bool emitShoot;
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
	public bool emitScreamer;
	public Vector3 damageForce;
	public string currentAni="";
	public bool fireK2;
	public bool emitFlashlight;
	//public bool emitKill;
	public bool sb7;
	private string currPos;
	private string prevPos;
	public bool emitPos;
	private int emitDodge;
	public bool isTravis;
	public MobileController gamePad;
	//public bool mobileGearAim;
	//private GameObject playerCam;
    private static utilities util;

	public GameObject currLight;
	public AudioSource audioSource;
    public AudioSource audioSource2, audioSource3;
    public AudioSource audioSourceSpeech;

	public bool lockControl = false;
	public bool isFemale = false;
    #region Start


    public void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        audioSource2 = gameObject.AddComponent<AudioSource>();
        audioSource2.spatialBlend = 1.0f;
        audioSource3 = gameObject.AddComponent<AudioSource>();
        audioSource3.spatialBlend = 1.0f;
        audioSourceSpeech = gameObject.AddComponent<AudioSource>();
        audioSourceSpeech.spatialBlend = 1.0f;


        targetPos = GameObject.Find("TargetLook").transform;
        cameraController = Camera.main.gameObject.GetComponent<Camera_Controller>();

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
        ouija = util.FindChildObject(this.gameObject.transform, "Ouija").gameObject;
        ouija.SetActive(false);
        //SB7.SetActive(false);
        
        camInventory.SetActive(false);
        //targetPos = GameDriver.instance.targetLook.transform;
		//playerCam = GameObject.Find("PlayerCamera");

        //GetComponent<WeaponParameters>().RigWeapons();
        GetComponent<ShootingSystem>().RigShooter();
        GetComponent<FlashlightSystem>().RigLights();


        k2.SetActive(false);
        fireK2 = false;
		canFlinch = true;

		//FEMALE
        if (transform.GetChild(0).name.ToLower().Contains("female")) { isFemale = true; } else { isFemale = false; }

    }



    void Start()
	{
        //GetComponent<WeaponParameters>().EnableInventoryPistol();
        GameDriver.instance.gearuicam.SetActive(true);
        anim = GetComponent<Animator>();
    }
    #endregion




    #region Update

    void Update() 
	{
        if (anim.GetBool("ouija")) { handWeight = 0f; anim.SetBool("Pistol", true); gear = 1; }

        if (anim.GetCurrentAnimatorClipInfo(0).Length > 0){ currentAni = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name; }

		if (currentAni != "React" && currentAni != "ReactV2" )
		{
			if (!lockControl) { Locomotion(); }

			if (currentAni != "dodgeRightAni" && currentAni != "dodgeLeftAni" )
			{
				if (!lockControl)
				{
					Running();
					ChangeGear(false, false);
					Throwing();
					GearAim();
				}
				CheckFlashlight();
			}
			else
            {
                gearAim = false;
                anim.SetBool("Pistol", false);
            }
		}
		else
        {
			if (NetworkDriver.instance.isMobile) { transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f); }//PREVENT CAM TILT ISSUE
            gearAim = false;
            anim.SetBool("Pistol", false);
        }


		//----------------------------------  E M I T          P L A Y E R             A C T I O N S -----------------------------------------------
        if (NetworkDriver.instance.TWOPLAYER && Time.time > emit_timer + emit_delay)
            {
			Vector3 crosshairPos;

            if (!NetworkDriver.instance.isMobile)
			{
				Ray ray = GameObject.Find("PlayerCamera").GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
				crosshairPos = ray.origin + ray.direction * 10f;///USE TO EMIT targPos
			}
			else
			{
				crosshairPos = targetPos.position ;
            }
            //--------------- DAMAGE EMIT-----------------
            /*string dmgString = "";
            if (emitDamage){
                dmgString = $",'dmg':'{emitDamage}','fx':'{damageForce.x.ToString("F2")}','fy':'{damageForce.y.ToString("F2")}','fz':'{damageForce.z.ToString("F2")}'";
                emitDamage = false;
            }*/
				//--------------- FLASHLIGHT EMIT-----------------
				string flashLightString = "";
				//if (emitFlashlight){
				bool anyFlashlight = false;
				if (GetComponent<FlashlightSystem>().FlashLight.GetComponent<Light>().enabled || GetComponent<FlashlightSystem>().WeaponLight.GetComponent<Light>().enabled) { anyFlashlight = true; }
				if(anyFlashlight){ flashLightString = $",'fl':'{anyFlashlight}'"; }
                //emitFlashlight = false;
				
				//--------------- K2 EMIT-----------------
				string k2String ="";
				if (fireK2){
                k2String = $",'k2':''";
                fireK2 = false;
				}
				//--------------- DODGE EMIT-----------------
				string dodgeString = "";
				if (emitDodge!=0)
				{
					dodgeString = $",'dg':'{emitDodge}'";
					emitDodge = 0;
				}
				//--------------- GEAR EMIT-----------------
				string gearString ="";
				if (emitGear){
                gearString = $",'gear':'{gear}'";
                emitGear = false;
				}
				//--------------- AIM EMIT-----------------
				string aimString = "";
				if (Input.GetMouseButton(1) || gamePad.joystickAim.GetComponent<GPButton>().buttonPressed)
            {
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
				if (currentAni == "Running" || anim.GetBool("Running"))
				{
					runString = $",'r':''";
				}
				//--------------- DEMON SCREAMER EMIT-----------------
				string screamerString = "";
				if (emitScreamer==true)
				{
					screamerString = $",'ds':''";
					emitScreamer = false;
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
            /*string shotString = "";
            if (emitShoot)
            {
                shotString = $",'shoot':'{shotName}'";
                if (shotName.Length > 1)					{
                    shotString = $",'shoot':'{shotName}','sdmg':'{shotDmg}'";
                }
                shotName = "";
                emitShoot = false;
            }*/

            //--------------- KILL EMIT-----------------
            /*string killString = "";
            if (emitKill){
                killString = $",'kill':''";
                emitKill = false;
            }*/
            //--------------- E M I T   S T R I N G ----------------------
            //'flintensity':'{gameObject.GetComponent<FlashlightSystem>().FlashLight.intensity}',
            string actions = $"{{'ax':'{crosshairPos.x.ToString("F0")}','ay':'{crosshairPos.y.ToString("F0")}','az':'{crosshairPos.z.ToString("F0")}'{flashLightString}{k2String}{gearString}{aimString}{walkString}{strafeString}{runString}{posString}{dodgeString}{screamerString}}}";
            //Debug.Log("------------------------------------------SENDING STRING " + actions);

            //Debug.Log("emitting data " + actions);
            if (actions != prevEmit) {  
				NetworkDriver.instance.sioCom.Instance.Emit("player_action", JsonConvert.SerializeObject(actions), false); prevEmit = actions; 
			}
			
			
			emit_timer = Time.time;//cooldown
			}

		//CHOOSE LIGHT SOURCE
        if (GetComponent<FlashlightSystem>().FlashLight.GetComponent<Light>().enabled) {  currLight = GetComponent<FlashlightSystem>().FlashLight.gameObject;  }
        else if (GetComponent<FlashlightSystem>().WeaponLight.GetComponent<Light>().enabled) {  currLight = GetComponent<FlashlightSystem>().WeaponLight.gameObject;  }
    }
    #endregion


    #region Locomotion
    public float doubleTapTimeThreshold = 0.3f;
    private float lastTapTime = -10f;
    //public VariableJoystick joystick;
	public GameObject stick, region;

    void Locomotion()
	{
		targetPosVec = targetPos.position;

       walk = Input.GetAxis("Vertical"); strafe = Input.GetAxis("Horizontal");

		//-------------------J O Y S T I C K ----------------------------------
		float joyMagnitude = 0;
		if (NetworkDriver.instance.isMobile) {
			//MOVE
			walk = gamePad.joystick.Vertical; strafe = gamePad.joystick.Horizontal;
            joyMagnitude =  Mathf.Sqrt(Mathf.Pow(walk, 2) + Mathf.Pow(strafe, 2));

			//if (!gamePad.camSup.AIMMODE) { targetPosVec = targetPos.position + Vector3.up; }
		
		}
       
		//-------DODGE
        /*if (strafe != 0 && Input.GetKeyDown(strafe > 0 ? KeyCode.D : KeyCode.A))
        {
            if (Time.time - lastTapTime < doubleTapTimeThreshold)
            {
				//anim.Play(strafe > 0 ? "dodgeRightAni" : "dodgeLeftAni");
				if (strafe > 0) { anim.Play("dodgeRightAni"); emitDodge = 1; }
				else { anim.Play("dodgeRightAni"); emitDodge = -1; }
				
            }
            lastTapTime = Time.time;
        }*/
        //---------------------A N I M A T I O N --------------------------
		//running ani
        if (NetworkDriver.instance.isMobile) {
		    anim.SetFloat("Walk", joyMagnitude);
            anim.SetFloat("Strafe", 0);
            if (joyMagnitude > 0.9f && !gamePad.camSup.AIMMODE) { anim.SetBool("Running", true); } else { anim.SetBool("Running", false); }
		}
        if(!NetworkDriver.instance.isMobile || gamePad.camSup.AIMMODE)
        {
            anim.SetFloat("Strafe", strafe);
            anim.SetFloat("Walk", walk);
        }
        //Debug.Log("SPEED---------------------------------------------------" + strafe + walk);

        //------------------  R O T A T E --------------------------------
        //if (!NetworkDriver.instance.isMobile)
		{
			if (walk != 0 || strafe != 0 || is_FlashlightAim == true || gearAim || CameraType.FPS == cameraController.cameraType)
			{
				Vector3 rot = transform.eulerAngles;
				transform.LookAt(targetPosVec);

				float angleBetween = Mathf.DeltaAngle(transform.eulerAngles.y, rot.y);
				if ((Mathf.Abs(angleBetween) > luft) || strafe != 0) { isPlayerRot = true; }
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
		//else
		{
            //transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(targetPos.transform.position - transform.position).eulerAngles.y, 0f); 
           // transform.LookAt(targetPos);
        }


        //------------------ T R A N S L A T E -------------------------
        if (currentAni == "Idle" || currentAni == "Idle_Flashlight" || currentAni == "Idle_Pistol") { speed = 0; }
            if (walk != 0 || strafe != 0) { speed = 2f; }
            if (currentAni == "Running" || anim.GetBool("Running")) { speed = 4f; }

			Vector3 movement = new Vector3(strafe, 0.0f, walk);
			if (NetworkDriver.instance.isMobile) { if (speed > 0) { transform.position = Vector3.MoveTowards(transform.position, transform.position + (Camera.main.transform.forward * walk + Camera.main.transform.right * strafe).normalized * 5f, joyMagnitude * speed * Time.deltaTime); } } //transform.position = Vector3.MoveTowards(transform.position, targetPos.transform.position, speed * Time.deltaTime); 
			else { transform.Translate(movement * speed * Time.deltaTime); }
            //movement = movement.normalized;
            //if (speed > 0) { Debug.Log("SPEED " + speed); }
            
            
    }

	private void Running()
	{
		//PC
		if (!NetworkDriver.instance.isMobile)
		{
			if (Input.GetKey(InputManager.instance.running) && walk != 0) { anim.SetBool("Running", true); }
			else if (Input.GetKeyUp(InputManager.instance.running)) { anim.SetBool("Running", false); }
		}
		if (gearAim == true)        {			anim.SetBool("Running", false);        }
	}
	#endregion

	public bool canFlinch; //---------USED FOR DDOGE
	/*public void TriggerCanFlinch() 
	{
		canFlinch = !canFlinch;
		Debug.Log("-------------------CAN FLINCH-------------------" + canFlinch);

	}*/
	//--------------------THROWING (REM POD)-----------------------
	void Throwing()
	{
		if(throwing)
		{
            anim.SetBool("Throw", false);
            //gearAim = true;
            anim.SetBool("Pistol", true);
            newHandWeight = 1f;

        }
	}
    public void ThrowRemPod()
    {       //ANIMATION EVENT
		GetComponent<ShootingSystem>().remPod.Release();
    }
    public void EndThrow()	{		throwing = false; }//ANIMATION EVENT

   
	public void ChangeGear(bool triggerSb7, bool bypass)
	{
        //END OF GEAR CHANGE
        //0 =SB7 1=CAM 2=K2 3=REM 4=GRID
        {

            if (Time.time > gear_timer)
			{
				changingGear = false;
				//START OF GEARCHANGE
				if ((((Input.GetKeyDown(InputManager.instance.gear) && !NetworkDriver.instance.isMobile) || (NetworkDriver.instance.isMobile && GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.gearBTN.buttonReleased)) || (triggerSb7 && !sb7) || (triggerSb7 && sb7)) || bypass)
				{
                    emitGear = true;
					anim.SetBool("GetGear", true);
                    SB7.SetActive(false);
                    gear += 1;
                    if (!hasRem && gear == 3) { gear += 1; }
                    if ((!hasGrid || GetComponent<ShootingSystem>().gridBatteryUI.fillAmount <=0) && gear == 4) { gear += 1; }
                    if (gear > 4) { gear = 1; }
					
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
                            GameDriver.instance.gearuicam.SetActive(false);
                            GameDriver.instance.gearuik2.SetActive(false);
                            GameDriver.instance.gearuiemp.SetActive(false); GameDriver.instance.gearuilaser.SetActive(false);
                            camera.SetActive(false); k2.SetActive(false); camInventory.SetActive(true); k2Inventory.SetActive(true); SB7.SetActive(true);
						}
					}
					//------Q
					else { sb7 = false; }
                    AudioManager.instance.Play("switchgear", audioSource2);
                    //GetComponentInChildren<laserGrid>().gameObject.SetActive(false); GetComponent<ShootingSystem>().remPod.gameObject.SetActive(false);
                    //GetComponent<ShootingSystem>().laserGrid.SetActive(false);
                    GameDriver.instance.gearuiemp.SetActive(false); GameDriver.instance.gearuilaser.SetActive(false);
                    GameDriver.instance.gearuicam.SetActive(false); GameDriver.instance.gearuik2.SetActive(false);  k2.SetActive(false); camera.SetActive(false); camInventory.SetActive(false); k2Inventory.SetActive(false);
                    if (gear == 1) { AudioManager.instance.Play("switchcam", audioSource3); GameDriver.instance.gearuicam.SetActive(true);  camera.SetActive(true);  k2Inventory.SetActive(true); }
					if (gear == 2) { AudioManager.instance.Play("switchk2", audioSource3); GameDriver.instance.gearuik2.SetActive(true);  k2.SetActive(true); camInventory.SetActive(true);  }
                    if (gear == 3) { AudioManager.instance.Play("EMPSwitch", audioSource3); GameDriver.instance.gearuiemp.SetActive(true); GetComponent<ShootingSystem>().remPod.gameObject.SetActive(true); }
                    if (gear == 4) { AudioManager.instance.Play("GridSwitch", audioSource3); GameDriver.instance.gearuilaser.SetActive(true); } // GetComponent<ShootingSystem>()lasGrid.SetActive(true);
                    if (gear == 0) { AudioManager.instance.Play("sb7sweep", audioSource); }
                    else { AudioManager.instance.StopPlaying("sb7sweep", audioSource); }

					GetComponent<ShootingSystem>().SwitchGear();
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
				if (gear == 0) { gearAim = true; }
                anim.SetBool("Pistol", true);
				newHandWeight = 1f;
            }
			
                if ( (Input.GetMouseButton(1) && !NetworkDriver.instance.isMobile) || (NetworkDriver.instance.isMobile && gamePad.joystickAim.GetComponent<GPButton>().buttonPressed))//AIMING  //&& gamePad.aimer.gameObject.activeSelf 
				{
                    if (!gearAim) { if (gear == 1) { AudioManager.instance.Play("camfocus", audioSource); } }

                    if (is_FlashlightAim)
					{
						anim.SetBool("Flashlight", false);
						if (gear == 1 || gear==4)
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

                   // if (anim.GetBool("ouija")) { newHandWeight = 0f; anim.SetBool("Pistol", true); gearAim = false; }
                    GetComponent<ShootingSystem>().Aiming();

                //-------------------------------SHOOTING -----------------------------------
				if (!NetworkDriver.instance.isMobile)
				{
					if (Input.GetMouseButtonDown(0))
					{
						if (gear == 1 || gear==4) { anim.SetBool("Shoot", true); GetComponent<ShootingSystem>().Shoot(); AudioManager.instance.StopPlaying("camfocus", audioSource); }
						if (gear == 3 && GetComponentInChildren<RemPod>().remPodSkin.activeSelf ) { anim.SetBool("Throw", true); GetComponentInChildren<RemPod>().StartThrow(); throwing = true;  }

					}
					else if (Input.GetMouseButtonUp(0))
					{
						anim.SetBool("Shoot", false);
						//anim.SetBool("Throw", false);
					}
				}
                CancelInvoke("ReleaseAim");
				if (!NetworkDriver.instance.isMobile) { Invoke("ReleaseAim", 0.2f); }
				else { Invoke("ReleaseAim", 0.5f); }
				}

				//--------------------------MOBILE SHOOTING--------------------------------
            if (NetworkDriver.instance.isMobile && gamePad.joystickAim.GetComponent<GPButton>().buttonReleased) //&& ((GetComponent<ShootingSystem>().target!=null && gamePad.camSup.AIMMODE)|| !gamePad.camSup.AIMMODE)
            {
                if (gear == 1 || gear==4) { anim.SetBool("Shoot", true); GetComponent<ShootingSystem>().Shoot(); AudioManager.instance.StopPlaying("camfocus", audioSource); }
                if (gear == 3  && GetComponentInChildren<RemPod>().remPodSkin.activeSelf ) { anim.SetBool("Throw", true); GetComponentInChildren<RemPod>().StartThrow(); throwing = true;  }
            }

				handWeight = Mathf.Lerp(handWeight, newHandWeight, Time.deltaTime * handSpeed);
				//OUIJA
				if (anim.GetBool("ouija")) { handWeight = 0f; anim.SetBool("Pistol", true); gear = 1; ouija.SetActive(true); camera.SetActive(false); k2.SetActive(false); camInventory.SetActive(true); k2Inventory.SetActive(true); } else { ouija.SetActive(false); }
            
		}
	}
	public void ReleaseAim()
	{
		
        gearAim = false;
        anim.SetBool("Pistol", false);
        anim.SetBool("Shoot", false);

        if (gear != 3) { newHandWeight = 0f; }

        if (is_FlashlightAim)
        {
            anim.SetBool("Flashlight", true);
            gameObject.GetComponent<FlashlightSystem>().handFlashlight.SetActive(true);
            gameObject.GetComponent<FlashlightSystem>().FlashLight.enabled = true;
            gameObject.GetComponent<FlashlightSystem>().WeaponLight.enabled = false;
        }


    }

	#region Flashlight
	private void CheckFlashlight()
    {
		if ((Input.GetKeyDown(InputManager.instance.flashlightSwitchV2) || (NetworkDriver.instance.isMobile && GetComponent<PlayerController>().gamePad.flashlightBTN.buttonReleased)) && is_FlashlightAim == false)
		{
			//if (gameObject.GetComponent<FlashlightSystem>().hasFlashlight == true)
            {
				is_Flashlight = true;
				is_FlashlightAim = true;
				anim.SetBool("Flashlight", true);
                
            }
		}
		else if ((Input.GetKeyDown(InputManager.instance.flashlightSwitchV2) || (NetworkDriver.instance.isMobile && GetComponent<PlayerController>().gamePad.flashlightBTN.buttonReleased)) && is_FlashlightAim == true)
		{
			is_Flashlight = false;
			is_FlashlightAim = false;
			anim.SetBool("Flashlight", false);
        }
	}



	#endregion

	void OnAnimatorIK()
	{
		if ((is_FlashlightAim || gearAim || gear==3) && !anim.GetBool("ouija"))
		{
			anim.SetLookAtWeight(lookIKWeight, bodyWeight);
			anim.SetLookAtPosition(targetPosVec);
		}

        //-----------------  STANCES --------------------------------
        Transform stanceRH = null;
        Transform stanceLH = null;
			if (gear == 1 || gear==4)
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
            if (gearAim || gear==3)
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

