using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadScene : MonoBehaviour
{

	[Header("Buttons")]
	public KeyCode reloadButton;

	void Update()
	{
		if (Input.GetKeyDown(reloadButton))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
}
