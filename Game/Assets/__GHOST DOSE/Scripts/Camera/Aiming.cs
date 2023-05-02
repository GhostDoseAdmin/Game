using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Aiming : MonoBehaviour {

	[Range(10,60)]
	public int zoom = 20;
	[Header("Sight size in height")]
	public int height = 40;
	[Header("Sight size in width")]
	public int width = 40;
	public GameObject crosshair;
    public GameObject K2;
	public int gear;

    private int smoothZoom = 5;//10
	public int normal = 60;//field of view
	private float isZoomed = 0;

	public GameObject player;
	private GameObject canvas;

	void Start()
    {
		crosshair.SetActive(false);
        K2.SetActive(false);

    }

	void Update()
	{
		if(gear==1) { zoom = 35; }
		if(gear==2) { zoom = 50;}

		if (player.GetComponent<PlayerController>().gearAim == true)
        {
			if (Input.GetMouseButton(1))
			{
				isZoomed = 1;
				if (gear == 1) { crosshair.SetActive(true); }
				if (gear == 2) { K2.SetActive(true); }
            }
			if (isZoomed == 1)
			{
				GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, zoom, Time.deltaTime * smoothZoom);
			}
		}
		if (Input.GetMouseButtonUp(1) || player.GetComponent<PlayerController>().gearAim == false)
		{
			isZoomed = 0;
            crosshair.SetActive(false);
            K2.SetActive(false);
        }
		if (isZoomed == 0)
		{
			GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, normal, Time.deltaTime * smoothZoom);
		}
	}
}
