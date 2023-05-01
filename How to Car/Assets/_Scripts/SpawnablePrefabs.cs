using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawnable Prefabs", menuName = "How To Car/Spawnable Prefab List")]
public class SpawnablePrefabs : ScriptableObject
{
	public List<GameObject> Prefabs = new List<GameObject>();
}
