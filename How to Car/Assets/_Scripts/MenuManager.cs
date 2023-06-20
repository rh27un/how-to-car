using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
	public GameObject playMenu;
	public GameObject optionsMenu;
	public GameObject levelEditorMenu;
	protected TMP_InputField filePath;
	public Serializer serializer;
	public GameObject levelButton;

	public Transform content;

	private string[] levels;
	private void Start() {
		levels = serializer.GetLevels();
		/*
		var button = Instantiate(buttonPrefab, content.transform.position, content.rotation, content);
			button.GetComponentInChildren<TMP_Text>().text = prefabList.Prefabs[i].name;
			button.GetComponent<Button>().onClick.AddListener(delegate { SetType(type); });
		}
		*/
		float contentHeight = 0;
		for(int i = 0; i < levels.Length; i++){
			var level = serializer.GetLevel(i);
			if (level != null)
			{
				int levelIndex = new int();
				levelIndex = i;
				var button = Instantiate(levelButton, content);
				contentHeight += button.GetComponent<RectTransform>().rect.height;
				button.GetComponentInChildren<TMP_Text>().text = serializer.GetLevel(i).prettyName;
				button.GetComponent<Button>().onClick.AddListener(delegate
				{ LoadGame(levelIndex); });
			}
		}
		content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
	}
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

	public void LoadGame(int index){
		serializer.filePath = levels[index];
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
