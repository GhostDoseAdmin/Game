using InteractionSystem;
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
    public Transform player;
    Transform target;
    public Transform head;
    public int visible;
    public int angleView;

    [Header("PLAYER RAGDOLL")]
    [Space(10)]
    public GameObject ragdollEnemy;

    [Header("ENEMY SOUNDS")]
    [Space(10)]
    [SerializeField] private string knife;

    Animator animEnemy;

    NavMeshAgent navmesh;


    void Start()
    {
        animEnemy = GetComponent<Animator>();
        navmesh = GetComponent<NavMeshAgent>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        head = animEnemy.GetBoneTransform(HumanBodyBones.Head).transform;

        handKnife.GetComponent<Collider>().enabled = false;
    }

    void Update()
    {
        FindTargetRayCast();
        Walking();
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
        navmesh.SetDestination(target.position);
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > 1.5f)
        {
            navmesh.isStopped = false;
            animEnemy.SetBool("Run", true);
            animEnemy.SetBool("Attack", false);
            transform.LookAt(player);
        }
        else
        {
            navmesh.isStopped = true;
            animEnemy.SetBool("Run", false);

            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10);

            animEnemy.SetBool("Attack", true);
            transform.LookAt(player);
        }

        if (target.GetComponent<HealthSystem>().Health <= 0)
        {
            target = null;
            navmesh.isStopped = false;
        }
    }

    public void FindTargetRayCast()
    {
        if (target == null)
        {
            float distance = Vector3.Distance(head.position, player.position);

            if (distance <= visible)
            {
                Quaternion look = Quaternion.LookRotation(player.position - head.position);
                float angle = Quaternion.Angle(head.rotation, look);

                if (angle <= angleView)
                {
                    RaycastHit hit;
                    Debug.DrawLine(head.position, player.position + Vector3.up * 1.6f);

                    if (Physics.Linecast(head.position, player.position + Vector3.up * 1.6f, out hit) && hit.transform != head && hit.transform != transform)
                    {
                        if (hit.transform == player)
                        {
                            target = player;
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
        else
        {
            RaycastHit hit;
            Debug.DrawLine(head.position, player.position + Vector3.up * 1.6f);

            if (Physics.Linecast(head.position, player.position + Vector3.up * 1.6f, out hit) && hit.transform != head && hit.transform != transform)
            {
                if (hit.transform == player)
                {
                    target = player;;
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
