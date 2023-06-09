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
    public bool zapActive;
    public int zapRange;

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
        //if (ZOZO) { if (closestPlayer != null) { target = closestPlayer; } }

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
        Debug.Log("-------------------------HAS LOOKED");
        hasLooked = true;
        alerted = false;
    }
    public void AI()
    {
        if (GetComponent<Teleport>().teleport > 0) {return; }

        //AGRO 
        /*if (distance <= 2)
        {
            if (closestPlayer == Player.transform) { if (Player.GetComponent<Animator>().GetBool("Running")) { Agro(false); } }
            if (closestPlayer == Client.transform) { if (Client.GetComponent<Animator>().GetBool("Running")) { Agro(true); } }
        }*/
        //-------------ATTENTION----------------------
        /*if (distance <= 3 && target==null)
        {
            if (closestPlayer == Player.transform)
            {
                 if (Player.GetComponent<Animator>().GetBool("Running")) { Engage(Player.transform);  } //TakeDamage(1,false);
            }
            if (closestPlayer ==Client.transform)
            {

                if (Client.GetComponent<Animator>().GetBool("Running")) { Engage(Client.transform); } //TakeDamage(1, true);
            }
        }*/

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
                    if (alertLevelPlayer > alertLevelClient && alertLevelPlayer > unawareness)
                    {
                        if (destination != PlayerWP) {
                            PlayerWP.transform.position = GameDriver.instance.Player.transform.position;
                            destination = PlayerWP;
                            hasLooked = false;
                        }
                    }
                }
                if (alertLevelClient > alertLevelPlayer && alertLevelClient > unawareness)
                {
                    alertLevelClient -= 1;
                    if (alertLevelClient > unawareness)
                    {
                        if (destination != ClientWP)
                        {
                            ClientWP.transform.position = GameDriver.instance.Client.transform.position;
                            destination = ClientWP;
                            hasLooked = false;
                        }
                    }
                }
            }

            //----HAPPENS ON BOTH LOCALS
            if(destination == ClientWP || destination == PlayerWP) {if(!hasLooked) { alerted = true; navmesh.SetDestination(destination.transform.position); if (Vector3.Distance(transform.position, destination.transform.position) > 1f) { navmesh.isStopped = false; animEnemy.SetBool("Walk", true); } else { alertLevelPlayer = 0; alertLevelClient = 0; if (animEnemy.GetCurrentAnimatorClipInfo(0).Length > 0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "lookAroundAni") { animEnemy.Play("lookAroundAni"); } } } }
            
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
                        
                        navmesh.SetDestination(destination2D);

                        float distance = Vector3.Distance(transform.position, destination2D);

                        if (distance > 1f)
                        {
                            animEnemy.SetBool("Walk", true);
                        }
                        else //waypoint reached
                        {
                            if (NetworkDriver.instance.HOST) { if (teleports && Random.value<0.3f) { GetComponent<Teleport>().CheckTeleport(true, false); } }
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
    public void TriggerZapOn() { zapActive = true; }
    public void TriggerZapOff() { zapActive = false; }
    public void Attack()
    {
        Debug.Log("target " + target);
        //MOVE TOWARDS TARGET
        navmesh.SetDestination(target.position);
        destination = target.gameObject;
        GetComponent<NavMeshAgent>().stoppingDistance = hitRange;
        if (!zozoLaser) { animEnemy.SetBool("Walk", true); }


        float distance = Vector3.Distance(transform.position, new Vector3(target.position.x, transform.position.y, target.position.z));//measured at same level Yaxis
         //-----------DISENGAGE--------------------
         if (distance > 15 && !ZOZO){ Disengage(); }



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

        //ZAP
        if (distance <= zapRange && distance > hitRange)
        {
            animEnemy.Play("agro");
        }

        if (!zozoLaser)
        {
            //RUN TO TARGET
            if (distance > hitRange)
            {
                animEnemy.SetBool("Fighting", false);
                animEnemy.SetBool("Run", true);
                animEnemy.SetBool("Attack", false);
                if (animEnemy.GetCurrentAnimatorClipInfo(0).Length>0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "EnemyAttack")
                {
                    GetComponent<NavMeshAgent>().speed = chaseSpeed;//DOESNT AFFECT THIS
                    if (agro) { GetComponent<NavMeshAgent>().speed = chaseSpeed * 2; }
                }
                if (animEnemy.GetCurrentAnimatorClipInfo(0).Length>0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name == "agro") { GetComponent<NavMeshAgent>().speed = 0; }
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

        //PLAYER DIES
        if (!target.gameObject.activeSelf) { target = null; }

    }

    public void FindTargetRayCast()
    {
        //if (target == null)
        if(!agro)
        {

            //Debug.Log("----------------------------------CLOSEST PLAYER---------------------------------------" + closestPlayer);

            angleView = startAngleView;
            range = startRange;
            persist = startPersist;

            //ALERTS
            if (distance <= 5)
            {
                if (Player.GetComponent<Animator>().GetFloat("Walk") > 0 || Player.GetComponent<Animator>().GetFloat("Strafe") > 0) { alertLevelPlayer += 2; }
                if (Client.GetComponent<Animator>().GetFloat("Walk") > 0 || Client.GetComponent<Animator>().GetFloat("Strafe") > 0) { alertLevelClient += 2; }
            }
            if (distance <= 7)
            {

                if (closestPlayer == Player.transform) { if (Player.GetComponent<Animator>().GetBool("Running")) { range = startRange + 2; persist = startPersist * 2; alertLevelPlayer += 6; } }
                if (closestPlayer == Client.transform) { if (Client.GetComponent<Animator>().GetBool("Running")) { range = startRange + 2; persist = startPersist * 2; alertLevelPlayer += 6; } }
            }
            if (animEnemy.GetCurrentAnimatorClipInfo(0).Length > 0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name == "lookAroundAni") { range = startRange +1 ;angleView = 50; }

            //Debug.Log("DISTANCE " + distance + " RANGE " + range);
            if (distance <= range)
            {

                // Debug.Log("WITHIN DISTANCE");
                Quaternion look = Quaternion.LookRotation(closestPlayer.position - head.position);
                float angle = Quaternion.Angle(head.rotation, look);

                //Debug.DrawLine(head.position, closestPlayer.position + Vector3.up * 1.4f, Color.red); //1.6

                if (angle <= angleView) // can u see target
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
        hasLooked = true;
        //NEW TARGET
        if (newTarget != target) {
            Debug.Log("----------------------------------ENGAGING-----------------------------------------" + newTarget);
            GetComponent<Teleport>().coolDown = Time.time;
            if (!engageSound) { if (ZOZO) { AudioManager.instance.Play("zozoEngage", null); } else { AudioManager.instance.Play("EnemyEngage", null); } engageSound = true; Invoke("resetEngageSound",10f); } 
            follow = persist; animEnemy.Play("Chase"); 
        }
        target = newTarget; 
    }
    void resetEngageSound() { engageSound = false; }
    private void Disengage()
    {
        target = null;

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
        if (animEnemy.GetCurrentAnimatorClipInfo(0).Length>0 && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "agro" && distance>=hitRange+0.5f && canFlinch) // && !animEnemy.GetCurrentAnimatorStateInfo(0).IsName("Attack")
        {
            //Debug.Log("-----------------------------FLINCH");
            animEnemy.Play("React");
        }
    }
    private void Agro(bool otherPlayer)
    {
        if (!agro)
        {
            if (NetworkDriver.instance.HOST)
            {   //AQUIRE TARGET
                if (!otherPlayer) { target = Player.transform; } else { target = Client.transform; }
                follow = persist;
            }
            if (!ZOZO) { AudioManager.instance.Play("Agro", null); }
            else { AudioManager.instance.Play("zozoAgro", null); }
            if (healthEnemy > 0) { animEnemy.Play("agro"); } 
            GameObject.Find("PlayerCamera").GetComponent<Camera_Controller>().InvokeShake(1f, 1f);
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
        if (!onlyOnceThisFrame)//prevent flash from interrupting
        {
            onlyOnceThisFrame = true;

            if (damageAmount > 5) { AudioManager.instance.Play("enemyflinchimpact", audioSource); }
            if (damageAmount == 100) { AudioManager.instance.Play("headshot", audioSource); }
            //if (animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip!= null && animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name != "agro" && GetComponent<Teleport>().teleport==0) { healthEnemy -= damageAmount; }
            healthEnemy -= damageAmount;
            //MAKE SUSPECIOUS
            if (!otherPlayer) { transform.LookAt(Player.transform); alertLevelPlayer = unawareness * 2; } else { transform.LookAt(Client.transform); alertLevelClient = unawareness * 2; }

            //CAMSHOT
            if (damageAmount <= 0) { range += 2; angleView = startAngleView + 30; Flinch(); }
            //--------AGRO-----------
            if (damageAmount > 0)
            {
                if (agro) { Flinch(); }
                Agro(otherPlayer);

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
                if (Shadower) { death.GetComponent<GhostVFX>().Shadower = true;  }
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
