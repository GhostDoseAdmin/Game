using GameManager;
using InteractionSystem;
using NetworkSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
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
    private bool ZOZO;
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
    public int persist;
    public bool canRespawn;
    public bool teleports;
    public bool canFlinch;
    public bool zozoLaser;

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
    private Outline outline;
    [HideInInspector] public bool activateOutline;
    [HideInInspector] Vector3 lookAtVec;
    public bool dead;

    //private float minDist = 0.03f; //debug enemy rotation when ontop of player
    private float distance, p1_dist,p2_dist;
    private bool onlyOnceThisFrame;
    public GameObject activeWayPoint;
    public AudioSource audioSource;
    private AudioClip audioClip;

    private void Awake()
    {
        GetComponent<GhostVFX>().Shadower = Shadower;
        active_timer = 99999;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        if (wayPoint == null)        {            wayPoint.Add(transform);        }
    }


    void Start()
    {
        
        //GD = GameObject.Find("GameController").GetComponent<GameDriver>();
        Player = GameDriver.instance.Player;
        Client = GameDriver.instance.Client;

        animEnemy = GetComponent<Animator>();
        navmesh = GetComponent<NavMeshAgent>();

        closestPlayer = Client.transform;
        head = animEnemy.GetBoneTransform(HumanBodyBones.Head).transform;

        startHealth = healthEnemy;
        startAngleView = angleView;
        startRange = range;
        hasRetreated = 0;

        update = false; //UPDATE POSITIONS\
        SKEL_ROOT.GetComponent<CapsuleCollider>().isTrigger = true;
        HIT_COL.GetComponent<SphereCollider>().isTrigger = true;
        HIT_COL.GetComponent<SphereCollider>().enabled = false;
        outline = transform.GetChild(0).GetComponent<Outline>();
        destination = this.gameObject;
        zozoLaser = false;
        if(GetComponent<ZozoControl>()!=null) { ZOZO = true; }
        //Debug.Log("----------------------------------------" + HIT_COL.GetComponent<SphereCollider>().enabled);


    }

    public Vector3 serverPosition;
    public float active_timer;
    void Update()
    {
        //---CLIENT SIDE PREDICTION--close position gap
        if (!NetworkDriver.instance.HOST)
        {
            //float distance = Vector3.Distance(transform.position, destination.transform.position);
            //float timeToTravel = distance / 0.2f + 0.00001f; //navmesh.speed * animation speed
            //if (target == null) { transform.position = Vector3.Lerp(transform.position, destination.transform.position, Time.deltaTime / timeToTravel); }

            if (Vector3.Distance(transform.position, serverPosition) > 0.2f) { transform.position = Vector3.Lerp(transform.position, serverPosition, 0.02f); }
            if (Vector3.Distance(transform.position, serverPosition) > 5f) { transform.position = serverPosition; }

            //-------------ACTIVE TIMER------------
            active_timer -= Time.deltaTime;
            if (active_timer <= 0)
            {
               this.gameObject.SetActive(false);
            }
        }

        p1_dist = Vector3.Distance(head.position, Player.transform.position);
        p2_dist = Vector3.Distance(head.position, Client.transform.position);

        //ALWAYS CHOOSE CLOSEST TARGET
        if (p1_dist < p2_dist) { distance = p1_dist; closestPlayer = Player.transform; } else { distance = p2_dist; closestPlayer = Client.transform; }
        if(ZOZO) { target = closestPlayer; }

        float teleport = GetComponent<Teleport>().teleport;

        if (this.gameObject.activeSelf) { if (teleport == 0) { AI(); } }

        if (NetworkDriver.instance.HOST) { if (teleport == 0 && canAttack) { FindTargetRayCast(); } } //dtermines & finds target

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
        
        }


    public void OnDisable()
    {
        transform.GetChild(0).GetComponent<Outline>().OutlineWidth = 0;
        GetComponent<NPCController>().activateOutline = false;
    }


    public void AI()
    {
        if (GetComponent<Teleport>().teleport > 0) {return; }
        
        //-----------RETREAT------------------
        if(hasRetreated == 1 && NetworkDriver.instance.HOST)
        {
            target = null;
            agro = false;
            range = 0;
            angleView = 0;
            navmesh.isStopped = false;
        }
        if (target != null)
        {
           Attack(); 
        }
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
                    if (alertLevelPlayer > alertLevelClient && alertLevelPlayer > unawareness)
                    {
                        destination = GameDriver.instance.Player;  
                    }
                }
                if (alertLevelClient > alertLevelPlayer && alertLevelClient > unawareness)
                {
                    alertLevelClient -= 1;
                    if (alertLevelClient > unawareness)
                    {
                         destination = GameDriver.instance.Client; 
                    }
                }
            }
            
            if (destination == GameDriver.instance.Client) { alerted = true; navmesh.SetDestination(destination.transform.position); if (Vector3.Distance(transform.position, GameDriver.instance.Client.transform.position) > 1f) { navmesh.isStopped = false; animEnemy.SetBool("Walk", true); } else { navmesh.isStopped = true; animEnemy.SetBool("Walk", false); } }
            if (destination == GameDriver.instance.Player) { alerted = true; navmesh.SetDestination(destination.transform.position); if (Vector3.Distance(transform.position, GameDriver.instance.Player.transform.position) > 1f ) { navmesh.isStopped = false; animEnemy.SetBool("Walk", true); } else { navmesh.isStopped = true; animEnemy.SetBool("Walk", false); } }
            
            if (alertLevelPlayer <= 0 && alertLevelClient<=0 && alerted) { alerted = false; }//DISENGAGE ALERT

            //-------------PATROL---------------
            if (!alerted){
                if (wayPoint.Count > 1)
                {
                    if (wayPoint.Count > curWayPoint)
                    {

                        if (NetworkDriver.instance.HOST) { destination= wayPoint[curWayPoint].gameObject; }
                        
                        Vector3 destination2D = new Vector3(destination.transform.position.x, transform.position.y, destination.transform.position.z);
                        
                        navmesh.SetDestination(destination2D);

                        float distance = Vector3.Distance(transform.position, destination2D);

                        if (distance > 1f)
                        {
                            animEnemy.SetBool("Walk", true);
                        }
                        else //waypoint reached
                        {
                            if (NetworkDriver.instance.HOST) { if (teleports) { GetComponent<Teleport>().CheckTeleport(true, false); } }
                            curWayPoint++;
                            if (hasRetreated==1) { hasRetreated = 2; range = startRange; angleView = startAngleView; }
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

                    navmesh.SetDestination(destination2D);

                    float distance = Vector3.Distance(transform.position, destination2D);

                    if (distance > 1f)
                    {
                        navmesh.isStopped = false;
                        animEnemy.SetBool("Walk", true);
                    }
                    else//waypoint reached
                    {
                        navmesh.isStopped = true;
                        animEnemy.SetBool("Walk", false);
                        if (hasRetreated == 1) { hasRetreated = 2; range = startRange; angleView = startAngleView; }
                    }
                }
                else
                {
                    navmesh.isStopped = true;
                    animEnemy.SetBool("Walk", false);
                }
            }
        }
    }

    public void Attack()
    {
        //MOVE TOWARDS TARGET
        navmesh.SetDestination(target.position);
        destination = target.gameObject;
        GetComponent<NavMeshAgent>().stoppingDistance = hitRange;
        animEnemy.SetBool("Walk", true);


        float distance = Vector3.Distance(transform.position, new Vector3(target.position.x, transform.position.y, target.position.z));//measured at same level Yaxis
         //-----------DISENGAGE--------------------
         if (distance > range){ Disengage(); }

        //------PUSH PLAYER AWAY
        if (distance < 0.4f)
        {
            Vector3 pushDirection = transform.forward;//target.transform.position - transform.position;
            pushDirection.y = 0f; // Set Y component to 0
            pushDirection.Normalize();
            float pushDistance = 1f; // The distance to push the target object
            Vector3 targetPosition = target.transform.position + pushDirection * pushDistance; // The target position
            float speed = 1.5f; // The speed of the movement
            target.transform.position = Vector3.Lerp(target.transform.position, targetPosition, speed * Time.deltaTime);
        }


        if(!zozoLaser)
        {
            //RUN TO TARGET
            if (distance > hitRange)
            {
                animEnemy.SetBool("Fighting", false);
                animEnemy.SetBool("Run", true);
                animEnemy.SetBool("Attack", false);
                if (animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip != null && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "EnemyAttack")
                {
                    GetComponent<NavMeshAgent>().speed = chaseSpeed;//DOESNT AFFECT THIS
                    if (agro) { GetComponent<NavMeshAgent>().speed = chaseSpeed * 2; }
                }
                if (animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip != null && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name == "agro") { GetComponent<NavMeshAgent>().speed = 0; }
                //GetComponent<NavMeshAgent>().enabled = true;
                navmesh.isStopped = false;


            }
            //ATTACK TARGET
            if (distance <= hitRange)
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

        
        //---------------PLAYER DIES
        if (target != null && NetworkDriver.instance.HOST)
        {
            if (target.gameObject == Player)
            {
                if (target.gameObject.GetComponent<HealthSystem>().Health <= 0 || !Player.activeSelf)
                {
                    target = null;
                    navmesh.isStopped = false;
                    agro = false;
                }
            }
            if (target.gameObject == Client)
            {
                if (target.gameObject.GetComponent<ClientPlayerController>().hp <= 0 || !Client.activeSelf)
                {
                    target = null;
                    navmesh.isStopped = false;
                    agro = false;
                }
            }
        }
        //if (zozo) { GetComponent<NavMeshAgent>().speed = 0; }
    }

    public void FindTargetRayCast()
    {
        if (target == null)
        {
            if (distance <= range)
            {
                if (Player.GetComponent<Animator>().GetBool("Running")){ alertLevelPlayer += 100; }
                if (Client.GetComponent<Animator>().GetBool("Running")){ alertLevelClient += 100; }
                // Debug.Log("TARGET IS " + targetPlayer.gameObject.name + " DISTANCE " + distance);

                Quaternion look = Quaternion.LookRotation(closestPlayer.position - head.position);
                float angle = Quaternion.Angle(head.rotation, look);


                if (angle <= angleView) // can u see target
                {
                    //Debug.Log("----------------------------------CAN SEE TARGET----------------------------------------");
                    RaycastHit hit;
                    Vector3 targPos = closestPlayer.position + Vector3.up * 1.4f;
                    Debug.DrawLine(head.position, targPos); //1.6
                    LayerMask mask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Default"));
                    if (xrayvision) { mask = (1 << LayerMask.NameToLayer("Player")); }//disregard default layer
                    if (Physics.Linecast(head.position, targPos, out hit, mask.value))
                    {
                        //Debug.Log("----------TARGET -------------------" + hit.collider.gameObject.name);
                        if (hit.transform == closestPlayer)
                        {
                            //Engage(closestPlayer);
                            target = closestPlayer;
                            AudioManager.instance.Play("EnemyEngage", null);
                            follow = persist;
                        }
                    }
                }
            }
        }
        else //DISENGAGE WHEN OUT OF SIGHT
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

    private void Disengage()
    {
        target = null;
        angleView = startAngleView;
        range = startRange;
    }

    public void TriggerHitEnable()
    {
        HIT_COL.GetComponent<SphereCollider>().enabled = true;

    }
    public void TriggerHitDisable()
    {
        HIT_COL.GetComponent<SphereCollider>().enabled = false;
    }

    private void Flinch()
    {
        if (animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "agro" && distance>=hitRange+0.5f && canFlinch) // && !animEnemy.GetCurrentAnimatorStateInfo(0).IsName("Attack")
        {
            Debug.Log("-----------------------------FLINCH");
            animEnemy.Play("React");
        }
    }
    public void TakeDamage(int damageAmount, bool otherPlayer)
    {
        if (!onlyOnceThisFrame)//prevent flash from interrupting
        {
            onlyOnceThisFrame = true;

            AudioManager.instance.Play("enemyflinchimpact", audioSource);
            if (damageAmount == 100) { AudioManager.instance.Play("headshot", audioSource); }
            if (animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip!= null && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "agro" && GetComponent<Teleport>().teleport==0) { healthEnemy -= damageAmount; }

            //MAKE SUSPECIOUS
            if (!otherPlayer) { transform.LookAt(Player.transform); alertLevelPlayer = unawareness * 2; } else { transform.LookAt(Client.transform); alertLevelClient = unawareness * 2; }

            //CAMSHOT
            if (damageAmount <= 0) { range += 2; angleView = startAngleView + 30; Flinch(); }
            //--------AGRO-----------
            if (damageAmount > 0)
            {
                if (agro) { Flinch(); }
                if (!agro)
                {
                    if (NetworkDriver.instance.HOST)
                    {   //AQUIRE TARGET
                        if (!otherPlayer) { target = Player.transform; } else { target = Client.transform; }
                        follow = persist;
                    }
                    AudioManager.instance.Play("Agro", null); animEnemy.Play("agro"); //animEnemy.CrossFade("agro", 0.25f);
                    GameObject.Find("PlayerCamera").GetComponent<Camera_Controller>().InvokeShake(2f, 1f);
                    range = 20; agro = true; angleView = 360;
                }

            }
            //-----------ENEMY DEATH---------------

            //-----------RETREAT-------------------
            if (healthEnemy < startHealth * retreatThreshold) { if (hasRetreated == 0) { hasRetreated = 1; } }

            //-----------DEATH--------------------
            if (healthEnemy <= 0 && !dead)
            {
                dead = true;
                GetComponent<Teleport>().teleport = 0;
                GetComponent<Teleport>().canTeleport = true;
                if (NetworkDriver.instance.HOST)
                {
                    GetComponent<Teleport>().CheckTeleport(true, true);
                    GetComponent<Teleport>().Invoke("Respawn", spawnTimer);
                }
                agro = false;
                target = null;
                hasRetreated = 0;
                this.gameObject.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
                this.gameObject.transform.GetChild(0).GetComponent<Outline>().OutlineWidth = 0;
                GameObject death = Instantiate(Death, transform.position, transform.rotation);
                if (Shadower) { death.GetComponent<GhostVFX>().Shadower = true; death.GetComponent<EnemyDeath>().Shadower = true; }
                HIT_COL.GetComponent<SphereCollider>().enabled = false;
                if (!canRespawn) { DestroyImmediate(this.gameObject); }


            }
        }
    }

    private void LateUpdate()
    {
        onlyOnceThisFrame = false;
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
