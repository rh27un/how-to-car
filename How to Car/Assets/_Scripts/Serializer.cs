using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LevelObject
{
	public int type;
	[NonSerialized]
	public Transform transform; // Keep a reference to the object's transform so we can keep track of changes made in the editor
	public Vector3 position;
	public Quaternion rotation;
}


[Serializable]
public class ClearData
{
	public ClearData()
	{
		playerName = "Test";
		data = new List<LevelClearData>()
		{
		};
	}
	public ClearData(string name)
	{
		playerName = name;
		data = new List<LevelClearData>()
		{
		};
	}
	public string playerName;
	[SerializeField]
	protected List<LevelClearData> data;

	public void Add(LevelClearData _data)
	{
		data.Add(_data);
	}
	public LevelClearData this[string guid]
	{
		get { return data.SingleOrDefault(d => d.levelGUID == guid);}
	}
}

[Serializable]
public class LevelClearData
{
	public LevelClearData(string _levelGUID, float time, int _stars)
	{
		levelGUID = _levelGUID;
		personalBestTime = time;
		stars = _stars;
	}
	public string levelGUID;
	public float personalBestTime;
	public int stars;
}
public class TrackJoint
{
	public TrackJoint() { }
	public TrackJoint(TrackJoint old)
	{
		offset = old.offset;
		forward = old.forward;
	}
	public Vector3 offset;
	public Vector3 forward;
	public GameObject takenBy;
}

public class TrackObject : LevelObject
{
	public TrackObject() { }
	public TrackObject(TrackObject old)
	{
		this.type = old.type;
		this.transform = old.transform;
		this.position = old.position;
		this.rotation = old.rotation;
		this.joints = new TrackJoint[old.joints.Length];
		for (int i = 0; i < old.joints.Length; i++)
		{
			joints[i] = new TrackJoint(old.joints[i]);
		}
	}

	public TrackObject(TrackObject old, Transform newTransform)
	{
		this.type = old.type;
		this.transform = newTransform;
		this.position = newTransform.position;
		this.rotation = newTransform.rotation;
		this.joints = new TrackJoint[old.joints.Length];
		for (int i = 0; i < old.joints.Length; i++)
		{
			joints[i] = new TrackJoint(old.joints[i]);
		}
	}
	public TrackJoint[] joints;

	public bool isPreview;
}



public static class JsonArray
{


	public static string ToJson<T>(T[] array, bool prettyPrint = false)
	{
		Wrapper<T> jsonArray = new Wrapper<T>(array);
		return JsonUtility.ToJson(jsonArray, prettyPrint);
	}

	public static T[] FromJson<T>(string json)
	{
		Wrapper<T> jsonArray = JsonUtility.FromJson<Wrapper<T>>(json);
		return jsonArray.items;
	}

	[Serializable]
	private class Wrapper<T>
	{
		public T[] items;
		public Wrapper(T[] items)
		{
			this.items = items;
		}
	}
}

[Serializable]
public class LevelData{
	public string guid;
	public string prettyName;
	public string description;
	public LevelObject[] objects;
}

public enum GameMode
{
	Play = 0,
	Editor = 1
}
public class Serializer : MonoBehaviour
{


	[SerializeField]
	protected SpawnablePrefabs prefabList;

	public string filePath;
	public GameMode gameMode;

	protected LevelData levelData;

	public string directory;

	protected string[] levels;

	protected string saveDataPath;
	[SerializeField]
	protected string profileDirectory;
	protected string[] profiles;

	protected ClearData selectedProfile;

	// Start is called before the first frame update
	void Awake() 
	{
		if(gameObject.tag == "Serializer")
		{
			if (GameObject.FindGameObjectsWithTag("Serializer").Length > 1)
			{
				Destroy(gameObject);
				return;
			}
			DontDestroyOnLoad(gameObject);
			SceneManager.sceneLoaded += OnSceneLoaded;
		}
		levels = Directory.EnumerateFiles(directory, "*.json").ToArray();
		profiles = Directory.EnumerateFiles(profileDirectory, "*.json").ToArray();

	}

	public string[] GetLevels(){
		return levels;
	}

	public string[] GetProfiles()
	{
		return profiles;
	}

	public ClearData GetProfile(int index)
	{
		var profilePath = profiles[index];
		if (File.Exists(profilePath))
		{
			string json = File.ReadAllText(profilePath);
			try
			{
				var profile = JsonUtility.FromJson<ClearData>(json);
				return profile;
			}
			catch (Exception x)
			{
				Debug.LogError("Error loading file " + profilePath + ", might be outdated");
				return null;
			}
		}
		else
		{
			Debug.LogWarning("The file " + profilePath + " could not be found");
			return null;
		}
	}

	public bool SelectProfile(int index)
	{
		saveDataPath = profiles[index];
		if (File.Exists(saveDataPath))
		{
			string json = File.ReadAllText(saveDataPath);
			try
			{
				selectedProfile = JsonUtility.FromJson<ClearData>(json);
				return true;
			}
			catch (Exception x)
			{
				Debug.LogError("Error loading file " + saveDataPath + ", might be outdated");
				return false;
			}
		}
		else
		{
			Debug.LogWarning("The file " + saveDataPath + " could not be found");
			return false;
		}
	}

	public bool FinishLevel(float time)
	{
		if (File.Exists(saveDataPath))
		{
			var json = File.ReadAllText(saveDataPath);
			var clearData = JsonUtility.FromJson<ClearData>(json);
			var levelClearData = clearData[levelData.guid];
			if (levelClearData != null)
			{
				if (time < levelClearData.personalBestTime)
				{
					levelClearData.personalBestTime = time;
					json = JsonUtility.ToJson(clearData);
					File.WriteAllText(saveDataPath, json);
					return true;
				}
			}
			else
			{
				clearData.Add(new LevelClearData(levelData.guid, time, 0));
				json = JsonUtility.ToJson(clearData);
				File.WriteAllText(saveDataPath, json);
			}
		}
		else
		{
			var clearData = new ClearData();
			clearData.Add(new LevelClearData(levelData.guid, time, 0));
			var json = JsonUtility.ToJson(clearData);
			File.WriteAllText(saveDataPath, json);
		}
		return false;
	}

	public void CreateNewProfile(string name)
	{
		string fileName = Guid.NewGuid().ToString();
		selectedProfile = new ClearData(name);
		var json = JsonUtility.ToJson(selectedProfile);
		File.WriteAllText($"{profileDirectory}/{fileName}.json", json);
		profiles = Directory.GetFiles(profileDirectory, "*.json").ToArray();
	}
	
	public void LoadLevel(int index){

	}
	public LevelData GetLevel(int index){
		var levelPath = levels[index];
		if (File.Exists(levelPath))
		{
			string json = File.ReadAllText(levelPath);
			try {
				levelData = JsonUtility.FromJson<LevelData>(json);
				
				return levelData;
			} catch(Exception x){
				Debug.LogError("Error loading file " + levelPath + ", might be outdated");
				return null;
			}
		}
		else
		{
			Debug.LogWarning("The file " + levelPath + " could not be found");
			return null;
		}
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex == 0)
			return;
		switch (gameMode)
		{
			case GameMode.Play:
				GameObject.Find("GAMEPLAY").SetActive(true);
				GameObject.Find("LEVEL EDITOR").SetActive(false);
				var objects = LoadLevel();
				if (objects != null)
				{
					foreach (var obj in objects)
					{
						var newObject = Instantiate(prefabList.Prefabs[obj.type], obj.position, obj.rotation);
						obj.transform = newObject.transform;
					}
				}
				break;
			case GameMode.Editor:
				GameObject.Find("GAMEPLAY").SetActive(false);
				GameObject.Find("LEVEL EDITOR").SetActive(true);
				break;
			default:
				Debug.LogError("Invalid Game Mode");
				return;
		}
	}

	

	public void SaveLevel(List<LevelObject> objects)
	{
		foreach (var obj in objects)
		{
			obj.position = obj.transform.position;
			obj.rotation = obj.transform.rotation;
		}
		if(levelData != null){
			levelData.guid = Guid.NewGuid().ToString();
			levelData.objects = objects.ToArray();
			string json = JsonUtility.ToJson(levelData, true);
			File.WriteAllText(filePath, json);
		} else {
			Debug.LogError("Failed to save level");
		}
	}

	public List<LevelObject> LoadLevel()
	{
		if (File.Exists(filePath))
		{
			string json = File.ReadAllText(filePath);
			try {
				levelData = JsonUtility.FromJson<LevelData>(json);
				var objects = levelData.objects.ToList();
				Debug.Log("Succesfully loaded " + filePath + " in " + gameMode.ToString() + " mode");
				return objects;
			} catch(Exception x){
				Debug.LogError("Error loading file " + filePath + ", might be outdated");
				return null;
			}
		}
		else
		{
			Debug.LogWarning("The file " + filePath + " could not be found");
			return null;
		}
	}

	public void SetLevelName(string name){
		if(levelData == null)
			levelData = new LevelData();
		levelData.prettyName = name;
	}

	public void SetLevelDescription(string description){
		if(levelData == null)
			levelData = new LevelData();
		levelData.description = description;
	}

	public string GetLevelName(){
		if(levelData != null)
		return levelData.prettyName;
		else
		return "No level data found";
	}

	public string GetLevelDescription(){
		if(levelData != null)
		return levelData.description;
		else
		return "No level data found";
	}
	
}
