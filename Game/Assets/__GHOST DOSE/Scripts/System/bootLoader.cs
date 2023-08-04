using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class bootLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("bootloader",0.01f);
    }

    private void bootloader()
    {

        SceneManager.LoadScene("Lobby");
    }

}
