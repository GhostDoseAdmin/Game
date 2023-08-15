using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using InteractionSystem;

//[ExecuteInEditMode]
public class laserGrid : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isClient;
    private AudioSource audioSource;


    public List<NPCController> enemyEmitList = new List<NPCController>();


    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        Result();
    }

    private void OnEnable()
    {
        StartCoroutine(DestroyAfterDelay());
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


        foreach (NPCController enemy in enemyEmitList)
        {

            // Calculate the distance between the shooter and the target
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            // Calculate the normalized distance between 0 and 1
            float normalizedDistance = Mathf.Clamp01(distance / 10);

            // Calculate the damage based on the normalized distance
            float calculatedDamage = Mathf.Lerp(50, 150, normalizedDistance);

            enemy.TakeDamage((int)calculatedDamage, false);
        }

        enemyEmitList.Clear();
        this.gameObject.SetActive(false);
    }

}
