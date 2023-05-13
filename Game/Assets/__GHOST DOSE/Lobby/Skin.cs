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


    void Start()
    {
        Texture2D texture = GetPrefabTexture();
        // Create a new sprite from the image asset
       Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        // Get reference to the image component on the object
        Image imageComponent = this.gameObject.GetComponent<Image>();
        // Set the image component's sprite to the loaded sprite
        imageComponent.sprite = sprite;
       Button btn = transform.parent.GetComponent<Button>();
       btn.onClick.AddListener(SelectSkin);
    }

    void SelectSkin()
    {
        // Find the game object in the room
        GameObject.Find("LobbyManager").GetComponent<RigManager>().UpdatePlayerRig(rig.name, rig, NetworkDriver.instance.isTRAVIS, false) ;

    }

    public Texture2D GetPrefabTexture()
    {

        // Load the prefab from the Resources folder
        //
        // Load all prefabs in the "Prefabs/Rigs" folder
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Rigs");

        // Find the prefab with the matching name and instantiate it
        string prefabName = rig.name; // Replace with the name of the prefab you want to instantiate
        GameObject instance = null;
        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i].name == prefabName)
            {
                 instance = Instantiate(prefabs[i]);
                // Do something with the instantiated game object
                break; // Exit the loop once the prefab is found
            }
        }
        // Set the position and rotation of the camera to capture the prefab
        GameObject snapShotCamObj = new GameObject("SnapShot");
        Camera camera = snapShotCamObj.AddComponent<Camera>();


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

        return texture;// texture;
    }







}
