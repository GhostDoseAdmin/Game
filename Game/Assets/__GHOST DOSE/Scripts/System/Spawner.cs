using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] enemies;

    private NetworkDriver ND;

    private float spawnX;
    private float spawnY;
    public float spawnSpeed;
    void Start()
    {
        // Call SpawnPrefab() method every 5 seconds
        InvokeRepeating("SpawnPrefab", 0f, 5f);
        ND = GameObject.Find("GameController").GetComponent<GameDriver>().ND;
    }

    void SpawnPrefab()
    {
        if (ND.HOST)
        {
            //choose enemy
            int randomEnemy = 0;//Random.Range(0, enemies.Length);
            //if shadower
            if (randomEnemy==0)//shadower
            {
                spawnX = Random.Range(-4, 4);
                spawnY = Random.Range(9, 20);
            }
            if (randomEnemy==1)
            {
                spawnX = Random.Range(-2, 2);
                spawnY = Random.Range(17, 27);
            }

                Vector3 spawnPos = new Vector3(spawnX, 0f, spawnY);
                GameObject enemy = Instantiate(enemies[randomEnemy], spawnPos, Quaternion.identity);
                enemy.name = enemy.name + (Time.time).ToString();//Time assigns unique name
                Debug.LogWarning("SPAWN " + spawnPos);

                string emitCmd = $"{{'index':'{randomEnemy}','name':'{enemy.name}',x:{spawnPos.x.ToString("F2")},y:{spawnPos.y.ToString("F2")},z:{spawnPos.z.ToString("F2")}}}";
                ND.sioCom.Instance.Emit("create", JsonConvert.SerializeObject(emitCmd), false);
                
            }
    }
}