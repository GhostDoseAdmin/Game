using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using NetworkSystem;
using GameManager;

public class Aiming : MonoBehaviour {

	[Range(10,60)]
	public int zoom = 20;
	public int startZoom;
	[Header("Sight size in height")]
	public int height = 40;
	[Header("Sight size in width")]
	public int width = 40;
	public GameObject crosshair;
    public GameObject K2;
	public int gear;
	public bool isOuija;

    private int smoothZoom = 5;//10
	public int normal = 60;//field of view
	private float isZoomed = 0;

	public GameObject player;
	private MobileController gamePad;

	void Start()
    {
		isOuija = false;
        startZoom = zoom;

        crosshair.SetActive(false);
        K2.SetActive(false);

        gamePad = GameDriver.instance.Player.GetComponent<PlayerController>().gamePad;

    }

	void Update()
	{

            if (gear == 1) { zoom = 35; }
			if (gear == 2) { zoom = 50; }
			if (isOuija) { zoom = 20; }

		bool aim = false;
		if(!NetworkDriver.instance.isMobile)
		{
            if (player.GetComponent<PlayerController>().gearAim == true) { aim = true; }
            //if (Input.GetMouseButtonUp(1)) { aim = false; }
        }
		if (NetworkDriver.instance.isMobile && gamePad.aimer.indicator && GameDriver.instance.Player.GetComponent<ShootingSystem>().target!=null && GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.aimer.crossHairTarg) { 
			aim = true;
            crosshair.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(GameDriver.instance.Player.GetComponent<ShootingSystem>().target.transform.position + Vector3.up);

        }
       
        if (aim)
        {
			//if (Input.GetMouseButton(1))
			{
				isZoomed = 1;
				if (gear == 1) { crosshair.SetActive(true); }
				if (gear == 2) { K2.SetActive(true); }
            }
			if (isZoomed == 1 && !NetworkDriver.instance.isMobile)
			{
				GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, zoom, Time.deltaTime * smoothZoom);
			}
		}
		else

        {
			isZoomed = 0;
            crosshair.SetActive(false);
            K2.SetActive(false);
        }
		if (isZoomed == 0)
		{
			if (!NetworkDriver.instance.isMobile) { GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, normal, Time.deltaTime * smoothZoom); }
		}
	}
}
