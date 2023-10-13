using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameManager;
using NetworkSystem;
using Newtonsoft.Json;


public class infoCheckedToggle : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject other;
    public void toggleChecked()
    {
        if ((!NetworkDriver.instance.TWOPLAYER) || (NetworkDriver.instance.TWOPLAYER && GameDriver.instance.info_timer >= 3f))
        {
            GameDriver.instance.info_timer = 0;

            bool on = false;

            if (GetComponent<UnityEngine.UI.Image>().color == Color.yellow)
            {
                GetComponent<UnityEngine.UI.Image>().color = Color.white;
            }
            else { GetComponent<UnityEngine.UI.Image>().color = Color.yellow; on = true; }



            if (NetworkDriver.instance.TWOPLAYER) { //NETWORK SEND
                NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject(new { info = on, obj = gameObject.name }), false);
            }
        }
        
    }

   
}
