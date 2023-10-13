using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPreview : MonoBehaviour
{
	public TrackObject trackObject;
	public int jointIndex;

	public void Set(TrackObject _trackObject, int _jointIndex)
	{
		trackObject = _trackObject;
		jointIndex = _jointIndex;
	}
}
