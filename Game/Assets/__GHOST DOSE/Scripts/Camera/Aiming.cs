using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using NetworkSystem;
using GameManager;
using UnityEngine.UIElements;

public class Aiming : MonoBehaviour {

	[Range(10,60)]
	public int zoom = 20;
	public int startZoom;
	[Header("Sight size in height")]
	public int height = 40;
	[Header("Sight size in width")]
	public int width = 40;
	public GameObject crosshair, gridcrosshair;
    public GameObject K2;
	public int gear;
	public bool isOuija;

    private int smoothZoom = 5;//10
	public int normal = 60;//field of view
	private float isZoomed = 0;

	public GameObject player;
	private MobileController gamePad;
	public bool aim;
	void Start()
    {
		isOuija = false;
        startZoom = zoom;

        crosshair.SetActive(false);
        K2.SetActive(false);

		if (NetworkDriver.instance.isMobile)
		{
			gamePad = GameDriver.instance.Player.GetComponent<PlayerController>().gamePad;
			crosshair.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
			crosshair.transform.GetChild(2).gameObject.SetActive(false);
            crosshair.transform.GetChild(3).gameObject.SetActive(false);
        }

    }

	void Update()
	{

            if (gear == 1) { zoom = 35; } //cam
		    if (gear == 2) { zoom = 50; } //k2
			if (gear == 3) { zoom = 70; }
			if (gear == 4) { zoom = 70; } //laser
			if (isOuija) { zoom = 20; }

		aim = false;
		if(!NetworkDriver.instance.isMobile)
		{
            if (player.GetComponent<PlayerController>().gearAim == true) { aim = true; }
			//if (Input.GetMouseButtonUp(1)) { aim = false; }
		}
		else
		{
			if (GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.camSup.AIMMODE && !GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.camSup.CAMFIX) { aim = true; }
            //crosshair.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(GameDriver.instance.Player.GetComponent<ShootingSystem>().targetLook.transform.position + Vector3.up);

        }
        //Debug.Log("-------------------------CROSS HAIRS " + crosshair.activeSelf);

        /*if (NetworkDriver.instance.isMobile && gamePad.aimer.AIMING && GameDriver.instance.Player.GetComponent<ShootingSystem>().target!=null && GameDriver.instance.Player.GetComponent<PlayerController>().gamePad.aimer.crossHairTarg) { 
			aim = true;
            crosshair.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(GameDriver.instance.Player.GetComponent<ShootingSystem>().target.transform.position + Vector3.up);

        }*/

        if (aim)
        {
			//if (Input.GetMouseButton(1))
			{
				isZoomed = 1;
				if (gear == 3) { isZoomed = 0; }
				if (gear == 1 || gear==4 || gear==3) { 
					crosshair.SetActive(true);
					
					if (gear == 1) { crosshair.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f); }
					if (gear == 4)  { gridcrosshair.SetActive(true); crosshair.transform.localScale = new Vector3(1f, 1f, 1f); }
				
				}
				if (gear == 2) { K2.SetActive(true); }
            }
			if (isZoomed == 1 )
			{
				if (!NetworkDriver.instance.isMobile) { GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, zoom, Time.deltaTime * smoothZoom); }
			}
		}
		else
        {
			isZoomed = 0;
            crosshair.SetActive(false);
            gridcrosshair.SetActive(false);
            K2.SetActive(false);
        }
		if (isZoomed == 0)
		{
			if (!NetworkDriver.instance.isMobile) { GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, normal, Time.deltaTime * smoothZoom); }
		}

       
    }
}
