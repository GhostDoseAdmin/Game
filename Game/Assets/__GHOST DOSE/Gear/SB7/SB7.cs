using UnityEngine;
using InteractionSystem;
using TMPro;
using GameManager;
using System.Collections.Generic;

public class SB7 : MonoBehaviour
{
    private bool isClient;
    private float timer = 0f;
    private float delay = 0.20f;
    private AudioSource audioSourceSweep;
    GameObject victim;
    int questionIndex;
    public List<Sound> QuestionsTravis;
    public List<Sound> QuestionsWestin;
    public List<Sound> AnswersYoung;
    public List<Sound> AnswersEvil;
    public List<Sound> AnswersMurdered;
    private GameObject currentColdSpot; 
    private float question_timer = 0f;
    private float question_delay = 5f;
    private bool askedQuestion;
    private AudioSource audioSourceVoices;
    // Start is called before the first frame update
    void Start()
    {

        audioSourceSweep = gameObject.AddComponent<AudioSource>();
        audioSourceSweep.spatialBlend = 1.0f;
        audioSourceVoices = gameObject.AddComponent<AudioSource>();
        audioSourceVoices.spatialBlend = 1.0f;

        if (GameDriver.instance.Player.transform.parent.name == "CLIENT") { isClient = true; }
        else if (GameDriver.instance.Player.transform.parent.name == "WESTIN" || GameDriver.instance.Player.transform.parent.name == "TRAVIS") { isClient = false;  }
        else { DestroyImmediate(this.gameObject); }//DEAD PLAYER

        gameObject.SetActive(false);
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



        // ChosenVictim = Victims[Random.Range(0, Victims.Count)];

        //ASK QUESTION
        //if (Time.time > question_timer + question_delay)
        {
            if (!askedQuestion) { AskQuestion(); askedQuestion = true; }
        
            //question_timer = Time.time;//cooldown
        }



    }
    void AskQuestion()
    {
        ColdSpot[] coldSpots = FindObjectsOfType<ColdSpot>();
        float closestDistance = 9999;
        foreach (ColdSpot coldspot in coldSpots)
        {
            float coldSpotDistance = Vector3.Distance(coldspot.gameObject.transform.position, transform.position);
            if (coldSpotDistance < closestDistance) { closestDistance = coldSpotDistance; currentColdSpot = coldspot.gameObject; }
        }
        Debug.Log("CURRENT COLD SPOT" + currentColdSpot.name);
        List<Sound> questionList = new List<Sound>();
        if (transform.root.name == "TRAVIS") { questionList = QuestionsTravis; }
        else { questionList = QuestionsWestin; }

        questionIndex = currentColdSpot.GetComponent<ColdSpot>().questionIndexYoungEvilMurder;//Random.Range(0, questionList.Count);
        audioSourceVoices.clip = questionList[questionIndex].clip;
        audioSourceVoices.pitch = 1f;
        audioSourceVoices.Play();
        
        Invoke("GetAnswer", 2f);
    }

    void GetAnswer()
    {
        victim = GameObject.Find("VictimManager").GetComponent<VictimControl>().ChosenVictim;
        Sound s = null ;
        //YOUNG?
        if (questionIndex == 0)
        {
            if (victim.GetComponent<Person>().isYoung) {s = AnswersYoung[1]; }//YES
            else { s = AnswersYoung[0]; }//NO
        }
        //EVIL?
        if (questionIndex == 1)
        {
            if (victim.GetComponent<Person>().isEvil) { s = AnswersEvil[1]; }//YES
            else { s = AnswersEvil[0]; }//NO
        }
        //MURDRERED?
        if (questionIndex == 2)
        {
            if (victim.GetComponent<Person>().isMurdered) { s = AnswersMurdered[1]; }//YES
            else {s = AnswersMurdered[0]; }//NO
        }
        if (s != null)
        {
            audioSourceVoices.clip = s.clip;
            audioSourceVoices.pitch = s.pitch * (1f + Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            audioSourceVoices.Play();
        }

        Invoke("GotAnswer", 2f);
    }

    void GotAnswer()
    {
        askedQuestion = false;
    }
}
