using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class infoCheckedToggle : MonoBehaviour
{
    // Start is called before the first frame update
    public void toggleChecked()
    {
        if (GetComponent<UnityEngine.UI.Image>().color == Color.yellow)
        {
            GetComponent<UnityEngine.UI.Image>().color = Color.white;
        }
        else { GetComponent<UnityEngine.UI.Image>().color = Color.yellow; }
        
    }
}
