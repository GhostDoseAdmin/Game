using InteractionSystem;
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
    public int curWayPoint;

    [Header("ENEMY PARAMETRS")]
    [Space(10)]
    public int healthEnemy = 100;
    public GameObject handKnife;

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
    private NetworkDriver ND;
    public Transform targetPlayer;
    private GameObject Player;
    private GameObject Client;
    private string prevAni;
    private string currentAni;
    private string actions;
    private string prevActions;
    private Vector3 destination;

    void Start()
    {
        ND = GameObject.Find("NetworkDriver").GetComponent<NetworkDriver>();
        Player = GameObject.Find("Player");
        Client = GameObject.Find("Client");

        animEnemy = GetComponent<Animator>();
        navmesh = GetComponent<NavMeshAgent>();

        //targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        head = animEnemy.GetBoneTransform(HumanBodyBones.Head).transform;

        handKnife.GetComponent<Collider>().enabled = false;
    }

    void Update()
    {
        if (ND.HOST)
        {
            FindTargetRayCast();//dtermines & finds target

            actions = $"{{'object':'{this.name}','target':'{target}'}}";
            if (actions != prevActions) //target changes
            {
                Debug.Log(actions);
                ND.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(actions), false);
                prevActions = actions;
            }
        }
            Walking();

            //bool isDmg; 
            //if (animEnemy.GetCurrentAnimatorStateInfo(0).IsName("Damage")) { isDmg = true; };

           /* actions = $"{{'object':'{this.name}','Attack':'{animEnemy.GetBool("Attack")}', 'Run':'{animEnemy.GetBool("Run")}', 'Walk':'{animEnemy.GetBool("Walk")}', 'wx':'{destination.x}' , 'wy':'{destination.y}', 'wz':'{destination.z}'}}";

            if (actions != prevActions)
            {
                Debug.Log("SENDING ENEMY DATA");
                ND.sioCom.Instance.Emit("enemy", JsonConvert.SerializeObject(actions), false);
                prevActions = actions;
            }*/

        //}
        
    }

    public void Walking()
    {
        if (target != null)
        {
            Attack();
        }
        else if (target == null)
        {
            animEnemy.SetBool("Attack", false);
            animEnemy.SetBool("Run", false);

            if (wayPoint.Count > 1)
            {
                if (wayPoint.Count > curWayPoint)
                {
                    navmesh.SetDestination(wayPoint[curWayPoint].position);
                    destination = wayPoint[curWayPoint].position;
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
                navmesh.SetDestination(wayPoint[0].position);
                destination = wayPoint[0].position;
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
        //Debug.Log("ATTACKING");
        navmesh.SetDestination(target.position);
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > 1.5f)
        {
            //Debug.Log("APPROACH PLAYER");
            navmesh.isStopped = false;
            animEnemy.SetBool("Run", true);
            animEnemy.SetBool("Attack", false);
            transform.LookAt(targetPlayer);
        }
        else
        {
            navmesh.isStopped = true;
            animEnemy.SetBool("Run", false);

            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10);

            animEnemy.SetBool("Attack", true);
            transform.LookAt(targetPlayer);
            //Debug.Log("ATTACKING PLAYER");
        }

        if (target != null)
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
                    Debug.DrawLine(head.position, targetPlayer.position + Vector3.up * 1.2f); //1.6

                    if (Physics.Linecast(head.position, targetPlayer.position + Vector3.up * 1.2f, out hit) && hit.transform != head && hit.transform != transform)
                    {
                        //Debug.Log("LOOKING AT TARGET");
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
            Debug.DrawLine(head.position, targetPlayer.position + Vector3.up * 1.2f); //

            if (Physics.Linecast(head.position, targetPlayer.position + Vector3.up * 1.2f, out hit) && hit.transform != head && hit.transform != transform)
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

        if (healthEnemy <= 0)
        {
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
        handKnife.GetComponent<Collider>().enabled = true;
    }

    public void TriggerDisable()
    {
        handKnife.GetComponent<Collider>().enabled = false;
    }
}
