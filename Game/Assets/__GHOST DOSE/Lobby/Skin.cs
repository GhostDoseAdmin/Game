using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GameManager;
using NetworkSystem;
public class Skin : MonoBehaviour
{
    public GameObject rig;
    private string prefabpath;
    // Start is called before the first frame update
    void Start()
    {

        prefabpath = FindPrefabPath(rig.name);
        
        Texture2D texture = GetPrefabTexture(prefabpath);
        // Create a new sprite from the image asset
       //Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
       /* // Get reference to the image component on the object
        Image imageComponent = this.gameObject.GetComponent<Image>();
        // Set the image component's sprite to the loaded sprite
        imageComponent.sprite = sprite;
       Button btn = transform.parent.GetComponent<Button>();
       btn.onClick.AddListener(SelectSkin);*/
    }

    void SelectSkin()
    {
        // Find the game object in the room
        GameObject.Find("LobbyManager").GetComponent<RigManager>().UpdatePlayerRig(prefabpath, rig, NetworkDriver.instance.isTRAVIS, false) ;

    }



    public static string FindPrefabPath(string prefabName)
    {
        string[] files = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            if (Path.GetFileNameWithoutExtension(file) == prefabName)
            {
                string path = "Assets" + file.Substring(Application.dataPath.Length);
                path = path.Substring(path.IndexOf("Prefabs")).Replace('\\', '/');
                path = path.Substring(0, path.LastIndexOf("."));
                return path;


            }
        }
        return null;
    }

    public Texture2D GetPrefabTexture(string prefabpath)
    {

        // Load the prefab from the Resources folder
        //
        //GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Rigs");
        
       // GameObject instance = Instantiate(Resources.Load<GameObject>(prefab)); //prefabpath

        GameDriver.instance.WriteGuiMsg(rig.name, 999f, true, Color.yellow);
        // Set the position and rotation of the camera to capture the prefab
        // GameObject snapShotCamObj = new GameObject("SnapShot");
        /*Camera camera = snapShotCamObj.AddComponent<Camera>();


        snapShotCamObj.transform.position = instance.transform.position + new Vector3(0, 1.497f, 0.921f);
        snapShotCamObj.transform.rotation = Quaternion.Euler(5.932f, 180f, 0);

        // Create a RenderTexture to hold the screenshot
        RenderTexture renderTexture = new RenderTexture(256, 256, 24);

        // Set the camera's target texture to the RenderTexture
        camera.targetTexture = renderTexture;

        // Render the camera to the RenderTexture
        camera.Render();

        // Create a Texture2D from the RenderTexture
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // Clean up the camera and prefab instance
        camera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(snapShotCamObj);
        DestroyImmediate(instance);
        */
        return null;// texture;
    }







}
