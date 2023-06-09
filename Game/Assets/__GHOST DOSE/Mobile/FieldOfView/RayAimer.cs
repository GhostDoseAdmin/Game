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
    private GameObject Player;
    private ShootingSystem SS;

    public bool AIMING;
    public GameObject RayTarget;
    Quaternion targetRotation;

    public bool crossHairTarg; // used for crosshairs
    public GameObject validTarget;
    private void Start() {
        Player = GameDriver.instance.Player;
        SS = Player.GetComponent<ShootingSystem>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material.SetColor("_Color", Color.cyan);
        origin = Vector3.zero;
        startViewDistance = viewDistance;
        startFov = 65f;
        DisableAimer();
    }

    public void EnableAimer()
    {
            AIMING = true;
            fov = startFov;
            GetComponent<MeshRenderer>().material.SetColor("_Color", Color.cyan); 

    }

    public void DisableAimer()
    {
        AIMING = false;
        crossHairTarg = false;
        viewDistance = startViewDistance;
        RayTarget = null;
        validTarget = null;
        SS.target = null;
        GetComponent<MeshRenderer>().material.SetFloat("_Alpha", 0f);
        fov = 0;
    }
    public float XANGLE, YANGLE, ZANGLE, offset;
        private void Update() {

        //transform.position = SS.shootPoint.position;// Player.transform.position + Vector3.up;
        //float yAngle = -Player.transform.eulerAngles.y + 180f;
        //if (yAngle > 180f) { yAngle -= 360f; }
        //transform.localRotation = Quaternion.Euler(90f, 0f, yAngle);

        transform.position = SS.shootPoint.position;
        transform.SetParent(SS.shootPoint);
        //transform.eulerAngles = new Vector3(297f, 180f, 172f);

        //CLOSEST TARGET DETERMIENS ANGLE / DEFAULT TARGET
        validTarget = FindValidTarget(viewDistance);
        if (validTarget != null)
        {
            Vector3 targPos = (Player.transform.position + Player.transform.forward * 5f) + Vector3.up;
            Vector3 targetLookHeight = new Vector3(targPos.x, validTarget.GetComponentInParent<Animator>().GetBoneTransform(HumanBodyBones.Hips).transform.position.y, targPos.z);
            Vector3 eulerRotation = Quaternion.LookRotation(targetLookHeight - transform.position, Vector3.up).eulerAngles;

            float distanceToTarget = Vector3.Distance(SS.shootPoint.transform.position, validTarget.transform.position);
            eulerRotation.x = -Mathf.Atan2(distanceToTarget, transform.position.y - targetLookHeight.y ) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(eulerRotation);



            /*Vector3 targetPosition = validTarget.GetComponentInParent<Animator>().GetBoneTransform(HumanBodyBones.Hips).transform.position; //closestTarget.transform.position + (Vector3.up*1.2f);
            Vector3 direction = targetPosition - transform.position;
            float xAngle = Mathf.Atan2(direction.z, direction.y) * Mathf.Rad2Deg;
            if (transform.position.z > targetPosition.z) { xAngle = xAngle - 180; }
            targetRotation = Quaternion.Euler(xAngle, 0f, yAngle);
            transform.localRotation = targetRotation;*/

        }

        //ENABLE/DISABLE AIMER
        if (validTarget != null)
        {
            if (!AIMING) { EnableAimer(); }
        }
        else { DisableAimer(); }

             fov -= 35f * Time.deltaTime;  //AIM TIME
            viewDistance+=0.1f;//0.2
            //SHOW AIMER
            if (fov < startFov && fov>0) { GetComponent<MeshRenderer>().material.SetFloat("_Alpha", 0.314f); }
            //HIDE AIMER
            if (fov <= 0f){  GetComponent<MeshRenderer>().material.SetFloat("_Alpha", 0f); RayTarget = null; } //StartAim = false;
            //RESTART AIM
            if (fov < -30f) { fov = startFov; }

        if (AIMING && fov>0)
        {
            SS.Damage = 35 + ((int)startFov - (int)fov) * 2;
            //HEADSHOT
            SS.isHeadshot = false;
            if (fov < 20f) { SS.isHeadshot = true; GetComponent<MeshRenderer>().material.SetColor("_Color", Color.yellow); SS.headShotDamage = 101; }
            if (fov < 10f) { SS.isHeadshot = true; GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red); SS.headShotDamage = 150; }

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

            //-------------RAY TARGET-----------------
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
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Default")) { continue; }
                    float targDist = Vector3.Distance(Player.transform.position, hitObject.transform.position);
                    if (targDist <= closestDistance)
                    {
                        if ((hitObject.layer == LayerMask.NameToLayer("Enemy") && hitObject.GetComponentInParent<Teleport>() != null && hitObject.GetComponentInParent<Teleport>().teleport == 0 && hitObject.GetComponentInParent<NPCController>().healthEnemy > 0) || (hitObject.tag.Contains("Victim")))
                        {
                            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                            {
                                if (hit.collider.gameObject.GetComponentInParent<GhostVFX>().Shadower && hit.collider.gameObject.GetComponentInParent<GhostVFX>().visible)
                                {
                                    closestDistance = targDist;
                                    RayTarget = hitObject;
                                }
                                else if (!hit.collider.gameObject.GetComponentInParent<GhostVFX>().Shadower && !hit.collider.gameObject.GetComponentInParent<GhostVFX>().invisible)
                                {
                                    closestDistance = targDist;
                                    RayTarget = hitObject;
                                }

                            }
                            else
                            {
                                closestDistance = targDist;
                                RayTarget = hitObject;
                            }

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
                //Debug.Log("RAY TARGET " + RayTarget.name);
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
            /*else
            { //FALLBACK ON CLOSEST TARGET
                if (closestTarget != null && Vector3.Distance(Player.transform.position, closestTarget.transform.position) < 2 && Vector3.Dot(GameDriver.instance.Player.transform.forward, closestTarget.transform.forward) < 0.6f) { SS.target = closestTarget; }
            }*/

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

    //CLOSEST TARGET WITHIN 180 VIEW ANGLE OF PLAYER 
    public GameObject FindValidTarget(float dist)
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
                    RaycastHit hit; //if the line between player and enemy hits a default object
                    if (Physics.Linecast(Player.GetComponent<ShootingSystem>().shootPoint.transform.position, target.GetComponentInParent<Animator>().GetBoneTransform(HumanBodyBones.Hips).transform.position, out hit, layerMask))
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Default")) { continue; }

                    if (hit.collider != null)
                    {
                        if ((hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") && hit.collider.gameObject.GetComponentInParent<Teleport>() != null && hit.collider.gameObject.GetComponentInParent<Teleport>().teleport == 0 && hit.collider.gameObject.GetComponentInParent<NPCController>().healthEnemy > 0) || (hit.collider.gameObject.tag.Contains("Victim")))
                        {
                            bool facingTarget = Vector3.Dot(Player.transform.forward, target.transform.position - Player.transform.position) > 0.95f;
                            if (facingTarget)
                            {
                                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                                {
                                    if (hit.collider.gameObject.GetComponentInParent<GhostVFX>().Shadower && hit.collider.gameObject.GetComponentInParent<GhostVFX>().visible)
                                    {
                                        closestTarget = target;
                                        closestDistance = targetDistance;
                                    }
                                    else if (!hit.collider.gameObject.GetComponentInParent<GhostVFX>().Shadower && !hit.collider.gameObject.GetComponentInParent<GhostVFX>().invisible)
                                    {
                                        closestTarget = target;
                                        closestDistance = targetDistance;
                                    }

                                }
                                else//VICTIM
                                {
                                    closestTarget = target;
                                    closestDistance = targetDistance;
                                }
                            }
                        }
                    }
                }
            }
        }

        return closestTarget;

    }
}
