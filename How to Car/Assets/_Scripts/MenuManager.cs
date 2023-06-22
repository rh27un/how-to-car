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
	public GameObject newProfileMenu;
	public GameObject levelSelectMenu;
	protected TMP_InputField filePath;
	[SerializeField]
	protected TMP_InputField profileName;
	public Serializer serializer;
	public GameObject levelButton;
	public GameObject profileButton;
	public Transform levelContent;
	public Transform profileContent;
	public Button newProfileButton;
	public TMP_Text profileNameError;
	private string[] levels;
	private string[] profiles;

	public void UpdateNewProfileButton(string name)
	{
		if (!string.IsNullOrWhiteSpace(name))
		{
			for(int i = 0; i < profiles.Length; i++)
			{
				string playerName = serializer.GetProfile(i).playerName;
				if(string.Equals(name, playerName, System.StringComparison.OrdinalIgnoreCase))
				{
					newProfileButton.interactable = false;
					profileNameError.text = $"Profile {name} already exists!";
					return;
				}
			}
			profileNameError.text = string.Empty;
			newProfileButton.interactable = true;
		}
		else
		{
			profileNameError.text = $"Profile name required";
			newProfileButton.interactable = false;
		}
	}
	private void Start() {
		serializer = GameObject.FindGameObjectWithTag("Serializer").GetComponent<Serializer>();
		levels = serializer.GetLevels();
		profiles = serializer.GetProfiles();
		/*
		var button = Instantiate(buttonPrefab, content.transform.position, content.rotation, content);
			button.GetComponentInChildren<TMP_Text>().text = prefabList.Prefabs[i].name;
			button.GetComponent<Button>().onClick.AddListener(delegate { SetType(type); });
		}
		*/
		
		PopulateProfilesList();
	}
	protected void PopulateLevelsList()
	{
		for (int i = 0; i < levelContent.childCount; i++)
		{
			Destroy(levelContent.GetChild(i).gameObject);
		}
		float contentHeight = 0;
		for (int i = 0; i < levels.Length; i++)
		{
			var level = serializer.GetLevel(i);
			if (level != null)
			{
				int levelIndex = new int();
				levelIndex = i;
				var button = Instantiate(levelButton, levelContent);
				contentHeight += button.GetComponent<RectTransform>().rect.height;
				button.GetComponentInChildren<TMP_Text>().text = serializer.GetLevel(i).prettyName;
				button.GetComponent<Button>().onClick.AddListener(delegate
				{ LoadGame(levelIndex); });
			}
		}
		levelContent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
	}
	protected void PopulateProfilesList()
	{
		for (int i = 0; i < profileContent.childCount; i++)
		{
			Destroy(profileContent.GetChild(i).gameObject);
		}
		float contentHeight = 0f;
		for (int i = 0; i < profiles.Length; i++)
		{
			ClearData profile = serializer.GetProfile(i);
			if (profile != null)
			{
				int profileIndex = new int();
				profileIndex = i;
				var button = Instantiate(profileButton, profileContent);
				contentHeight += button.GetComponent<RectTransform>().rect.height;
				button.GetComponentInChildren<TMP_Text>().text = profile.playerName;
				button.GetComponent<Button>().onClick.AddListener(delegate
				{ SelectProfile(profileIndex); });
			}
		}
		profileContent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
	}
	public void SelectProfile(int index)
	{
		serializer.SelectProfile(index);
		levelSelectMenu.SetActive(true);
		PopulateLevelsList();
	}

	public void OpenNewProfileMenu()
	{
		newProfileMenu.SetActive(true);
	}

	public void CloseNewProfileMenu()
	{
		newProfileMenu.SetActive(false);
	}

	public void CreateNewProfile()
	{
		string name = profileName.text;
		if (!string.IsNullOrWhiteSpace(name))
		{
			for (int i = 0; i < profiles.Length; i++)
			{
				string playerName = serializer.GetProfile(i).playerName;
				if (string.Equals(name, playerName, System.StringComparison.OrdinalIgnoreCase))
				{
					Debug.LogError($"Profile {name} already exists!");
					return;
				}
			}
		}
		else
		{
			Debug.LogError("Profile name required");
			return;
		}
		serializer.CreateNewProfile(profileName.text);
		CloseNewProfileMenu();
		PopulateProfilesList();
		levelSelectMenu.SetActive(true);
	}

	public void PlayButton()
	{
		playMenu.SetActive(true);
		optionsMenu.SetActive(false);
		levelEditorMenu.SetActive(false);
		levelSelectMenu.SetActive(false);
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
		serializer.SetFilePathManually(filePath.text);
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
		serializer.SetFilePathManually(filePath.text);
		serializer.gameMode = GameMode.Editor;
		SceneManager.LoadScene("Bruh");
	}
}
