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
	public string playerName;
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

	// Start is called before the first frame update
	void Start()
	{
		var levels = Directory.EnumerateFiles(directory, "*.json").ToArray();
		
		/*
		var button = Instantiate(buttonPrefab, content.transform.position, content.rotation, content);
			button.GetComponentInChildren<TMP_Text>().text = prefabList.Prefabs[i].name;
			button.GetComponent<Button>().onClick.AddListener(delegate { SetType(type); });
		}
		*/
		if(gameObject.tag == "Serializer")
		{
			DontDestroyOnLoad(gameObject);
			SceneManager.sceneLoaded += OnSceneLoaded;
		}
	}

	public string[] GetLevels(){
		return levels;
	}

	public LevelData GetLevel(string levelPath){
		if (File.Exists(levelPath))
		{
			string json = File.ReadAllText(levelPath);
			try {
				levelData = JsonUtility.FromJson<LevelData>(json);
				var objects = levelData.objects.ToList();
				Debug.Log("Succesfully loaded " + levelPath + " in " + gameMode.ToString() + " mode");
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
		if (scene.buildIndex != 1)
		{// main scene
			Destroy(gameObject);
			return;
		}
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
