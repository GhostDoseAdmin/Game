using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientWeaponParameters : MonoBehaviour
{
    [Header("KNIFE PARAMETERS")]
    [Space(10)]
    [SerializeField] public bool hasKnife = false;
    [SerializeField] public GameObject handKnife;
    [SerializeField] public GameObject inventoryKnife;

    [Header("PISTOL PARAMETERS")]
    [Space(10)]
    [SerializeField] public bool hasPistol = false;
    [SerializeField] public GameObject handPistol;
    [SerializeField] public GameObject inventoryPistol;

    [Header("KNIFE SOUNDS")]
    [Space(10)]
    [SerializeField] private string pickUp;
    [SerializeField] private string attackKnife;

    public static ClientWeaponParameters instance;

    private void Awake()
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

    void Start()
    {
        handKnife.SetActive(false);
        inventoryKnife.SetActive(false);
        handKnife.GetComponent<Collider>().enabled = false;

        handPistol.SetActive(false);
        inventoryPistol.SetActive(false);
    }




    #region Knife
    void EnableKnife()
    {
        handKnife.SetActive(true);
        inventoryKnife.SetActive(false);
    }
    void DisableKnife()
    {
        handKnife.SetActive(false);
        inventoryKnife.SetActive(true);
    }
    public void TriggerEnable()
    {
        handKnife.GetComponent<Collider>().enabled = true;
    }

    public void TriggerDisable()
    {
        handKnife.GetComponent<Collider>().enabled = false;
    }


    #endregion

    #region Pistol
    void EnablePistol()
    {
        handPistol.SetActive(true);
        inventoryPistol.SetActive(false);
    }
    void DisablePistol()
    {
        handPistol.SetActive(false);
        inventoryPistol.SetActive(true);
    }
    #endregion
}
