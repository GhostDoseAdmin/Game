using UnityEngine;

//An example script that allows the user to click on an object that has a Shockwave component attached to trigger a new shockwave.
public class ClickExample : MonoBehaviour
{
	void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ViewportPointToRay(Camera.main.ScreenToViewportPoint(Input.mousePosition));

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000))
            {
                Debug.DrawRay(hit.point, hit.normal * 5, Color.cyan, 5, true);

                Shockwave shockwave = hit.transform.root.GetComponent<Shockwave>();

                if(shockwave != null)
                {
                    shockwave.NewShockwave(hit.point, 3);
                }
            }
        }
    }
}
