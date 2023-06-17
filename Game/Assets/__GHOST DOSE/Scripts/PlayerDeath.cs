
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
       // AudioSource thisPlayerSource;
        string audioString;
        if (NetworkDriver.instance.isTRAVIS) { audioString = "travdeath"; }
        else { audioString = "wesdeath"; }
        if (!otherPlayer)
        { //PLAYER
           // thisPlayerSource = GameDriver.instance.Player.GetComponent<PlayerController>().audioSourceSpeech;
        }
        else
        {//CLIENT
           // thisPlayerSource = GameDriver.instance.Client.GetComponent<ClientPlayerController>().audioSourceSpeech;
        }
        AudioManager.instance.Play(audioString, null);
    }
    public void Update()
    {
        if (reviveIndicator)
        {

            if (reviveIndicator.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length > 0 && reviveIndicator.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name=="reviveDoneAni") 
            {
                Debug.Log("Player REVIVED");
                if (GameDriver.instance.Player.GetComponent<HealthSystem>().dead) {
                    GameDriver.instance.Player.gameObject.SetActive(true);
                    GameDriver.instance.Player.GetComponent<HealthSystem>().Revive();
                }
                reviveIndicator.SetActive(false);
                Destroy(this.gameObject);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            reviveIndicator.SetActive(true);
            reviveIndicator.GetComponent<Animator>().Play("Reviving");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            reviveIndicator.SetActive(false);
        }
    }
}
