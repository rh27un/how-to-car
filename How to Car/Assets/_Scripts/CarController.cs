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
		float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

		foreach(var axleInfo in axleInfos)
		{
			WheelFrictionCurve rightCurve = axleInfo.right.sidewaysFriction;
			WheelFrictionCurve leftCurve = axleInfo.left.sidewaysFriction;

			WheelHit rightHit;
			WheelHit leftHit;

			if (axleInfo.right.GetGroundHit(out rightHit))
			{
				if(Mathf.Abs(rightHit.sidewaysSlip) > 0.5f || Mathf.Abs(rightHit.forwardSlip) > 0.5f)
				{
					axleInfo.right.transform.GetChild(1).GetComponent<TrailRenderer>().emitting = true;

				} else
				{
					axleInfo.right.transform.GetChild(1).GetComponent<TrailRenderer>().emitting = false;
				}
				
				if (rightHit.collider.material.name == "Dirt") // etc
				{
					rightCurve.stiffness = 0.7f;
				} else
				{
					rightCurve.stiffness = 1f;
				}
			}
			if (axleInfo.left.GetGroundHit(out leftHit))
			{
				if (Mathf.Abs(leftHit.sidewaysSlip) > 0.5f || Mathf.Abs(leftHit.forwardSlip) > 0.5f)
				{
					axleInfo.left.transform.GetChild(1).GetComponent<TrailRenderer>().emitting = true;

				}
				else
				{
					axleInfo.left.transform.GetChild(1).GetComponent<TrailRenderer>().emitting = false;
				}
				if (leftHit.collider.material.name == "Dirt") // etc
				{
					leftCurve.stiffness = 0.7f;
				}
				else
				{
					leftCurve.stiffness = 1f;
				}
			}

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
			if (Input.GetButton("Jump"))
			{
				braking = (maxSteeringAngle - Mathf.Abs(steering)) * maxBrakeTorque;
				if(axleInfo.drifting)
				{
					rightCurve.stiffness /= 2f;
					leftCurve.stiffness /= 2f;
				}
			}
			axleInfo.right.sidewaysFriction = rightCurve;
			axleInfo.left.sidewaysFriction = leftCurve;
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
	public bool drifting;
}