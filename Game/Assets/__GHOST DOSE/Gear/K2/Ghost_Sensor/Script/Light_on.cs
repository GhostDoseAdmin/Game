using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light_on : MonoBehaviour
{
    // Start is called before the first frame update
    public int Level = 0; // Level 0 - 5 0, all lamps disable and 5 all lamps enable
    public bool random_mode = false;
    public bool start = true;

    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (random_mode == false)
        {
            Detect();
        }
        else
        {
            if (start == true)
            {
                StartCoroutine(Randomico());
            }
           
        }

       

    }


        void Detect()
        {
            switch (Level)
            {
                case 5:
                
                    gameObject.GetComponent<Renderer>().materials[1].EnableKeyword("_EMISSION");
                    gameObject.GetComponent<Renderer>().materials[2].EnableKeyword("_EMISSION");
                    gameObject.GetComponent<Renderer>().materials[3].EnableKeyword("_EMISSION");
                    gameObject.GetComponent<Renderer>().materials[4].EnableKeyword("_EMISSION");
                    gameObject.GetComponent<Renderer>().materials[5].EnableKeyword("_EMISSION");
                break;
                case 4:
                
                     gameObject.GetComponent<Renderer>().materials[2].DisableKeyword("_EMISSION");
                     gameObject.GetComponent<Renderer>().materials[1].EnableKeyword("_EMISSION");
                     gameObject.GetComponent<Renderer>().materials[3].EnableKeyword("_EMISSION");
                     gameObject.GetComponent<Renderer>().materials[4].EnableKeyword("_EMISSION");
                     gameObject.GetComponent<Renderer>().materials[5].EnableKeyword("_EMISSION");

                break;
                case 3:
               
                     gameObject.GetComponent<Renderer>().materials[5].DisableKeyword("_EMISSION");
                     gameObject.GetComponent<Renderer>().materials[2].DisableKeyword("_EMISSION");
                     gameObject.GetComponent<Renderer>().materials[1].EnableKeyword("_EMISSION");
                     gameObject.GetComponent<Renderer>().materials[5].EnableKeyword("_EMISSION");
                     gameObject.GetComponent<Renderer>().materials[3].EnableKeyword("_EMISSION");
                break;
                case 2:
                    gameObject.GetComponent<Renderer>().materials[1].EnableKeyword("_EMISSION");
                    gameObject.GetComponent<Renderer>().materials[3].EnableKeyword("_EMISSION");
                    gameObject.GetComponent<Renderer>().materials[4].DisableKeyword("_EMISSION");
                    gameObject.GetComponent<Renderer>().materials[2].DisableKeyword("_EMISSION");
                    gameObject.GetComponent<Renderer>().materials[5].DisableKeyword("_EMISSION");
                    break;
                case 1:
                     gameObject.GetComponent<Renderer>().materials[1].EnableKeyword("_EMISSION");
                     gameObject.GetComponent<Renderer>().materials[2].DisableKeyword("_EMISSION");
                     gameObject.GetComponent<Renderer>().materials[3].DisableKeyword("_EMISSION");
                     gameObject.GetComponent<Renderer>().materials[4].DisableKeyword("_EMISSION");
                     gameObject.GetComponent<Renderer>().materials[5].DisableKeyword("_EMISSION");
                    break;
                case 0:
                    gameObject.GetComponent<Renderer>().materials[1].DisableKeyword("_EMISSION");
                    gameObject.GetComponent<Renderer>().materials[2].DisableKeyword("_EMISSION");
                    gameObject.GetComponent<Renderer>().materials[3].DisableKeyword("_EMISSION");
                    gameObject.GetComponent<Renderer>().materials[4].DisableKeyword("_EMISSION");
                    gameObject.GetComponent<Renderer>().materials[5].DisableKeyword("_EMISSION");
                    break;
            default:
              
                    break;
            }
        }
    IEnumerator Randomico()
    {
        start = false;
        yield return new WaitForSeconds(0.5f);
        Level = Random.Range(1,5);
        Detect();
        yield return new WaitForSeconds(0.5f);
        start = true;

    }




}
