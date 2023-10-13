using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Transactions;
using System.Data;

public class LevelEditor : MonoBehaviour
{
	protected enum EditorState
	{
		Simple = 0, // Click & Drag to move objects
		Advanced = 1, // Click to select objects, use gizmo
		Place = 2  // Click to place object
	};

	protected EditorState currentState = EditorState.Simple;
	protected EditorState moveState = EditorState.Simple;

	//[SerializeField]
	//protected GameObject[] spawnablePrefabs;

	[SerializeField]
	protected SpawnablePrefabs prefabList;

	protected List<LevelObject> objects;

	new protected Camera camera;

	[SerializeField]
	protected Transform content;

	[SerializeField]
	protected GameObject buttonPrefab;
	[SerializeField]
	protected GameObject gizmoPrefab;
	protected GameObject preview;

	protected List<GameObject> roadPreviews = new List<GameObject>();

	protected GameObject gizmo;
	[SerializeField]
	protected Material previewMaterial;

	protected int typeToSpawn;

	//protected GameObject selectedObject;

	protected List<GameObject> selectedObjects = new List<GameObject>();

	[SerializeField]
	protected TMP_Text moveModeText;

	protected bool multiPlace = false;
	[SerializeField]
	protected float gizmoSize;

	protected Vector3 selectedAxis;

	protected Vector3 simpleMoveOffset;
	[SerializeField]
	protected float advancedMoveSpeed = 10f;

	protected int currentJoint = 0;

	protected Serializer serializer;

	[SerializeField]
	protected TMP_InputField levelName;
	[SerializeField]
	protected TMP_InputField levelDescription;
	[SerializeField]
	protected TMP_InputField inputXPos;
	[SerializeField]
	protected TMP_InputField inputYPos;
	[SerializeField]
	protected TMP_InputField inputZPos;
	[SerializeField]
	protected TMP_InputField inputXRot;
	[SerializeField]
	protected TMP_InputField inputYRot;
	[SerializeField]
	protected TMP_InputField inputZRot;
	[SerializeField]
	protected TMP_Dropdown inputObjectType;
	[SerializeField]
	protected 
	Dictionary<int, TrackObject> trackObjects = new Dictionary<int, TrackObject>()

	{
		{
			4, new TrackObject()
			{
				type = 4,
				joints = new TrackJoint[2]
				{
					new TrackJoint() { offset = new Vector3(0f, 0f, 7.5f), forward = Vector3.forward },
					new TrackJoint() { offset = new Vector3(0f, 0f, -7.5f), forward = Vector3.back }
				}
			}
		},
		{
			5, new TrackObject()
			{
			type = 5,
				joints = new TrackJoint[2]
				{
					new TrackJoint() { offset = new Vector3(-6.8f, 0f, -6.8f), forward = Vector3.left },
					new TrackJoint() { offset = Vector3.zero, forward = Vector3.forward }
				}
			}
		},
		{
			6, new TrackObject()
			{
				type = 6,
				joints = new TrackJoint[2]
				{
					new TrackJoint() { offset = Vector3.zero, forward = Vector3.forward },
					new TrackJoint() { offset = new Vector3(-2.84f, 0f, -6.32f), forward = new Vector3(-0.707106f, 0f, -0.707106f)}
				}
			}
		},
		{
			7, new TrackObject()
			{
			type = 7,
				joints = new TrackJoint[3]
				{
					new TrackJoint() { offset = Vector3.zero, forward = Vector3.forward },
					new TrackJoint() { offset = new Vector3(-8.92f, 0f, -12.63f), forward = Vector3.left },
					new TrackJoint() { offset = new Vector3(8.92f, 0f, -12.63f), forward = Vector3.right },
				}
			}
		},
		{
			8, new TrackObject()
			{
				type = 8,
				joints = new TrackJoint[2]
				{
					new TrackJoint() { offset = new Vector3(0f, 0f, 7.5f), forward = Vector3.forward },
					new TrackJoint() { offset = new Vector3(0f, 0f, -7.5f), forward = Vector3.back }
				}
			}
		},
		{
			9, new TrackObject()
			{
				type = 9,
				joints = new TrackJoint[2]
				{
					new TrackJoint() { offset = new Vector3(0f, 0f, 7.5f), forward = Vector3.forward },
					new TrackJoint() { offset = new Vector3(0f, 0f, -7.5f), forward = Vector3.back }
				}
			}
		}
	};

	int gizmoLayer = (1 << 6);
	int nextGroup = 0;
	[SerializeField]
	private bool debugLevelEditor;

	public void PlaceNewTrack(TrackObject old, int oldJointIndex, TrackObject _new, int newJointIndex)
	{
		TrackJoint oldJoint = old.joints[oldJointIndex];
		TrackJoint newJoint = _new.joints[newJointIndex];
		// Find the position of the old joint
		Vector3 pos = old.position + oldJoint.offset;
		// Place the new joint in that position
		_new.gameObject.transform.position = pos + newJoint.offset;
		// rotation that points in the same direction as the old joint we want to connect to
		Quaternion quaternion = Quaternion.LookRotation(oldJoint.forward, Vector3.up);
		quaternion *= old.rotation;
		quaternion *= Quaternion.Inverse(Quaternion.LookRotation(-newJoint.forward, Vector3.up));
		_new.gameObject.transform.rotation = quaternion;
		Vector3 oldJointWorldPos = old.gameObject.transform.TransformPoint(oldJoint.offset);
		Vector3 newJointWorldOffset = _new.gameObject.transform.position - _new.gameObject.transform.TransformPoint(newJoint.offset);
		_new.gameObject.transform.position = oldJointWorldPos + newJointWorldOffset;

	}

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
		selectedObjects.Clear();
		typeToSpawn = type;
		var mesh = prefabList.Prefabs[type].GetComponentInChildren<MeshFilter>().sharedMesh;
		preview.GetComponent<MeshFilter>().sharedMesh = mesh;
		preview.GetComponent<MeshCollider>().sharedMesh = mesh;
		if (prefabList.Prefabs[type].transform.childCount > 0)
		{
			preview.transform.rotation = prefabList.Prefabs[type].transform.GetChild(0).rotation;
		}
		else
		{
			preview.transform.rotation = Quaternion.identity;
		}
		if (trackObjects.ContainsKey(type))
		{
			currentJoint = currentJoint % trackObjects[typeToSpawn].joints.Length;
			foreach (var trackObject in objects.Where(o => trackObjects.ContainsKey(o.type)))
			{
				var track = (TrackObject)trackObject;
				for (int i = 0; i < track.joints.Length; i++)
				{
					// if joint is taken by preview
					var joint = track.joints[i];
					if (joint.takenBy.isPreview)
					{
						var trackPreview = joint.takenBy.gameObject;
						trackPreview.GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
						trackPreview.GetComponentInChildren<MeshCollider>().sharedMesh = mesh;
						track.joints[i].takenBy.gameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
						PlaceNewTrack((TrackObject)track, i, new TrackObject(trackObjects[type], trackPreview), currentJoint);
					}
				}
			}
		}
		else
		{
			foreach (var trackObject in objects.Where(o => trackObjects.ContainsKey(o.type)))
			{
				var track = (TrackObject)trackObject;
				for (int i = 0; i < track.joints.Length; i++)
				{
					if (track.joints[i].takenBy.isPreview)
						track.joints[i].takenBy.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
				}
			}
		}
	}

	public void SwitchMoveMode()
	{
		if (moveState == EditorState.Simple)
		{
			moveState = EditorState.Advanced;
		}
		else
		{
			moveState = EditorState.Simple;
		}
		if (currentState == EditorState.Simple)
		{
			currentState = EditorState.Advanced;
			selectedObjects.Clear();
		}
		else if (currentState == EditorState.Advanced)
		{
			gizmo.transform.position = transform.forward * -1000f;
			currentState = EditorState.Simple;
			selectedObjects.Clear();
		}
		moveModeText.text = moveState == EditorState.Simple ? "Simple" : "Advanced";
	}

	private void Start()
	{
		var serializerObject = GameObject.FindGameObjectWithTag("Serializer");
		objects = new List<LevelObject>();
		if (serializerObject == null) // assume we're debugging
		{
			if (debugLevelEditor)
			{
				GameObject.Find("GAMEPLAY").SetActive(false);
				serializer = gameObject.AddComponent<Serializer>();
				serializer.filePath = "Debug.json";
			}
			else
			{
				GameObject.Find("LEVEL EDITOR").SetActive(false);
				return;
			}
		} else
		{
			serializer = serializerObject.GetComponent<Serializer>();
			if(!string.IsNullOrEmpty(serializer.filePath))
				Load();
		}
		moveModeText.text = moveState == EditorState.Simple ? "Simple" : "Advanced";
		camera = GetComponent<Camera>();
		gizmo = Instantiate(gizmoPrefab, transform.forward * -1000f, Quaternion.identity);
		preview = Instantiate(prefabList.Prefabs[0], Vector3.zero, Quaternion.identity);
		preview.GetComponent<MeshRenderer>().material = previewMaterial;
		preview.layer = 2;
		var bussy = prefabList.Prefabs[0].GetComponent<MeshFilter>();
		var mesh = bussy.sharedMesh;
		if (prefabList.Prefabs[0].transform.childCount > 0)
		{
			preview.transform.rotation = prefabList.Prefabs[0].transform.GetChild(0).rotation;
		}
		else
		{
			preview.transform.rotation = Quaternion.identity;
		}
		preview.GetComponent<MeshFilter>().sharedMesh = mesh;
		for (int i = 0; i < prefabList.Prefabs.Count(); i++)
		{
			int type = new int();
			type = i;
			var button = Instantiate(buttonPrefab, content.transform.position, content.rotation, content);
			button.GetComponentInChildren<TMP_Text>().text = prefabList.Prefabs[i].name;
			button.GetComponent<Button>().onClick.AddListener(delegate { SetType(type); });
		}
	}

	private void Update()
	{
		if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.tag == "InputField")
			return;
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
					if (selectedObjects.Count == 0)
					{
						if (hit.collider.tag != "Static")
						{
							if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
							{
								selectedObjects.Add(hit.collider.gameObject);
								selectedObjects[0].layer = 2;
								simpleMoveOffset = selectedObjects[0].transform.position - hit.point;
							}
						}
					}
					else
					{
						PlaceObject(selectedObjects[0], hit, simpleMoveOffset);
						if (Input.GetMouseButtonUp(0))
						{
							selectedObjects[0].layer = 0;
							selectedObjects.Clear();
						}
					}
				}
				break;
			case EditorState.Advanced:
				if (selectedObjects.Count > 0)
				{
					if(Input.GetKeyDown(KeyCode.Delete)){
						
						foreach(GameObject selectedObject in selectedObjects)
						{
							var topMost = selectedObject;
							while(topMost.transform.parent != null)
							{
								topMost = transform.parent.gameObject;
							}
							objects.Remove(objects.SingleOrDefault(o => o.gameObject == topMost));
							Destroy(topMost);
						}
					}
					if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 1000f, gizmoLayer))
					{
						if (selectedObjects.Count > 0 && hit.collider.tag == "Gizmo")
						{

							if (selectedAxis == Vector3.zero)
							{
								if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
								{
									string name = hit.collider.gameObject.name;
									if (name == "up")
										selectedAxis = Vector3.up;
									else if (name == "right")
										selectedAxis = Vector3.right;
									else if (name == "forward")
										selectedAxis = Vector3.forward;
									else if (name == "yaw")
										selectedAxis = -Vector3.up;
									else if (name == "roll")
										selectedAxis = -Vector3.forward;
									else if (name == "pitch")
										selectedAxis = -Vector3.right;
								}
							}
							break;
						}
					}
				}
				if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 1000f))
				{

					if (hit.collider.tag != "Static" && hit.collider.tag != "Gizmo")
					{
						if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
						{
							var newObject = hit.collider.gameObject;
							while (newObject.transform.parent != null)
							{
								newObject = newObject.transform.parent.gameObject;
							}
							if (selectedObjects.Contains(newObject))
							{
								var thisObject = objects.SingleOrDefault(o => o.gameObject == newObject);
								foreach (var obj in objects.Where(o => o.group_id == thisObject.group_id && !selectedObjects.Contains(o.gameObject)))
								{
									selectedObjects.Add(obj.gameObject);
								}
							}
							else if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
							{
								selectedObjects.Clear();
							}
							
							if (!selectedObjects.Contains(newObject))
							{
								selectedObjects.Add(newObject);
								gizmo.transform.position = selectedObjects[0].transform.position;
							}
						}
					}

				}
				if (selectedAxis != Vector3.zero)
				{
					foreach (GameObject selectedObject in selectedObjects)
					{
						if (selectedAxis == Vector3.right)
							selectedObject.transform.position += selectedAxis * Input.GetAxis("Mouse Y") * advancedMoveSpeed * camera.orthographicSize;
						else if (selectedAxis == Vector3.forward)
							selectedObject.transform.position -= selectedAxis * Input.GetAxis("Mouse X") * advancedMoveSpeed * camera.orthographicSize;
						else if (selectedAxis == Vector3.up)
							selectedObject.transform.position += selectedAxis * Input.GetAxis("Mouse X") * advancedMoveSpeed * camera.orthographicSize;
						else if (selectedAxis == -Vector3.right)
							selectedObject.transform.RotateAround(gizmo.transform.position, selectedAxis, Input.GetAxis("Mouse Y"));
						else if (selectedAxis == -Vector3.forward)
							selectedObject.transform.RotateAround(gizmo.transform.position, selectedAxis, Input.GetAxis("Mouse Y"));
						else if (selectedAxis == -Vector3.up)
							selectedObject.transform.RotateAround(gizmo.transform.position, selectedAxis, Input.GetAxis("Mouse X"));
					}
					if (Input.GetMouseButtonUp(0) && !IsPointerOverUIObject())
					{
						selectedAxis = Vector3.zero;
					}
					gizmo.transform.position = selectedObjects[0].transform.position;
				}
				if (selectedObjects.Count > 0)
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
					if (hit.collider.tag != "RoadPreview")
						PlaceObject(preview, hit, Vector3.zero);
					else
						preview.transform.position = Vector3.down;

					preview.transform.Rotate(Vector3.up, Input.GetAxis("Rotate") * Time.deltaTime * 100f, Space.World);

					if (trackObjects.ContainsKey(typeToSpawn))
					{
						if (Input.GetKeyDown(KeyCode.Q))
						{
							currentJoint = (currentJoint + 1) % trackObjects[typeToSpawn].joints.Length;
							var mesh = prefabList.Prefabs[typeToSpawn].GetComponentInChildren<MeshFilter>().sharedMesh;
							foreach (var trackObject in objects.Where(o => trackObjects.ContainsKey(o.type)))
							{
								var track = (TrackObject)trackObject;
								for (int i = 0; i < track.joints.Length; i++)
								{
									// if joint is taken by preview
									var joint = track.joints[i];
									if (joint.takenBy.isPreview)
									{
										var trackPreview = joint.takenBy.gameObject;
										trackPreview.GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
										trackPreview.GetComponentInChildren<MeshCollider>().sharedMesh = mesh;
										track.joints[i].takenBy.gameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
										PlaceNewTrack((TrackObject)track, i, new TrackObject(trackObjects[typeToSpawn], trackPreview.gameObject), currentJoint);
									}
								}
							}
						}
						if (Input.GetKeyDown(KeyCode.E))
						{
							currentJoint = (currentJoint + trackObjects[typeToSpawn].joints.Length - 1) % trackObjects[typeToSpawn].joints.Length;
							var mesh = prefabList.Prefabs[typeToSpawn].GetComponentInChildren<MeshFilter>().sharedMesh;
							foreach (var trackObject in objects.Where(o => trackObjects.ContainsKey(o.type)))
							{
								var track = (TrackObject)trackObject;
								for (int i = 0; i < track.joints.Length; i++)
								{
									// if joint is taken by preview
									var joint = track.joints[i];
									if (joint.takenBy.isPreview)
									{
										var trackPreview = joint.takenBy.gameObject;
										trackPreview.GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
										trackPreview.GetComponentInChildren<MeshCollider>().sharedMesh = mesh;
										track.joints[i].takenBy.gameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
										PlaceNewTrack((TrackObject)track, i, new TrackObject(trackObjects[typeToSpawn], trackPreview), currentJoint);
									}
								}
							}
						}
					}

					if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
					{
						var newObject = Instantiate(prefabList.Prefabs[typeToSpawn], hit.point, preview.transform.rotation);
						if (newObject.transform.childCount > 0)
						{
							newObject.transform.Rotate(Vector3.left, -90f, Space.Self);
						}
						PlaceObject(newObject, hit, Vector3.zero);

						// If we're attaching a track object to another (mouse pointer is on a RoadPreview gameobject)
						if (trackObjects.ContainsKey(typeToSpawn) && hit.collider.tag == "RoadPreview")
						{
							var newTrack = new TrackObject(trackObjects[typeToSpawn]);
							newTrack.gameObject = newObject;
							newObject.transform.position = hit.collider.transform.parent.position;
							newObject.transform.rotation = hit.collider.transform.parent.rotation;
							newTrack.position = newObject.transform.position;
							newTrack.rotation = newObject.transform.rotation;
							newTrack.isPreview = false;
							var roadPreview = hit.collider.GetComponent<RoadPreview>();
							var oldTrack = roadPreview.trackObject;
							newTrack.group_id = oldTrack.group_id;
							Destroy(hit.collider.transform.parent.gameObject);
							for (int i = 0; i < newTrack.joints.Length; i++)
							{
								if (i != currentJoint)
								{
									var joint = newTrack.joints[i];
									var previewObject = Instantiate(prefabList.Prefabs[4]);
									previewObject.GetComponentInChildren<MeshRenderer>().material = previewMaterial;
									previewObject.transform.GetChild(0).gameObject.AddComponent<RoadPreview>().Set(newTrack, i);
									previewObject.tag = "RoadPreview";
									previewObject.transform.GetChild(0).tag = "RoadPreview";
									TrackObject previewTrack = new TrackObject(trackObjects[4], previewObject);
									previewTrack.isPreview = true;
									joint.takenBy = previewTrack;
								} else
								{
									oldTrack.joints[roadPreview.jointIndex].takenBy = newTrack;
									newTrack.joints[i].takenBy = oldTrack;
								}
							}
							objects.Add(newTrack);

						}
						else if (trackObjects.ContainsKey(typeToSpawn)) // otherwise if we're just placing a new trackobject
						{
							var newTrack = new TrackObject(trackObjects[typeToSpawn]);
							newTrack.gameObject = newObject;
							newTrack.position = newObject.transform.position;
							newTrack.rotation = newObject.transform.rotation;
							newTrack.group_id = nextGroup;
							nextGroup++;
							for (int i = 0; i < newTrack.joints.Length; i++)
							{
								var joint = newTrack.joints[i];
								var previewObject = Instantiate(prefabList.Prefabs[4]);
								previewObject.GetComponentInChildren<MeshRenderer>().material = previewMaterial;
								previewObject.transform.GetChild(0).gameObject.AddComponent<RoadPreview>().Set(newTrack, i);
								previewObject.tag = "RoadPreview";
								previewObject.transform.GetChild(0).tag = "RoadPreview";
								TrackObject previewTrack = new TrackObject(trackObjects[4], previewObject);
								previewTrack.isPreview = true;
								joint.takenBy = previewTrack;
							}
							
							objects.Add(newTrack);
						}
						else //otherwise we're just adding a regular object
						{
							objects.Add(new LevelObject() { type = typeToSpawn, group_id = nextGroup, gameObject = newObject, position = newObject.transform.position, rotation = newObject.transform.rotation });
							nextGroup++;
						}

						if (!multiPlace)
						{
							currentState = moveState;
							if (currentState == EditorState.Advanced)
							{
								selectedObjects.Clear();
								selectedObjects.Add(newObject);
								gizmo.transform.position = selectedObjects[0].transform.position;
							}
							preview.transform.position = (transform.forward * -1000f);
							foreach (var trackObject in objects.Where(o => trackObjects.ContainsKey(o.type)))
							{
								var track = (TrackObject)trackObject;
								for (int i = 0; i < track.joints.Length; i++)
								{
									if (track.joints[i].takenBy.isPreview)
										track.joints[i].takenBy.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
								}
							}
						}
						else if (trackObjects.ContainsKey(typeToSpawn))
						{
							var mesh = prefabList.Prefabs[typeToSpawn].GetComponentInChildren<MeshFilter>().sharedMesh;
							foreach (var trackObject in objects.Where(o => trackObjects.ContainsKey(o.type)))
							{
								var track = (TrackObject)trackObject;
								for (int i = 0; i < track.joints.Length; i++)
								{
									// if joint is taken by preview
									var joint = track.joints[i];
									if (joint.takenBy.isPreview)
									{
										var trackPreview = joint.takenBy.gameObject;
										trackPreview.GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
										trackPreview.GetComponentInChildren<MeshCollider>().sharedMesh = mesh;
										track.joints[i].takenBy.gameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
										PlaceNewTrack((TrackObject)track, i, new TrackObject(trackObjects[typeToSpawn], trackPreview), currentJoint);
									}
								}
							}
						}
					}
					if (Input.GetMouseButtonDown(1) && !IsPointerOverUIObject())
					{
						currentState = moveState;
						preview.transform.position = (transform.forward * -1000f);
					}
				}
				break;
		}

		//if (Input.GetKeyDown(KeyCode.C))
		//{
		//	Save();
		//}
		//if (Input.GetKeyDown(KeyCode.V))
		//{
		//	Load();
		//}
	}

	private void AddObject(GameObject _obj, int _type)
	{
		if (trackObjects.ContainsKey(_type))
		{
			TrackObject trobj = new TrackObject(trackObjects[typeToSpawn]);
			trobj.gameObject = _obj;
			trobj.position = _obj.transform.position;
			trobj.rotation = _obj.transform.rotation;
			for (int i = 0; i < trobj.joints.Length; i++)
			{
				var joint = trobj.joints[i];
				var previewObject = Instantiate(prefabList.Prefabs[4]);
				previewObject.GetComponentInChildren<MeshRenderer>().material = previewMaterial;
				previewObject.transform.GetChild(0).gameObject.AddComponent<RoadPreview>().Set(trobj, i);
				previewObject.tag = "RoadPreview";
				previewObject.transform.GetChild(0).tag = "RoadPreview";
				TrackObject previewTrack = new TrackObject(trackObjects[4], previewObject);
				previewTrack.isPreview = true;
				joint.takenBy = previewTrack;
			}
			objects.Add(trobj);
			trobj.id = objects.IndexOf(trobj);
		} else
		{
			LevelObject obj = new LevelObject() { type = _type, gameObject = _obj, position = _obj.transform.position, rotation = _obj.transform.rotation };
			objects.Add(obj);
			obj.id = objects.IndexOf(obj);
		}

	}
	private void PlaceObject(GameObject go, RaycastHit info, Vector3 offset)
	{
		while(go.transform.parent != null)
		{
			go = go.transform.parent.gameObject;
		}
		float distance = 10f;
		Vector3 normal = info.normal;
		go.transform.position = info.point + offset + normal * distance;
		Vector3 closestPoint = go.GetComponentInChildren<Collider>().ClosestPoint(info.point);
		distance -= Vector3.Distance(info.point, closestPoint);
		go.transform.position = info.point + offset + normal * distance;

	}

	public void Save()
	{
		serializer.SetLevelName(levelName.text);
		serializer.SetLevelDescription(levelDescription.text);
		serializer.SetFilePathManually(levelName.text);
		serializer.SaveLevel(objects);
	}

	private void Load()
	{
		nextGroup = 0;
		var levelData = serializer.LoadLevel();
		var objectsPlain = levelData.objects.ToList();
		levelName.text = levelData.prettyName;
		levelDescription.text = levelData.description;
		if(objectsPlain == null)
			return;
		foreach(var obj in objectsPlain)
		{
			if(obj.group_id >= nextGroup)
			{
				nextGroup = obj.group_id + 1;
			}
			string[] rawData;
			Dictionary<string, string> data = null;
			if (!string.IsNullOrEmpty(obj.data))
			{
				rawData = obj.data.Split(';');
				data = rawData.Where(d => !string.IsNullOrEmpty(d)).ToDictionary(k => k.Split(':')[0], e => e.Split(':')[1]);
			}
			if (trackObjects.ContainsKey(obj.type))
			{
				var newTrack = new TrackObject(trackObjects[obj.type]);
				newTrack.group_id = obj.group_id;
				newTrack.position = obj.position;
				newTrack.rotation = obj.rotation;
				for (int i = 0; i < newTrack.joints.Length; i++)
				{
					// this is where we check to see if the joint in the object data is
					Dictionary<int, int> joints = null;
					if (data != null && data.ContainsKey("JOINTS"))
					{
						if (!string.IsNullOrEmpty(data["JOINTS"])){
							joints = data["JOINTS"].Split(',').Where(j => !string.IsNullOrEmpty(j)).ToDictionary(k => int.Parse(k.Split('=')[0]), v => int.Parse(v.Split('=')[1]));
						}
					}
					if (joints != null && joints.ContainsKey(i))
					{
						var joint = newTrack.joints[i];
						joint.takenBy = new TrackObject();
						//(TrackObject)objectsPlain[joints[i]];
					}
					else
					{
						var joint = newTrack.joints[i];
						var previewObject = Instantiate(prefabList.Prefabs[4]);
						previewObject.GetComponentInChildren<MeshRenderer>().material = previewMaterial;
						previewObject.transform.GetChild(0).gameObject.AddComponent<RoadPreview>().Set(newTrack, i);
						previewObject.tag = "RoadPreview";
						previewObject.transform.GetChild(0).tag = "RoadPreview";
						TrackObject previewTrack = new TrackObject(trackObjects[4], previewObject);
						previewTrack.isPreview = true;
						joint.takenBy = previewTrack;
					}
				}

				objects.Add(newTrack);
			}
			else 
			{

				objects.Add(obj);
			}
		}
		if(objects == null)
		{
			objects = new List<LevelObject>();
		}
		foreach (var obj in objects)
		{
			var newObject = Instantiate(prefabList.Prefabs[obj.type], obj.position, obj.rotation);
			obj.gameObject = newObject;
		}
	}
}
