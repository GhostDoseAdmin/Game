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
    private GameObject travCurrentRig;
    public GameObject westinProp;
    private GameObject wesCurrentRig;
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

        UpdatePlayerRig(null, travBasicRig, true);
        UpdatePlayerRig(null, wesBasicRig, false);
    }

    // Update is called once per frame
    public void UpdatePlayerRig(string rigPath, GameObject rig, bool isTravis)
    {
        GameObject playerProp;
        GameObject currentRig;

        if (isTravis) { playerProp = travisProp; currentRig = travCurrentRig; } else { playerProp = westinProp; currentRig = wesCurrentRig; }
        DestroyImmediate(currentRig); 

        currentRig = Instantiate(rig, playerProp.transform);
        currentRig.transform.SetParent(playerProp.transform);
        playerProp.GetComponentInChildren<K2>().gameObject.SetActive(false);
        StartCoroutine(util.ReactivateAnimator(playerProp));

        currentRigPath = rigPath;

        if (isTravis) { travCurrentRig = currentRig; } else { wesCurrentRig = currentRig; }

        GetComponent<LobbyControlV2>().skinName.GetComponent<TextMeshPro>().text = rig.name;
    }

    public void RetreiveSkins()
    {
        if(!hasRetrievedSkins) { RetreiveLevelData("level1speed"); RetreiveLevelData("level2speed"); }
        hasRetrievedSkins = true;
    }

    void RetreiveLevelData(string data)    {NetworkDriver.instance.sioCom.Instance.Emit("get_level_speed", JsonConvert.SerializeObject(new { username = GameDriver.instance.USERNAME, level = data }), false);;    }

    //void RetreiveLevel2Data() { NetworkDriver.instance.sioCom.Instance.Emit("get_level2_speed", JsonConvert.SerializeObject(new { username = GameDriver.instance.USERNAME }), false); }
    public void ReceivedLevelData(int level, int speed) { leveldata[level] = speed; UpdateSkinsList(); }
    //public void ReceivedLevel2Data(int data) { level1data = data; UpdateSkinsList(); }//LAST CALL

    public void UpdateSkinsList()
    {
        Debug.Log("--------------UPDATING SKINS LIST");
        //TEST
        //leveldata = new int[3];
        //leveldata[1] = 200;
        //leveldata[2] = 50;

        List<GameObject> thisRewardsList;
        List<GameObject> updatedList = new List<GameObject>();

        //ADD BASIC RIGS
        if (GameDriver.instance.isTRAVIS) { updatedList.Add(travBasicRig); }
        else { updatedList.Add(wesBasicRig); }


        for (int i = 1; i<leveldata.Length; i++)
        {
            //LEVEL1
            if (i == 1) {
                if (GameDriver.instance.isTRAVIS) { thisRewardsList = travLevel1RewardRigs; } 
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
                if (GameDriver.instance.isTRAVIS) {  thisRewardsList = travLevel2RewardRigs; }
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
        if (GameDriver.instance.isTRAVIS) { travRigList = updatedList; }
        else { wesRigList = updatedList; }


        //DELETE SKINS
        foreach (Transform child in SkinsList.transform)
        {
            Destroy(child.gameObject);
        }
        //CREATE NEW SKINS FOR CANVAS
        foreach (GameObject rig in updatedList)
        {
            Debug.Log(rig.name);
           GameObject placeHolderSkin =  Instantiate(skin, SkinsList.transform);
            placeHolderSkin.transform.GetChild(0).GetComponent<Skin>().rig = rig;//REFERS TO THUMBNAIL
        }

    }


}
