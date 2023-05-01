using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
	public GameObject playMenu;
	public GameObject optionsMenu;
	public GameObject levelEditorMenu;
	public void PlayButton()
	{
		playMenu.SetActive(true);
		optionsMenu.SetActive(false);
		levelEditorMenu.SetActive(false);
	}

	public void OptionsButton()
	{
		playMenu.SetActive(false);
		optionsMenu.SetActive(true);
		levelEditorMenu.SetActive(false);
	}

	public void LevelEditorButton()
	{
		playMenu.SetActive(false);
		optionsMenu.SetActive(false);
		levelEditorMenu.SetActive(true);
	}

	public void QuitButton()
	{
		Application.Quit();
	}

	public void NewGame()
	{
		SceneManager.LoadScene("Bruh");
	}
}
