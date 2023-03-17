using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class ReadInput : MonoBehaviour
{
    public TMP_InputField inputField;

    private void Update()
    {
        Debug.Log("Input field text: " + inputField.text);
    }
}
