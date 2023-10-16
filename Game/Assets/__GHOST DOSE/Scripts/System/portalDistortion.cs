using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManager;
using UnityEngine.SceneManagement;

public class portalDistortion : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject distortionFX;
    void Start()
    {
        distortionFX = transform.GetChild(2).gameObject;
        if (SceneManager.GetActiveScene().name != "Lobby") { Invoke("TogglePortal", 3); }
    }

    void TogglePortal()
    {
        distortionFX.SetActive(false);
        if (Vector3.Distance(GameDriver.instance.Player.transform.position, transform.position) < 5) { distortionFX.SetActive(true); }
        //if (Vector3.Distance(GameDriver.instance.Client.transform.position, transform.position) < 5) { distortionFX.SetActive(true); }
        Invoke("TogglePortal", 3);
    }

}
