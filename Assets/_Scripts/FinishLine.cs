using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FinishLine : MonoBehaviour
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
    }

	void OnTriggerEnter(Collider other) {
		if(hasBeenTriggered)
			return;
        if(manager.CrossFinishLine(gameObject))
	    	hasBeenTriggered = true;
	}
}
