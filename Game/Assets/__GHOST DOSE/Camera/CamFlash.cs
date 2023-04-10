using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[ExecuteInEditMode]
public class CamFlash : MonoBehaviour
{
    // Start is called before the first frame update
    private List<NPCController> ghostObjects;

    IEnumerator DestroyAfterDelay(GameObject obj)
    {
        yield return new WaitForSeconds(1f);
        this.gameObject.GetComponent<Light>().spotAngle = 0; //cleanup for ghostVFX - resets material to previous state
        yield return new WaitForSeconds(0.1f);
        Destroy(obj);
    }

    public void Start()
    {
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
            Debug.DrawLine(spotlight.transform.position, ghost.gameObject.transform.position + Vector3.up * hitHeight, Color.blue);
            float angleToObject = Vector3.Angle(spotlight.transform.forward, directionToObject);
            bool inCone = angleToObject <= adjustedSpotAngle * 0.5f;

            //CHECK FOR OBSTRUCTION
            if (inCone && inRange)
            {
                LayerMask mask = 1 << LayerMask.NameToLayer("Default");
                hitHeight = 1.3f; // adjust the hit height 
                Vector3 targPos = new Vector3(ghost.gameObject.transform.position.x, ghost.gameObject.transform.position.y + hitHeight, ghost.gameObject.transform.position.z);
                Ray ray = new Ray(spotlight.transform.position, (targPos - spotlight.transform.position).normalized);
                float distance = Vector3.Distance(spotlight.transform.position, targPos);
                Vector3 endPoint = ray.GetPoint(distance);
                Debug.DrawLine(spotlight.transform.position, endPoint, Color.yellow);
                RaycastHit hit;
                if (Physics.Linecast(spotlight.transform.position, endPoint, out hit, mask.value))
                {
                    if (hit.collider.transform.root.gameObject == ghost.gameObject)
                    {
                       // Debug.Log("ENYM HIT -------------------------------------------------------");
                        ghost.TakeDamage(0);
                        ghost.visible = 10;//AGRO
                    }
                }

            }
        }
        
    }


}
