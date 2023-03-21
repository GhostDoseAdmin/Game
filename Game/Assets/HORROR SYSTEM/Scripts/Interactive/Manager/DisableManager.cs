using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableManager : MonoBehaviour
{
    public static DisableManager instance;

    [SerializeField] private PlayerController player = null;
    [SerializeField] private Camera_Supplyer cameraPlayer = null;
    [SerializeField] private Animator animatorPlayer = null;


    void Awake()
    {
        if (instance != null) 
        { 
            Destroy(gameObject); 
        }
        else 
        { 
            instance = this; 
        }
    }

    public void DisablePlayer(bool disable)
    {
        if (disable)
        {
            player.enabled = false;
            animatorPlayer.enabled = false;
            cameraPlayer.enabled = false;
        }

        else
        {
            player.enabled = true;
            animatorPlayer.enabled = true;
            cameraPlayer.enabled = true;
        }
    }
}
