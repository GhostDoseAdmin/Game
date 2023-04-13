using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Teleport : MonoBehaviour
{
    public int teleport;
    private int notVisible; //check visiblility over serveral frames 
    public bool realNotVisible;
    private Vector3 b4Pos;//before descending
    private bool delayForEmit;

    public Transform target;
    public float minRadius = 4.0f;
    public float maxRadius = 4.0f;

    private float timer = 0.0f;
    private float delay = 2.0f; //relocate interval
    private int relocate = 0;
    private bool canTeleport = true;
    void Start()
    {
        timer = 0;
        teleport = 0;
        relocate = 0;
        canTeleport = true;
    }


    private void Update()
    {
        if(teleport > 0) { GetComponent<GhostVFX>().Fade(false, 1f); }
        
        //TRIGGER
        if (teleport == 0 && GetComponent<NPCController>().GD.ND.HOST && canTeleport)
        {
            if (GetComponent<GhostVFX>().invisible && !GetComponent<GhostVFX>().visible)
            {
                if (GetComponent<NPCController>().agro && GetComponent<NPCController>().target != null)
                    {
                        if (Vector3.Distance(transform.position, GetComponent<NPCController>().target.transform.position) > 3)
                        {
                        teleport = 1;
                            b4Pos = transform.position;
                            target = GetComponent<NPCController>().target;
                            relocate = 0;
                            delayForEmit = true;
                            canTeleport = false;
                        //EMIT DISAPPEAR
                        }
                    }
                
            }
        }
        //STEP 1 - DESCEND ------CLIENT 
        if (teleport == 1)
        {
            if (!GetComponent<NPCController>().GD.ND.HOST) { GetComponent<NPCController>().enabled = false; }
            GetComponent<NavMeshAgent>().enabled = false;
            GetComponent<Animator>().enabled = false;
            //GetComponent<Animator>().SetBool("Attack", false);
          //GetComponent<Animator>().SetBool("Walk", false);
          //GetComponent<Animator>().SetBool("Run", false);
            Vector3 currPos = transform.position;
            currPos.y -= 0.1f;//descend speed
            transform.position = currPos;
            if (transform.position.y < b4Pos.y - 5) {
                if (GetComponent<NPCController>().GD.ND.HOST) { teleport = 2; }
                this.gameObject.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
               
            }

        }
        //STEP 2 - RELOCATE
        if (teleport == 2)
        {
            //EVER delay SECONDS RELOCATE
            if ((Time.time > timer + delay))
            {
                relocate++;
                transform.position = new Vector3(transform.position.x, b4Pos.y, transform.position.z);
                Vector2 randomDirection = Random.insideUnitCircle;
                float randomDistance = Random.Range(minRadius, maxRadius);
                Vector3 randomOffset = new Vector3(randomDirection.x, 0.0f, randomDirection.y) * randomDistance;
                transform.position = target.position + randomOffset;

                timer = Time.time;
            }
            //--REAPPEAR--
            if(GetComponent<GhostVFX>().invisible && !GetComponent<GhostVFX>().visible && relocate>1)
            {
                teleport = 3;
            }
        }
        //SETP 3 - REAPPEAR --CLIENT
        if(teleport == 3)
        {
            if (!delayForEmit)
            { //skip a frame to allow NPCcontroller to emit state 3
                teleport = 0;
                GetComponent<NavMeshAgent>().enabled = true;
                GetComponent<NPCController>().enabled = true;
                GetComponent<Animator>().enabled = true;
                this.gameObject.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = true;
                GetComponent<NPCController>().target = target;
                StartCoroutine(resetCanTeleport());
                //if (!GetComponent<NPCController>().GD.ND.HOST){StartCoroutine(clientAttackingAnimationDebug());}
            }
            if (delayForEmit){ delayForEmit = false; }
        }

        IEnumerator resetCanTeleport()//--------CONNECT HELPER--------->
        {
                yield return new WaitForSeconds(3f);
                canTeleport = true;
        }

        /*IEnumerator clientAttackingAnimationDebug()//--------CONNECT HELPER--------->
        {
            
            yield return new WaitForSeconds(0.5f);
            GetComponent<NPCController>().attacking = false;
            yield return new WaitForSeconds(0.5f);
            GetComponent<NPCController>().attacking = true;
        }*/



     }


}
