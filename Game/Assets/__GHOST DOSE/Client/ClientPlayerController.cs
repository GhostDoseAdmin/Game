using InteractionSystem;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
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

	Animator anim;

	public Vector3 targetPosVec;
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
	public bool toggleFlashlight = false;//command sent from other player to turn on/off flashlight
	public bool aim = false;
	public bool triggerShoot;
    private NetworkDriver ND;
	

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





    #region Start
    void Start()
	{
        ND = GameObject.Find("GameController").GetComponent<NetworkDriver>();

        anim = GetComponent<Animator>();

        rightHandTrans = rightHand != null ? rightHand.GetComponentsInChildren<Transform>() : new Transform[0];
		leftHandTrans = leftHand != null ? leftHand.GetComponentsInChildren<Transform>() : new Transform[0];

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
    #endregion

    private void FixedUpdate()
    {
		if (!ND.twoPlayer){ return; }

		//------------------------------------- M A I N ---------------------------------------------------
        targetPosVec = Vector3.Lerp(targetPosVec, targetPos.position, 0.1f);

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


	void Attack()
	{
			if (aim)
			{

				is_PistolAim = true;
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
                    RaycastHit hit;
                    if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, distance))
                    {
                        if (hit.transform.GetComponent<Rigidbody>())
                        {
                            hit.transform.GetComponent<Rigidbody>().AddForceAtPosition(shootPoint.forward * force, hit.point);
                        }
                    }
                    GameObject myBullet = Instantiate(bullet);
                    myBullet.transform.position = shootPoint.position;
                    myBullet.transform.rotation = shootPoint.rotation;
                    Destroy(myBullet, shootFireLifeTime);
					
					triggerShoot = false;

                }
                else // if (Input.GetMouseButtonUp(0))
                {
                    anim.SetBool("Shoot", false);
                }

            }
			else 
			{
				is_PistolAim = false;
				anim.SetBool("Pistol", false);
				anim.SetBool("Shoot", false);
				newHandWeight = 0f;
				canShoot = false;

			}
			handWeight = Mathf.Lerp(handWeight, newHandWeight, Time.deltaTime * handSpeed);
		
	}
	#endregion


	public void Flashlight(bool on)//TRIGGRED BY EMIT
	{
		is_FlashlightAim = false;
		is_Flashlight = on;

		//TOGGLE FLASHLIGHT
		if ((GetComponent<ClientFlashlightSystem>().FlashLight.enabled == false && on) || (GetComponent<ClientFlashlightSystem>().FlashLight.enabled == true && !on)){
			GetComponent<ClientFlashlightSystem>().Flashlight(); 
        }

            if (aim == false)
			{
            gameObject.GetComponent<ClientFlashlightSystem>().WeaponLight.enabled = false;
				if (on)
				{
					anim.SetBool("Flashlight", true);//regular flashlight hold
				}
				if (!on)
				{
					anim.SetBool("Flashlight", false);//regular flashlight hold
				}

			}

        if (aim == true)
        {
            anim.SetBool("Flashlight", false);//regular flashlight hold

            if (on)
			{ 
				is_FlashlightAim = true;
                anim.SetBool("Flashlight", false);
                gameObject.GetComponent<ClientFlashlightSystem>().handFlashlight.SetActive(false);
                gameObject.GetComponent<ClientFlashlightSystem>().FlashLight.enabled = false;
                gameObject.GetComponent<ClientFlashlightSystem>().WeaponLight.enabled = true;

            }

        }

    }




    void OnAnimatorIK()
    {
		
        if (is_FlashlightAim || is_KnifeAim || is_PistolAim || is_Flashlight)
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