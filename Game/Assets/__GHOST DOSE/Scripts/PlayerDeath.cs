
using UnityEngine;
using GameManager;
using NetworkSystem;


public class PlayerDeath : MonoBehaviour
{
    private GameObject death;
    public GameObject deathAnimator;
    private static utilities util;
    public bool otherPlayer = false;

    void Start()
    {
        util = new utilities();
        if (!otherPlayer) { death = Instantiate(NetworkDriver.instance.myRig, transform.position, transform.rotation); }
        else { death = Instantiate(NetworkDriver.instance.theirRig, transform.position, transform.rotation); }
        death.transform.SetParent(deathAnimator.transform);
        StartCoroutine(util.ReactivateAnimator(deathAnimator));
    }

}
