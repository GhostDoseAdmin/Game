using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    private NetworkDriver ND;
    public float xMin = -5;
    public float xMax = 5;
    public float zMin = 22-5;
    public float zMax = 22+5;


    void Start()
    {
        // Call SpawnPrefab() method every 5 seconds
        InvokeRepeating("SpawnPrefab", 0f, 10f);
        ND = GameObject.Find("NetworkDriver").GetComponent<NetworkDriver>();
    }

    void SpawnPrefab()
    {
        if (ND.HOST)
        {
            // Calculate random x and z values within the specified range
            float x = Random.Range(xMin, xMax);
            float z = Random.Range(zMin, zMax);

            // Instantiate the prefab at the calculated position and rotation
            string objPref = "prefab";
            Vector3 spawnPos = new Vector3(x, 0f, z);
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            enemy.name = enemy.name + (Time.time).ToString();//Time assigns unique name

            string dict = $"{{'object':'{objPref}','name':'{enemy.name}',x:{spawnPos.x.ToString("F2")},y:{spawnPos.y.ToString("F2")},z:{spawnPos.z.ToString("F2")}}}";
            ND.sioCom.Instance.Emit("create", JsonConvert.SerializeObject(dict), false);
        }
    }
}