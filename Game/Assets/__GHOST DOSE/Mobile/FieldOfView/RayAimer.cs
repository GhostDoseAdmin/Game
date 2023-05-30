using System.Net;
using UnityEngine;
using GameManager;

public class RayAimer : MonoBehaviour {

    [SerializeField] private LayerMask layerMask;
    public Mesh mesh;
    public float fov;
    private float startFov;
    public float viewDistance;
    private Vector3 origin;
    public float startingAngle;
    private bool StartAim;
    public bool isHeadshot;

    private void Start() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material.SetColor("_Color", Color.cyan);

        origin = Vector3.zero;
        startFov = fov;
        StartAim = true;
    }

    
    private void LateUpdate() {

        //transform.position = GameDriver.instance.Player.transform.position + Vector3.up;

        if (StartAim)
        {
            fov -= 10f * Time.deltaTime; //AIM TIME
            //END AIM
            if (fov < 0f){fov = startFov; StartAim = false; isHeadshot = false; GetComponent<MeshRenderer>().material.SetColor("_Color", Color.cyan); }
            //HEADSHOT
            if (fov < 10f) { isHeadshot = true; GetComponent<MeshRenderer>().material.SetColor("_Color", Color.yellow); }

            int rayCount = 50;

            float angle = startingAngle + (fov * 0.5f);
            float angleIncrease = fov / rayCount;

            Vector3[] vertices = new Vector3[rayCount + 1 + 1];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[rayCount * 3];

            vertices[0] = origin;

            int vertexIndex = 1;
            int triangleIndex = 0;
            for (int i = 0; i <= rayCount; i++)
            {
                Vector3 vertex = origin + GetVectorFromAngle(angle) * viewDistance;
                RaycastHit hit;

                Vector3 raycastOrigin = transform.TransformPoint(origin); // Transform the origin to the mesh's position
                Vector3 raycastDirection = transform.TransformDirection(GetVectorFromAngle(angle)); // Transform the direction to the mesh's orientation

                bool isHit = Physics.Raycast(raycastOrigin, raycastDirection, out hit, viewDistance, layerMask);
                Debug.DrawLine(raycastOrigin, raycastOrigin + raycastDirection * viewDistance, Color.yellow);

                if (isHit)
                {
                    // Hit object
                    Debug.Log("HIT" + hit.collider.gameObject.name);
                }

                vertices[vertexIndex] = vertex;

                if (i > 0)
                {
                    triangles[triangleIndex + 0] = 0;
                    triangles[triangleIndex + 1] = vertexIndex - 1;
                    triangles[triangleIndex + 2] = vertexIndex;

                    triangleIndex += 3;
                }

                vertexIndex++;
                angle -= angleIncrease;
            }


            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.bounds = new Bounds(origin, Vector3.one * 1000f);

        }
    }


    private static Vector3 GetVectorFromAngle(float angle)
    {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

}
