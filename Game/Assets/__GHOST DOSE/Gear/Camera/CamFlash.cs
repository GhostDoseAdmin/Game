using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using InteractionSystem;

//[ExecuteInEditMode]
public class CamFlash : MonoBehaviour
{
    // Start is called before the first frame update
    private List<NPCController> ghostObjects;
    public bool isClient;
    private AudioSource audioSource;
    public bool shotgun;
    IEnumerator DestroyAfterDelay(GameObject obj)
    {
        yield return new WaitForSeconds(0.5f);
        this.gameObject.GetComponent<Light>().spotAngle = 0; //cleanup for ghostVFX - resets material to previous state
        yield return new WaitForSeconds(0.1f);
        //Destroy(obj);
    }

    public void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;

        AudioManager.instance.Play("camshot", audioSource);

        StartCoroutine(DestroyAfterDelay(this.gameObject));
       
        ghostObjects = new List<NPCController>();

        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        GameObject[] shadowers = GameObject.FindGameObjectsWithTag("Shadower");

        
        foreach (GameObject shadower in shadowers)
        {
            NPCController enemy = shadower.GetComponent<NPCController>();
            if (enemy != null)
            {
                ghostObjects.Add(enemy);
            }
        }

        foreach (GameObject ghost in ghosts)
        {
            NPCController enemy = ghost.GetComponent<NPCController>();
            if (enemy != null)
            {
                ghostObjects.Add(enemy);
            }
        }



        foreach (NPCController ghost in ghostObjects)
        {
            Light spotlight = GetComponent<Light>();
            //CHECK FOR IN CONE OF LIGHT
            float adjustedSpotAngle = spotlight.spotAngle;
            float hitHeight = 1f;
            float distanceToObject = Vector3.Distance(ghost.gameObject.transform.position + Vector3.up * hitHeight, spotlight.transform.position);
            bool inRange = distanceToObject <= spotlight.range;
            Vector3 directionToObject = (ghost.gameObject.transform.position + Vector3.up * hitHeight - spotlight.transform.position).normalized;
            Debug.DrawLine(spotlight.transform.position, ghost.gameObject.transform.position + Vector3.up * hitHeight, Color.magenta);
            float angleToObject = Vector3.Angle(spotlight.transform.forward, directionToObject);
            bool inCone = angleToObject <= adjustedSpotAngle * 0.5f;

            //CHECK FOR OBSTRUCTION
            if (inCone && inRange)
            {
                /*LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Enemy");
                hitHeight = 1.3f; // adjust the hit height 
                Vector3 targPos = new Vector3(ghost.gameObject.transform.position.x, ghost.gameObject.transform.position.y + hitHeight, ghost.gameObject.transform.position.z);
                Ray ray = new Ray(spotlight.transform.position, (targPos - spotlight.transform.position).normalized);
                float distance = Vector3.Distance(spotlight.transform.position, targPos);
                Vector3 endPoint = ray.GetPoint(distance);
                Debug.DrawLine(spotlight.transform.position, endPoint, Color.yellow);
                RaycastHit hit;
                if (Physics.Linecast(spotlight.transform.position, endPoint, out hit, mask.value))*/
                {

                    //if (FindEnemyMain(hit.collider.gameObject.transform) == ghost.gameObject)
                    {
                        if (ghost.gameObject.GetComponent<Teleport>().teleport == 0 && ghost.healthEnemy > 0)
                        {
                            int damage = 0;
                            //if (shotgun) { damage = 5000; }
                            ghost.TakeDamage(damage, isClient);
                        }
                        //ghost.agro = true;
                        //ghost.range = ghost.range * 2;
                    }
                }
            }
        }
    }

    GameObject FindEnemyMain(Transform head)
    {
        Transform currentTransform = head;
        while (currentTransform != null)
        {
            if (currentTransform.GetComponent<NPCController>() != null)
            {
                return currentTransform.gameObject;
            }
            currentTransform = currentTransform.parent;
        }
        return null;

    }
}
