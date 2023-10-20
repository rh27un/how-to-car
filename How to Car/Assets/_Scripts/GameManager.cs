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
	protected TMP_Text endTime;
	protected int numUnorderedCheckpoints;
	protected int numClearedUnorderedCheckpoints;

	private void Start()
	{
		state = GameState.Unstarted;
		preGameMenu.SetActive(true);
		postGameMenu.SetActive(false);
		pauseMenu.SetActive(false);
		hud.SetActive(false);
		numUnorderedCheckpoints = GameObject.FindAllGameObjectWithTag("UnorderedCheckpoint").;
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
