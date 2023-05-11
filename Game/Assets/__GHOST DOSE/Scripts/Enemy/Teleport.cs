using System.Collections;
using GameManager;
using NetworkSystem;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Controls;
using InteractionSystem;
using Newtonsoft.Json;

public class Teleport : MonoBehaviour
{
    public float teleport;
    private int notVisible; //check visiblility over serveral frames 
    public bool realNotVisible;
    private Vector3 b4Pos;//before descending
    private bool delayForEmit;

    public Transform target;
    public float minRadius;
    public float maxRadius;

    private float fadeTimer = 0;
    private float timer = 0.0f;
    private float delay = 4.0f; //relocate interval / musta llow time to fade
    private int relocate = 0;
    private float prevOutline;
    public bool canTeleport = true;
    public bool debugAttack;//resets attack ani state for client as would get stuck after restarting animator 
    private bool isWaypoint;
    public bool DEATH;
    private Outline outline;
    void Start()
    {
        timer = 0;
        teleport = 0;
        relocate = 0;
        canTeleport = true;
        isWaypoint = false;
        outline = transform.GetChild(0).GetComponent<Outline>();
    }

    public void CheckTeleport(bool WayPoints, bool death)
    {
        if (teleport == 0 && canTeleport)
        {
            if ((GetComponent<GhostVFX>().invisible && !GetComponent<GhostVFX>().visible) || (death))
            {
                teleport = 1;
                b4Pos = transform.position;
                relocate = 0;
                delayForEmit = true;
                canTeleport = false;
                isWaypoint = WayPoints;
                DEATH = death;
                if (!death) { prevOutline = outline.OutlineWidth; }
                outline.OutlineWidth = 0;
                //outline.enabled = false;

            }
        }
    }

    private void Update()
    {
        //---------------AGRO----------------------
        if (NetworkDriver.instance.HOST)
        {
            if (GetComponent<NPCController>().target != null && GetComponent<NPCController>().teleports && !GetComponent<NPCController>().zozoLaser) //GetComponent<NPCController>().agro && 
            {
               // if (Vector3.Distance(transform.position, GetComponent<NPCController>().target.transform.position) > 3)//3
                {
                    CheckTeleport(false, false);
                }
            }
        }
        //STEP 1 - SETUP
        if (teleport == 1)
        {
            if (NetworkDriver.instance.HOST) { NetworkDriver.instance.sioCom.Instance.Emit("teleport", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','tp':'1','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}'}}"), false); }
            fadeTimer = Time.time;
            if (GetComponent<NPCController>().target != null) { target = GetComponent<NPCController>().target; }
            if (target != null && GetComponent<NPCController>().healthEnemy>0) { AudioManager.instance.Play("Disappear", gameObject.GetComponent<NPCController>().audioSource); }
            if (!NetworkDriver.instance.HOST) { GetComponent<NPCController>().enabled = false; }
            debugAttack = true;
            GetComponent<NavMeshAgent>().enabled = false;
            GetComponent<Animator>().enabled = false;
            b4Pos = transform.position;
            if (GetComponent<NPCController>().healthEnemy > 0 && !isWaypoint) { AudioManager.instance.Play("haunting", null); }
            //GetComponent<NPCController>().SKEL_ROOT.GetComponent<CapsuleCollider>().isTrigger = true;
            //GetComponent<NPCController>().HIT_COL.GetComponent<SphereCollider>().isTrigger = true;
            teleport = 1.5f;

        }
        if(teleport == 1.5)
        {
            //FADE OUT
            if (Time.time - fadeTimer < 1f){
                Vector3 currPos = transform.position; 
                currPos.y += 0.30f; 
                GetComponent<GhostVFX>().Fade(false, 10f, 0);
                //if (!NetworkDriver.instance.HOST) { GetComponent<GhostVFX>().Fade(false, 16f, 0); }//client fade faster, prevent seeing them
                transform.position = currPos;//descend
            }
            else//NEXT STEP
            {
                this.gameObject.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
                if (NetworkDriver.instance.HOST) { teleport = 2; }
            }
        }
        //STEP 2 - RELOCATE
        if (teleport == 2)//HOST ONLY
        {
            //EVER delay SECONDS RELOCATE
            if ((Time.time > timer + delay))
            {
                relocate++;
               // GetComponent<GhostVFX>().invisibleCounter = 0;//reset counter to test new position
                transform.position = new Vector3(transform.position.x, b4Pos.y, transform.position.z);

                if(!isWaypoint)
                {
                    Vector2 randomDirection = Random.insideUnitCircle;
                    float randomDistance = Random.Range(minRadius, maxRadius);
                    Vector3 randomOffset = new Vector3(randomDirection.x, 0f, randomDirection.y) * randomDistance;//MAKE SURE SLIGHTLY ELEVATED
                    transform.position = target.position + randomOffset;
                }
                else//USE WAYPOINTS / FOR DEATH AS WELL
                {
                    // Get a random index from the list
                    int randomWP = Random.Range(0, GetComponent<NPCController>().wayPoint.Count);
                    transform.position = GetComponent<NPCController>().wayPoint[randomWP].transform.position;
                }


                timer = Time.time;
            }
            //--REAPPEAR--
            if(GetComponent<GhostVFX>().invisible && !GetComponent<GhostVFX>().visible && relocate>2 && !DEATH)
            {
                teleport = 3;
                if (NetworkDriver.instance.HOST) { NetworkDriver.instance.sioCom.Instance.Emit("teleport", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','tp':'3','x':'{transform.position.x}','y':'{transform.position.y}','z':'{transform.position.z}'}}"), false); }
            }
        }
        //SETP 3 - REAPPEAR --CLIENT
        if(teleport == 3)//runs twice
        {
            
            if (!delayForEmit)
            { //skip a frame to allow NPCcontroller to emit state 3
                teleport = 0;
                //if (!GetComponent<NPCController>().GD.ND.HOST) { transform.position = new Vector3(transform.position.x, b4Pos.y, transform.position.z); }
                if (GetComponent<NPCController>().agro  && !isWaypoint) { AudioManager.instance.Play("Reappear", gameObject.GetComponent<NPCController>().audioSource); }
                GetComponent<NavMeshAgent>().enabled = true;
                GetComponent<NPCController>().enabled = true;
                GetComponent<Animator>().enabled = true;
                this.gameObject.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = true;
                StartCoroutine(resetOutline());
                StartCoroutine(resetCanTeleport());
                AudioManager.instance.StopPlaying("haunting", null);
                //CLIENT RESPAWN
                if (!NetworkDriver.instance.HOST && GetComponent<NPCController>().healthEnemy<=0){ Respawn(); }
            }
            if (delayForEmit){ delayForEmit = false; }
        }

        IEnumerator resetCanTeleport()
        {
                yield return new WaitForSeconds(3f);
                canTeleport = true;
        }
        //--------FADE OUTLINE BACK IN 
        IEnumerator resetOutline()
        {
            //outline.enabled = true;
            while (true)
            {
                yield return new WaitForSeconds(0.2f);
                if (GameObject.Find("K2") != null) { 
                    if (outline.OutlineWidth < prevOutline) { outline.OutlineWidth += (prevOutline * 0.1f); } 
                    else { StopCoroutine(resetOutline()); } 
                }
                else { StopCoroutine(resetOutline()); }
            }
            
        }
        /*IEnumerator clientAttackingAnimationDebug()
        {
            
            yield return new WaitForSeconds(0.5f);
            GetComponent<NPCController>().attacking = false;
            yield return new WaitForSeconds(0.5f);
            GetComponent<NPCController>().attacking = true;
        }*/



    }

    public void Respawn()
    {
        DEATH = false;
        GetComponent<NPCController>().dead = false;
        GetComponent<NPCController>().healthEnemy = GetComponent<NPCController>().startHealth;
        GetComponent<NPCController>().angleView = GetComponent<NPCController>().startAngleView;
        GetComponent<NPCController>().range = GetComponent<NPCController>().startRange;
    }


}
