﻿using InteractionSystem;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Newtonsoft.Json;

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
	public ShootingSystem shootPistol;

	[Header("HAND PARAMETRS")]
	[Space(10)]
	public Transform rightHandTarget;
	public Transform rightHand;
	Transform[] rightHandTrans;

	public Transform leftHandTarget;
	public Transform leftHand;
	Transform[] leftHandTrans;

	float handWeight;
	public float handSpeed;

	float newHandWeight = 0f;

	[Header("INVENTORY BOOL")]
	[Space(10)]
	public bool is_Flashlight = false;
	public bool is_FlashlightAim = false;

	[Space(10)]
	public bool is_Knife = false;
	public bool is_KnifeAim = false;

	[Space(10)]
	public bool is_Pistol = true;
	public bool is_PistolAim = false;
	public bool canShoot { get; private set; }

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

    private NetworkDriver ND;
    private float action_timer = 0.0f;
    private float action_delay = 0.33f;//0.25
	public float speed;
	public float prevSpeed;
	private string prevEmit;

    #region Start

    void Start()
	{
		anim = GetComponent<Animator>();
        ND = GameObject.Find("NetworkDriver").GetComponent<NetworkDriver>();
        GetComponent<WeaponParameters>().EnableInventoryPistol();


        rightHandTrans = rightHand != null ? rightHand.GetComponentsInChildren<Transform>() : new Transform[0];
		leftHandTrans = leftHand != null ? leftHand.GetComponentsInChildren<Transform>() : new Transform[0];

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
    #endregion

    #region Update
    void Update() 
	{
        Locomotion();
		Running();

		CheckKnife();
		KnifeAttack();

		CheckPistol();
		PistolAttack();

		CheckFlashlight();

		if (Input.GetKeyUp(KeyCode.Escape))
        {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}

		//----------------------------------  E M I T          P L A Y E R             A C T I O N S -----------------------------------------------
        Ray ray = GameObject.Find("PlayerCamera").GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 crosshairPos = ray.origin + ray.direction * 10f;///USE TO EMIT targPos

        if (Time.time > action_timer + action_delay)
            {
                string actions = $"{{'flashlight':'{is_FlashlightAim}','aim':'{Input.GetMouseButton(1)}','walk':'{walk.ToString("F0")}','strafe':'{strafe.ToString("F0")}','run':'{Input.GetKey(InputManager.instance.running)}','x':'{transform.position.x.ToString("F6")}','y':'{transform.position.y.ToString("F6")}','z':'{transform.position.z.ToString("F6")}','speed':'{speed.ToString("F2")}','rx':'{transform.eulerAngles.x.ToString("F0")}','ry':'{transform.eulerAngles.y.ToString("F0")}','rz':'{transform.eulerAngles.z.ToString("F0")}','ax':'{crosshairPos.x.ToString("F0")}','ay':'{crosshairPos.y.ToString("F0")}','az':'{crosshairPos.z.ToString("F0")}'}}";
				if (actions != prevEmit) { ND.sioCom.Instance.Emit("player_action", JsonConvert.SerializeObject(actions), false); prevEmit = actions; }
                action_timer = Time.time;
			}

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

        AnimatorClipInfo[] clipInfo = anim.GetCurrentAnimatorClipInfo(0);
        string animName = clipInfo[0].clip.name;
		Debug.Log(" ANIMATION " + animName);

        if (walk != 0 || strafe != 0 || is_FlashlightAim == true || is_KnifeAim == true || is_PistolAim == true || CameraType.FPS == cameraController.cameraType)
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
        if (animName == "Idle" || animName =="Idle_Flashlight" || animName == "Idle_Pistol") { speed = 0; }
        else if (animName == "Running") { speed = 4f; }
        else { speed = 2f; }//walk

        Vector3 movement = new Vector3(strafe, 0.0f, walk);
        movement = movement.normalized;
        transform.Translate(movement * speed * Time.deltaTime);

    }

	private void Running()
	{
		if (Input.GetKey(InputManager.instance.running) && walk != 0)        {			anim.SetBool("Running", true);        }		
		else if (Input.GetKeyUp(InputManager.instance.running))		{			anim.SetBool("Running", false);        }

		if (is_KnifeAim == true || is_PistolAim == true)        {			anim.SetBool("Running", false);        }
	}
	#endregion

	#region Melee Сombat
	private void CheckKnife()
	{
		if (Input.GetKeyDown(InputManager.instance.knife) && is_Pistol == false && is_Knife == false)
		{
			if (gameObject.GetComponent<WeaponParameters>().hasKnife == true)
			{
				is_Knife = true;
				anim.SetBool("Knife", true);
			}
		}
		else if (Input.GetKeyDown(InputManager.instance.knife) && is_Knife == true)
		{
			is_Knife = false;
			anim.SetBool("Knife", false);
		}
	}

	void KnifeAttack()
	{
		if(is_Knife)
		{
			if (Input.GetMouseButton(1))
			{
				is_KnifeAim = true;

				if (Input.GetMouseButtonDown(0))
				{
					anim.SetTrigger("LeftMouseClick");
				}
			}
			else
			{
				is_KnifeAim = false;
			}
		}
	}
	#endregion

	#region Ranged Сombat
	private void CheckPistol()
	{
		if (Input.GetKeyDown(InputManager.instance.pistol) && is_Knife == false && is_Pistol == false)
		{
			if (gameObject.GetComponent<WeaponParameters>().hasPistol == true)
			{
				is_Pistol = true;
				anim.SetBool("GetPistol", true);
			}
		}
		else if (Input.GetKeyDown(InputManager.instance.pistol) && is_Pistol == true)
		{
			is_Pistol = false;
			anim.SetBool("GetPistol", false);
		}
	}

	void PistolAttack()
	{
		if (is_Pistol)
		{
			if (Input.GetMouseButton(1)) //AIMING
			{
				if (is_FlashlightAim)
                {
					anim.SetBool("Flashlight", false);
					gameObject.GetComponent<FlashlightSystem>().handFlashlight.SetActive(false);
					gameObject.GetComponent<FlashlightSystem>().flashlightSpot.enabled = false;
					gameObject.GetComponent<FlashlightSystem>().flashlightSpotPistol.enabled = true;
				}
				else
                {
					gameObject.GetComponent<FlashlightSystem>().flashlightSpotPistol.enabled = false;
				}

				is_PistolAim = true;
				anim.SetBool("Pistol", true);
				newHandWeight = 1f;
				canShoot = true;

				if (Input.GetMouseButton(0))
				{
					anim.SetBool("Shoot", true);
					shootPistol.Shoot();
				}
				else if (Input.GetMouseButtonUp(0))
				{
					anim.SetBool("Shoot", false);
				}
			}
			else if (Input.GetMouseButtonUp(1))
			{
				is_PistolAim = false;
				anim.SetBool("Pistol", false);
				anim.SetBool("Shoot", false);
				newHandWeight = 0f;
				canShoot = false;

				if (is_FlashlightAim)
				{
					anim.SetBool("Flashlight", true);
					gameObject.GetComponent<FlashlightSystem>().handFlashlight.SetActive(true);
					gameObject.GetComponent<FlashlightSystem>().flashlightSpot.enabled = true;
					gameObject.GetComponent<FlashlightSystem>().flashlightSpotPistol.enabled = false;
				}
			}
			handWeight = Mathf.Lerp(handWeight, newHandWeight, Time.deltaTime * handSpeed);
		}
	}
	#endregion

	#region Flashlight
	private void CheckFlashlight()
    {
		if (Input.GetKeyDown(InputManager.instance.flashlightSwitch) && is_FlashlightAim == false)
		{
			if (gameObject.GetComponent<FlashlightSystem>().hasFlashlight == true)
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

		if (gameObject.GetComponent<FlashlightSystem>().flashlightSpot.intensity <= 0)
        {
			is_Flashlight = false;
			is_FlashlightAim = false;
			anim.SetBool("Flashlight", false);
		}
	}

	/*protected void IKFlashlight1()
	{
		this.pivot.position = this.shoulder.position;

		if (is_FlashlightAim)
		{
			this.pivot.LookAt(this.targetPos);
			this.SetisFlashlightWeight(1f, 0.3f, 1f);
		}
		else
		{
			this.SetisFlashlightWeight(0.3f, 0, 0);
		}
	}*/

	/*private void SetisFlashlightWeight(float weight, float bodyWeight, float headWeight)
	{
		this.anim.SetLookAtWeight(weight, bodyWeight, headWeight);
		this.anim.SetLookAtPosition(this.targetPos.position);
	}*/
	#endregion

	void OnAnimatorIK()
	{
		if (is_FlashlightAim || is_KnifeAim || is_PistolAim)
		{
			anim.SetLookAtWeight(lookIKWeight, bodyWeight);
			anim.SetLookAtPosition(targetPosVec);
		}

		if (rightHandTarget != null || leftHandTarget != null)
		{
            if (is_PistolAim) 
			{
				anim.SetIKPositionWeight(AvatarIKGoal.RightHand, handWeight);
				anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);

				anim.SetIKRotationWeight(AvatarIKGoal.RightHand, handWeight);
				anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);


				anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, handWeight);
				anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);

				anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, handWeight);
				anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
			}
		}
	}

	void GetFromSoundEvent()
	{
		AudioManager.instance.Play(getFrom);
	}
}