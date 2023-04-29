using GameManager;
using InteractionSystem;
using NetworkSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class NPCController : MonoBehaviour
{
    [Header("WAY POINTS")]
    [Space(10)]
    public List<Transform> wayPoint;
    [HideInInspector] public int curWayPoint;

    [Header("SETUP")]
    [Space(10)]

    public GameObject SKEL_ROOT;
    public GameObject HIT_COL;
    public GameObject Death;

    [Header("ENEMY PARAMETRS")]
    [Space(10)]
    public bool Shadower;
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
    private bool emitDest;
    [HideInInspector] public float teleEmit;
    private Transform prevTarg;
    private bool prevAttack;
    private bool emitPos;

    [HideInInspector] public Transform targetPlayer;
    private GameObject Player;
    private GameObject Client;
    private string actions;
    private string send;
    private int hasRetreated;
    public int alertLevelPlayer;
    public int alertLevelClient;
    public bool prevAlerted;
    public bool alerted;
    public Vector3 destination;
    [HideInInspector] public Vector3 truePosition;
    [HideInInspector] public bool attacking = false;
    [HideInInspector] public bool update;
    private Outline outline;
    [HideInInspector] public bool activateOutline;
    [HideInInspector] Vector3 lookAtVec;

    [HideInInspector] public Vector3 clientWaypointDest;
    //private float minDist = 0.03f; //debug enemy rotation when ontop of player
    private float distance, p1_dist,p2_dist;

    private void Awake()
    {
        GetComponent<GhostVFX>().Shadower = Shadower;
    }


    void Start()
    {
        
        //GD = GameObject.Find("GameController").GetComponent<GameDriver>();
        Player = GameDriver.instance.Player;
        Client = GameDriver.instance.Client;

        animEnemy = GetComponent<Animator>();
        navmesh = GetComponent<NavMeshAgent>();

        targetPlayer = Client.transform;
        head = animEnemy.GetBoneTransform(HumanBodyBones.Head).transform;

        startHealth = healthEnemy;
        startAngleView = angleView;
        startRange = range;
        hasRetreated = 0;
        if (wayPoint[0] == null) { 
            wayPoint[0].position = transform.position; 
        }//First waypoint is always self
        update = false; //UPDATE POSITIONS\
        SKEL_ROOT.GetComponent<CapsuleCollider>().isTrigger = true;
        HIT_COL.GetComponent<SphereCollider>().isTrigger = true;
        HIT_COL.GetComponent<SphereCollider>().enabled = false;
        outline = transform.GetChild(0).GetComponent<Outline>();
        //Debug.Log("----------------------------------------" + HIT_COL.GetComponent<SphereCollider>().enabled);
        

    }

    void Update()
    {

         p1_dist = Vector3.Distance(head.position, Player.transform.position);
         p2_dist = Vector3.Distance(head.position, Client.transform.position);
        //ALWAYS CHOOSE CLOSEST TARGET
        if (p1_dist < p2_dist) { distance = p1_dist; targetPlayer = Player.transform; } else { distance = p2_dist; targetPlayer = Client.transform; }

        float teleport = GetComponent<Teleport>().teleport;

        if (this.gameObject.activeSelf) { if (teleport == 0) { AI(); } }

        if (NetworkDriver.instance.HOST) { if (teleport == 0 && canAttack) { FindTargetRayCast(); } } //dtermines & finds target

        if (teleport > 0) { target = GetComponent<Teleport>().target; }

        //=================================== E M I T =============================================
        if (GameDriver.instance.twoPlayer && NetworkDriver.instance.HOST)
        {
            

            //--------------- DESTINATION EMIT-----------------
            string destString = "";
            if (emitDest){
                destString = $",'wp':'{curWayPoint}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'";
                emitDest = false;
                emitPos = true;
            }
            //--------------- TELEPORT EMIT-----------------
            string teleString = "";
            if (teleEmit>0){
                teleString = $",'tele':'{teleEmit}'";
                emitPos = true;
            }
            //--------------- TARGET EMIT-----------------
            string targString = "";
            if (prevTarg != target){
                targString = $",'targ':'{target}'";
            }
            prevTarg = target;
            //--------------- ATTACK EMIT-----------------
            string attackString = "";
            if (prevAttack != attacking){
                attackString = $",'attk':'{attacking}'";
                emitPos = true; 
            }
            prevAttack = attacking;
            //--------------- POSITION EMIT-----------------
            string posString = "";
            if (emitPos){
                posString = $",'x':'{transform.position.x.ToString("F2")}','y':'{transform.position.y.ToString("F2")}','z':'{transform.position.z.ToString("F2")}'";
                emitPos = false;
            }
            //actions = $"{{{target} {destination} {attacking} {teleChange}'}}";//determines what events to emit on change
            //if (actions != prevActions || update) //actions change
            if (destString.Length > 1 || teleEmit>0 || targString.Length > 1 || attackString.Length>1)
            {
                //Debug.Log("--------------------------------SENDING PLAYER JOINED-----------------------------------" + playerJoined); 
                send = $"{{'obj':'{this.name}'{destString}{teleString}{targString}{attackString}{posString}}}";
                //Debug.Log("SENDING DATA " + send);
                NetworkDriver.instance.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);
                
                teleEmit = 0;
                //prevActions = actions;
            }
        }

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
    public void OnEnable()
    {
        emitPos = true;
    }


    public void TriggerHitEnable()
    {
        HIT_COL.GetComponent<SphereCollider>().enabled = true;

    }
    public void TriggerHitDisable()
    {
        HIT_COL.GetComponent<SphereCollider>().enabled = false;
    }


    public void AI()
    {
        if (GetComponent<Teleport>().teleport > 0) {return; }
        
        //-----------RETREAT------------------
        if(hasRetreated == 1)
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

            //GetComponent<NavMeshAgent>().enabled = true;
            GetComponent<NavMeshAgent>().speed = 0.1f;
            if (hasRetreated == 1) { GetComponent<NavMeshAgent>().speed = chaseSpeed * 2; }
            GetComponent<NavMeshAgent>().stoppingDistance = 0;
            animEnemy.SetBool("Attack", false);
            animEnemy.SetBool("Run", false);
           
            if (prevAlerted != alerted && alerted) { AudioManager.instance.Play("enemyalert"); Debug.Log("---------------------------------------ALERT"); }
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
                        if (destination != GameDriver.instance.Player.transform.position) { destination = GameDriver.instance.Player.transform.position; emitDest = true; }
                    }
                }
                if (alertLevelClient > alertLevelPlayer && alertLevelClient > unawareness)
                {
                    alertLevelClient -= 1;
                    if (alertLevelClient > unawareness)
                    {
                        if (destination != GameDriver.instance.Client.transform.position) { destination = GameDriver.instance.Client.transform.position; emitDest = true; }
                    }
                }
            }
            if (destination == GameDriver.instance.Client.transform.position) { alerted = true; navmesh.SetDestination(destination); if (Vector3.Distance(transform.position, GameDriver.instance.Client.transform.position) > 1f) { navmesh.isStopped = false; animEnemy.SetBool("Walk", true); } else { navmesh.isStopped = true; animEnemy.SetBool("Walk", false); } }
            if (destination == GameDriver.instance.Player.transform.position) { alerted = true; navmesh.SetDestination(destination); if (Vector3.Distance(transform.position, GameDriver.instance.Player.transform.position) > 1f ) { navmesh.isStopped = false; animEnemy.SetBool("Walk", true); } else { navmesh.isStopped = true; animEnemy.SetBool("Walk", false); } }
            if (clientWaypointDest == GameDriver.instance.Client.transform.position) { alerted = true; navmesh.SetDestination(clientWaypointDest); if (Vector3.Distance(transform.position, GameDriver.instance.Client.transform.position) > 1f) { navmesh.isStopped = false; animEnemy.SetBool("Walk", true); } else { navmesh.isStopped = true; animEnemy.SetBool("Walk", false); } }
            if (clientWaypointDest == GameDriver.instance.Player.transform.position) { alerted = true; navmesh.SetDestination(clientWaypointDest); if (Vector3.Distance(transform.position, GameDriver.instance.Player.transform.position) > 1f) { navmesh.isStopped = false; animEnemy.SetBool("Walk", true); } else { navmesh.isStopped = true; animEnemy.SetBool("Walk", false); } }
            
            //-------------PATROL---------------
            if (!alerted){
                if (wayPoint.Count > 1)
                {
                    if (wayPoint.Count > curWayPoint)
                    {

                        if (NetworkDriver.instance.HOST) { if (destination != wayPoint[curWayPoint].position) { destination = wayPoint[curWayPoint].position; emitDest = true;  } navmesh.SetDestination(wayPoint[curWayPoint].position); }
                        else { navmesh.SetDestination(clientWaypointDest); }

                        float distance = Vector3.Distance(transform.position, wayPoint[curWayPoint].position);

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

                    if (NetworkDriver.instance.HOST) { if (destination != wayPoint[0].position) { destination = wayPoint[0].position; emitDest = true;   } navmesh.SetDestination(wayPoint[0].position); }
                    else { navmesh.SetDestination(clientWaypointDest); }

                    float distance = Vector3.Distance(transform.position, wayPoint[curWayPoint].position);

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
        GetComponent<NavMeshAgent>().stoppingDistance = hitRange;
        animEnemy.SetBool("Walk", true);


        float distance = Vector3.Distance(transform.position, new Vector3(target.position.x, transform.position.y, target.position.z));//measured at same level Yaxis

        //------PUSH PLAYER AWAY
        //Debug.Log("-----------------------------DISTANCE " + distance);
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
        //---------LOOKING-------
        // Rotate towards the target position along the y-axis only
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.transform.position- transform.position), 100f * Time.deltaTime);
        //------------------------------ H O S T ----------------------------------------
        if (NetworkDriver.instance.HOST)
        {
            //-----------DISENGAGE--------------------
            if (distance > range)// && !agro) || (distance > range*1.5 && agro))
            {
                target = null;

            }
                //RUN TO TARGET
                if (distance > hitRange)
            {
                animEnemy.SetBool("Fighting", false);
                animEnemy.SetBool("Run", true);
                animEnemy.SetBool("Attack", false);
                GetComponent<NavMeshAgent>().speed = chaseSpeed * 2;//DOESNT AFFECT THIS
                if (GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "agro") { GetComponent<NavMeshAgent>().speed = 0; }
                //GetComponent<NavMeshAgent>().enabled = true;
                if (attacking)
                {
                    attacking = false;
                }
                navmesh.isStopped = false;


            }
            //ATTACK TARGET
            if (distance <= hitRange)
            {
                attacking = true;
                animEnemy.SetBool("Fighting", true);
                animEnemy.SetBool("Run", true);
                animEnemy.SetBool("Attack", true);
                //GetComponent<NavMeshAgent>().enabled = false;
                GetComponent<NavMeshAgent>().speed = 0;

                if (!attacking)
                {
                    attacking = true;
                }
                navmesh.isStopped = true;
            }
        }
        else//------------------------- C L I E N T  --------------------------------------------
        {
            targetPlayer = target;
            //RUN TO TARGET
            if (!attacking)
            {
                animEnemy.SetBool("Fighting", false);
                animEnemy.SetBool("Run", true);
                animEnemy.SetBool("Attack", false);
                //GetComponent<NavMeshAgent>().speed = walkSpeed;
                GetComponent<NavMeshAgent>().speed = chaseSpeed * 2;
                if (GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "agro") { GetComponent<NavMeshAgent>().speed = 0; }

                navmesh.isStopped = false;
                if (distance <= hitRange) { navmesh.isStopped = true; }
                
            }
            //ATTACK TARGET
            else
            {
                animEnemy.SetBool("Fighting", true);
                animEnemy.SetBool("Run", true);
                animEnemy.SetBool("Attack", true);
                //GetComponent<NavMeshAgent>().speed = 0;
                GetComponent<NavMeshAgent>().speed = 0;

                //KEEP NAVMESH IN SYNC WITH LERP
                if (distance > hitRange*1.1f)
                {
                    transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * 1f);
                }

                navmesh.isStopped = true;
                
                if (GetComponent<Teleport>().debugAttack) { animEnemy.Play("Attack"); GetComponent<Teleport>().debugAttack = false; }
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
    }

    public void FindTargetRayCast()
    {
        if (target == null)
        {

            if (distance <= range)
            {

                // Debug.Log("TARGET IS " + targetPlayer.gameObject.name + " DISTANCE " + distance);

                Quaternion look = Quaternion.LookRotation(targetPlayer.position - head.position);
                float angle = Quaternion.Angle(head.rotation, look);


                if (angle <= angleView) // can u see target
                {
                    //Debug.Log("----------------------------------CAN SEE TARGET----------------------------------------");
                    RaycastHit hit;
                    Vector3 targPos = targetPlayer.position + Vector3.up * 1.4f;
                    Debug.DrawLine(head.position, targPos); //1.6
                    LayerMask mask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Default"));
                    if (xrayvision) { mask = (1 << LayerMask.NameToLayer("Player")); }//disregard default layer
                    if (Physics.Linecast(head.position, targPos, out hit, mask.value))
                    {
                        //Debug.Log("----------TARGET -------------------" + hit.collider.gameObject.name);
                        if (hit.transform == targetPlayer)
                        {
                            target = targetPlayer;
                            AudioManager.instance.Play("EnemyEngage");
                            follow = persist;
                        }
                        else
                        {
                             DisEngage();
                        }
                    }
                }
                else
                {
                    DisEngage();
                }
            }
            else
            {
                 DisEngage();
            }
        }
        else //DISENGAGE WHEN OUT OF SIGHT
        {
            RaycastHit hit;
            Vector3 targPos = target.position + Vector3.up * 1.4f;
            Debug.DrawLine(head.position, targPos); //
            LayerMask mask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Default"));
            //if (xrayvision) { mask = (1 << LayerMask.NameToLayer("Player")); }//disregard default layer
            if (Physics.Linecast(head.position, targPos, out hit, mask.value))
            {
                if (hit.collider.transform != target)
                {//HIDING
                    if (follow > 0) { follow--; } else {  DisEngage(); }

                }
                else { follow = persist; }
            }
        }
    }

    public void DisEngage()
    {
        target = null;
        agro = false;
        angleView = startAngleView;
        range = startRange;
        
    }


    public void TakeDamage(int damageAmount, bool otherPlayer)
    {
        if (damageAmount == 100) { AudioManager.instance.Play("Headshot"); }
        //--------AGRO-----------
        if (!agro) { AudioManager.instance.Play("Agro"); animEnemy.SetBool("agro", true); } else { if (damageAmount > 0) { animEnemy.SetBool("agro", false); } }
        
        //Debug.Log("--------------------------------------ANIM " + animEnemy.GetBool("agro") + damageAmount);
        AudioManager.instance.Play("enemyflinchimpact");
        //if (damageAmount == 0) { if (!agro) { range = 10; } } //CAM SHOT
        //else { range = 20; agro = true; angleView = 360; } //REGULAR
        range = 20; agro = true; angleView = 360;


        //-----------ENEMY DEATH---------------
        if (!otherPlayer) { healthEnemy -= damageAmount; }//do damage only locally
        //-----------RETREAT-------------------
        if(healthEnemy< startHealth* retreatThreshold)        {            if(hasRetreated==0)            {                hasRetreated = 1;            }        }

        if (healthEnemy <= 0)
        {
            GetComponent<Teleport>().teleport = 0;
            GetComponent<Teleport>().canTeleport = true;
            if (NetworkDriver.instance.HOST) {
                GetComponent<Teleport>().CheckTeleport(true, true);
                GetComponent<Teleport>().Invoke("Respawn", spawnTimer); 
            }
            AudioManager.instance.Play("EnemyDeath");
            agro = false;
            target = null;
            hasRetreated = 0;
            this.gameObject.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
            this.gameObject.transform.GetChild(0).GetComponent<Outline>().OutlineWidth = 0;
            GameObject death = Instantiate(Death, transform.position, transform.rotation);
            if (Shadower) { death.GetComponent<GhostVFX>().Shadower = true; death.GetComponent<EnemyDeath>().Shadower = true; }
            if (!canRespawn) { DestroyImmediate(this.gameObject); }
            HIT_COL.GetComponent<SphereCollider>().enabled = false;
            //healthEnemy = startHealth;


        }
        else
        {
            //animEnemy.SetTrigger("Damage"); 
            animEnemy.SetBool("Attack", false);
            animEnemy.Play("React");
            //if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName)) ;
        }
    }





    void AttackKnife()
    {
        AudioManager.instance.Play("EnemyAttack");
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
