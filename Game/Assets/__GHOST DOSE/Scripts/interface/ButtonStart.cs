using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class ButtonStart : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        string room_text = transform.parent.Find("InputField (TMP)").GetComponent<TMP_InputField>().text;
         
        if (room_text.Length > 1)
        {
            PlayerPrefs.SetString("room", room_text);
            SceneManager.LoadScene(sceneName);
        }
    }
}
