
using UnityEngine;



public class K2Wave : MonoBehaviour
{
    public Vector3 startPoint;
    private Vector3 direction;
    public bool isClient = false;
    // Start is called before the first frame update
    void Start()
    {

        if (!isClient) { direction = (GameObject.Find("Player").GetComponent<ShootingSystem>().targetLook.transform.position - GameObject.Find("PlayerCamera").transform.position).normalized; }
        else { direction = (GameObject.Find("Client").GetComponent<ClientPlayerController>().targetPos.transform.position - GameObject.Find("Client").transform.position).normalized; }

        startPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //---MOVEMENT
       // transform.position += direction * 10f * Time.deltaTime;
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
       // transform.rotation = lookRotation;

        //----------SHRINK AND 0.025DESTROY when Small enough
        float distance = Vector3.Distance(transform.position, startPoint);
        if (distance > 20) { transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 0.001f, Time.deltaTime * 1); }
        if (transform.localScale.x <= 0.01f)
        {
            Destroy(gameObject); // destroy the object
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.root.tag == "Ghost" || other.gameObject.transform.root.tag == "Shadower")
        {

            //other.gameObject.transform.root.GetChild(0).GetComponent<Outline>().OutlineWidth += 1 ;
            other.gameObject.transform.root.GetComponent<NPCController>().activateOutline = true;
        }
    }
}
