using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GameState
{
	Unstarted,
	Started,
	Finished,
	Paused
}

public class GameManager : MonoBehaviour
{
	protected float startTime;
	protected float finishTime;
	protected GameState state = GameState.Unstarted;
	[SerializeField]
	protected GameObject preGameMenu;
	[SerializeField]
	protected GameObject postGameMenu;
	[SerializeField]
	protected GameObject pauseMenu;
	[SerializeField]
	protected GameObject hud;
	[SerializeField]
	protected TMP_Text gameTimer;
	[SerializeField]
	protected TMP_Text endTimer;
	[SerializeField]
	protected TMP_Text levelName;
	[SerializeField]
	protected TMP_Text levelDescription;
	[SerializeField]
	protected TMP_Text objective;
	protected int numUnorderedCheckpoints;
	protected int numClearedUnorderedCheckpoints;
	protected Serializer serializer;

	private void Start()
	{
		var serializerObject = GameObject.FindGameObjectWithTag("Serializer");
		if(serializerObject != null){
			serializer = serializerObject.GetComponent<Serializer>();
			levelName.text = serializer.GetLevelName();
			levelDescription.text = serializer.GetLevelDescription();
			objective.text = serializer.GetLevelDescription();
		}
		state = GameState.Unstarted;
		preGameMenu.SetActive(true);
		postGameMenu.SetActive(false);
		pauseMenu.SetActive(false);
		hud.SetActive(false);
		numUnorderedCheckpoints = GameObject.FindGameObjectsWithTag("UnorderedCheckpoint").Length;
		numClearedUnorderedCheckpoints = 0;
	}

	public void ClearUnorderedCheckpoint(GameObject checkpointObject){
		// Increment cleared checkpoints
		numClearedUnorderedCheckpoints++;
		Destroy(checkpointObject);
		Debug.Log("Cleared " + numClearedUnorderedCheckpoints + " out of " + numUnorderedCheckpoints + " checkpoints");
	}

	public bool CrossFinishLine(GameObject finishLine){
		Debug.Log("Crossed Finish Line");
		if(numClearedUnorderedCheckpoints < numUnorderedCheckpoints){
			Debug.Log("But not enough checkpoints were crossed");
			return false;
		}
		Debug.Log("and finished the game");
		Destroy(finishLine);
		FinishGame();
		return true;
	}

	public void FinishGame(){
		finishTime = Time.time;
		state = GameState.Finished;
		preGameMenu.SetActive(false);
		postGameMenu.SetActive(true);
		pauseMenu.SetActive(false);
		hud.SetActive(false);
		endTimer.text = TimeSpan.FromSeconds(finishTime - startTime).ToString(@"mm\:ss\.ff");
	}
	public void StartGame() {
		startTime = Time.time;
		state = GameState.Started;
		preGameMenu.SetActive(false);
		postGameMenu.SetActive(false);
		pauseMenu.SetActive(false);
		hud.SetActive(true);
	}

	public void PauseGame()
	{
		Time.timeScale = 0f;
		state = GameState.Paused;
		preGameMenu.SetActive(false);
		postGameMenu.SetActive(false);
		pauseMenu.SetActive(true);
		hud.SetActive(false);
	}

	public void ResumeGame()
	{
		Time.timeScale = 1f;
		state = GameState.Started;
		preGameMenu.SetActive(false);
		postGameMenu.SetActive(false);
		pauseMenu.SetActive(false);
		hud.SetActive(true);
	}
	private void Update()
	{
		if(state == GameState.Started)
		{
			gameTimer.text = TimeSpan.FromSeconds(Time.time - startTime).ToString(@"mm\:ss\.ff");
			if (Input.GetButtonDown("Cancel"))
			{
				PauseGame();
			}
		}

	}
}
