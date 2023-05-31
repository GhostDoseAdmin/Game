using InteractionSystem;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using NetworkSystem;
using GameManager;
using System.Net;


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
	public MobileController gamePad;
	//public bool mobileGearAim;
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
        ouija = util.FindChildObject(this.gameObject.transform, "Ouija").gameObject;
        ouija.SetActive(false);
        SB7.SetActive(false);
        
        camInventory.SetActive(false);
        //targetPos = GameDriver.instance.targetLook.transform;
		//playerCam = GameObject.Find("PlayerCamera");

        //GetComponent<WeaponParameters>().RigWeapons();
        GetComponent<ShootingSystem>().RigShooter();
        GetComponent<FlashlightSystem>().RigLights();


        k2.SetActive(false);
        fireK2 = false;
		canFlinch = true;

    }



    void Start()
	{
       
        //GetComponent<WeaponParameters>().EnableInventoryPistol();

        anim = GetComponent<Animator>();
    }
    #endregion

    #region Update

    void Update() 
	{
        if (anim.GetBool("ouija")) { handWeight = 0f; anim.SetBool("Pistol", true); gear = 1; }

        if (anim.GetCurrentAnimatorClipInfo(0).Length > 0){ currentAni = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name; }

		if (currentAni != "React")
		{
            MobileControls();
            Locomotion();

			if (currentAni != "dodgeRightAni" && currentAni != "dodgeLeftAni" )
			{
				Running();
				ChangeGear(false);
				Throwing();
				GearAim();
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
				if (Input.GetMouseButton(1) || gamePad.aimShootBTN.buttonPressed)
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
            string actions = $"{{'flintensity':'{gameObject.GetComponent<FlashlightSystem>().FlashLight.intensity}','ax':'{crosshairPos.x.ToString("F0")}','ay':'{crosshairPos.y.ToString("F0")}','az':'{crosshairPos.z.ToString("F0")}'{flashLightString}{k2String}{gearString}{aimString}{walkString}{strafeString}{runString}{posString}{dodgeString}}}";
            //Debug.Log("------------------------------------------SENDING STRING " + actions);

            //Debug.Log("emitting data " + actions);
            if (actions != prevEmit) {  NetworkDriver.instance.sioCom.Instance.Emit("player_action", JsonConvert.SerializeObject(actions), false); prevEmit = actions; }
			
			
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
    void MobileControls()
	{
        //------------MOBILE CONTROLS----------
        /*if (NetworkDriver.instance.isMobile)
        {
            float minDist = 1; //minimum distance to move
            if (Input.GetMouseButton(0))//finger on screen
            {
                walk = Mathf.Lerp(walk, 1f, 0.8f);
                runningMobile = false;
				mobileGearAim = false;
                //LayerMask mask = 1 << LayerMask.NameToLayer("Environment");
                Vector3 mouse = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mouse);
                RaycastHit[] hits;
                hits = Physics.RaycastAll(ray, Mathf.Infinity);
                foreach (RaycastHit hit in hits)
                {
                    Vector3 newPos = new Vector3(hit.point.x, hit.point.y + 0.7f, hit.point.z);
                    //MOVEMENT
                    if (hit.normal.y > 0.5f && hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))// check to ensure its a ground point  && hit.point.y < transform.position.y + 0.5f
                    {
                        
                        float distance = Vector3.Distance(newPos, new Vector3(transform.position.x, newPos.y, transform.position.z));
                       
                        if (distance < minDist)//keep targPos at distance
                        {
                            Vector3 directionToTarget = (newPos - new Vector3(transform.position.x, newPos.y, transform.position.z)).normalized;
                            targetPos.transform.position = newPos + directionToTarget * minDist;
							walk = 0;
                        }
                        else
                        {
                            targetPos.transform.position = newPos;
                            if (Vector3.Distance(transform.position, hit.point) > 3) { runningMobile = true; }
                        }
                        //break;
                    }
                    //DETECT ENEMY
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                    {
                        targetPos.transform.position = hit.point;
                        //transform.LookAt(hit.collider.gameObject.transform.position);
                        //Quaternion targetRotation = Quaternion.LookRotation(newPos);
                        //transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
                        mobileGearAim = true;
                        walk = 0;
						break;
                    }

                }
            }
            else { walk = 0; }
        }*/
    }

    void Locomotion()
	{
		targetPosVec = targetPos.position;

       walk = Input.GetAxis("Vertical"); strafe = Input.GetAxis("Horizontal");

		//-------------------J O Y S T I C K ----------------------------------
		float joyMagnitude = 0;
		if (NetworkDriver.instance.isMobile) {
			walk = gamePad.joystick.Vertical; strafe = gamePad.joystick.Horizontal;
            joyMagnitude =  Mathf.Sqrt(Mathf.Pow(walk, 2) + Mathf.Pow(strafe, 2));
            Vector3 aimPos = transform.position + (Camera.main.transform.forward * walk + Camera.main.transform.right * strafe).normalized * 5f;
            if (walk!=0 || strafe!=0)
			{
                aimPos.y = transform.position.y + 1f;
                targetPos.position = aimPos;
            }
            //if (mobileGearAim) { walk = 0; strafe = 0; }
        }
       
		//-------DODGE
         if (strafe != 0 && Input.GetKeyDown(strafe > 0 ? KeyCode.D : KeyCode.A))
        {
            if (Time.time - lastTapTime < doubleTapTimeThreshold)
            {
				//anim.Play(strafe > 0 ? "dodgeRightAni" : "dodgeLeftAni");
				if (strafe > 0) { anim.Play("dodgeRightAni"); emitDodge = 1; }
				else { anim.Play("dodgeRightAni"); emitDodge = -1; }
				
            }
            lastTapTime = Time.time;
        }
		//---------------------A N I M A T I O N --------------------------
        if (NetworkDriver.instance.isMobile) {
				anim.SetFloat("Walk", joyMagnitude);
				if (joyMagnitude > 0.9f && !gamePad.aimShootBTN.buttonPressed) { anim.SetBool("Running", true); } else { anim.SetBool("Running", false); }
		}
		else
		{
            anim.SetFloat("Strafe", strafe);
            anim.SetFloat("Walk", walk);
        }
        //Debug.Log("SPEED---------------------------------------------------" + strafe + walk);

        //------------------  R O T A T E --------------------------------
        if (!NetworkDriver.instance.isMobile)
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
		else{ transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(targetPos.transform.position - transform.position).eulerAngles.y, 0f); }


        //------------------ T R A N S L A T E -------------------------
        if (currentAni == "Idle" || currentAni == "Idle_Flashlight" || currentAni == "Idle_Pistol") { speed = 0; }
            if (walk != 0 || strafe != 0) { speed = 2f; }
            if (currentAni == "Running" || anim.GetBool("Running")) { speed = 4f; }

			Vector3 movement = new Vector3(strafe, 0.0f, walk);
			if (NetworkDriver.instance.isMobile) { if (speed > 0) { transform.position = Vector3.MoveTowards(transform.position, targetPos.transform.position, joyMagnitude * speed * Time.deltaTime); } } //transform.position = Vector3.MoveTowards(transform.position, targetPos.transform.position, speed * Time.deltaTime); 
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
		//if (gearAim == true)        {			anim.SetBool("Running", false);        }

		if (anim.GetBool("Running")) { ResetAniFromAim(); }

    }
	#endregion

	public bool canFlinch;
	public void TriggerCanFlinch()
	{
		canFlinch = !canFlinch;
		Debug.Log("-------------------CAN FLINCH-------------------" + canFlinch);

	}
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
							camera.SetActive(false); k2.SetActive(false); camInventory.SetActive(true); k2Inventory.SetActive(true); SB7.SetActive(true);
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

	void MobileAim()
	{

        if (NetworkDriver.instance.isMobile)
        {
            //mobileGearAim = false;
            NPCController[] enemies = FindObjectsOfType<NPCController>();
            NPCController closestEnemy = null;
            float closestDistance = 99999;
            foreach (NPCController enemy in enemies)
            {
                float enemyDist = Vector3.Distance(transform.position, enemy.transform.position);
                //Debug.Log("CHECKING DISTANCE     " + enemyDist);
                if (enemyDist > 10) { continue; }
                if (enemyDist > closestDistance) { continue; }
				//check if fov
                Quaternion look = Quaternion.LookRotation(enemy.transform.position - transform.position);
                float angle = Quaternion.Angle(transform.rotation, look);
				if (angle > 30) { continue; }
                //check line of sight
                Ray ray = new Ray(transform.position + Vector3.up, ((enemy.transform.position + Vector3.up) - (transform.position + Vector3.up)).normalized);
                LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Enemy") | 1 << LayerMask.NameToLayer("Ghost");
                RaycastHit[] hits = Physics.RaycastAll(ray, enemyDist+0.5f, mask);
                Debug.DrawLine(transform.position + Vector3.up, enemy.transform.position + Vector3.up, Color.blue);
                bool inLineOfSight = false;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Enemy")) { inLineOfSight = false; break; }//environment obstruction
					else { inLineOfSight = true;  }
                }
				if (!inLineOfSight) { continue; }
                closestDistance = enemyDist; 
				closestEnemy = enemy;
            }
			//if (closestEnemy != null) { mobileGearAim = true; }
			//else { mobileGearAim = false; }
        }
    }
    void GearAim()
	{
		MobileAim();

        if (!changingGear)
		{
            if (gear == 3 || gear==0) //SPIRIT BOX AND REM
            {
				gearAim = true;
                anim.SetBool("Pistol", true);
				newHandWeight = 1f;
            }
			
                if ( (Input.GetMouseButton(1) && !NetworkDriver.instance.isMobile) || (gamePad.aimShootBTN.buttonPressed && gamePad.aimer.gameObject.activeSelf && NetworkDriver.instance.isMobile) )//AIMING
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

                   // if (anim.GetBool("ouija")) { newHandWeight = 0f; anim.SetBool("Pistol", true); gearAim = false; }
                    GetComponent<ShootingSystem>().Aiming(gear);

					//-------------------------------SHOOTING -----------------------------------
					if ((Input.GetMouseButtonDown(0) && !NetworkDriver.instance.isMobile) )
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
				else if ((Input.GetMouseButtonUp(1) && !NetworkDriver.instance.isMobile) || (gamePad.aimShootBTN.buttonReleased && NetworkDriver.instance.isMobile))
                {
					//MOBILE SHOOT
					if(NetworkDriver.instance.isMobile && gamePad.aimer.gameObject.activeSelf)
					{
                    if (gear == 1) { anim.SetBool("Shoot", true); GetComponent<ShootingSystem>().Shoot(); AudioManager.instance.StopPlaying("camfocus", audioSource); }
                    if (gear == 3) { anim.SetBool("Throw", true); throwing = true; }
					}



					
					Invoke("ResetAniFromAim", 1);
					//anim.SetBool("Throw", false);

					

					if (is_FlashlightAim)
					{
						anim.SetBool("Flashlight", true);
						gameObject.GetComponent<FlashlightSystem>().handFlashlight.SetActive(true);
						gameObject.GetComponent<FlashlightSystem>().FlashLight.enabled = true;
						gameObject.GetComponent<FlashlightSystem>().WeaponLight.enabled = false;
					}
				}
				handWeight = Mathf.Lerp(handWeight, newHandWeight, Time.deltaTime * handSpeed);
				//OUIJA
				if (anim.GetBool("ouija")) { handWeight = 0f; anim.SetBool("Pistol", true); gear = 1; ouija.SetActive(true); camera.SetActive(false); k2.SetActive(false); camInventory.SetActive(true); k2Inventory.SetActive(true); } else { ouija.SetActive(false); }
            
		}
	}
	public void ResetAniFromAim()
	{
        //smooths out ani so no rapid repetitive aim
        gearAim = false;
        anim.SetBool("Pistol", false);
        anim.SetBool("Shoot", false);
        if (gear != 3) { newHandWeight = 0f; }
    }

	#region Flashlight
	private void CheckFlashlight()
    {
		if ((Input.GetKeyDown(InputManager.instance.flashlightSwitchV2) || InputManager.instance.GetFLkeyDown) && is_FlashlightAim == false)
		{
			//if (gameObject.GetComponent<FlashlightSystem>().hasFlashlight == true)
            {
				is_Flashlight = true;
				is_FlashlightAim = true;
				anim.SetBool("Flashlight", true);
                
            }
		}
		else if ((Input.GetKeyDown(InputManager.instance.flashlightSwitchV2) || InputManager.instance.GetFLkeyDown) && is_FlashlightAim == true)
		{
			is_Flashlight = false;
			is_FlashlightAim = false;
			anim.SetBool("Flashlight", false);
        }
	}



	#endregion

	void OnAnimatorIK()
	{
		if ((is_FlashlightAim || gearAim) && !anim.GetBool("ouija"))
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

