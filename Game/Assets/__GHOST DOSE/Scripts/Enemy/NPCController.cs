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
    public float walkSpeed = 1f;
    public float disEngageRange;
    public float spawnTimer;
    public int damage;
    public float force;
    [HideInInspector] public int startHealth;

    [Header("TESTING")]
    [Space(10)]
    public bool canAttack = true;
    public bool agro = true;//HAUNTS THE PLAYER
    public Transform target;

    //public Transform player;

    [HideInInspector] private Transform head;
    [HideInInspector] public Animator animEnemy;
    [HideInInspector] public NavMeshAgent navmesh;

    //NETWORK
    //[HideInInspector]public GameDriver GD;

    [HideInInspector] public Transform targetPlayer;
    private GameObject Player;
    private GameObject Client;
    private string actions;
    private string send;
    private string prevActions;
    private float prevTeleport;
    [HideInInspector] public Vector3 destination;
    [HideInInspector] public Vector3 truePosition;
    [HideInInspector] public bool attacking = false;
    [HideInInspector] public bool update;
    private Outline outline;
    [HideInInspector] public bool activateOutline;


    [HideInInspector] public Vector3 clientWaypointDest;
    private float minDist = 0.03f; //debug enemy rotation when ontop of player
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

        wayPoint[0].position = transform.position;
        update = false; //UPDATE POSITIONS
        HIT_COL.GetComponent<SphereCollider>().enabled = false;
        outline = transform.GetChild(0).GetComponent<Outline>();
        Debug.Log("----------------------------------------" + HIT_COL.GetComponent<SphereCollider>().enabled);
        

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
        
        //=================================== E M I T =============================================
        if (GameDriver.instance.twoPlayer && NetworkDriver.instance.HOST)
        {
            //actions = this.name + target + destination + attacking; 
            
            //ONLY EMIT on STEPS 1 & 3
            float teleChange = teleport;
            if (teleport == 2 || teleport ==1.5) { teleChange = 1; }//skip step 2 of tele
            if (teleport == 0 && prevTeleport == 3) { teleChange = 3; } //dont trigger emit keep client on 3, client side changes to 0
            prevTeleport = teleChange;
            if (teleport > 0) { target = GetComponent<Teleport>().target; }



            actions = $"{{{target} {destination} {attacking} {teleChange}'}}";//determines what events to emit on change

            if (actions != prevActions || update) //actions change
            {
                //Debug.Log("--------------------------------SENDING PLAYER JOINED-----------------------------------" + playerJoined); 
                send = $"{{'object':'{this.name}','dead':'false','Attack':'{attacking}','target':'{target}','teleport':'{teleChange}','curWayPoint':'{curWayPoint}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                    NetworkDriver.instance.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);

                update = false;
                prevActions = actions;
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
        if (activateOutline){            if (outline.OutlineWidth < 7) { outline.OutlineWidth += 0.1f;  } else { activateOutline = false; }         }
        if (distance > 18) { if (outline.OutlineWidth > 0) { outline.OutlineWidth -= 0.1f; activateOutline = false; } }
        if (outline.OutlineWidth < 0) { outline.OutlineWidth = 0; }
        




        }


    public void OnDisable()
    {
        transform.GetChild(0).GetComponent<Outline>().OutlineWidth = 0;
        GetComponent<NPCController>().activateOutline = false;
    }
    public void OnEnable()
    {
        GetComponent<NPCController>().update = true;
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
        if (target != null)
        {
           Attack(); 
        }
        //-------------------------WAY POINTS ------------------------
        else if (target == null)
        {
            //GetComponent<NavMeshAgent>().enabled = true;
            GetComponent<NavMeshAgent>().speed = walkSpeed;
            GetComponent<NavMeshAgent>().stoppingDistance = 0;
            //if (GD.ND.HOST)
            {
                animEnemy.SetBool("Attack", false);
                animEnemy.SetBool("Run", false);

                if (wayPoint.Count > 1)
                {
                    if (wayPoint.Count > curWayPoint)
                    {

                        if (NetworkDriver.instance.HOST) { destination = wayPoint[curWayPoint].position; navmesh.SetDestination(wayPoint[curWayPoint].position); }
                        else { navmesh.SetDestination(clientWaypointDest); }

                        float distance = Vector3.Distance(transform.position, wayPoint[curWayPoint].position);

                        if (distance > 1f)
                        {
                            animEnemy.SetBool("Walk", true);
                        }
                        else //waypoint reached
                        {
                            GetComponent<Teleport>().CheckTeleport(true, false);
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

                    if (NetworkDriver.instance.HOST) { navmesh.SetDestination(wayPoint[0].position); destination = wayPoint[0].position; }
                    else { navmesh.SetDestination(clientWaypointDest); }

                    float distance = Vector3.Distance(transform.position, wayPoint[curWayPoint].position);

                    if (distance > 1f)
                    {
                        navmesh.isStopped = false;
                        animEnemy.SetBool("Walk", true);
                    }
                    else
                    {
                        navmesh.isStopped = true;
                        animEnemy.SetBool("Walk", false);
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
        float distance = Vector3.Distance(transform.position, target.position);
        animEnemy.SetBool("Walk", true);

        //------------------------------ H O S T ----------------------------------------
        if (NetworkDriver.instance.HOST)
        {
            if (distance > disEngageRange && !agro)
            {
                target = null;

            }
                //RUN TO TARGET
                if (distance > hitRange)
            {
                animEnemy.SetBool("Fighting", false);
                animEnemy.SetBool("Run", true);
                animEnemy.SetBool("Attack", false);
                GetComponent<NavMeshAgent>().speed = walkSpeed*2;//DOESNT AFFECT THIS
                //GetComponent<NavMeshAgent>().enabled = true;
                if (attacking)
                {
                    attacking = false;
                    //send = $"{{'object':'{this.name}','Attack':'{attacking}','target':'{target}','curWayPoint':'{curWayPoint}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                    //ND.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);

                }
                navmesh.isStopped = false;
               
                //if(GetComponent<GhostVFX>().invisible) { animEnemy.SetBool("Run", false); animEnemy.SetBool("walk", true); }
                
                if (distance > minDist) { transform.LookAt(targetPlayer); }
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
                    //send = $"{{'object':'{this.name}','Attack':'{attacking}','target':'{target}','curWayPoint':'{curWayPoint}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                    //ND.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);

                }
                navmesh.isStopped = true;
              
                // animEnemy.SetBool("Walk", false);
                //TURN TO TARGET
                if (distance > minDist)
                {
                    Vector3 direction = (target.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10);
                }

               
                if (distance > minDist) { transform.LookAt(targetPlayer); }
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
                GetComponent<NavMeshAgent>().speed = walkSpeed*2;

                navmesh.isStopped = false;
                if (distance <= hitRange) { navmesh.isStopped = true; }
                
               // if (GetComponent<GhostVFX>().invisible) { animEnemy.SetBool("Run", false); animEnemy.SetBool("walk", true); }
                
                if (distance > minDist) { transform.LookAt(targetPlayer); }
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
                
               // animEnemy.SetBool("Walk", false);
                //TURN TO TARGET
                if (distance > minDist)
                {
                    Vector3 direction = (target.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10);
                }

                
                
                if (GetComponent<Teleport>().debugAttack) { animEnemy.Play("Attack"); GetComponent<Teleport>().debugAttack = false; }
                if (distance > minDist) { transform.LookAt(targetPlayer); }
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
                    Debug.DrawLine(head.position, targetPlayer.position + Vector3.up * 1.4f); //1.6
                   // LayerMask mask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Default"));
                   LayerMask mask = (1 << LayerMask.NameToLayer("Player"));
                    if (Physics.Linecast(head.position, targetPlayer.position + Vector3.up * 1.4f, out hit, mask.value) && hit.transform != head && hit.transform != transform)
                    {
                        //Debug.Log("----------TARGET -------------------" + hit.collider.gameObject.name);
                        if (hit.transform == targetPlayer)
                        {
                            target = targetPlayer;
                            AudioManager.instance.Play("EnemyEngage");
                        }
                        else
                        {
                            target = null;
                        }
                    }
                }
                else
                {
                    target = null;
                }
            }
            else
            {
                target = null;
            }
        }
        else //DISENGAGE WHEN OUT OF SIGHT
        {
            RaycastHit hit;
            Debug.DrawLine(head.position, targetPlayer.position + Vector3.up * 1.4f); //
            LayerMask mask = 1 << LayerMask.NameToLayer("Player");
            if (Physics.Linecast(head.position, targetPlayer.position + Vector3.up * 1.4f, out hit, mask.value) && hit.transform != head && hit.transform != transform)
            {
                if (hit.transform == targetPlayer)
                {
                    target = targetPlayer; ;
                }
                else
                {
                    target = null;
                }
            }
        }
    }

    public void TakeDamage(int damageAmount, bool otherPlayer)
    {
        if (damageAmount == 100) { AudioManager.instance.Play("Headshot"); }
        //--------AGRO-----------
        if (!agro) { AudioManager.instance.Play("Agro"); }
        agro = true; angleView = 360;
        if (damageAmount == 0) { range = 10; }
        else { range = 30; }

        //-----------ENEMY DEATH---------------
        if (!otherPlayer) { healthEnemy -= damageAmount; }//do damage only locally

        if (healthEnemy <= 0)
        {
            if (GameDriver.instance.twoPlayer) //&& GD.ND.HOST
            {
                if(!otherPlayer)
                {
                    send = $"{{'object':'{this.name}','dead':'true','Attack':'{attacking}','target':'{target}','teleport':'{0}','curWayPoint':'{curWayPoint}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                    NetworkDriver.instance.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);
                }
            }
            GetComponent<Teleport>().CheckTeleport(true, true);
            if (NetworkDriver.instance.HOST) { GetComponent<Teleport>().Invoke("Respawn", spawnTimer); }
            AudioManager.instance.Play("EnemyDeath");
            this.gameObject.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
            this.gameObject.transform.GetChild(0).GetComponent<Outline>().OutlineWidth = 0;
            GameObject death = Instantiate(Death, transform.position, transform.rotation);
            if (Shadower) { death.GetComponent<GhostVFX>().Shadower = true; death.GetComponent<EnemyDeath>().Shadower = true; }
            healthEnemy = startHealth;


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
