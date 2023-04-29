using UnityEngine;
using InteractionSystem;
using TMPro;

public class SB7 : MonoBehaviour
{
    private bool isClient;
    private float timer = 0f;
    private float delay = 0.20f;

    // Start is called before the first frame update
    void Start()
    {
        if (transform.root.name == "CLIENT") { isClient = true; }
        else if (transform.root.name == "WESTIN" || transform.root.name == "TRAVIS") { isClient = false; }
        else { DestroyImmediate(this.gameObject); }//DEAD PLAYER
    }

    // Update is called once per frame
    void Update()
    {
        //-----CHANGE STATION---------
        if (Time.time > timer + delay)
        {
            float station = Mathf.Round(Random.Range(0f, 99.99f) * 100) / 100;
            transform.GetChild(0).GetComponent<TextMeshPro>().text = station.ToString();
            timer = Time.time;//cooldown
        }
    }
    private void OnDisable()
    {
        AudioManager.instance.StopPlaying("sb7sweep");
    }
    private void OnEnable()
    {
        AudioManager.instance.Play("sb7sweep");
    }
}
