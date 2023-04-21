
using UnityEngine;



public class K2Wave : MonoBehaviour
{
    public Vector3 startPoint;
    private Vector3 direction;
    public bool isClient = false;
    private float xoffset;
    // Start is called before the first frame update
    void Start()
    {


        GetComponent<Shockwave>().NewShockwave(startPoint, 2);//3
        if (!isClient) { direction = (GameObject.Find("Player").GetComponent<ShootingSystem>().targetLook.transform.position - GameObject.Find("PlayerCamera").transform.position).normalized; }
        else { direction = (GameObject.Find("Client").GetComponent<ClientPlayerController>().targetPos.transform.position - GameObject.Find("Client").transform.position).normalized; }

        startPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        xoffset += 0.002f;//0.01
        //GetComponent<MeshRenderer>().materials[0].SetTextureOffset("_MainTex", new Vector2(0f, xoffset));

        //---MOVEMENT
         transform.position += direction * 8f * Time.deltaTime;

        // Set the rotation of the object to the new rotation
        transform.rotation = Quaternion.LookRotation(direction, Vector3.Cross(direction, Vector3.up)) * Quaternion.Euler(90f, 0f, 0f);

        //----------SHRINK AND 0.025DESTROY when Small enough
        float distance = Vector3.Distance(transform.position, startPoint);
        if (distance > 20) { transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 0.001f, Time.deltaTime * 1); }
       // else { if (transform.localScale.x < 2) { transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 10f, Time.deltaTime * 1); } }
        if (transform.localScale.x <= 0.01f)
        {
            Destroy(gameObject); // destroy the object
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Vector3 closestPoint = other.ClosestPointOnBounds(transform.position);
        //Debug.Log("------------------------------------------COLLIDING");
        if (!other.gameObject.isStatic) { GetComponent<Shockwave>().NewShockwave(closestPoint, 2); }//2

        if (other.gameObject.transform.root.tag == "Ghost" || other.gameObject.transform.root.tag == "Shadower")
        {

            if (other.gameObject.transform.root.GetComponent<Teleport>().teleport==0) { other.gameObject.transform.root.GetComponent<NPCController>().activateOutline = true; }
            
        }
    }
}
