using System.Net;
using UnityEngine;
using GameManager;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using NetworkSystem;

public class RayAimer : MonoBehaviour {

    [SerializeField] private LayerMask layerMask;
    public Mesh mesh;
    public float fov;
    public float startFov = 45;
    public float viewDistance;
    private float startViewDistance;
    private Vector3 origin;
    public float startingAngle;
    //private bool StartAim = false;
    public bool shrink = false;
    private GameObject Player;
    private ShootingSystem SS;

    public bool indicator;

    Quaternion targetRotation;

    public bool crossHairTarg; // used for crosshairs

    private void Start() {
        Player = GameDriver.instance.Player;
        SS = Player.GetComponent<ShootingSystem>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material.SetColor("_Color", Color.cyan);
        indicator = false;
        origin = Vector3.zero;
        startViewDistance = viewDistance;
        startFov = 65f;
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (NetworkDriver.instance.isMobile)
        {
            shrink = true;
            fov = startFov;
            if (SS != null) { SS.isHeadshot = false; }
            if (GetComponent<MeshRenderer>() != null && GetComponent<MeshRenderer>().material!=null) {
                GetComponent<MeshRenderer>().material.SetColor("_Color", Color.cyan); 
                GetComponent<MeshRenderer>().material.SetFloat("_Alpha", 0); 
            }
        }
    }

    void OnDisable()
    {
        if (NetworkDriver.instance.isMobile)
        {
        SS.target = null;
        indicator = false;
        crossHairTarg = false;
        viewDistance = startViewDistance;
        }
    }
        private void Update() {

        transform.position = SS.shootPoint.position;// Player.transform.position + Vector3.up;

        float yAngle = -Player.transform.eulerAngles.y + 180f;
        if (yAngle > 180f) { yAngle -= 360f; }
        //Debug.Log(yAngle);
        if (shrink) { transform.localRotation = Quaternion.Euler(90f, 0f, yAngle); }


        //ANGLE RAY BASED ON HEIGHT OF CLOSEST ENEMY
        GameObject closestTarget = FindTarget(viewDistance);
        if (closestTarget != null)
        {
            if(Vector3.Distance(transform.position, closestTarget.transform.position)>2)
            {
                bool facingEnemy = Vector3.Dot(Player.transform.forward, closestTarget.transform.position - Player.transform.position) > 0f;
                if (facingEnemy)
                {
                    Vector3 targetPosition = closestTarget.GetComponentInParent<Animator>().GetBoneTransform(HumanBodyBones.Hips).transform.position; //closestTarget.transform.position + (Vector3.up*1.2f);
                    Vector3 direction = targetPosition - transform.position;
                    float xAngle = Mathf.Atan2(direction.z, direction.y) * Mathf.Rad2Deg;
                    if (transform.position.z > targetPosition.z) { xAngle = xAngle - 180; }
                    targetRotation = Quaternion.Euler(xAngle, 0f, yAngle);


                    if (shrink) { transform.localRotation = targetRotation; }
                }
            }
        }



        {
           // fov = 45;
            if(shrink){ fov -= 50f * Time.deltaTime; } //AIM TIME
            viewDistance+=0.2f;
            //SHOW AIMER
            if (fov < startFov -5) { GetComponent<MeshRenderer>().material.SetFloat("_Alpha", 0.314f); indicator = true; }
            //END AIM
            if (fov < 0f){ fov = 0; } //StartAim = false;
            SS.Damage = 35 + ((int)startFov - (int)fov)*2;
            //HEADSHOT
            if (fov < 40f) { SS.isHeadshot = true; GetComponent<MeshRenderer>().material.SetColor("_Color", Color.yellow); SS.headShotDamage =  101; }
            if (fov < 20f) { SS.isHeadshot = true; GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red); SS.headShotDamage =  150; }

            int rayCount = 50;

            float angle = startingAngle + (fov * 0.5f);
            float angleIncrease = fov / rayCount;

            Vector3[] vertices = new Vector3[rayCount + 1 + 1];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[rayCount * 3];

            vertices[0] = origin;

            int vertexIndex = 1;
            int triangleIndex = 0;
            
            //Shoot Ray at the Y level of closest target to ensure hit regardless of heigh difference
            //Vector3 raycastOrigin = new Vector3(transform.TransformPoint(origin).x, FindTarget().transform.position.y, transform.TransformPoint(origin).z) + Vector3.up;
            Vector3 raycastOrigin = transform.TransformPoint(origin); // Transform the origin to the mesh's position
            for (int i = 0; i <= rayCount; i++)
            {
                Vector3 vertex = origin + GetVectorFromAngle(angle) * viewDistance;
                RaycastHit hit;

                Vector3 raycastDirection = transform.TransformDirection(GetVectorFromAngle(angle)); // Transform the direction to the mesh's orientation

               bool isHit = Physics.Raycast(raycastOrigin, raycastDirection, out hit, viewDistance, layerMask);
                Debug.DrawLine(raycastOrigin, raycastOrigin + raycastDirection * viewDistance, Color.yellow);

               
                float closestDistance = 99999;
                if (isHit)
                {
                    GameObject ClosestTarget = hit.collider.gameObject;
                    float targDist = Vector3.Distance(transform.position, ClosestTarget.transform.position);
                    if (targDist <= closestDistance) {
                        closestDistance = targDist;
                        if (ClosestTarget.layer == LayerMask.NameToLayer("Enemy")) {
                            if (ClosestTarget.GetComponentInParent<Teleport>()!=null && ClosestTarget.GetComponentInParent<Teleport>().teleport == 0)
                            {
                                if (!ClosestTarget.GetComponentInParent<GhostVFX>().Shadower) { SS.isVisible = !ClosestTarget.GetComponentInParent<GhostVFX>().invisible; if (SS.isVisible) { crossHairTarg = SS.isVisible; } }
                                else { SS.isVisible = ClosestTarget.GetComponentInParent<GhostVFX>().visible; if (ClosestTarget.GetComponentInParent<NPCController>().target != null) { crossHairTarg = true; } }
                                SS.target = ClosestTarget.GetComponentInParent<NPCController>().gameObject;
                            }
                        }
                        else {
                            SS.isVisible = true;
                            SS.target = ClosestTarget.GetComponentInParent<Person>().gameObject; 
                        }

                    }
                }

                vertices[vertexIndex] = vertex;

                if (i > 0)
                {
                    triangles[triangleIndex + 0] = 0;
                    triangles[triangleIndex + 1] = vertexIndex - 1;
                    triangles[triangleIndex + 2] = vertexIndex;

                    triangleIndex += 3;
                }

                vertexIndex++;
                angle -= angleIncrease;
            }


            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.bounds = new Bounds(origin, Vector3.one * 1000f);

        }
    }


    private static Vector3 GetVectorFromAngle(float angle)
    {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public GameObject FindTarget(float dist)
    {
        List<NPCController> enemies = new List<NPCController>(FindObjectsOfType<NPCController>());
        List<Person> victims = new List<Person>(FindObjectsOfType<Person>());

        List<GameObject> targets = new List<GameObject>();

        foreach (NPCController enemy in enemies)
        {
            targets.Add(enemy.gameObject);
        }

        foreach (Person victim in victims)
        {
            targets.Add(victim.gameObject);
        }

        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            float targetDistance = Vector3.Distance(Player.transform.position, target.transform.position);
            if (targetDistance < dist)
            {
                if (targetDistance < closestDistance)
                {
                    closestTarget = target;
                    closestDistance = targetDistance;
                }
            }
        }

        return closestTarget;

    }
}
