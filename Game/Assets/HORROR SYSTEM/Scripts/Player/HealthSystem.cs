using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
	[Header("HEALTH PARAMETRS")]
	[Space(10)]
	public float maxHealth;
	public float Health;
	[Space(10)]
	public float restoringHealth;
	public float reducedHealth;

	[Header("FIRSTAIDKIT PARAMETERS")]
	[Space(10)]
	[SerializeField] public int kitCount = 1;
	[SerializeField] private float replaceKitTimer = 1.0f;
	[SerializeField] private float maxReplaceKitTimer = 1.0f;

	[Header("HEALTH UI")]
	[Space(10)]
	[SerializeField] private Image healthLevel = null;
	[SerializeField] private Text kitCountUI = null;
	[SerializeField] private Image kitIndicator = null;
	[SerializeField] private Image bloodEffect = null;
	[SerializeField] private float healthPrecent;

	[Header("PLAYER RAGDOLL")]
	[Space(10)]
	public GameObject ragdoll;
	public GameObject cameraPlayer;
	public static bool dead = false;
	public bool deadPlayer = false;

	[Header("HEALTH SOUNDS")]
	[Space(10)]
	[SerializeField] private string pickUp;
	[SerializeField] private string treatmentKit;

	private bool shouldUpdate = false;

	public static HealthSystem kitinstance;


	private void Awake()
	{
		if (kitinstance != null) 
		{ 
			Destroy(gameObject); 
		}
		else 
		{ 
			kitinstance = this; 
		}

		if (kitCount < 1)
		{
			kitIndicator.enabled = false;
		}
		else if (kitCount > 0)
		{
			kitIndicator.enabled = true;
		}
	}

	void Start()
	{
		dead = false;
		kitCountUI.text = kitCount.ToString("0");
	}

    public void CollectKit(int firstkit)
	{
		kitCount = kitCount + firstkit;
		kitCountUI.text = kitCount.ToString("0");
		kitIndicator.enabled = true;
		AudioManager.instance.Play(pickUp);
	}

	void Update()
	{
		HealthPlayer();
		Treatment();
	}

	void UpdateHealth()
    {
		healthPrecent = (maxHealth - Health) / 100;
		bloodEffect.color = new Color (255, 0, 0, healthPrecent);
	}

	void HealthPlayer()
	{
		if (Health <= 0)
		{
			Death();
		}

		if (Health < maxHealth)
		{
			Health += (restoringHealth / 10) * Time.deltaTime;
			healthLevel.fillAmount += (restoringHealth / 1000) * Time.deltaTime;
			UpdateHealth();
		}

		if (Health >= maxHealth)
		{
			Health = maxHealth;
		}
	}

	public void HealthDamage(int damage)
    {
		Health -= damage;
		healthLevel.fillAmount -= damage * 0.01f;
		UpdateHealth();
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.tag == "DamageBox")
		{
			Health -= (reducedHealth) * Time.deltaTime;
			healthLevel.fillAmount -= ((reducedHealth / 100) * Time.deltaTime);
			UpdateHealth();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Damage")
		{
			Health -= reducedHealth;
			healthLevel.fillAmount -= (reducedHealth / 100);
			UpdateHealth();
		}
	}

	public void Death()
	{
		gameObject.SetActive(false);
		//cameraPlayer.SetActive(false);
		Instantiate(ragdoll, transform.position, transform.rotation);
		dead = true;
		deadPlayer = true;
	}

	void Treatment()
    {
		if (Input.GetKey(InputManager.instance.treatment) && kitCount > 0 && Health < maxHealth)
		{
			shouldUpdate = false;
			replaceKitTimer -= Time.deltaTime;
			kitIndicator.fillAmount = replaceKitTimer;

			if (replaceKitTimer <= 0)
			{
				kitCount--;
				kitCountUI.text = kitCount.ToString("0");
				Health += maxHealth;
				AudioManager.instance.Play(treatmentKit);
				healthLevel.fillAmount = maxHealth;
				replaceKitTimer = maxReplaceKitTimer;
				kitIndicator.fillAmount = maxReplaceKitTimer;
				UpdateHealth();

				if (kitCount < 1)
				{
					kitIndicator.enabled = false;
				}
			}
		}
        else
        {
			if (shouldUpdate)
			{
				replaceKitTimer += Time.deltaTime;
				kitIndicator.fillAmount = replaceKitTimer;

				if (replaceKitTimer >= maxReplaceKitTimer)
				{
					replaceKitTimer = maxReplaceKitTimer;
					kitIndicator.fillAmount = maxReplaceKitTimer;
					shouldUpdate = false;
				}
			}
		} 
	}
}
