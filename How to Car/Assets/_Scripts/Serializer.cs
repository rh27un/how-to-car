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
	// Start is called before the first frame update
	void Start()
	{
		if(gameObject.tag == "Serializer")
		{
			DontDestroyOnLoad(gameObject);
			SceneManager.sceneLoaded += OnSceneLoaded;
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
		string json = JsonArray.ToJson(objects.ToArray(), true);
		File.WriteAllText(filePath, json);
	}

	public List<LevelObject> LoadLevel()
	{
		if (File.Exists(filePath))
		{
			string json = File.ReadAllText(filePath);
			LevelObject[] importedObjects = JsonArray.FromJson<LevelObject>(json);
			var objects = importedObjects.ToList();
			Debug.Log("Succesfully loaded " + filePath + " in " + gameMode.ToString() + " mode");
			return objects;
		}
		else
		{
			Debug.LogWarning("Cannot load " + filePath);
			return null;
		}
	}
}
