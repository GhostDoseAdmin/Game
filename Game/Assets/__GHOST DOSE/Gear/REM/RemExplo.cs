using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkSystem;

public class RemExplo : MonoBehaviour
{
    public List<NPCController> enemyEmitList = new List<NPCController>();

    // Start is called before the first frame update
    public void AddEnemyToEmitList(NPCController objectToAdd)
    {
        // Check if the object is already in the list
        if (!enemyEmitList.Contains(objectToAdd))
        {
            enemyEmitList.Add(objectToAdd);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 5f, Time.deltaTime * 1);
        if (transform.localScale.x < -10f)
        {
            Result();
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("---------------------------COLLIDING EXPLO" + other.gameObject.name);
        if (other.gameObject.GetComponentInParent<NPCController>() != null)
        {
            AddEnemyToEmitList(other.GetComponentInParent<NPCController>());
        }
    }

    public void Result()
    {

       // if (!OTHERPLAYER)
        {
            Dictionary<string, Dictionary<string, string>> dmgObjs = new Dictionary<string, Dictionary<string, string>>();

            foreach (NPCController enemy in enemyEmitList)
            {

                Debug.Log("------------------------ REMPOD DAMAGE " + enemy.gameObject.name);
                enemy.TakeDamage(200, false);
                //Network
                Dictionary<string, string> dmgDict = new Dictionary<string, string>();
                dmgDict.Add("dmg", "200");
                dmgObjs.Add(enemy.gameObject.name, dmgDict);

            }

            NetworkDriver.instance.sioCom.Instance.Emit("rem_pod", JsonConvert.SerializeObject(dmgObjs), false);

        }
    }

}
