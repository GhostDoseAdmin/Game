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

    [Header("ENEMY PARAMETRS")]
    [Space(10)]
    public int healthEnemy = 100;
    //public GameObject handKnife;

    [Header("ENEMY TARGET")]
    [Space(10)]
    //public Transform player;
    public Transform target;
    public Transform head;
    public int visible;
    public int angleView;

    [Header("PLAYER RAGDOLL")]
    [Space(10)]
    public GameObject ragdollEnemy;

    [Header("ENEMY SOUNDS")]
    [Space(10)]
    [SerializeField] private string knife;

    public Animator animEnemy;

    public NavMeshAgent navmesh;

    //NETWORK
    private GameDriver GD;
    public Transform targetPlayer;
    private GameObject Player;
    private GameObject Client;
    private string actions;
    private string send;
    private string prevActions;
    public Vector3 destination;
    public Vector3 truePosition;
    public bool attacking = false;
    public float hitRange = 1.5f;

    private float attack_emit_timer = 0.0f;
    private float attack_emit_delay = 0.25f;//0.25
    private bool agro;

    void Start()
    {
        GD = GameObject.Find("GameController").GetComponent<GameDriver>();
        Player = GameObject.Find("GameController").GetComponent<GameDriver>().Player;
        Client = GameObject.Find("GameController").GetComponent<GameDriver>().Client;

        animEnemy = GetComponent<Animator>();
        navmesh = GetComponent<NavMeshAgent>();

        targetPlayer = Client.transform;
        head = animEnemy.GetBoneTransform(HumanBodyBones.Head).transform;

        //handKnife.GetComponent<Collider>().enabled = false;

    }

    void Update()
    {
        if (this.gameObject.activeSelf) { AI(); }

        if (GD.ND.HOST) { FindTargetRayCast(); } //dtermines & finds target
        if (GD.twoPlayer && GD.ND.HOST)
        {
           


            //actions = this.name + target + destination + attacking; //  + animEnemy.GetCurrentAnimatorClipInfo(0)[0].clip.name; //+ attacking
            actions = $"{{{target} {destination} {attacking}'}}";//determines what events to emit on change
            if (actions != prevActions) //actions change
            {
                //Debug.LogWarning(attacking);

                
                //if (Time.time > attack_emit_timer + attack_emit_delay)//prevent sending 2 msgs at once
                {
                    //if (attacking) {  Debug.Log("AAAAAAAAAAAAAAAAAAAAAATAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAk"); }
                    // send = $"{{'object':'{this.name}','target':'{target}','Attack':'{animEnemy.GetBool("Attack")}', 'Run':'{animEnemy.GetBool("Run")}', 'Walk':'{animEnemy.GetBool("Walk")}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                    send = $"{{'object':'{this.name}','dead':'false','Attack':'{attacking}','target':'{target}','curWayPoint':'{curWayPoint}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                    GD.ND.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);

                    //attack_emit_timer = Time.time;//cooldown
                }
                prevActions = actions;
            }
        }

       


    }

    public void AI()
    {
        if (target != null)
        {
            Attack();
        }
        //-------------------------WAY POINTS ------------------------
        else if (target == null)
        {
            animEnemy.SetBool("Attack", false);
            animEnemy.SetBool("Run", false);

            if (wayPoint.Count > 1)
            {
                if (wayPoint.Count > curWayPoint)
                {

                    if (GD.ND.HOST){destination = wayPoint[curWayPoint].position;    navmesh.SetDestination(wayPoint[curWayPoint].position);}
                    else {navmesh.SetDestination(destination);}

                    float distance = Vector3.Distance(transform.position, wayPoint[curWayPoint].position);

                    if (distance > 1f)
                    {
                        animEnemy.SetBool("Walk", true);
                    }
                    else
                    {
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
                
                if (GD.ND.HOST) {                    navmesh.SetDestination(wayPoint[0].position);                    destination = wayPoint[0].position;                }
                else                {                    navmesh.SetDestination(destination);                }

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

    public void Attack()
    {

        //MOVE TOWARDS TARGET
        navmesh.SetDestination(target.position);

        float distance = Vector3.Distance(transform.position, target.position);
        //if(!ND.HOST) { distance -= 0.35f; }// IS THE DELAY FOR PLAYER ACTION EMITS 

        //----------FOR HOST
        if (GD.ND.HOST)
        {
            //RUN TO TARGET
            if (distance > hitRange)
            {
                if (attacking)
                {
                    attacking = false;
                    //send = $"{{'object':'{this.name}','Attack':'{attacking}','target':'{target}','curWayPoint':'{curWayPoint}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                    //ND.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);

                }
                navmesh.isStopped = false;
                animEnemy.SetBool("Run", true);
                animEnemy.SetBool("Attack", false);
                transform.LookAt(targetPlayer);
            }
            //ATTACK TARGET
            if (distance <= hitRange)
            {
                attacking = true;

                if (!attacking)
                {
                    attacking = true;
                    //send = $"{{'object':'{this.name}','Attack':'{attacking}','target':'{target}','curWayPoint':'{curWayPoint}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                    //ND.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);

                }
                navmesh.isStopped = true;
                animEnemy.SetBool("Run", false);

                Vector3 direction = (target.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10);

                animEnemy.SetBool("Attack", true);
                transform.LookAt(targetPlayer);
            }
        }
        else//---------FOR CLIENT
        {

            //RUN TO TARGET
            if (!attacking)
            {

                navmesh.isStopped = false;
                animEnemy.SetBool("Run", true);
                animEnemy.SetBool("Attack", false);
                transform.LookAt(targetPlayer);
            }
            //ATTACK TARGET
            else
            {
                navmesh.isStopped = true;
                animEnemy.SetBool("Run", false);

                Vector3 direction = (target.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10);

                animEnemy.SetBool("Attack", true);
                transform.LookAt(targetPlayer);
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

            if (p1_dist < p2_dist) { distance = p1_dist; targetPlayer = Player.transform; } else { distance = p2_dist; targetPlayer =Client.transform; }

           // Debug.Log("DISTANCE FROM TARGET " + distance);

            if (distance <= visible)
            {
                
                // Debug.Log("TARGET IS " + targetPlayer.gameObject.name + " DISTANCE " + distance);

                Quaternion look = Quaternion.LookRotation(targetPlayer.position - head.position);
                float angle = Quaternion.Angle(head.rotation, look);
                //Debug.Log("TARGET VISIBLE  " + angle + " VIEW ANGLE " + angleView);

                if (angle <= angleView) // can u see target
                {
                    RaycastHit hit;
                    Debug.DrawLine(head.position, targetPlayer.position + Vector3.up * 1.4f); //1.6
                    LayerMask mask = 1 << LayerMask.NameToLayer("Player");
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

    public void TakeDamage(int damageAmount)
    {
        healthEnemy -= damageAmount;

        //-----------ENEMY DEATH---------------
        if (healthEnemy <= 0)
        {
            if (GD.twoPlayer && GD.ND.HOST)
            {
                send = $"{{'object':'{this.name}','dead':'true','Attack':'{attacking}','target':'{target}','curWayPoint':'{curWayPoint}','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}','dx':'{destination.x}','dy':'{destination.y}','dz':'{destination.z}'}}";
                GD.ND.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(send), false);
            }
            
            gameObject.SetActive(false);
            Instantiate(ragdollEnemy, transform.position, transform.rotation);
        }
        else
        {
            animEnemy.SetTrigger("Damage"); 
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
