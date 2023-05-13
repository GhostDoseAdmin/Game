using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using NetworkSystem;
using GameManager;
using TMPro;


[System.Serializable]
public class RigManager : MonoBehaviour
{
    public List<GameObject> travRigList;
    public List<GameObject> wesRigList;
     
    public GameObject travBasicRig;
    public GameObject wesBasicRig;

    public int[] lvl1SpeedTeirs;
    public int[] lvl2SpeedTeirs;
    public List<GameObject> travLevel1RewardRigs;
    public List<GameObject> travLevel2RewardRigs;
    public List<GameObject> wesLevel1RewardRigs;
    public List<GameObject> wesLevel2RewardRigs;

    public GameObject travisProp;//PLAYER GAME OBJECT
    public GameObject travCurrentRig;
    public GameObject westinProp;
    public GameObject wesCurrentRig;
    [SerializeField] public int travCurrRig = 0; // INDEX of rig array 
    [SerializeField] public int wesCurrRig = 0;
    [SerializeField] public int travRigCap;
    [SerializeField] public int wesRigCap;
    private bool hasRetrievedSkins = false;
    private static utilities util;

    public GameObject SkinsList;
    public GameObject skin;

    public int[] leveldata;
    public string currentRigPath;

   
    // Start is called before the first frame update
    void Start()
    {
        leveldata = new int[5];//NUMBER OF LEVELS, index 0 not used
        util = new utilities();

        //UpdatePlayerRig(null, travBasicRig, true, false);
        //UpdatePlayerRig(null, wesBasicRig, false, false);
    }
    public void Update()
    {
        //----------------------------------OTHER USERNAME--------------------------------
        //NetworkDriver.instance.otherUSERNAME = "DEEZ NUTS";
        //if (NetworkDriver.instance.otherUSERNAME.Length > 0 && Client != null && GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), Client.GetComponentInChildren<SkinnedMeshRenderer>(false).bounds))
        {
            //otherUserName.GetComponent<TextMeshProUGUI>().text = NetworkDriver.instance.otherUSERNAME;
            // Update the name tag position based on the player's position
            //Vector3 worldPosition = new Vector3(Client.transform.position.x, Client.transform.position.y + 1.5f, Client.transform.position.z);
            //Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            //otherUserName.GetComponent<RectTransform>().position = screenPosition;
        }
    }
 
    public void UpdatePlayerRig(string rigPath, GameObject rig, bool isTravis, bool otherPlayer)
    {
        GameObject playerProp;
        GameObject currentRig;

        if (isTravis) { playerProp = travisProp; currentRig = travCurrentRig; } else { playerProp = westinProp; currentRig = wesCurrentRig; }
        DestroyImmediate(currentRig); 

        currentRig = Instantiate(rig, playerProp.transform);
        currentRig.transform.SetParent(playerProp.transform);
        playerProp.GetComponentInChildren<K2>().gameObject.SetActive(false);
        StartCoroutine(util.ReactivateAnimator(playerProp));

        //update path for emits and game creation
        if (rigPath != null) { currentRigPath = rigPath; }

        if (!otherPlayer) { 
            if (isTravis) { travCurrentRig = currentRig; } else { wesCurrentRig = currentRig; }
            GetComponent<LobbyControlV2>().skinName.GetComponent<TextMeshPro>().text = rig.name;
        }

       
    }

    public void RetreiveSkins()
    {
        if(!hasRetrievedSkins) { RetreiveLevelData("level1speed"); RetreiveLevelData("level2speed"); }
        hasRetrievedSkins = true;
    }

    void RetreiveLevelData(string data)    {NetworkDriver.instance.sioCom.Instance.Emit("get_level_speed", JsonConvert.SerializeObject(new { username = NetworkDriver.instance.USERNAME, level = data }), false); Debug.Log("-------REQUEST DATA-----"); }

    //void RetreiveLevel2Data() { NetworkDriver.instance.sioCom.Instance.Emit("get_level2_speed", JsonConvert.SerializeObject(new { username = GameDriver.instance.USERNAME }), false); }
    public void ReceivedLevelData(int level, int speed) { leveldata[level] = speed; UpdateSkinsList(); Debug.Log("LEVEL SPEED DATA " + leveldata[1] + " " + leveldata[1]); }
    //public void ReceivedLevel2Data(int data) { level1data = data; UpdateSkinsList(); }//LAST CALL

    public void UpdateSkinsList()
    {
        //TEST
        //leveldata = new int[3];
        //leveldata[1] = 200;
        //leveldata[2] = 50;

        List<GameObject> thisRewardsList;
        List<GameObject> updatedList = new List<GameObject>();

        //ADD BASIC RIGS
        if (NetworkDriver.instance.isTRAVIS) { updatedList.Add(travBasicRig); }
        else { updatedList.Add(wesBasicRig); }


        for (int i = 1; i<leveldata.Length; i++)
        {
            //LEVEL1
            if (i == 1) {
                if (NetworkDriver.instance.isTRAVIS) { thisRewardsList = travLevel1RewardRigs; } 
                else { thisRewardsList = wesLevel1RewardRigs; }

                //CHECK TEIRS
                for (int j = 0; j < lvl1SpeedTeirs.Length; j++)
                {
                    if (leveldata[i] <= lvl1SpeedTeirs[j])
                    {
                        updatedList.Add(thisRewardsList[j]);

                    }
                }
            }
            //LEVEL2
            if (i == 2)
            {
                if (NetworkDriver.instance.isTRAVIS) {  thisRewardsList = travLevel2RewardRigs; }
                else { thisRewardsList = wesLevel2RewardRigs; }

                //CHECK TEIRS
                for (int j = 0; j < lvl2SpeedTeirs.Length; j++)
                {
                    if (leveldata[i] <= lvl2SpeedTeirs[j])
                    {
                        updatedList.Add(thisRewardsList[j]);

                    }
                }
            }
        }

        //UPDATE LIST
        if (NetworkDriver.instance.isTRAVIS) { travRigList = updatedList; }
        else { wesRigList = updatedList; }


        //DELETE SKINS
        foreach (Transform child in SkinsList.transform)
        {
            Destroy(child.gameObject);
        }
        //CREATE NEW SKINS FOR CANVAS
        foreach (GameObject rig in updatedList)
        {
            //Debug.Log(rig.name);
            if (skin != null)
            {
                GameObject placeHolderSkin = Instantiate(skin, SkinsList.transform);//create skin thumbnail
                placeHolderSkin.transform.GetChild(0).GetComponent<Skin>().rig = rig;//REFERS TO THUMBNAIL
            }
        }

    }


}
