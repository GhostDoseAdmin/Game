
using UnityEngine;



public class K2Wave : MonoBehaviour
{
    public Vector3 startPoint;
    private Vector3 direction;
    public bool isClient = false;
    public GameObject K2Source;
    public GameObject hud;
    private 
    // Start is called before the first frame update
    void Start()
    {

        hud.transform.localScale = Vector3.one*40;
            // transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 0.0001f, Time.deltaTime * 1);
        GetComponent<Shockwave>().NewShockwave(startPoint, 2);//3
        if (!isClient) { direction = (GameObject.Find("Player").GetComponent<ShootingSystem>().targetLook.transform.position - GameObject.Find("PlayerCamera").transform.position).normalized; }
        else { direction = (GameObject.Find("Client").GetComponent<ClientPlayerController>().targetPos.transform.position - GameObject.Find("Client").transform.position).normalized; }

        startPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //----------------------HUD-----------------------------
        hud.transform.localScale = Vector3.Lerp(hud.transform.localScale, hud.transform.localScale * 0.0015f, Time.deltaTime * 1);
        // Get position of object to track
        Vector3 objectPosition = gameObject.transform.position;
        // Calculate distance between object and camera
        Vector3 cameraPosition = Camera.main.transform.position;
        // Position HUD relative to object position
        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(objectPosition);
        Vector2 hudPosition = new Vector2(
            (viewportPosition.x * Screen.width) + new Vector2(-Screen.width*0.5f, -Screen.height * 0.5f).x,
            (viewportPosition.y * Screen.height) + new Vector2(-Screen.width * 0.5f, -Screen.height * 0.5f).y
        );

        hud.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        hud.GetComponent<RectTransform>().anchoredPosition = hudPosition;



        //---MOVEMENT
        transform.position += direction * 8f * Time.deltaTime;

        // Set the rotation of the object to the new rotation
        transform.rotation = Quaternion.LookRotation(direction, Vector3.Cross(direction, Vector3.up)) * Quaternion.Euler(90f, 0f, 0f);

        //----------SHRINK AND 0.025DESTROY when Small enough
        float distance = Vector3.Distance(transform.position, startPoint);
        GetComponent<MeshRenderer>().material.SetFloat("_Fade", GetComponent<MeshRenderer>().material.GetFloat("_Fade") - 0.0045f);
        if (distance > 10) {  }// transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 0.0001f, Time.deltaTime * 1); }
                               // else { if (transform.localScale.x < 2) { transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 10f, Time.deltaTime * 1); } }
                               //if (transform.localScale.x <= 0.01f)
        if (GetComponent<MeshRenderer>().material.GetFloat("_Fade") < 0)
        {
            hud.GetComponent<Animator>().enabled = false;
            Destroy(gameObject); // destroy the object
        }
        else { hud.GetComponent<Animator>().enabled = true; }
    }


    private void OnTriggerEnter(Collider other)
    {
        Vector3 closestPoint = other.ClosestPointOnBounds(transform.position);

        //Debug.Log("------------------------------------------COLLIDING" + other.name);
        if (!other.gameObject.isStatic) { GetComponent<Shockwave>().NewShockwave(closestPoint, 2); }//2

        if (other.gameObject.tag == "Ghost" || other.gameObject.tag == "Shadower")
        {

            if ((other.gameObject.GetComponentInParent<Teleport>().teleport==0)) {
                other.gameObject.GetComponentInParent<NPCController>().activateOutline = true;
            }
        }
        //COLD SPOT
        if (other.gameObject.transform.parent != null)
        {
            if (other.gameObject.transform.parent.GetComponent<ColdSpot>() != null)
            {
                other.gameObject.transform.parent.GetComponent<ColdSpot>().Exposed();
            }
        }
    }


}
