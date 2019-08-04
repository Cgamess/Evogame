﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.Evolution {

	public class ObstacleSpawner : MonoBehaviour {

		private const string OBSTACLE_PREFAB_PATH = "Prefabs/Obstacle Ball"; 

		/// <summary>
		/// The force with which the obstacles are accelerated after spawn.
		/// </summary>
		private float Obstacle_Force = 5000f;
		/// <summary>
		/// The time distance between the spawn of two obstacles in seconds.
		/// </summary>
		private float Obstacle_Distance = 5f;

		public Transform spawnPoint;

		public BestCreaturesController BCController;

		public GameObject obstacle;
		private Rigidbody obsRigidbody;

		public bool BCScene;


		void Start () {

			obsRigidbody = obstacle.GetComponent<Rigidbody>();

			StartCoroutine(SpawnObstacle());
		}
		
		private IEnumerator SpawnObstacle() {

			while(true) {

				obsRigidbody.velocity = Vector3.zero;
				obstacle.transform.position = spawnPoint.position;
				obsRigidbody.AddForce(new Vector3(-Obstacle_Force, 0f, 0f));

				UpdateObstacleKnowledge();

				yield return new WaitForSeconds(Obstacle_Distance);
			}

			//yield return null;
		} 

		private void UpdateObstacleKnowledge() {
			if (!BCScene) {
				GameObject.Find("Evolution").GetComponent<Evolution>().UpdateCreaturesWithObstacle(obstacle);	
			} else {
				BCController.CurrentBest.Obstacle = obstacle;
			}
		}
	}
}
