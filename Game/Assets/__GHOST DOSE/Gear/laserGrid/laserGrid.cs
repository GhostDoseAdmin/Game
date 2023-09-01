using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using InteractionSystem;
using Newtonsoft.Json;
using NetworkSystem;

//[ExecuteInEditMode]
public class laserGrid : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isClient;
    private AudioSource audioSource;
    public GameObject laserGridProj, flash;
    private bool OTHERPLAYER = false;

    public List<NPCController> enemyEmitList = new List<NPCController>();

    public void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f;
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        Result();
    }

    public void Shoot(bool otherPlayer)
    {
        OTHERPLAYER = otherPlayer;

        AudioManager.instance.Play("GridShoot", audioSource);
        StartCoroutine(DestroyAfterDelay());
        enemyEmitList.Clear();
        GetComponent<Light>().enabled = true;
        flash.SetActive(true);
        //--------------LASERGRID PROJ----------------- laserGrid->ZozoLaser->Hovl_Laser
        GameObject laserGridProjObj = Instantiate(laserGridProj);
        ZozoLaser[] lasers = laserGridProjObj.GetComponentsInChildren<ZozoLaser>();
        foreach (ZozoLaser laserorigin in lasers)
        {
            laserorigin.laserGridOrigin = this.GetComponent<laserGrid>();
        }
        //laserGridProjObj.transform.parent = transform;
        laserGridProjObj.transform.position = transform.position;
        laserGridProjObj.transform.rotation = transform.rotation;
        //newFlash.name = "CamFlashPlayer";
        //---POINT FLASH IN DIRECTION OF THE SHOT
        //Quaternion newYRotation = Quaternion.Euler(0f, shootPoint.rotation.eulerAngles.y, 0f);
        //newFlash.transform.rotation = newYRotation;


    }
    // Function to add an object to the list if it doesn't exist already
    public void AddEnemyToEmitList(NPCController objectToAdd)
    {
        // Check if the object is already in the list
        if (!enemyEmitList.Contains(objectToAdd))
        {
            enemyEmitList.Add(objectToAdd);
        }
    }

    public void Result()
    {
        GetComponent<Light>().enabled = false;
        
        if (!OTHERPLAYER)
        {
            Dictionary<string, Dictionary<string, string>> dmgObjs = new Dictionary<string, Dictionary<string, string>>();

            //ADD TARGET FROM CROSSHAIR
            NPCController crosshairTarget = null;
            if (GetComponentInParent<ShootingSystem>().target != null && GetComponentInParent<ShootingSystem>().target.GetComponent<NPCController>()!=null) { crosshairTarget = GetComponentInParent<ShootingSystem>().target.GetComponent<NPCController>(); }
            if(crosshairTarget != null) { AddEnemyToEmitList(crosshairTarget); }

            foreach (NPCController enemy in enemyEmitList)
            {

                // Calculate the distance between the shooter and the target
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                // Calculate the normalized distance between 0 and 1
                float normalizedDistance = Mathf.Clamp01(distance / 10);

                // Calculate the damage based on the normalized distance
                float calculatedDamage = Mathf.Lerp(200, 99, normalizedDistance);

                enemy.TakeDamage((int)calculatedDamage, false);
                Debug.Log("------------------------ LASERGRID " + enemy.gameObject.name + " DAMAGE " + (int)calculatedDamage);

                //Network
                Dictionary<string, string> dmgDict = new Dictionary<string, string>();
                dmgDict.Add("dmg", ((int)calculatedDamage).ToString());
                dmgObjs.Add(enemy.gameObject.name, dmgDict);

            }

            NetworkDriver.instance.sioCom.Instance.Emit("laser_grid", JsonConvert.SerializeObject(dmgObjs), false);

        }


       
        enemyEmitList.Clear();
        //this.gameObject.SetActive(false);
    }

}
