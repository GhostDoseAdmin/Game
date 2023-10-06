
using UnityEngine;
using GameManager;
using NetworkSystem;
using GameManager;
using InteractionSystem;


public class PlayerDeath : MonoBehaviour
{
    private GameObject death;
    public GameObject deathAnimator;
    private static utilities util;
    public bool otherPlayer = false;
    private GameObject reviveIndicator;

    private void Awake()
    {
        reviveIndicator = GameDriver.instance.reviveIndicator;
    }
    void Start()
    {
        util = new utilities();
        if (!otherPlayer) { death = Instantiate(GameDriver.instance.myRig, transform.position, transform.rotation); }
        else { death = Instantiate(GameDriver.instance.theirRig, transform.position, transform.rotation); }
        death.transform.SetParent(deathAnimator.transform);
        StartCoroutine(util.ReactivateAnimator(deathAnimator));
        deathAnimator.GetComponentInChildren<K2>().gameObject.SetActive(false);

        //DEATH SOUND
        string audioString;
        if (!otherPlayer)
        { //PLAYER
            if (GameDriver.instance.Player.GetComponent<PlayerController>().isTravis) { audioString = "travdeath"; } else { audioString = "wesdeath"; }
        }
        else
        {//CLIENT
            if (GameDriver.instance.Client.GetComponent<ClientPlayerController>().isTravis) { audioString = "travdeath"; } else { audioString = "wesdeath"; }

        }
        AudioManager.instance.Play(audioString, null);
    }
    public void Update()
    {
        if (reviveIndicator.activeSelf) { GameDriver.instance.WriteGuiMsg("REVIVING", 1f, false, Color.green); }
        if (reviveIndicator)
        {
            if (reviveIndicator.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length > 0 && reviveIndicator.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name=="reviveDoneAni") 
            {
                Debug.Log("Player REVIVED");
                if (GameDriver.instance.Player.GetComponent<HealthSystem>().dead) {
                    GameDriver.instance.Player.gameObject.SetActive(true);
                    GameDriver.instance.Player.GetComponent<HealthSystem>().Revive();
                    GameDriver.instance.mainCam.SetActive(true);
                    GameDriver.instance.DeathCam.SetActive(false);
                }
                reviveIndicator.SetActive(false);
                Destroy(this.gameObject);
            }
        }

        //
        if (GetComponent<ClientPlayerController>().hp <= 0) { reviveIndicator.SetActive(false); }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<ClientPlayerController>() != null)
        {
            reviveIndicator.SetActive(true);
            reviveIndicator.GetComponent<Animator>().Play("Reviving");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<ClientPlayerController>() != null) 
        {
            reviveIndicator.SetActive(false);
        }
    } 
}
