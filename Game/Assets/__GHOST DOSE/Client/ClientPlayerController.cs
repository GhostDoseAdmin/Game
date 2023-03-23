using InteractionSystem;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;

public class ClientPlayerController : MonoBehaviour
{
	[Header("PLAYER PARAMETRS")]
	[Space(10)]
	public float lookIKWeight;
	public float bodyWeight;
	public Transform targetPos;
	public float angularSpeed;
	bool isPlayerRot;
	public float luft;
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


	[Header("PLAYER SOUND")]
	[Space(10)]
	[SerializeField] private string getFrom;

	private Transform shoulder;
	private Transform pivot;

	Animator anim;

	public Vector3 targetPosVec;
	float newRunWeight = 1f;
	public float walk = 0f;
	public float strafe = 0f;
    public float targStrafe = 0f;
    public float targWalk = 0f;

    //NETWORKER
    public string animation;
	public bool state = false;
	public float Float;
	public Vector3 destination;
	public float speed;
	public bool running;
	public Vector3 targetRotation;
	public bool flEmit = false;
	public bool aim = false;

	#region Start
	void Start()
	{
		anim = GetComponent<Animator>();

        rightHandTrans = rightHand != null ? rightHand.GetComponentsInChildren<Transform>() : new Transform[0];
		leftHandTrans = leftHand != null ? leftHand.GetComponentsInChildren<Transform>() : new Transform[0];

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
    #endregion

    private void FixedUpdate()
    {
        //targetPosVec = targetPos.position;
        targetPosVec = Vector3.Lerp(targetPosVec, targetPos.position, 0.1f);


        if (speed>0f)
		{
			strafe = Mathf.Lerp(strafe, targStrafe, 0.1f);
            walk = Mathf.Lerp(walk, targWalk, 0.1f);

            anim.SetFloat("Strafe", strafe);
            anim.SetFloat("Walk", walk);
			if (running) { anim.SetBool("Running", true);  }
			else { anim.SetBool("Running", false); }
        }
		PistolAttack();
        if (flEmit) { CheckFlashlight();  GetComponent<ClientFlashlightSystem>().flEmit = true;  }


        if (speed == 0) { speed = 4f; } //almost move to target destination
		//KEEP POS UPDATED
        if(Vector3.Distance(transform.position, destination)>1.5)
		{
            transform.position = new Vector3(destination.x, destination.y, destination.z);
        }
        transform.position = Vector3.MoveTowards(transform.position, destination, speed *0.95f * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetRotation), 150f * Time.deltaTime);


    }


    #region Update
    void Update() 
	{
		


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
		/*if (Input.GetKeyDown(InputManager.instance.pistol) && is_Knife == false && is_Pistol == false)
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
		}*/
	}

	void PistolAttack()
	{
		//if (is_Pistol)
		{
			if (aim)
			{
				if (is_FlashlightAim)//if flashlight is on
                {
					anim.SetBool("Flashlight", false);
					gameObject.GetComponent<ClientFlashlightSystem>().handFlashlight.SetActive(false);
					gameObject.GetComponent<ClientFlashlightSystem>().flashlightSpot.enabled = false;
					gameObject.GetComponent<ClientFlashlightSystem>().flashlightSpotPistol.enabled = true;
				}
				else
                {
					gameObject.GetComponent<ClientFlashlightSystem>().flashlightSpotPistol.enabled = false;
				}

				is_PistolAim = true;
				anim.SetBool("Pistol", true);
				newHandWeight = 1f;
				canShoot = true;

				/*if (Input.GetMouseButton(0))
				{
					anim.SetBool("Shoot", true);
					shootPistol.Shoot();
				}
				else if (Input.GetMouseButtonUp(0))
				{
					anim.SetBool("Shoot", false);
				}*/
			}
			else //if (Input.GetMouseButtonUp(1))
			{
				is_PistolAim = false;
				anim.SetBool("Pistol", false);
				anim.SetBool("Shoot", false);
				newHandWeight = 0f;
				canShoot = false;

				if (is_FlashlightAim)
				{
					anim.SetBool("Flashlight", true);
					gameObject.GetComponent<ClientFlashlightSystem>().handFlashlight.SetActive(true);
					gameObject.GetComponent<ClientFlashlightSystem>().flashlightSpot.enabled = true;
					gameObject.GetComponent<ClientFlashlightSystem>().flashlightSpotPistol.enabled = false;
				}
			}
			handWeight = Mathf.Lerp(handWeight, newHandWeight, Time.deltaTime * handSpeed);
		}
	}
	#endregion


	public void CheckFlashlight()
    {
		if (is_FlashlightAim == false)
		{
				is_Flashlight = true;
				is_FlashlightAim = true;
				anim.SetBool("Flashlight", true);

			
		}
		else if (is_FlashlightAim == true)
		{
			is_Flashlight = false;
			is_FlashlightAim = false;
			anim.SetBool("Flashlight", false);
		}

        /*if (gameObject.GetComponent<FlashlightSystem>().flashlightSpot.intensity <= 0)
        {
			is_Flashlight = false;
			is_FlashlightAim = false;
			anim.SetBool("Flashlight", false);
		}*/

        flEmit = false;
    }




    void OnAnimatorIK()
    {
		
        if (is_FlashlightAim || is_KnifeAim || is_PistolAim)
        {
            
            anim.SetLookAtWeight(lookIKWeight, bodyWeight);
            anim.SetLookAtPosition(targetPosVec);

           // Debug.Log("LOOOOOOOOOOOOOOOOOOOOOKING" + targetPosVec);
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