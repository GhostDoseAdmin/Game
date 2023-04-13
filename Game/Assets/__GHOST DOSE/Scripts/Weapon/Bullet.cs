using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bullet : MonoBehaviour
{
	[Header("Bullet Speed")]
	public int Speed;
	public int damageAmount = 20;
	[Header("Target Effect")]
	Vector3 lastPos;
	public GameObject hitStoneEffect;
	public GameObject hitBloodEffect;
	public GameObject hitWoodEffect;
	public GameObject hitMetalEffect;
	public GameObject hitSandEffect;

	[Header("Bullet Damage")]
	public static int bulletDamage = 50;

	void Start()
	{
		lastPos = transform.position;
	}

	void Update()
	{
		transform.Translate(Vector3.forward * Speed * Time.deltaTime);

		RaycastHit hit;
		Debug.DrawLine(lastPos, transform.position);
		if (Physics.Linecast(lastPos, transform.position, out hit))
		{
			if ((hit.collider.tag == "Ghost" || hit.collider.tag == "Shadower"))
			{
				hit.transform.root.GetComponent<NPCController>().TakeDamage(0, false); //damageAmount

				GameObject h = Instantiate(hitBloodEffect);
				h.transform.position = hit.point + hit.normal * 0.001f;
				h.transform.rotation = Quaternion.LookRotation(-hit.normal);

				Destroy(h, 2);
				Destroy(gameObject);
			}

			if ((hit.collider.tag == "Wood"))
			{
				GameObject h = Instantiate(hitWoodEffect);
				h.transform.position = hit.point + hit.normal * 0.001f;
				h.transform.rotation = Quaternion.LookRotation(-hit.normal);

				Destroy(h, 2);
				Destroy(gameObject);
			}

			if ((hit.collider.tag == "Metal"))
			{
				GameObject h = Instantiate(hitMetalEffect);
				h.transform.position = hit.point + hit.normal * 0.001f;
				h.transform.rotation = Quaternion.LookRotation(-hit.normal);

				Destroy(h, 2);
				Destroy(gameObject);
			}

			if ((hit.collider.tag == "Blood"))
			{
				GameObject h = Instantiate(hitBloodEffect);
				h.transform.position = hit.point + hit.normal * 0.001f;
				h.transform.rotation = Quaternion.LookRotation(-hit.normal);

				Destroy(h, 2);
				Destroy(gameObject);
			}

			if ((hit.collider.tag == "Sand"))
			{
				GameObject h = Instantiate(hitSandEffect);
				h.transform.position = hit.point + hit.normal * 0.001f;
				h.transform.rotation = Quaternion.LookRotation(-hit.normal);

				Destroy(h, 2);
				Destroy(gameObject);
			}

			if ((hit.collider.tag == "Stone"))
			{
				GameObject h = Instantiate(hitStoneEffect);
				h.transform.position = hit.point + hit.normal * 0.001f;
				h.transform.rotation = Quaternion.LookRotation(-hit.normal);

				Destroy(h, 2);
				Destroy(gameObject);
			}
		}
		lastPos = transform.position;
	}
}