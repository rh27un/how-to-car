using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UnorderedCheckpoint : MonoBehaviour
{
    protected GameManager manager;
	// To hopefully prevent any HOME - We're Finally Landing moments
	protected bool hasBeenTriggered = false;
    void Awake()
    {
		var controller = GameObject.FindGameObjectWithTag("GameController");
		if(controller != null)
		{
        	manager = controller.GetComponent<GameManager>();
		} else {
			Destroy(gameObject);
		}
		gameObject.tag = "UnorderedCheckpoint";
    }

	void OnTriggerEnter(Collider other) {
		if(hasBeenTriggered)
			return;
		manager.ClearUnorderedCheckpoint(gameObject);
		hasBeenTriggered = true;
	}
}
