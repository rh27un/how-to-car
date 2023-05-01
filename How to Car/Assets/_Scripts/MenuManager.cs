using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
	public GameObject playMenu;
	public GameObject optionsMenu;
	public GameObject levelEditorMenu;
	protected TMP_InputField filePath;
	public Serializer serializer;
	public void PlayButton()
	{
		playMenu.SetActive(true);
		optionsMenu.SetActive(false);
		levelEditorMenu.SetActive(false);
		filePath = playMenu.GetComponentInChildren<TMP_InputField>();
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
		filePath = levelEditorMenu.GetComponentInChildren<TMP_InputField>();
	}

	public void QuitButton()
	{
		Application.Quit();
	}

	public void NewGame()
	{
		serializer.filePath = string.Empty;
		serializer.gameMode = GameMode.Play;
		SceneManager.LoadScene("Bruh");
	}

	public void NewLevel()
	{
		serializer.filePath = string.Empty;
		serializer.gameMode = GameMode.Editor;
		SceneManager.LoadScene("Bruh");
	}

	public void LoadGame()
	{
		serializer.filePath = filePath.text;
		serializer.gameMode = GameMode.Play;
		SceneManager.LoadScene("Bruh");
	}
	public void LoadEditor()
	{
		serializer.filePath = filePath.text;
		serializer.gameMode = GameMode.Editor;
		SceneManager.LoadScene("Bruh");
	}
}
