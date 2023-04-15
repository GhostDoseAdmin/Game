using InteractionSystem;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

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
    public GameObject ragdollEnemy;
    [SerializeField] private string knife;

    [Header("ENEMY PARAMETRS")]
    [Space(10)]
    public int healthEnemy = 100;
    public int range;
    [HideInInspector] public int startRange;
    public int angleView;
    [HideInInspector] public int startAngleView;
    public float hitRange = 1.5f;
    public float walkSpeed = 1f;
    public float disEngageRange;
    [HideInInspector] public int startHealth;

    [Header("TESTING")]
    [Space(10)]
    public bool canAttack = true;

    //public Transform player;
    [HideInInspector] public Transform target;
    [HideInInspector] private Transform head;
    [HideInInspector] public Animator animEnemy;
    [HideInInspector] public NavMeshAgent navmesh;

    //NETWORK
    [HideInInspector]public GameDriver GD;

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
    
    
    [HideInInspector] public bool agro = true;//HAUNTS THE PLAYER
    [HideInInspector] public Vector3 clientWaypointDest;
    private float minDist = 0.03f; //debug enemy rotation when ontop of player

    void Start()
    {
        GD = GameObject.Find("GameController").GetComponent<GameDriver>();
        Player = GameObject.Find("GameController").GetComponent<GameDriver>().Player;
        Client = GameObject.Find("GameController").GetComponent<GameDriver>().Client;

        animEnemy = GetComponent<Animator>();
        navmesh = GetComponent<NavMeshAgent>();

        targetPlayer = Client.transform;
        head = animEnemy.GetBoneTransform(HumanBodyBones.Head).transform;

        startHealth = healthEnemy;
        startAngleView = angleView;
        startRange = range;
        //handKnife.GetComponent<Collider>().enabled = false;

    }

    void Update()
    {

        if (agro) { animEnemy.SetFloat("Speed", 1f); }

        float teleport = GetComponent<Teleport>().teleport;

        if (this.gameObject.activeSelf) { if (teleport == 0) { AI(); } }

        if (GD.ND.HOST) { if (teleport == 0 && canAttack) { FindTargetRayCast(); } } //dtermines & finds target
        
        //=================================== E M I T =============================================
        if (GD.twoPlayer && GD.ND.HOST)
        {
            //actions = this.name + target + destination + attacking; 

            //ONLY EMIT on STEPS 1 & 3
            float teleChange = teleport;
            if (teleport == 2 || teleport ==1.5) { teleChange = 1; }//skip step 2 of tele
            if (teleport == 0 && prevTeleport == 3) { teleChange = 3; } //dont trigger emit keep client on 3, client side changes to 0
            prevTeleport = teleChange;
            if (teleport > 0) { target = GetComponent<Teleport>().target; }
            actions = $"{{{target} {destination} {attacking} {teleChange}'}}";//determines what events to emit on change

            if (actions != prevActions) //actions change
            {
                    send = $"{{'object':'{this.name}','dead':'false','Attack':'{attacking}','target':'{target}','teleport':'{teleChange}','curWayPoint':'{curWayPoint}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                    GD.ND.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);

                prevActions = actions;
            }
        }

       


    }


    public void TriggerHitEnable()
    {
        HIT_COL.GetComponent<EnemyDamage>().triggerHit = true;

    }
    public void TriggerHitDisable()
    {
        HIT_COL.GetComponent<EnemyDamage>().triggerHit = true;
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

                        if (GD.ND.HOST) { destination = wayPoint[curWayPoint].position; navmesh.SetDestination(wayPoint[curWayPoint].position); }
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

                    if (GD.ND.HOST) { navmesh.SetDestination(wayPoint[0].position); destination = wayPoint[0].position; }
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
       // Debug.Log("=-================================ ENEMY DISTANCE==================================" + distance);
        //if(!ND.HOST) { distance -= 0.35f; }// IS THE DELAY FOR PLAYER ACTION EMITS 

        //----------FOR HOST
        if (GD.ND.HOST)
        {
            if (distance > disEngageRange && !agro)
            {
                target = null;

            }
                //RUN TO TARGET
                if (distance > hitRange)
            {
                animEnemy.SetBool("Fighting", false);
                GetComponent<NavMeshAgent>().speed = walkSpeed;//DOESNT AFFECT THIS
                //GetComponent<NavMeshAgent>().enabled = true;
                if (attacking)
                {
                    attacking = false;
                    //send = $"{{'object':'{this.name}','Attack':'{attacking}','target':'{target}','curWayPoint':'{curWayPoint}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                    //ND.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);

                }
                navmesh.isStopped = false;
                animEnemy.SetBool("Run", true);
                animEnemy.SetBool("Attack", false);
                if (distance > minDist) { transform.LookAt(targetPlayer); }
            }
            //ATTACK TARGET
            if (distance <= hitRange)
            {
                attacking = true;
                animEnemy.SetBool("Fighting", true);
                //GetComponent<NavMeshAgent>().enabled = false;
                GetComponent<NavMeshAgent>().speed = 0;

                if (!attacking)
                {
                    attacking = true;
                    //send = $"{{'object':'{this.name}','Attack':'{attacking}','target':'{target}','curWayPoint':'{curWayPoint}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                    //ND.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);

                }
                navmesh.isStopped = true;
                animEnemy.SetBool("Run", false);
                //TURN TO TARGET
                if (distance > minDist)
                {
                    Vector3 direction = (target.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10);
                }

                animEnemy.SetBool("Attack", true);
                if (distance > minDist) { transform.LookAt(targetPlayer); }
            }
        }
        else//---------FOR CLIENT
        {
            targetPlayer = target;
            //RUN TO TARGET
            if (!attacking)
            {
                animEnemy.SetBool("Fighting", false);
                //GetComponent<NavMeshAgent>().speed = walkSpeed;
                GetComponent<NavMeshAgent>().speed = walkSpeed;

                navmesh.isStopped = false;
                if (distance <= hitRange) { navmesh.isStopped = true; }
                animEnemy.SetBool("Run", true);
                animEnemy.SetBool("Attack", false);
                if (distance > minDist) { transform.LookAt(targetPlayer); }
            }
            //ATTACK TARGET
            else
            {
                animEnemy.SetBool("Fighting", true);
                //GetComponent<NavMeshAgent>().speed = 0;
                GetComponent<NavMeshAgent>().speed = 0;

                //KEEP NAVMESH IN SYNC WITH LERP
                if (distance > hitRange*1.1f)
                {
                    transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * 1f);
                }

                navmesh.isStopped = true;
                animEnemy.SetBool("Run", false);
                //TURN TO TARGET
                if (distance > minDist)
                {
                    Vector3 direction = (target.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10);
                }

                animEnemy.SetBool("Attack", true);
                
                if (GetComponent<Teleport>().debugAttack) { animEnemy.Play("Attack"); GetComponent<Teleport>().debugAttack = false; }
                if (distance > minDist) { transform.LookAt(targetPlayer); }
            }

        }

        if (target != null && GD.ND.HOST)
        {
            if (target == Player)
            {
                if (target.GetComponent<HealthSystem>().Health <= 0)
                {
                    target = null;
                    navmesh.isStopped = false;
                }
            }
            if (target == Client)
            {
                if (target.GetComponent<ClientPlayerController>().hp <= 0)
                {
                    target = null;
                    navmesh.isStopped = false;
                }
            }
        }
    }

    public void FindTargetRayCast()
    {
        if (target == null)
        {
            
            //DETERMINE TARGET
            float distance;
            float p1_dist = Vector3.Distance(head.position, Player.transform.position);
            float p2_dist = Vector3.Distance(head.position, Client.transform.position);

            //ALWAYS CHOOSE CLOSEST TARGET
            if (p1_dist < p2_dist) { distance = p1_dist; targetPlayer = Player.transform; } else { distance = p2_dist; targetPlayer =Client.transform; }

           // Debug.Log("DISTANCE FROM TARGET " + distance);

            if (distance <= range)
            {
                
                // Debug.Log("TARGET IS " + targetPlayer.gameObject.name + " DISTANCE " + distance);

                Quaternion look = Quaternion.LookRotation(targetPlayer.position - head.position);
                float angle = Quaternion.Angle(head.rotation, look);
               

                if (angle <= angleView) // can u see target
                {
                    Debug.Log("----------------------------------CAN SEE TARGET----------------------------------------");
                    RaycastHit hit;
                    Debug.DrawLine(head.position, targetPlayer.position + Vector3.up * 1.4f); //1.6
                   // LayerMask mask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Default"));
                   LayerMask mask = (1 << LayerMask.NameToLayer("Player"));
                    if (Physics.Linecast(head.position, targetPlayer.position + Vector3.up * 1.4f, out hit, mask.value) && hit.transform != head && hit.transform != transform)
                    {
                        Debug.Log("----------TARGET -------------------" + hit.collider.gameObject.name);
                        if (hit.transform == targetPlayer)
                        {
                            target = targetPlayer;
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

        //--------AGRO-----------
        agro = true; angleView = 360; 
        if (damageAmount == 0) { range = 10; }
        else { range = 30; }

        //-----------ENEMY DEATH---------------
        if (!otherPlayer) { healthEnemy -= damageAmount; }//do damage only locally

        if (healthEnemy <= 0)
        {
            if (GD.twoPlayer) //&& GD.ND.HOST
            {
                if(!otherPlayer)
                {
                    send = $"{{'object':'{this.name}','dead':'true','Attack':'{attacking}','target':'{target}','teleport':'{0}','curWayPoint':'{curWayPoint}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                    GD.ND.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);
                }
            }
            GetComponent<Teleport>().CheckTeleport(true, true);
            if (GD.ND.HOST) { GetComponent<Teleport>().Invoke("Respawn", 10f); }

            Instantiate(ragdollEnemy, transform.position, transform.rotation);
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
        AudioManager.instance.Play(knife);
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
