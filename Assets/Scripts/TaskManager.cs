﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour {

	public GameObject OBSJumpingTaskAddons;
	public GameObject ClimbingTaskAddons;

	public GameObject[] flatGrounds;

	public GameObject backButtonBG;

	public CameraFollowScript[] cameras;

	private Evolution evolution;

	// Use this for initialization
	void Start () {

		evolution = GameObject.FindGameObjectWithTag("Evolution").GetComponent<Evolution>();

		SetupTask();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void SetupTask() {

		switch(evolution.Settings.task) {
			
		case EvolutionTask.RUNNING: SetupRunningTask(); break;
		case EvolutionTask.JUMPING: SetupJumpingTask(); break;
		case EvolutionTask.OBSTACLE_JUMP: SetupObstacleJumpingTask(); break;
		case EvolutionTask.CLIMBING: SetupClimbingTask(); break;
		}
	}

	private void SetupRunningTask() {
		OBSJumpingTaskAddons.SetActive(false);
		ClimbingTaskAddons.SetActive(false);
		SetFlatGroundsActive(true);
		LockCamerasDiagonal(false);
	}

	private void SetupJumpingTask() {
		OBSJumpingTaskAddons.SetActive(false);
		ClimbingTaskAddons.SetActive(false);
		SetFlatGroundsActive(true);
		LockCamerasDiagonal(false);
	}

	private void SetupObstacleJumpingTask() {
		OBSJumpingTaskAddons.SetActive(true);
		ClimbingTaskAddons.SetActive(false);
		SetFlatGroundsActive(true);
		LockCamerasDiagonal(false);
	}

	private void SetupClimbingTask() {
		OBSJumpingTaskAddons.SetActive(false);
		ClimbingTaskAddons.SetActive(true); 
		SetFlatGroundsActive(false);
		backButtonBG.SetActive(false);
		LockCamerasDiagonal(true);
	}

	private void SetFlatGroundsActive(bool value) {
		foreach (var ground in flatGrounds) {
			ground.SetActive(value);
		}
	}

	private void LockCamerasDiagonal(bool value) {
		foreach (var camera in cameras) {
			camera.DiagonalLock = value;
		}
	}
}
