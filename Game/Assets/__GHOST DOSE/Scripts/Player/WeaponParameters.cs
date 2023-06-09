using InteractionSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponParameters : MonoBehaviour
{
    [Header("KNIFE PARAMETERS")]
    [Space(10)]
    [SerializeField] public bool hasKnife = false;
    [SerializeField] public GameObject handKnife;
    [SerializeField] public GameObject inventoryKnife;

    [Header("PISTOL PARAMETERS")]
    [Space(10)]
    [SerializeField] public bool hasPistol = false;
    [SerializeField] public GameObject weapHand;
    [SerializeField] public GameObject inventoryCam;

    [Header("KNIFE SOUNDS")]
    [Space(10)]
    [SerializeField] private string pickUp;
    [SerializeField] private string attackKnife;

    public static WeaponParameters instance;
    private static utilities util;

    public void RigWeapons()
    {
        util = new utilities();

        handKnife = util.FindChildObject(this.gameObject.transform, "Knife_Hand");
        inventoryKnife = util.FindChildObject(this.gameObject.transform, "Knife_Inventory");
        weapHand = util.FindChildObject(this.gameObject.transform, "WeapHand");
        inventoryCam= util.FindChildObject(this.gameObject.transform, "CamInventory");

    }

    void Start()
    {


        handKnife.SetActive(false);
        inventoryKnife.SetActive(false);
        handKnife.GetComponent<Collider>().enabled = false;

        weapHand.SetActive(true);
        inventoryCam.SetActive(false);
    }

    public void EnableInventoryKnife()
    {
        hasKnife = true;
        inventoryKnife.SetActive(true);
        AudioManager.instance.Play(pickUp, gameObject.GetComponent<PlayerController>().audioSource);
    }

    public void EnableInventoryPistol()
    {
        hasPistol = true;
        inventoryCam.SetActive(true);
        AudioManager.instance.Play(pickUp, gameObject.GetComponent<PlayerController>().audioSource);
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


}
