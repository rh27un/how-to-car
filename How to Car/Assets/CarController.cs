using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CarController : MonoBehaviour
{
	public TMP_Text speed;
	public List<AxleInfo> axleInfos= new List<AxleInfo>();
	public int gear = 1;
	public float maxMotorTorque;
	public float maxSteeringAngle;
	public float maxBrakeTorque;
	protected Rigidbody rb;
	public const float msToMph = 2.237f;

	private void Start()
	{
		rb= GetComponent<Rigidbody>();
	}
	protected void ApplyLocalPositionToVisuals(WheelCollider collider)
	{
		if(collider.transform.childCount == 0) return;

		Transform visualWheel = collider.transform.GetChild(0);

		Vector3 pos;
		Quaternion rot;
		collider.GetWorldPose(out pos, out rot);
		visualWheel.position = pos;
		visualWheel.rotation = rot;
	}

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			gear++;
		}
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			gear--;
		}
	}
	public void FixedUpdate()
	{
		float motor;
		float braking;
		float leftTrigger = Mathf.Clamp(Input.GetAxis("Throttle"), -1, 0);
		float rightTrigger = Mathf.Clamp(Input.GetAxis("Throttle"), 0, 1);
		Vector3 velocity = rb.velocity;
		if(velocity.magnitude < 0.1f)
		{
			motor = (maxMotorTorque * rightTrigger) + (maxMotorTorque * leftTrigger);
			braking = 0f;
		}
		else if(Vector3.Dot(transform.forward, velocity) > 0)
		{
			motor = maxMotorTorque * rightTrigger;
			braking = 0 - maxBrakeTorque * leftTrigger;
		}
		else
		{
			motor = maxMotorTorque * leftTrigger;
			braking = maxMotorTorque * rightTrigger;
		}
		Debug.Log(Input.GetAxis("Throttle"));
		float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

		foreach(var axleInfo in axleInfos)
		{
			if (axleInfo.steering)
			{
				axleInfo.right.steerAngle = steering;
				axleInfo.left.steerAngle = steering;
			}
			if (axleInfo.motor)
			{
				//Debug.Log(axleInfo.left.rpm + " " + axleInfo.right.rpm);
				axleInfo.right.motorTorque = motor;
				axleInfo.left.motorTorque = motor;
			}
			axleInfo.right.brakeTorque = braking;
			axleInfo.left.brakeTorque = braking;
			ApplyLocalPositionToVisuals(axleInfo.left);
			ApplyLocalPositionToVisuals(axleInfo.right);
		}
	}
}

[System.Serializable]
public class AxleInfo
{
	public WheelCollider left;
	public WheelCollider right;
	public bool motor;
	public bool steering;
}