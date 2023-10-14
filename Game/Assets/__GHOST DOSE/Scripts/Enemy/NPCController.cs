using GameManager;
using InteractionSystem;
using NetworkSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class NPCController : MonoBehaviour
{
    [Header("WAY POINTS")]
    [Space(10)]
    public List<Transform> wayPoint;
    public int curWayPoint;

    [Header("SETUP")]
    [Space(10)]

    public GameObject SKEL_ROOT;
    public GameObject HIT_COL;
    public GameObject Death;

    [Header("ENEMY PARAMETRS")]
    [Space(10)]
    public bool Shadower;
    public bool teddy;
    public bool ZOZO;
    public int healthEnemy = 100;
    public int range;
    [HideInInspector] public int startRange;
    public int angleView;
    [HideInInspector] public int startAngleView;
    public float hitRange = 1.5f;
    public float chaseSpeed = 1f;
    public float spawnTimer;
    public int damage;
    public float force;
    public float retreatThreshold;
    public int unawareness;
    public bool xrayvision;
    public int laserForce;
    public int laserDamage;
    public int persist;
    private int startPersist;
    public bool canRespawn;
    public bool teleports;
    public bool canFlinch;
    public bool zozoLaser;

    //zap
    public bool zapActive;
    private bool canZap;
    public int zapRange;
    private bool zaps;
    public bool brute;

    [HideInInspector] public int startHealth;

    [Header("TESTING")]
    [Space(10)]
    public bool canAttack = true;
    public bool agro = false;//HAUNTS THE PLAYER
    public Transform target;
    public int follow;
    
    //public Transform player;

    [HideInInspector] private Transform head;
    [HideInInspector] public Animator animEnemy;
    [HideInInspector] public NavMeshAgent navmesh;

    //NETWORK
    [HideInInspector] public Transform closestPlayer;
    private GameObject Player;
    private GameObject Client;
    private int hasRetreated;
    public int alertLevelPlayer;
    public int alertLevelClient;
    public bool prevAlerted;
    public bool alerted;
    public GameObject destination;
    [HideInInspector] public Vector3 truePosition;
    [HideInInspector] public bool update;
    public Outline outline;
    [HideInInspector] public bool activateOutline;
    public bool dead;
    private bool engageSound;
    //private float minDist = 0.03f; //debug enemy rotation when ontop of player
    private float distance, p1_dist,p2_dist;
    private bool onlyOnceThisFrame;
    public GameObject activeWayPoint;
    public AudioSource audioSource;
    private AudioClip audioClip;
    private bool hasLooked = true;
    GameObject PlayerWP, ClientWP;
    public GameObject prev_dest;
    public Transform prev_targ;
    public GameObject bruteExplosion;


    private void Awake()
    {
        destination = this.gameObject;
        GetComponent<GhostVFX>().Shadower = Shadower;
        if (GetComponent<ZozoControl>() != null) { ZOZO = true; }
        active_timer = 99999;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
       
    }


    void Start()
    {
        //zapActive = true;
        if (spawnTimer == 0) { spawnTimer = 9999; }
        if (wayPoint.Count == 0)        {            wayPoint = new List<Transform>(); wayPoint.Add(transform);        }

        PlayerWP = GameObject.Find("PlayerWavePoint");
        ClientWP = GameObject.Find("ClientWavePoint");
        //GD = GameObject.Find("GameController").GetComponent<GameDriver>();
        Player = GameDriver.instance.Player;
        Client = GameDriver.instance.Client;

        animEnemy = GetComponent<Animator>();
        navmesh = GetComponent<NavMeshAgent>();

        //closestPlayer = Client.transform;
        head = animEnemy.GetBoneTransform(HumanBodyBones.Head).transform;


        //SINGLE PLAYER HANDICAP
        if (!NetworkDriver.instance.TWOPLAYER)
        {
            if (teddy || brute)
            {
                healthEnemy = (int)(healthEnemy * 0.5f); 
            }
        }

        startHealth = healthEnemy;
        startAngleView = angleView;
        startRange = range;
        startPersist = persist;

        hasRetreated = 0;

        update = false; //UPDATE POSITIONS\
        SKEL_ROOT.GetComponent<CapsuleCollider>().isTrigger = true;
        HIT_COL.GetComponent<SphereCollider>().isTrigger = true;
        HIT_COL.GetComponent<SphereCollider>().enabled = false;
        outline = transform.GetChild(0).GetComponent<Outline>();

        zozoLaser = false;
        if (Shadower && !ZOZO) { zaps = true; canZap = true; }
        //Debug.Log("----------------------------------------" + HIT_COL.GetComponent<SphereCollider>().enabled);
        if (NetworkDriver.instance.isMobile) { head.GetComponent<SphereCollider>().radius = 0.28f; }

    }

    private int syncDeath = 0;
    public void KeepActive(float timer, bool testIfAlive)//CLIENT ONLY
    {
        active_timer = timer;
            //IF DEAD ON CLIENT BUT NOT ON HOST (DEBUG)
            if(testIfAlive && dead)
            {
            syncDeath++;
                if (syncDeath >= 5) //if received 5 messages from HOST - REVIVE - LWV 2
                {
                    syncDeath = 0;
                    dead = false;
                    healthEnemy = 1;
                    this.gameObject.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = true;
                    //if still getting data from host, reveal skin and make not dead and give some health
                }

        }
    }

    public Vector3 serverPosition;
    public float active_timer;
    void Update()
    {



        //-------------------DEBUG TURNS OFF ANIMATION----------------------
        if (!brute)
        {
            //DEBUG ZAP --TURN OFF ZAP
            if (animEnemy.GetCurrentAnimatorClipInfo(0).Length > 0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "zapAni" && zapActive) { zapActive = false; }
            //CLIENT ZAP
            zapClient--;
            if (zapClient > 0)
            {
                if (animEnemy.GetCurrentAnimatorClipInfo(0).Length > 0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "zapAni")
                {
                    animEnemy.Play("zapAni");
                }
            }
            if (dead) { zapActive = false; }
        }
        else
        //BRUTE JUMP
        {
            //DEBUG ZAP --TURN OFF ZAP
            //if (animEnemy.GetCurrentAnimatorClipInfo(0).Length > 0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "zapAni" && zapActive) { zapActive = false; }
            //CLIENT FORE BRUTE JUMP
            zapClient--;
            if (zapClient > 0)
            {
                if (animEnemy.GetCurrentAnimatorClipInfo(0).Length > 0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "zapAni")
                {
                    animEnemy.Play("zapAni");
                }
            }
            if (dead) { zapActive = false; }
            //-----------JUMP TO SPOT
            if (zapActive)
            {
                if (target != null)
                {
                    Debug.Log("--------------------------------------------JUMPING");
                    transform.position = Vector3.Lerp(transform.position, target.transform.position, 1 * Time.deltaTime);
                }

            }
        }


        //---CLIENT SIDE PREDICTION--close position gap
        if (!NetworkDriver.instance.HOST)
        {
            if (target == null)
            {
                if (Vector3.Distance(transform.position, serverPosition) > 0.2f) { transform.position = Vector3.Lerp(transform.position, serverPosition, 0.02f); }

                if (!teddy && !brute)
                {
                    if (Vector3.Distance(transform.position, serverPosition) > 5f) { transform.position = serverPosition; GetComponent<NavMeshAgent>().enabled = false; Invoke("ResetNavmesh", 0.1f); }
                    //if (!agro) { if (Vector3.Distance(transform.position, serverPosition) > 3.5f) { transform.position = serverPosition; } }
                    //else { if (Vector3.Distance(transform.position, serverPosition) > 5f) { transform.position = serverPosition; } }
                }
                if (teddy)
                {
                    if (Vector3.Distance(transform.position, serverPosition) > 5f) { transform.position = serverPosition; GetComponent<NavMeshAgent>().enabled = false; Invoke("ResetNavmesh", 0.1f); }
                }
                if (brute)
                {
                    if (!zapActive) { if (Vector3.Distance(transform.position, serverPosition) > 5f) { transform.position = serverPosition; GetComponent<NavMeshAgent>().enabled = false; Invoke("ResetNavmesh", 0.1f); } }
                }
            }
                //-------------ACTIVE TIMER------------
                active_timer -= Time.deltaTime;
            if (active_timer <= 0)//NO LONGET GETTING MESSAGES FROM SERVER
            {
                //syncDeath = 0;
               this.gameObject.SetActive(false);
            }

        }

        p1_dist = Vector3.Distance(head.position, Player.transform.position);
        p2_dist = Vector3.Distance(head.position, Client.transform.position);

        //ALWAYS CHOOSE CLOSEST TARGET
        if (p1_dist < p2_dist) { distance = p1_dist; closestPlayer = Player.transform; } else { distance = p2_dist; closestPlayer = Client.transform; }
        if (!Player.gameObject.activeSelf) { distance = p2_dist; closestPlayer = Client.transform; }
        if (!Client.gameObject.activeSelf) { distance = p1_dist; closestPlayer = Player.transform; }

        //if (ZOZO) { if (closestPlayer != null) { target = closestPlayer; } }

        float teleport = GetComponent<Teleport>().teleport;

        if (this.gameObject.activeSelf) { if (teleport == 0) { AI(); } }

        if (NetworkDriver.instance.HOST) { 
            if (teleport == 0 && canAttack) { FindTargetRayCast(); } 
        } //dtermines & finds target

        if (teleport > 0) { target = GetComponent<Teleport>().target; }

        //----------------------RESET OUTLINE---------------------
        if (GameObject.Find("K2") == null)
        {
            activateOutline = false;
            if (outline.OutlineWidth > 0) { outline.OutlineWidth -= 0.01f; }
        }
        else
        {
            if (outline.OutlineWidth > 0) { outline.OutlineWidth -= 0.005f; } 
        }
        if (activateOutline){if (outline.OutlineWidth < 7) { outline.OutlineWidth += 0.1f;  } else { activateOutline = false; }}//fade in
        if (distance > 15) { if (outline.OutlineWidth > 0) { outline.OutlineWidth -= 0.1f; activateOutline = false; } }//fade fast out of range
        else { if (!activateOutline) { outline.OutlineWidth -= (distance*0.0025f); } }

        if (outline.OutlineWidth < 0) { outline.OutlineWidth = 0; }



        //RESET NAVMESH - PLAYER DEATH
        //if (target == Client.transform && Client.GetComponent<ClientPlayerController>().hp <= 0) { Invoke("ResetNavmesh", 3f); GetComponent<NavMeshAgent>().enabled = false; }
        // if (target == Player.transform && Player.GetComponent<HealthSystem>().Health <= 0) { Invoke("ResetNavmesh", 3f); GetComponent<NavMeshAgent>().enabled = false; }

        //RESET WAYPOINTS IF PLAYER DIES
        // if (Client.GetComponent<ClientPlayerController>().hp <= 0 && destination == ClientWP) { alertLevelClient = 0; alerted = false; hasLooked = true; Invoke("ResetNavmesh", 3f); }
        // if (Player.GetComponent<HealthSystem>().Health <= 0 && destination == PlayerWP) { alertLevelPlayer = 0; alerted = false; hasLooked = true; Invoke("ResetNavmesh", 3f); }


        if (Client.GetComponent<ClientPlayerController>().hp <= 0) { if (alertLevelClient > 0) { GetComponent<NavMeshAgent>().enabled = false; Invoke("ResetNavmesh", 2f); alertLevelClient = 0;  }  }
        if (Player.GetComponent<HealthSystem>().Health <= 0) { if (alertLevelPlayer > 0) { GetComponent<NavMeshAgent>().enabled = false; alertLevelPlayer = 0; Invoke("ResetNavmesh", 2f); } }


    }


    public void OnDisable()
    {
        transform.GetChild(0).GetComponent<Outline>().OutlineWidth = 0;
        GetComponent<NPCController>().activateOutline = false;
    }
    /*public void OnEnable()
    {
         GetComponent<NavMeshAgent>().enabled = false;
        Invoke("ResetNavmesh", 2f);
    }*/

    public void ClientUpdateDestination(GameObject newDest)
    {
        //ALERTED
        if (destination != PlayerWP && newDest == PlayerWP){ PlayerWP.transform.position = GameDriver.instance.Client.transform.position; hasLooked = false; }
        if (destination != ClientWP && newDest == ClientWP) { ClientWP.transform.position = GameDriver.instance.Player.transform.position;  hasLooked = false; }
        if ((destination == PlayerWP || destination == ClientWP) && newDest != PlayerWP && newDest != ClientWP) {hasLooked = true; alerted = false; }//CANCEL LOOK AROUND

        destination = newDest;
    }

    public void TriggerHasLooked()
    {
        
        hasLooked = true;
        alerted = false;
    }


    void ResetNavmesh()
    {
        Debug.Log("-------------------------RESTTING NAVMESH");
        GetComponent<NavMeshAgent>().enabled = true;
    }


    public void AI()
    {
        if (GetComponent<Teleport>().teleport > 0) {return; }
        if (teddy) { alerted = false; alertLevelPlayer = 0; alertLevelClient = 0; }

        if (target != null) { hasLooked = true; Attack();         }
        //-------------------------WAY POINTS ------------------------
        else if (target == null)
        {
            agro = false;

            //GetComponent<NavMeshAgent>().enabled = true;
            GetComponent<NavMeshAgent>().speed = 0.1f;
            GetComponent<NavMeshAgent>().stoppingDistance = 0;
            animEnemy.SetBool("Attack", false);
            animEnemy.SetBool("Run", false);
            
            if (prevAlerted != alerted && alerted) { AudioManager.instance.Play("enemyalert", audioSource);}
            prevAlerted = alerted;
            alerted = false;
            //-------------ALERT------------------
            if (NetworkDriver.instance.HOST)
            {

                if (alertLevelPlayer > 0)
                {
                    alertLevelPlayer -= 1;
                    if (!GameDriver.instance.Player.GetComponent<FlashlightSystem>().isFlashlightOn) { alertLevelPlayer -= 1; }
                    if (alertLevelPlayer > alertLevelClient && alertLevelPlayer > unawareness)
                    {
                        if (destination != PlayerWP) {
                            PlayerWP.transform.position = GameDriver.instance.Player.transform.position;
                            destination = PlayerWP;
                            hasLooked = false;
                        }
                    }
                }if (alertLevelPlayer > unawareness * 2) { alertLevelPlayer = unawareness * 2; }

                if (alertLevelClient > alertLevelPlayer && alertLevelClient > unawareness)
                {
                    alertLevelClient -= 1;
                    if (!GameDriver.instance.Client.GetComponent<ClientPlayerController>().flashlighton) { alertLevelClient -= 1; }
                    if (alertLevelClient > unawareness)
                    {
                        if (destination != ClientWP)
                        {
                            ClientWP.transform.position = GameDriver.instance.Client.transform.position;
                            destination = ClientWP;
                            hasLooked = false;
                            Debug.Log("-----------------------------------------------DESTINATION SET TO ALERT CLIENT");
                        }
                    }
                    if (alertLevelClient > unawareness * 2) { alertLevelClient = unawareness * 2; }
                }
            }

            //----HAPPENS ON BOTH LOCALS
            if(destination == ClientWP || destination == PlayerWP) {if(!hasLooked) { alerted = true;  navmesh.SetDestination(destination.transform.position);  if (Vector3.Distance(transform.position, destination.transform.position) > 1f) { navmesh.isStopped = false; animEnemy.SetBool("Walk", true); } else { alertLevelPlayer = 0; alertLevelClient = 0; if (animEnemy.GetCurrentAnimatorClipInfo(0).Length > 0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "lookAroundAni") { animEnemy.Play("lookAroundAni"); } } } }
            
             //if (alertLevelPlayer <= 0 && alertLevelClient<=0) { alerted = false; }//DISENGAGE ALERT
            //-------------PATROL---------------
            if (!alerted && hasLooked)
            {
                if (wayPoint.Count > 1)
                {
                    if (wayPoint.Count > curWayPoint)
                    {

                        if (NetworkDriver.instance.HOST) { destination= wayPoint[curWayPoint].gameObject; } //Debug.Log("WALKING TO DESTINATION " + destination.name);

                        Vector3 destination2D = new Vector3(destination.transform.position.x, transform.position.y, destination.transform.position.z);

                        if (GetComponent<NavMeshAgent>().isOnNavMesh) { navmesh.SetDestination(destination2D); }

                        float distance = Vector3.Distance(transform.position, destination2D);

                        if (distance > 1f)
                        {
                            animEnemy.SetBool("Walk", true);
                        }
                        else //waypoint reached
                        {
                            if (NetworkDriver.instance.HOST) { if (teleports && Random.value<0.3f) { GetComponent<Teleport>().CheckTeleport(true, false); } }
                            curWayPoint++;
                        }
                    }
                    else if (wayPoint.Count == curWayPoint)
                    {
                        curWayPoint = 0;
                    }
                }
                else if (wayPoint.Count == 1)
                {
                    curWayPoint = 0;

                    if (NetworkDriver.instance.HOST) {  destination = wayPoint[curWayPoint].gameObject; }

                    Vector3 destination2D = new Vector3(destination.transform.position.x, transform.position.y, destination.transform.position.z);

                    if (GetComponent<NavMeshAgent>().isOnNavMesh) { navmesh.SetDestination(destination2D); }

                    float distance = Vector3.Distance(transform.position, destination2D);

                    if (distance > 1f)
                    {
                        if (GetComponent<NavMeshAgent>().isOnNavMesh) { navmesh.isStopped = false; }
                        animEnemy.SetBool("Walk", true);
                    }
                    else//waypoint reached
                    {
                        if (GetComponent<NavMeshAgent>().isOnNavMesh) { navmesh.isStopped = true; }
                        animEnemy.SetBool("Walk", false);
                        if (hasRetreated == 1) { hasRetreated = 2; range = startRange; angleView = startAngleView; }
                    }
                }
                else
                {
                    if (GetComponent<NavMeshAgent>().isOnNavMesh) { navmesh.isStopped = true; }
                    animEnemy.SetBool("Walk", false);
                }
            }
        }
    }
    public void TriggerZapOn() { 
        zapActive = true;
        if (brute) { AudioManager.instance.Play("BruteJump", audioSource); }
    }
    public void TriggerZapOff() { zapActive = false;
        if (brute) {
            GameObject bruteExplo = Instantiate(bruteExplosion);
            bruteExplo.transform.position = this.gameObject.transform.position;
            bruteExplo.GetComponent<bruteExplosion>().main = this.gameObject;
            //AudioManager.instance.Play("BruteSmash", audioSource);

        }
    }
    public void ZapReset() { canZap = true; }

    public int zapClient;
    public void Zap()
    {
        animEnemy.Play("zapAni"); canZap = false;
        if (!brute) { Invoke("ZapReset", 8f); }{ Invoke("ZapReset", 5f); }//COOL DOWN FOR JUMP AND ZAP 15f
        if (NetworkDriver.instance.HOST && NetworkDriver.instance.TWOPLAYER) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { obj = this.gameObject.name, zap = true }), false); }
    }
    public void Attack()
    {
        //Debug.Log("target " + target);
        //MOVE TOWARDS TARGET
        navmesh.SetDestination(target.position);
        destination = target.gameObject;
        GetComponent<NavMeshAgent>().stoppingDistance = hitRange;
        if (!zozoLaser) { animEnemy.SetBool("Walk", true); }


        float distance = Vector3.Distance(transform.position, new Vector3(target.position.x, transform.position.y, target.position.z));//measured at same level Yaxis
                                                                                                                                       //-----------DISENGAGE--------------------
        if (NetworkDriver.instance.HOST && GetComponent<Teleport>().teleport==0)
        {
            if (distance > 15 && !ZOZO && !agro) { Disengage(); return; }
            if (teddy && distance > 10) { Disengage(); return; }
        }


        //------PUSH PLAYER AWAY
        if (!teddy && distance < 0.4f)
        {
            Vector3 pushDirection = transform.forward;//target.transform.position - transform.position;
            pushDirection.y = 0f; // Set Y component to 0
            pushDirection.Normalize();
            float pushDistance = 1f; // The distance to push the target object
            Vector3 targetPosition = target.transform.position + pushDirection * pushDistance; // The target position
            float speed = 1.5f; // The speed of the movement
            target.transform.position = Vector3.Lerp(target.transform.position, targetPosition, speed * Time.deltaTime);
        }
        //------PUSH TEDDY AWAY
        if (teddy && distance < 1f)
        {
            Vector3 pushDirection = -transform.forward;//target.transform.position - transform.position;
            pushDirection.y = 0f; // Set Y component to 0
            pushDirection.Normalize();
            float pushDistance = 5f; // The distance to push the target object
            Vector3 targetPosition = target.transform.position + pushDirection * pushDistance; // The target position
            float speed = 1.5f; // The speed of the movement
            transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
        }

        //ZAP
        if (NetworkDriver.instance.HOST)
        {
            if(!brute)
            {
                if (distance <= zapRange && canZap && zaps) //&& distance > range
                {
                    if (canZap)
                    {
                        Zap();
                    }
                }
            }
            if(brute)
            {
                if (distance > zapRange && canZap && zaps) //&& distance > range
                {
                    if (canZap)
                    {
                        Zap();
                    }
                }

            }

        }


        if (!zozoLaser && !zapActive)
        {
            //if (animEnemy.GetCurrentAnimatorClipInfo(0).Length > 0 && (animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name == "agro")) { return; }

            //RUN TO TARGET
            if (distance > hitRange)
            {
                animEnemy.SetBool("Fighting", false);
                animEnemy.SetBool("Run", true);
                animEnemy.SetBool("Attack", false);
                if (animEnemy.GetCurrentAnimatorClipInfo(0).Length>0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "EnemyAttack")
                {
                    GetComponent<NavMeshAgent>().speed = chaseSpeed;//DOESNT AFFECT THIS
                    if (agro && !ZOZO) { GetComponent<NavMeshAgent>().speed = chaseSpeed * 1.5f; }

                }
                if (animEnemy.GetCurrentAnimatorClipInfo(0).Length>0 && (animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name == "agro" || animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name == "ReactV2")) { GetComponent<NavMeshAgent>().speed = 0; }
                //GetComponent<NavMeshAgent>().enabled = true;
                navmesh.isStopped = false;


            }
            //ATTACK TARGET
            else
            {
                if(this.gameObject.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled)
                {
                    //---------LOOKING-------
                    // Rotate towards the target position along the y-axis only
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.transform.position - transform.position), 100f * Time.deltaTime);

                    animEnemy.SetBool("Fighting", true);
                    animEnemy.SetBool("Run", true);
                    animEnemy.SetBool("Attack", true);
                    //GetComponent<NavMeshAgent>().enabled = false;
                    GetComponent<NavMeshAgent>().speed = 0;
                    navmesh.isStopped = true;
                }

            }
        }

        //PLAYER DIES
        if (target) { if (!target.gameObject.activeSelf) { target = null; } }

    }

    public void FindTargetRayCast()
    {
        //if (target == null)

   

        if (!agro)
        {
            //CHANGE RANGE IF CAN ZAP


            angleView = startAngleView;
            range = startRange;
            persist = startPersist;

            if (canZap && zaps && !brute) { range = zapRange; }

            //if (Client.GetComponent<ClientPlayerController>().hp <= 0 && destination == ClientWP) { alertLevelClient = 0; alerted = false; hasLooked = true; }
            //if (Player.GetComponent<HealthSystem>().Health <= 0 && destination == PlayerWP) { alertLevelPlayer = 0; alerted = false; hasLooked = true; }
            //ALERTS
            if(Player.GetComponent<HealthSystem>().Health > 0)
            {
                if (p1_dist < 7) { if (Mathf.Abs(Player.transform.position.y - transform.position.y) <= 2) { if (Player.GetComponent<Animator>().GetBool("Running")) { range = startRange + 2; alertLevelPlayer += 6; } } }
                if (p1_dist < 5) { if ((Player.GetComponent<Animator>().GetFloat("Walk") > 0 || Player.GetComponent<Animator>().GetFloat("Strafe") > 0) && (Mathf.Abs(Player.transform.position.y - transform.position.y) <= 2)) { alertLevelPlayer += 6; } }
                if (p1_dist < 5) { if (Player.GetComponent<Animator>().GetBool("Running") && (Mathf.Abs(Player.transform.position.y - transform.position.y) <= 2)) { range = startRange + 2; angleView = 360; alertLevelPlayer += 200; } }
                if (p1_dist < 2) { if (Mathf.Abs(Player.transform.position.y - transform.position.y) <= 2) { range = startRange + 2; angleView = 360; alertLevelPlayer += 200; } }


            }
            if (Client.GetComponent<ClientPlayerController>().hp > 0)
            { //7,5,3,2
                if (p2_dist < 7) { if (Mathf.Abs(Client.transform.position.y - transform.position.y) <= 2) { if (Client.GetComponent<Animator>().GetBool("Running")) { range = startRange + 2; alertLevelClient += 6; } } }
                if (p2_dist < 5) { if ((Client.GetComponent<Animator>().GetFloat("Walk") > 0 || Client.GetComponent<Animator>().GetFloat("Strafe") > 0) && (Mathf.Abs(Client.transform.position.y - transform.position.y) <= 2)) { alertLevelClient += 6; } }
                if (p2_dist < 5) { if (Client.GetComponent<Animator>().GetBool("Running") && (Mathf.Abs(Client.transform.position.y - transform.position.y) <= 2)) { range = startRange + 2; angleView = 90; alertLevelClient += 200; } }
                if (p2_dist < 2) { if (Mathf.Abs(Client.transform.position.y - transform.position.y) <= 2) { range = startRange + 2; angleView = 240; alertLevelClient += 200; } }
            }

               

            //LOOK AROUND
            if (animEnemy.GetCurrentAnimatorClipInfo(0).Length > 0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name == "lookAroundAni") { range = 8 ;angleView = 90; } // range = startRange +1 ;angleView = 50;

            //Debug.Log("DISTANCE " + distance + " RANGE " + range);
            if (distance <= range)
            {

                // Debug.Log("WITHIN DISTANCE");
                Quaternion look = Quaternion.LookRotation(closestPlayer.position - head.position);
                float angle = Quaternion.Angle(head.rotation, look);

                //Debug.DrawLine(head.position, closestPlayer.position + Vector3.up * 1.4f, Color.red); //1.6

                if (angle <= angleView*0.5f) // can u see target
                {
                   // Debug.Log("WITHIN ANGLE");
                    RaycastHit hit;
                    Vector3 targPos = closestPlayer.position + Vector3.up * 1.4f;
                    //Debug.DrawLine(head.position, targPos, Color.red); //1.6
                    LayerMask mask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Default"));
                    if (xrayvision) { mask = (1 << LayerMask.NameToLayer("Player")); }//disregard default layer
                    if (Physics.Linecast(head.position, targPos, out hit, mask.value))
                    {
                       // Debug.Log("----------TARGET -------------------" + hit.collider.gameObject.name);
                        if (hit.collider.transform == closestPlayer)
                        {
                             Engage(closestPlayer); 
                           
                        }
                    }
                }
            }
        }
        if (target != null)
        //else //DISENGAGE WHEN OUT OF SIGHT
        {
            RaycastHit hit;
            Vector3 targPos = target.position + Vector3.up * 1.4f;
            Debug.DrawLine(head.position, targPos); //
            LayerMask mask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Default"));
            if (xrayvision) { mask = (1 << LayerMask.NameToLayer("Player")); }//disregard default layer
            if (Physics.Linecast(head.position, targPos, out hit, mask.value))
            {
                if (hit.collider.transform != target)
                {//HIDING
                    if (GetComponent<Teleport>().teleport == 0) { if (follow > 0) { follow--; } else { Disengage(); } }

                }
                else { follow = persist; }
            }
        }
    }
    public void Engage(Transform newTarget)
    {
        canZap = true;
        hasLooked = true;

        bool agroEngage = false;//prevents chase ani from overtaking agro ani
        //NEW TARGET
        if (newTarget != target) {

            //AGRO
            //if (Vector3.Distance(newTarget.transform.position, transform.position) < 3)
            if(Player.GetComponent<Animator>().GetBool("Running") && !teddy)
                {

                Vector3 oppositeForce = GetComponent<NPCController>().transform.forward * force * 0.1f;
                oppositeForce.y = 0f; // Set the y component to 0

                    if (newTarget == Player.transform)
                    {
                        //persist = startPersist * 2;
                        Player.GetComponent<HealthSystem>().HealthDamage(100, oppositeForce, true);
                        if (NetworkDriver.instance.TWOPLAYER) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { shoot = true, obj = gameObject.name, dmg = 1 }), false); }
                        GetComponent<NPCController>().TakeDamage(1, false);//FORCE AGRO
                        agroEngage = true;
                            
                    }
                    /*if (newTarget == Client.transform)//client flinch
                    {
                        persist = startPersist * 2;
                        Client.GetComponent<ClientPlayerController>().Flinch(oppositeForce, true );
                    }*/
                }
            Debug.Log("----------------------------------ENGAGING-----------------------------------------" + newTarget);
            GetComponent<Teleport>().coolDown = Time.time + Random.Range(0, 10); //RESET teleport
            if (!engageSound) { if (ZOZO) { AudioManager.instance.Play("zozoEngage", null); } else { AudioManager.instance.Play("EnemyEngage", null); } engageSound = true; Invoke("resetEngageSound",10f); } 
            follow = persist; if (!agroEngage) { if (animEnemy.GetCurrentAnimatorClipInfo(0).Length > 0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "agro") { animEnemy.Play("Chase"); } else { agro = true; } }
        }
        target = newTarget; 
    }
    void resetEngageSound() { engageSound = false; }
    private void Disengage()
    {
        alertLevelPlayer = 0; alertLevelClient = 0;
        target = null;

    }

    public void TriggerHitEnable()
    {
        if (brute) { AudioManager.instance.Play("BruteAttack", GetComponent<AudioSource>()); }
        if (ZOZO) { AudioManager.instance.Play("zozoattack", null); }
        HIT_COL.GetComponent<SphereCollider>().enabled = true;

    }
    public void TriggerHitDisable()
    {
        HIT_COL.GetComponent<SphereCollider>().enabled = false;
    }

    private void Flinch(bool hard)
    {
        if (!hard)
        {
            Debug.Log("---------------------------------------------SHOTGUN");
            if (canFlinch && animEnemy.GetCurrentAnimatorClipInfo(0).Length > 0)
            {
                if (animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "agro") //&& !animEnemy.GetCurrentAnimatorStateInfo(0).IsName("Attack") 
                {
                    if(distance >= hitRange + 0.5f) //&& !animEnemy.GetCurrentAnimatorStateInfo(0).IsName("Attack")
                    {
                        if (!teddy) { animEnemy.Play("React"); }
                        if (teddy && canAttack) { animEnemy.Play("React"); }
                    }

                }
            }
        }
        else { animEnemy.Play("ReactV2");  }

    }

    private void Agro(bool otherPlayer)
    {
        if (!agro)
        {
            
            //if (NetworkDriver.instance.HOST)
            {   //AQUIRE TARGET

                if (NetworkDriver.instance.HOST) { if (!otherPlayer) { target = Player.transform; } else { target = Client.transform; } }
                follow = persist;
            }
            if (!ZOZO) { AudioManager.instance.Play("Agro", null); if (brute) { AudioManager.instance.Play("BruteAgro", audioSource); } }
            else { AudioManager.instance.Play("zozoAgro", null); }
            if (healthEnemy > 0) { animEnemy.Play("agro"); }
            if (GameDriver.instance.mainCam.activeSelf) { GameDriver.instance.mainCam.GetComponent<Camera_Controller>().InvokeShake(1f, 1f); }
            range = 20; agro = true; angleView = 360;
            Invoke("AgroTimeout", 20f);
        }
    }
    private void AgroTimeout()
    {
        agro = false;
        range = startRange;
        angleView = startAngleView;
    }
    public void TakeDamage(int damageAmount, bool otherPlayer)
    {
        if ((!onlyOnceThisFrame && damageAmount<=1) || (damageAmount>1))//prevent flash from interrupting
        {
            onlyOnceThisFrame = true;
           
            if(teddy && !canAttack) { return; }

            if (damageAmount > 5 && damageAmount<=100) { AudioManager.instance.Play("enemyflinchimpact", audioSource); }
            if (damageAmount > 100 && damageAmount < 150) { AudioManager.instance.Play("headshot", null); Debug.Log("---------------------------------------" + damageAmount); }

            //if (animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip!= null && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "agro" && GetComponent<Teleport>().teleport==0) { healthEnemy -= damageAmount; }
            if (damageAmount > 1) { healthEnemy -= damageAmount; }
            //MAKE SUSPECIOUS
            if (!otherPlayer) { transform.LookAt(Player.transform); alertLevelPlayer = unawareness * 2; } else { transform.LookAt(Client.transform); alertLevelClient = unawareness * 2; }

            //CAMSHOT
            if (damageAmount <= 0) { range += 2; angleView = startAngleView + 30; if (!brute) { Flinch(false); } }
            //--------AGRO-----------
            bool canAgro = true;
            if (damageAmount > 0)
            {
               // if (agro) { 
                   // Flinch(false);
                //}
                if (damageAmount > 60) { //SHOTGUN & HEADSHOT
                    //Flinch(true);
                    if (!brute) { Flinch(true); } 
                    else {//BRUTE FLINCHES
                        //if (damageAmount == 101 || damageAmount == 202) { Flinch(false); }//headshot damage
                        if (damageAmount == 500) { Flinch(true); canAgro = false; }//EMP DAMAGE
                        else { Flinch(false);  } //shotgun
                    }
                } 
                else { 
                    if (damageAmount >1 && !brute) { Flinch(false); } 
                }

                if (canAgro) { Agro(otherPlayer); persist = startPersist * 2; if (damageAmount == 1) { persist = startPersist; } }

                //AGRO OTHER PLAYER FLINCH
                if (damageAmount == 1 && otherPlayer)
                {
                    Vector3 oppositeForce = GetComponent<NPCController>().transform.forward * force * 0.1f;
                    oppositeForce.y = 0f; // Set the y component to 0
                    Client.GetComponent<ClientPlayerController>().Flinch(oppositeForce, true);
                }

            }

            //-----------RETREAT-------------------
            if (healthEnemy < startHealth * retreatThreshold) { if (hasRetreated == 0) { hasRetreated = 1; } }

            //ZOZO DAMAGE
            if (ZOZO) { GetComponent<ZozoControl>().HP -= damageAmount; }

            //-----------DEATH--------------------
            if (healthEnemy <= 0 && !dead)
            {

                //PLAY KILL SOUNDS
               // if (Random.value < 0.5f)
                {
                    int i = Random.Range(1, 4);
                    AudioSource thisPlayerSource;
                    string audioString;
                    if (!otherPlayer)
                    { //PLAYER
                        thisPlayerSource = GameDriver.instance.Player.GetComponent<PlayerController>().audioSourceSpeech;
                        if (GameDriver.instance.Player.GetComponent<PlayerController>().isTravis) { audioString = "travkill"; } else { audioString = "weskill"; }
                    }
                    else
                    {//CLIENT
                        thisPlayerSource = GameDriver.instance.Client.GetComponent<ClientPlayerController>().audioSourceSpeech;
                        if (GameDriver.instance.Client.GetComponent<ClientPlayerController>().isTravis) { audioString = "travkill"; } else { audioString = "weskill"; }

                    }
                    AudioManager.instance.Play(audioString + i.ToString(), thisPlayerSource);
                    //Debug.Log("PLAYING AUDIO " + audioString + i.ToString());
                }
                if (!otherPlayer) { GameDriver.instance.KILLS += 1; } else { GameDriver.instance.OTHER_KILLS += 1; }
                dead = true;
               
                GetComponent<Teleport>().teleport = 0;
                GetComponent<Teleport>().canTeleport = true;
                zapActive = false;
                agro = false;
                target = null;
                hasRetreated = 0;
                syncDeath = 0;
                this.gameObject.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
                this.gameObject.transform.GetChild(0).GetComponent<Outline>().OutlineWidth = 0;
                GameObject death = Instantiate(Death, transform.position, transform.rotation);
                death.transform.localScale = transform.localScale;
                if (!teddy) { death.GetComponent<EnemyDeath>().main = this.gameObject; }
                if (teddy) { death.transform.position = death.transform.position + Vector3.up; }
                //if (brute) { AudioManager.instance.Play("BruteSmash", null); }
                if (!teddy && !brute) { if (Shadower) { death.GetComponent<EnemyDeath>().Shadower = true; } }
                if (!teddy) { death.GetComponent<EnemyDeath>().brute = brute; }
                HIT_COL.GetComponent<SphereCollider>().enabled = false;
                if (!canRespawn) { this.gameObject.SetActive(false); }

                if (NetworkDriver.instance.HOST)
                {
                    GetComponent<Teleport>().CheckTeleport(true, true);
                    GetComponent<Teleport>().Invoke("Respawn", spawnTimer);
                }


            }
        }
    }

    public bool emitNewDest = true;
    private void LateUpdate()
    {
        onlyOnceThisFrame = false;

        //DETECT TARGET CHANGE
        if (prev_dest != destination) { emitNewDest = true;  }
        prev_dest = destination;
    }



    void AttackKnife()
    {
        AudioManager.instance.Play("EnemyAttack", audioSource);
    }

    public void TriggerEnable()
    {
        //handKnife.GetComponent<Collider>().enabled = true;
    }

    public void TriggerDisable()
    {
        //handKnife.GetComponent<Collider>().enabled = false;
    }
}
