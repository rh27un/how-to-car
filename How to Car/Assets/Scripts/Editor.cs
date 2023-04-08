using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.SearchService;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Transactions;
using TreeEditor;

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
public class LevelObject
{
	public int type;
	[NonSerialized]
	public Transform transform; // Keep a reference to the object's transform so we can keep track of changes made in the editor
	public Vector3 position;
	public Quaternion rotation;
}

public class Editor : MonoBehaviour
{
	protected enum EditorState 
	{ 
		Simple = 0, // Click & Drag to move objects
		Advanced = 1, // Click to select objects, use gizmo
		Place = 2  // Click to place object
	};

	protected EditorState currentState = EditorState.Simple;
	protected EditorState moveState = EditorState.Simple;

	[SerializeField]
	protected GameObject[] spawnablePrefabs;

	protected List<LevelObject> objects;

	new protected Camera camera;

	[SerializeField]
	protected Transform content;

	[SerializeField]
	protected string filePath;
	[SerializeField]
	protected GameObject buttonPrefab;
	[SerializeField]
	protected GameObject gizmoPrefab;
	protected GameObject preview;
	protected GameObject gizmo;
	[SerializeField]
	protected Material previewMaterial;

	protected int typeToSpawn;

	protected GameObject selectedObject;

	[SerializeField]
	protected TMP_Text moveModeText;

	protected bool multiPlace = false;
	[SerializeField]
	protected float gizmoSize;

	protected Vector3 selectedAxis;
	[SerializeField]
	protected float advancedMoveSpeed = 10f;
	public static bool IsPointerOverUIObject()
	{
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		//Debug.Log(results[0].gameObject.name);
		return results.Count > 0;
	}

	public void SetType(int type)
	{
		currentState = EditorState.Place;
		gizmo.transform.position = transform.forward * -1000f;
		selectedObject = null;
		typeToSpawn = type;
		var mesh = spawnablePrefabs[type].GetComponent<MeshFilter>().sharedMesh;
		preview.GetComponent<MeshFilter>().sharedMesh = mesh;
	}

	public void SwitchMoveMode()
	{
		if(moveState == EditorState.Simple)
		{
			moveState = EditorState.Advanced;
		}
		else
		{
			moveState = EditorState.Simple;
		}
		if(currentState == EditorState.Simple)
		{
			currentState = EditorState.Advanced;
			selectedObject = null;
		} 
		else if(currentState == EditorState.Advanced)
		{
			gizmo.transform.position = transform.forward * -1000f;
			currentState = EditorState.Simple;
			selectedObject = null;
		}
		moveModeText.text = moveState == EditorState.Simple ? "Simple" : "Advanced";
	}

	private void Start()
	{
		moveModeText.text = moveState == EditorState.Simple ? "Simple" : "Advanced";
		objects = new List<LevelObject>();
		camera = GetComponent<Camera>();
		gizmo = Instantiate(gizmoPrefab, transform.forward * -1000f, Quaternion.identity);
		preview = Instantiate(spawnablePrefabs[0], Vector3.zero, Quaternion.identity);
		preview.GetComponent<MeshRenderer>().material = previewMaterial;
		preview.layer = 2;
		var bussy = spawnablePrefabs[0].GetComponent<MeshFilter>();
		var mesh = bussy.sharedMesh;
		preview.GetComponent<MeshFilter>().sharedMesh = mesh;

		for(int i = 0; i < spawnablePrefabs.Length; i++)
		{
			int type = new int();
			type = i;
			var button = Instantiate(buttonPrefab, content.transform.position, content.rotation, content);
			button.GetComponentInChildren<TMP_Text>().text = spawnablePrefabs[i].name;
			button.GetComponent<Button>().onClick.AddListener(delegate { SetType(type); } );
		}
	}

	private void Update()
	{
		if (Input.GetButtonDown("SwitchMode"))
		{
			SwitchMoveMode();
		}
		RaycastHit hit;
		switch (currentState)
		{
			case EditorState.Simple:
				if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 1000f))
				{
					if (selectedObject == null)
					{
						if (hit.collider.tag != "Static")
						{
							if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
							{
								selectedObject = hit.collider.gameObject;
								selectedObject.layer = 2;
							}
						}
					}
					else
					{
						PlaceObject(selectedObject, hit);
						if (Input.GetMouseButtonUp(0))
						{
							selectedObject.layer = 0;
							selectedObject = null;
						}
					}
				} 
				break;
			case EditorState.Advanced:
				if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 1000f))
				{
					
					if (hit.collider.tag != "Static" && hit.collider.tag != "Gizmo")
					{
						if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
						{
							selectedObject = hit.collider.gameObject;
							gizmo.transform.position = selectedObject.transform.position;
						}
					}
					if(selectedObject != null && hit.collider.tag == "Gizmo")
					{
						
						if(selectedAxis == Vector3.zero)
						{
							if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
							{
								string name = hit.collider.gameObject.name;
								if (name == "up") selectedAxis = Vector3.up;
								else if (name == "right") selectedAxis = Vector3.right;
								else if (name == "forward") selectedAxis = Vector3.forward;
							}
						}
					}
				}
				if (selectedAxis != Vector3.zero)
				{
					if(selectedAxis == Vector3.right)
						selectedObject.transform.position += selectedAxis * Input.GetAxis("Mouse X") * advancedMoveSpeed * camera.orthographicSize;
					else if(selectedAxis == Vector3.forward)
						selectedObject.transform.position -= selectedAxis * Input.GetAxis("Mouse X") * advancedMoveSpeed * camera.orthographicSize;
					else if(selectedAxis == Vector3.up)
						selectedObject.transform.position += selectedAxis * Input.GetAxis("Mouse Y") * advancedMoveSpeed * camera.orthographicSize;

					if (Input.GetMouseButtonUp(0) && !IsPointerOverUIObject())
					{
						selectedAxis = Vector3.zero;
					}
					gizmo.transform.position = selectedObject.transform.position;
				}
				if (selectedObject != null)
				{
					gizmo.transform.localScale = Vector3.one * camera.orthographicSize * gizmoSize;
				}
				break;
			case EditorState.Place:
				if (Input.GetButtonDown("Multiplace") || Input.GetButtonUp("Multiplace"))
				{
					multiPlace = !multiPlace;
				}
				if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 1000f))
				{
					PlaceObject(preview, hit);
					if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
					{
						var newObject = Instantiate(spawnablePrefabs[typeToSpawn], hit.point, Quaternion.identity);
						PlaceObject(newObject, hit);
						objects.Add(new LevelObject() { type = typeToSpawn, transform = newObject.transform, position = newObject.transform.position, rotation = newObject.transform.rotation });
						if (!multiPlace)
						{
							currentState = moveState;
							if (currentState == EditorState.Advanced)
							{
								selectedObject = newObject;
								gizmo.transform.position = selectedObject.transform.position;
							}
							preview.transform.position = (transform.forward * -1000f);
						}
					}
					if(Input.GetMouseButtonDown(1) && !IsPointerOverUIObject())
					{
						currentState = moveState;
						preview.transform.position = (transform.forward * -1000f);
					}
				}
				break;
		}
		
		if(Input.GetKeyDown(KeyCode.C))
		{
			Save();
		}
		if(Input.GetKeyDown(KeyCode.V))
		{
			Load();
		}
	}

	private void PlaceObject(GameObject go, RaycastHit info)
	{
		float distance = 10f;
		Vector3 normal = info.normal;
		go.transform.position = info.point + normal * distance;
		Vector3 closestPoint = go.GetComponent<Collider>().ClosestPoint(info.point);
		distance -= Vector3.Distance(info.point, closestPoint);
		go.transform.position = info.point + normal * distance;

	}
	private void Save()
	{
		foreach(var obj in objects)
		{
			obj.position = obj.transform.position;
			obj.rotation = obj.transform.rotation;
		}
		string json = JsonArray.ToJson(objects.ToArray(), true);
		File.WriteAllText(filePath, json);
	}

	private void Load()
	{
		string json = File.ReadAllText(filePath);
		LevelObject[] importedObjects = JsonArray.FromJson<LevelObject>(json);
		objects = importedObjects.ToList();
		foreach(var obj in objects)
		{
			var newObject = Instantiate(spawnablePrefabs[obj.type], obj.position, obj.rotation);
			obj.transform = newObject.transform;
		}
	}
}
