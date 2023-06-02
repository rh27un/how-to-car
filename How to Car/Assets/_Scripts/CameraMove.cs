using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMove : MonoBehaviour
{
	[SerializeField]
	public float sensitivity;
	[SerializeField]
	public float zoomSensitivity;

	new protected Camera camera;

	[SerializeField]
	protected Vector2 viewMin;
	[SerializeField]
	protected Vector2 viewMax;
	[SerializeField]
	protected float zoomMin;
	[SerializeField]
	protected float zoomMax;

	private void Start()
	{
		camera = GetComponent<Camera>();
	}
	private void Update()
	{
		if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.tag == "InputField")
			return;
		Vector3 forward = transform.forward;
		forward.y = 0;
		Vector3 right = transform.right;
		right.y = 0;
		Vector3 unclampedPos = transform.position + (forward.normalized * Input.GetAxis("Vertical") * camera.orthographicSize * sensitivity * Time.deltaTime) + (right.normalized * Input.GetAxis("Horizontal") * camera.orthographicSize * sensitivity * Time.deltaTime);
		transform.position = new Vector3(Mathf.Clamp(unclampedPos.x, viewMin.x, viewMax.x), unclampedPos.y, Mathf.Clamp(unclampedPos.z, viewMin.y, viewMax.y));
		camera.orthographicSize = Mathf.Clamp(camera.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * camera.orthographicSize * zoomSensitivity, zoomMin, zoomMax);
	}
}
