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
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;


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
    public GameObject myRig;
    public GameObject otherPlayerProp;
    public GameObject otherPlayerRig;

    private bool hasRetrievedSkins = false;
    private static utilities util;

    public GameObject SkinsList;
    public GameObject skin;

    public int[] leveldata;
    public string currentRigName, otherPlayerRigName;

  
    // Start is called before the first frame update
    void Start()
    {
        leveldata = new int[5];//NUMBER OF LEVELS, index 0 not used
        util = new utilities();
        
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
           //create initial rigs
            UpdatePlayerRig(wesBasicRig.name, false, false);
            UpdatePlayerRig(travBasicRig.name, true, false);
        }
    }
 
    public void UpdatePlayerRig(string rigName, bool isTravis, bool otherPlayer)
    {
        Debug.Log("-----------------UPDATING RIG with name" + rigName);

        GameObject playerProp;
        GameObject currentRig;


        if (isTravis) { playerProp = travisProp; currentRig = travCurrentRig; } else { playerProp = westinProp; currentRig = wesCurrentRig; }
        if (otherPlayer) { playerProp = otherPlayerProp; currentRig = otherPlayerRig; }
        if (currentRig) { DestroyImmediate(currentRig); }

        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Rigs");
        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i].name == rigName)
            {
                currentRig = Instantiate(prefabs[i], playerProp.transform);
                break;
            }
        }
        
        currentRig.transform.SetParent(playerProp.transform);

        //Debug.Log("-----------------PROP NAME" + playerProp.name);
        //if (util != null)
        {
            GameObject k2 = util.FindChildObject(playerProp.transform, "K2");
            if (k2 != null) { k2.SetActive(false); }
            GameObject ouija = util.FindChildObject(playerProp.transform, "Ouija");
            if (ouija != null) { ouija.SetActive(false); }
            GameObject sb7 = util.FindChildObject(playerProp.transform, "SB7");
            if (sb7 != null) { sb7.SetActive(false); }

            StartCoroutine(util.ReactivateAnimator(playerProp));
        }

        

       

        if (!otherPlayer) { 
            currentRigName = rigName;
            if (isTravis) { travCurrentRig = currentRig; } 
            else { wesCurrentRig = currentRig; }
        } 
        else { otherPlayerRigName = rigName; otherPlayerRig = currentRig; }

        if (SceneManager.GetActiveScene().name == "Lobby") { GameObject.Find("LobbyManager").GetComponent<LobbyControlV2>().skinName.GetComponent<TextMeshPro>().text = rigName; }

    }



    public void RetreiveLevelSpeeds()
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
        Debug.Log("UPDATING SKINS LIST ----------------------------------------------------------- " + leveldata[1] + " " + leveldata[2]);
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
            if (i == 1 &&  leveldata[1] > 0) {
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
            if (i == 2 &&  leveldata[2] > 0)
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

    public bool UnlockSkins(GameObject unlockedSkinsPanel, float prevSpeed, float speed)
    {
        List<GameObject> updatedList = new List<GameObject>();


        int levelIndex = NetworkDriver.instance.LEVELINDEX;

            //LEVEL1
            if (levelIndex==1)
            {
                //CHECK TEIRS
                for (int j = 0; j < lvl1SpeedTeirs.Length; j++)
                {
                    if (speed <= lvl1SpeedTeirs[j] &&  prevSpeed > lvl1SpeedTeirs[j])
                    {
                    updatedList.Add(travLevel1RewardRigs[j]);
                    updatedList.Add(wesLevel1RewardRigs[j]);
                    }
                }
            }
            //LEVEL2
            if (levelIndex == 2)
            {
                //CHECK TEIRS
                for (int j = 0; j < lvl2SpeedTeirs.Length; j++)
                {
                    if (speed <= lvl2SpeedTeirs[j] && prevSpeed > lvl2SpeedTeirs[j])
                {
                        updatedList.Add(travLevel2RewardRigs[j]);
                        updatedList.Add(wesLevel2RewardRigs[j]);
                    }
                }
            }

        //CREATE NEW SKINS FOR CANVAS
        foreach (GameObject rig in updatedList)
        {
            //Debug.Log(rig.name);
            if (skin != null)
            {
                GameObject placeHolderSkin = Instantiate(skin, unlockedSkinsPanel.transform);//create skin thumbnail
                placeHolderSkin.transform.GetChild(0).GetComponent<Skin>().rig = rig;//REFERS TO THUMBNAIL
            }
        }

        if (updatedList.Count > 0) { return true; } else { return false; }
    }

}
