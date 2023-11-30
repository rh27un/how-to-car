using JetBrains.Rider.Unity.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.GameCenter;

[Serializable]
public class LevelObject
{
	public string id; // for objects to reference each other. for simplicity, this is the same as the object's index in objects
	public int type;
	public GameObject gameObject; // Keep a reference to the object's transform so we can keep track of changes made in the editor
	public Vector3 position;
	public Quaternion rotation;
	public int group_id; // objects can be grouped
	public string data;	// string representation of special object data not covered in levelobject. 
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
	public TrackObject takenBy;
}

public class TrackObject : LevelObject
{
	public TrackObject() { }
	public TrackObject(TrackObject old)
	{
		this.type = old.type;
		this.gameObject = old.gameObject;
		this.position = old.position;
		this.rotation = old.rotation;
		this.joints = new TrackJoint[old.joints.Length];
		for (int i = 0; i < old.joints.Length; i++)
		{
			joints[i] = new TrackJoint(old.joints[i]);
		}
	}

	public TrackObject(TrackObject old, GameObject gameObject)
	{
		this.type = old.type;
		this.gameObject = gameObject;
		this.position = gameObject.transform.position;
		this.rotation = gameObject.transform.rotation;
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
	public int car;
	//time set by the author of the map to verify it
	public float authorTime;
	//a slightly more lenient time required to achieve the bonus star
	public float bonusTime;
	//a much more lenient time required to beat the level for one star
	public float parTime;
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
	[SerializeField]
	protected SpawnablePrefabs carList;

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

	protected GameObject gamePlay;
	protected GameObject editor;

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

	public void VerifyLevel(float time)
	{
		float authorTime = levelData.authorTime;
		if(time < authorTime || authorTime == 0f)
		{
			levelData.authorTime = time;
		}
	}

	public void SetFilePathManually(string text)
	{
		text = text.Replace(" ", string.Empty);
		if (!text.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
		{
			text += ".json";
		}
		filePath = $"{directory}/{text}";
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

	public void StartGame(bool verify = false)
	{
		foreach(var go in GameObject.FindGameObjectsWithTag("LevelObject"))
		{
			Destroy(go);
		}
		gamePlay.SetActive(true);
		editor.SetActive(false);
		var level = LoadLevel();
		var objects = level.objects.ToList();
		
		if (objects != null)
		{
			foreach (var obj in objects)
			{
				var newObject = Instantiate(prefabList.Prefabs[obj.type], obj.position, obj.rotation);
				obj.gameObject = newObject;
			}
		}
		var car = GameObject.FindGameObjectWithTag("Player");
		if (car == null)
			car = Instantiate(carList.Prefabs[level.car], Vector3.zero, Quaternion.identity);
		else
		{
			car.GetComponent<Rigidbody>().velocity = Vector3.zero;
			foreach(var tr in car.GetComponentsInChildren<TrailRenderer>())
			{
				tr.Clear();
			}
		}
		var spawn = GameObject.FindGameObjectWithTag("Spawn");
		if (spawn != null)
		{
			car.transform.position = spawn.transform.position;
			car.transform.rotation = spawn.transform.rotation;
		}
		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamFollow>().Setup();
		GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().verifying = verify;
	}

	public void StartEditor()
	{
		Destroy(GameObject.FindGameObjectWithTag("Player"));
		gamePlay.SetActive(false);
		editor.SetActive(true);
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex == 0)
			return;
		gamePlay = GameObject.Find("GAMEPLAY");
		editor = GameObject.Find("LEVEL EDITOR");
		switch (gameMode)
		{
			case GameMode.Play:
				StartGame();
				break;
			case GameMode.Editor:
				StartEditor();
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
			obj.position = obj.gameObject.transform.position;
			obj.rotation = obj.gameObject.transform.rotation;
			
			if(obj is TrackObject)
			{
				TrackObject trobj = (TrackObject)obj;
				string jointsString = "JOINTS:";
				for(int i = 0; i < trobj.joints.Length; i++)
				{
					var joint = trobj.joints[i];
					if (!joint.takenBy.isPreview)
					{
						jointsString += $"{i}={objects.IndexOf(joint.takenBy)},";
					}
				}
				obj.data += jointsString + ";";
			}
		}
		if(levelData != null){
			levelData.guid = Guid.NewGuid().ToString();
			levelData.objects = objects.ToArray();
			string json = JsonUtility.ToJson(levelData, true);
			File.WriteAllText($"{filePath}", json);
		} else {
			Debug.LogError("Failed to save level");
		}
	}

	public LevelData LoadLevel()
	{
		if (File.Exists($"{filePath}"))
		{
			string json = File.ReadAllText($"{filePath}");
			try {
				levelData = JsonUtility.FromJson<LevelData>(json);
				var objects = levelData.objects.ToList();
				Debug.Log("Succesfully loaded " + filePath + " in " + gameMode.ToString() + " mode");
				return levelData;
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

	public void SetCar(int car)
	{
		if(levelData == null)
			levelData = new LevelData();
		levelData.car = car;
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
