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

    public bool AIMING;
    public GameObject RayTarget;
    Quaternion targetRotation;

    public bool crossHairTarg; // used for crosshairs
    private GameObject closestTarget;
    private void Start() {
        Player = GameDriver.instance.Player;
        SS = Player.GetComponent<ShootingSystem>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material.SetColor("_Color", Color.cyan);
        AIMING = false;
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

        AIMING = false;
        crossHairTarg = false;
        viewDistance = startViewDistance;
        RayTarget = null;
        closestTarget = null;
        SS.target = null;
        }
    }
        private void Update() {

        transform.position = SS.shootPoint.position;// Player.transform.position + Vector3.up;

        float yAngle = -Player.transform.eulerAngles.y + 180f;
        if (yAngle > 180f) { yAngle -= 360f; }
        //Debug.Log(yAngle);
        if (shrink) { transform.localRotation = Quaternion.Euler(90f, 0f, yAngle); }


        //CLOSEST TARGET DETERMIENS ANGLE / DEFAULT TARGET
        closestTarget = FindTarget(viewDistance);
        if (closestTarget != null)
        {
            if(Vector3.Distance(transform.position, closestTarget.transform.position)>2)
            {
                bool facingTarget = Vector3.Dot(Player.transform.forward, closestTarget.transform.position - Player.transform.position) > 0f;
                if (facingTarget)
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
            if(shrink) { fov -= 35f * Time.deltaTime; } //AIM TIME
            viewDistance+=0.1f;//0.2
            //SHOW AIMER
            if (fov < startFov -5) { GetComponent<MeshRenderer>().material.SetFloat("_Alpha", 0.314f); AIMING = true; }
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
            
            RayTarget = null;
            float closestDistance = 99999;
            //Shoot Ray at the Y level of closest target to ensure hit regardless of heigh difference
            //-------------RAY AIMER-----------------
            Vector3 raycastOrigin = transform.TransformPoint(origin); // Transform the origin to the mesh's position
            for (int i = 0; i <= rayCount; i++)
            {
                Vector3 vertex = origin + GetVectorFromAngle(angle) * viewDistance;
                RaycastHit hit;

                Vector3 raycastDirection = transform.TransformDirection(GetVectorFromAngle(angle)); // Transform the direction to the mesh's orientation

                bool isHit = Physics.Raycast(raycastOrigin, raycastDirection, out hit, viewDistance, layerMask);
                Debug.DrawLine(raycastOrigin, raycastOrigin + raycastDirection * viewDistance, Color.yellow);

                if (isHit)
                {
                    GameObject hitObject = hit.collider.gameObject;
                    float targDist = Vector3.Distance(Player.transform.position, hitObject.transform.position);
                    if (targDist <= closestDistance)
                    {
                        if ((hitObject.layer == LayerMask.NameToLayer("Enemy") && hitObject.GetComponentInParent<Teleport>() != null && hitObject.GetComponentInParent<Teleport>().teleport == 0 && hitObject.GetComponentInParent<NPCController>().healthEnemy > 0) || (hitObject.tag.Contains("Victim")))
                        {
                            closestDistance = targDist;
                            RayTarget = hitObject;
                        }
                    }
                }
                    //FOV CONE MESH
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

            //TARGET PARAMS
            crossHairTarg = false;
            if (RayTarget != null)
            {
                Debug.Log("RAY TARGET " + RayTarget.name);
                if (RayTarget.layer == LayerMask.NameToLayer("Enemy"))
                {
                    if (!RayTarget.GetComponentInParent<GhostVFX>().Shadower) { SS.isVisible = !RayTarget.GetComponentInParent<GhostVFX>().invisible; if (SS.isVisible) { crossHairTarg = SS.isVisible; } }
                    else { SS.isVisible = RayTarget.GetComponentInParent<GhostVFX>().visible; if (RayTarget.GetComponentInParent<NPCController>().target != null) { crossHairTarg = true; } }
                    SS.target = RayTarget.GetComponentInParent<NPCController>().gameObject;
                }
                else
                {
                    SS.isVisible = true;
                    SS.target = RayTarget.GetComponentInParent<Person>().gameObject;
                    crossHairTarg = true;
                }
            }
            else { //FALLBACK ON CLOSEST TARGET
                if (closestTarget != null && Vector3.Distance(Player.transform.position, closestTarget.transform.position) < 2 && Vector3.Dot(GameDriver.instance.Player.transform.forward, closestTarget.transform.forward) < 0.6f) { SS.target = closestTarget; }
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
