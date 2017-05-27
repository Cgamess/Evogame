﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class CreatureBuilder : MonoBehaviour {

	public enum BodyPart {
		None,
		Joint, 
		Bone, 
		Muscle
	}

	[SerializeField] private bool gridEnabled = false;

	public ButtonManager buttonManager;

	/** The joint that can connect multiple Bones. */
	public GameObject jointPreset;

	/** 
	 * The connection objects that represent the skeleton of the creature. 
	 * Bodyconnections can only be placed between two existing joints.
	*/
	public GameObject bonePreset;

	/** The material for the muscles. */
	public Material muscleMaterial;

	public Texture2D mouseDeleteTexture;

	public Evolution evolution;

	public SaveDialog saveDialog;

	//private CreatureSaver creatureSaver;


	/** The joints of the creature that have been placed in the scene. */
	private List<Joint> joints;
	/** The bones that have been placed in the scene. */
	private List<Bone> bones;
	/** The muscles that have been placed in the scene. */
	private List<Muscle> muscles;

	private BodyPart selectedPart;
	public BodyPart SelectedPart {
		get { return selectedPart; }
		set {
			selectedPart = value;
			UpdateHoverables();
		}
	}

	/** The Bone that is currently being placed. */
	private Bone currentBone;

	/** The Muscle that is currentyl being placed. */
	private Muscle currentMuscle;


	public static float CONNECTION_WIDHT = 0.5f;
	private bool TESTING_ENABLED = true;

	/// <summary>
	/// Indicates whether it is the first time starting the program ( = no evolution has taken place yet)
	/// </summary>
	private static bool firstTime = true;
	/// <summary>
	/// The name of the last created creature
	/// </summary>
	private static string lastCreatureName = "Creature";

	// Use this for initialization
	void Start () {

		CreatureSaver.SetupPlayerPrefs();

		// initialize arrays
		joints = new List<Joint>();
		bones = new List<Bone>();
		muscles = new List<Muscle>();

		// Joints are selected by default.
		selectedPart = BodyPart.Joint;

		buttonManager.SetupDropDown();
		//creatureSaver =  new CreatureSaver();
		if (!firstTime) {
			CreatureSaver.LoadCurrentCreature(this);
			//print("Last creature name: " + lastCreatureName);
			buttonManager.SetDropDownToValue(lastCreatureName);
		}

		firstTime = false;


		//CreatureSaver.Test();

	}
	
	// Update is called once per frame
	void Update () {

		HandleClicks();

		HandleKeyboardInput();
	}

	private bool isPointerOverUIObject(){

		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

		return results.Count > 0;
	}

	/// <summary>
	/// Checks for click / touch events and handles them appropiately depending on the 
	/// currently selected body part.
	/// </summary>
	private void HandleClicks() {

		if ( Input.GetMouseButtonDown(0) ) { 	// user clicked

			if (EventSystem.current.IsPointerOverGameObject()) return;
			if (isPointerOverUIObject()) return;
			
			if (selectedPart == BodyPart.Joint) {			// Place a JOINT
				PlaceJoint(ScreenToWorldPoint(Input.mousePosition));

			} else if (selectedPart == BodyPart.Bone ) {	// Start placing BONE
				// find the selected joint
				Joint joint = GetHoveringObject<Joint>(joints);

				if (joint != null) {

					CreateBoneFromJoint(joint);
					PlaceConnectionBetweenPoints(currentBone.gameObject, joint.center, ScreenToWorldPoint(Input.mousePosition), CONNECTION_WIDHT);
				}

			} else if (selectedPart == BodyPart.Muscle) {	// Start placing MUSCLE
				// find the selected bone
				Bone bone = GetHoveringObject<Bone>(bones);

				if (bone != null) {

					Vector3 mousePos = ScreenToWorldPoint(Input.mousePosition);
					MuscleJoint joint = bone.muscleJoint;

					CreateMuscleFromJoint(joint);
					//PlaceConnectionBetweenPoints(currentMuscle.gameObject, joint.position, mousePos, CONNECTION_WIDHT);
					currentMuscle.SetLinePoints(joint.position, mousePos);
				}
			} else if (selectedPart == BodyPart.None) { // Delete selected object

				//UpdateDeletedObjects();

				BodyComponent joint = GetHoveringObject<Joint>(joints);
				BodyComponent bone = GetHoveringObject<Bone>(bones);
				BodyComponent muscle = GetHoveringObject<Muscle>(muscles);
	
				BodyComponent toDelete = joint != null ? joint : ( bone != null ? bone : muscle ) ;

				if (toDelete != null) {

					toDelete.Delete();
					UpdateDeletedObjects();

					Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
				}

				//UpdateDeletedObjects();

			}

		} else if (Input.GetMouseButton(0)) {
			// Mouse click & hold
			if (selectedPart == BodyPart.Bone ) {

				if (currentBone != null) {
					// check if user is hovering over an ending joint which is not the same as the starting
					// joint of the currentBone
					Joint joint = GetHoveringObject<Joint>(joints);
					Vector3 endingPoint = ScreenToWorldPoint(Input.mousePosition);

					if (joint != null && !joint.Equals(currentBone.startingJoint)) {
						endingPoint = joint.center;
						currentBone.endingJoint = joint;
					} 

					PlaceConnectionBetweenPoints(currentBone.gameObject, currentBone.startingPoint, endingPoint, CONNECTION_WIDHT);	
				}	

			} else if (selectedPart == BodyPart.Muscle) {

				if (currentMuscle != null) {
					// check if user is hovering over an ending joint which is not the same as the starting
					// joint of the currentMuscle
					Bone bone = GetHoveringObject<Bone>(bones);
					Vector3 endingPoint = ScreenToWorldPoint(Input.mousePosition);

					if(bone != null) {
						
						MuscleJoint joint = bone.muscleJoint;

						if (!joint.Equals(currentMuscle.startingJoint)) {
							endingPoint = joint.position;
							currentMuscle.endingJoint = joint;	
						} else {
							currentMuscle.endingJoint = null;
						}
					} else {
						currentMuscle.endingJoint = null;
					}

					//PlaceConnectionBetweenPoints(currentMuscle.gameObject, currentMuscle.startingPoint, endingPoint, CONNECTION_WIDHT);
					currentMuscle.SetLinePoints(currentMuscle.startingPoint, endingPoint);
				}
			}

		} else if ( Input.GetMouseButtonUp(0) ) {

			if (selectedPart == BodyPart.Bone ) {
				
				PlaceCurrentBone();	
			} else if (selectedPart == BodyPart.Muscle) {

				PlaceCurrentMuscle();
			}
		} 

	}

	/** Handles all possible keyboard controls / shortcuts. */
	private void HandleKeyboardInput() {

		if (saveDialog != null && saveDialog.gameObject.activeSelf) return;

		if (Input.anyKeyDown) {
		
			// J = place Joint
			if (Input.GetKeyDown(KeyCode.J)) {
				SelectedPart = BodyPart.Joint;
			}

			// B = place body connection
			else if (Input.GetKeyDown(KeyCode.B)) {
				SelectedPart = BodyPart.Bone;
			}

			// M = place muscle
			else if (Input.GetKeyDown(KeyCode.M)) {
				SelectedPart = BodyPart.Muscle;
			}

			// D = Delete component
			else if (Input.GetKeyDown(KeyCode.D)) {
				SelectedPart = BodyPart.None;
			}

			// S = Save Creature
			else if (Input.GetKeyDown(KeyCode.S) && Application.platform != RuntimePlatform.WebGLPlayer) {
				//SaveCreature();
				PromptCreatureSave();
			}

			// L = Load Creature
			else if (Input.GetKeyDown(KeyCode.L)) {
				//LoadCreature();
			}

			// E = Go to Evolution Scene
			else if (Input.GetKeyDown(KeyCode.E)) {
				Evolve();
			}


			buttonManager.selectButton(selectedPart);
		}
	}

	/**
	 * Sets the shouldHighlight variable appropiately on every hoverable object
	 * depending on the currently selected part.
	 */
	private void UpdateHoverables() {

		SetMouseHoverTexture(null);

		if (selectedPart == BodyPart.Joint ) {
			// disable Highlights
			DisableAllHoverables();
		} else if (selectedPart == BodyPart.Bone ) {
			// make the joints hoverable
			SetShouldHighlight(joints, true);
			SetShouldHighlight(bones, false);
			SetShouldHighlight(muscles, false);
		} else if (selectedPart == BodyPart.Muscle ) {
			// make the bones hoverables
			SetShouldHighlight(joints, false);
			SetShouldHighlight(bones, true);
			SetShouldHighlight(muscles, false);
		} else if (selectedPart == BodyPart.None) {
			// make everything highlightable
			SetShouldHighlight(joints, true);
			SetShouldHighlight(bones, true);
			SetShouldHighlight(muscles, true);

			SetMouseHoverTexture(mouseDeleteTexture);
		}
	}

	/** Removes the already destroyed object that are still left in the lists. */
	private void UpdateDeletedObjects() {

		bones = UpdateDeletedObjects<Bone>(bones);
		joints = UpdateDeletedObjects<Joint>(joints);
		muscles = UpdateDeletedObjects<Muscle>(muscles);
	}

	/** Removes the already destroyed object that are still left in the list. */
	private List<T> UpdateDeletedObjects<T>(List<T> objects) where T: BodyComponent {

		List<T> removed = new List<T>(objects);
		foreach (T obj in objects) {
			if (obj == null || obj.Equals(null) || obj.gameObject == null || obj.gameObject.Equals(null) || obj.deleted) {
				//print("Removed component of type: " + typeof(T));
				removed.Remove(obj);
			}
		}
		return removed;
	}

	/// <summary>
	/// Deletes the currently visible creature.
	/// </summary>
	private void DeleteCreature() {
		foreach(var joint in joints) {
			joint.Delete();
		}

		UpdateDeletedObjects();
	}

	private void DisableAllHoverables() {

		SetShouldHighlight(joints, false);
		SetShouldHighlight(bones, false);
		SetShouldHighlight(muscles, false);
	}

	private void ResetHoverableColliders() {

		ResetHoverableColliders(joints);
		ResetHoverableColliders(bones);
	}

	private void ResetHoverableColliders<T>(List<T> hoverables) where T: Hoverable {

		foreach (Hoverable hov in hoverables) {

			hov.ResetHitbox();
		}
	} 

	private void SetMouseHoverTexture(Texture2D texture) {

		foreach (Joint joint in joints) {
			joint.mouseHoverTexture = texture;
		}
		foreach (Bone bone in bones) {
			bone.mouseHoverTexture = texture;
		}
		foreach (Muscle muscle in muscles) {
			muscle.mouseHoverTexture = texture;
		}
	}

	/** 
	 * Sets the shouldHighlight property of a Hoverable script entry in the specified list to the boolean value specified by
	 * shouldHighlight.
	*/
	private void SetShouldHighlight<T>(List<T> hoverables, bool shouldHighlight) where T: Hoverable {

		foreach (var obj in hoverables) {

			obj.shouldHighlight = shouldHighlight;
		}
	} 

	/** 
	 * Returns the Object that the mouse is currently hovering over or null if there is no such object in the given list. 
	 * The list contains scripts that are attached to gameobjects which have a HoverableScript attached.
	*/
	private T GetHoveringObject<T>(List<T> objects) where T: MonoBehaviour {

		foreach (T obj in objects) {
			
			if (obj.gameObject.GetComponent<Hoverable>().hovering) {
				return obj;
			}
		}

		return null;
	}


	/** Placed a new joint object at the specified point. */
	private void PlaceJoint(Vector3 point) {

		point.z = 0;

		//GameObject joint = (GameObject) Instantiate(jointPreset, point, Quaternion.identity);
		//joints.Add(joint.GetComponent<Joint>());
		joints.Add(Joint.CreateAtPoint(point));
	}

	/** Instantiates a muscle at the specified point. */
	private void CreateMuscleFromJoint(MuscleJoint joint) {

		Vector3 point = joint.position;
		point.z = 0;

		/*GameObject muscleEmpty = new GameObject();
		muscleEmpty.name = "Muscle";
		currentMuscle = muscleEmpty.AddComponent<Muscle>();
		currentMuscle.AddLineRenderer();
		currentMuscle.SetMaterial(muscleMaterial);*/
		//currentMuscle = ((GameObject) Instantiate(musclePreset, point, Quaternion.identity)).GetComponent<Muscle>();
		currentMuscle = Muscle.Create();
		currentMuscle.startingJoint = joint;
		currentMuscle.SetLinePoints(joint.position, joint.position);
	}

	/** Transforms the given gameObject between the specified points. (Points flattened to 2D). */
	public static void PlaceConnectionBetweenPoints(GameObject connection, Vector3 start, Vector3 end, float width) {

		// flatten the vectors to 2D
		start.z = 0;
		end.z = 0;

		Vector3 offset = end - start;
		Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
		Vector3 position = start + (offset / 2.0f);


		connection.transform.position = position;
		connection.transform.up = offset;
		connection.transform.localScale = scale;
	}

	/** Instantiates a bone at the specified point. */
	private void CreateBoneFromJoint(Joint joint){

		Vector3 point = joint.center;
		point.z = 0;
		//currentBone = ((GameObject) Instantiate(bonePreset, point, Quaternion.identity)).GetComponent<Bone>();
		currentBone = Bone.CreateAtPoint(point);
		currentBone.startingJoint = joint;
	}

	/** Places the currentBone between the specified points. (Points flattened to 2D) */
	private void PlaceBoneBetweenPoints(Vector3 start, Vector3 end, float width) {

		// flatten the vectors to 2D
		start.z = 0;
		end.z = 0;

		Vector3 offset = end - start;
		Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
		Vector3 position = start + (offset / 2.0f);


		currentBone.transform.position = position;
		currentBone.transform.up = offset;
		currentBone.transform.localScale = scale;

	}

	/** 
	 * Checks to see if the current bone is valid (attached to two joints) and if so
	 * adds it to the list of bones.
	 */
	private void PlaceCurrentBone() {

		if (currentBone == null) return;

		if (currentBone.endingJoint == null || GetHoveringObject<Joint>(joints) == null || currentBone.endingJoint.Equals(currentBone.startingJoint)) {
			// The connection has no connected ending -> Destroy
			Destroy(currentBone.gameObject);
		
		} else {
			currentBone.ConnectToJoints();
			bones.Add(currentBone);
		}

		currentBone = null;
	} 

	/** 
	 * Checks to see if the current muscle is valid (attached to two joints) and if so
	 * adds it to the list of muscles.
	 */
	private void PlaceCurrentMuscle() {
			
		if (currentMuscle == null) return;

		if (currentMuscle.endingJoint == null || GetHoveringObject<Bone>(bones) == null) {
			// The connection has no connected ending -> Destroy
			Destroy(currentMuscle.gameObject);
		} else {

			// Validate the muscle doesn't exist already
			foreach (Muscle muscle in muscles) {
				if (muscle.Equals(currentMuscle)) {
					Destroy(currentMuscle.gameObject);
					currentMuscle = null;
					return;
				}
			}

			currentMuscle.ConnectToJoints();
			currentMuscle.AddCollider();
			muscles.Add(currentMuscle);
		}

		currentMuscle = null;
	}

	public void SetBodyComponents(List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {
		this.joints = joints;
		this.bones = bones;
		this.muscles = muscles;
	}

	private Vector3 ScreenToWorldPoint(Vector3 point) {
		return Camera.main.ScreenToWorldPoint(point);
	}

	/** Returns the gameobject that consists of the bodyparts that have been placed in the scene */
	public Creature BuildCreature() {

		GameObject creatureObj = new GameObject();
		creatureObj.name = "Creature";
		Creature creature = creatureObj.AddComponent<Creature>();

		foreach (Joint joint in joints) {
			joint.transform.SetParent(creatureObj.transform);
		}

		foreach (Bone connection in bones) {
			connection.transform.SetParent(creatureObj.transform);
		}

		foreach (Muscle muscle in muscles) {
			muscle.transform.SetParent(creatureObj.transform);
		}

		creature.joints = joints;
		creature.bones = bones;
		creature.muscles = muscles;

		DisableAllHoverables();

		return creature;
	} 

	/** Generates a creature and brings it to the testScene. */
	public void TakeCreatureToTestScene() {

		/*Creature creature = BuildCreature();
		DontDestroyOnLoad(creature.gameObject);

		SceneManager.LoadScene("TestingScene");*/
	}

	/// <summary>
	/// Creates the creature and attached it to the evolution.
	/// </summary>
	public void AttachCreatureToEvolution(Evolution evolution) {

		ResetHoverableColliders();

		CreatureSaver.SaveCurrentCreature(lastCreatureName, joints, bones, muscles);

		Creature creature = BuildCreature();
		DontDestroyOnLoad(creature.gameObject);

		evolution.creature = creature;
	}

	/** Generates a creature and starts the evolution simulation. */
	public void Evolve() {

		// don't attempt evolution if there is no creature
		if (joints.Count == 0) return;

		ResetHoverableColliders();

		CreatureSaver.SaveCurrentCreature(lastCreatureName, joints, bones, muscles);

		Creature creature = BuildCreature();
		DontDestroyOnLoad(creature.gameObject);

		//SceneManager.LoadScene("EvolutionScene");
		AsyncOperation sceneLoading = SceneManager.LoadSceneAsync("EvolutionScene");
		sceneLoading.allowSceneActivation = true;
		DontDestroyOnLoad(evolution.gameObject);
		evolution.creature = creature;

		evolution.PopulationSize = buttonManager.GetPopulationInput();
		evolution.SimulationTime = buttonManager.GetSimulationTime();

		Evolution.task = buttonManager.GetTask();

		StartCoroutine(WaitForEvolutionSceneToLoad(sceneLoading));
		DontDestroyOnLoad(this);
		//evolution.StartEvolution();
	}

	IEnumerator WaitForEvolutionSceneToLoad(AsyncOperation loadingOperation) {

		while(!loadingOperation.isDone){

			//print(loadingOperation.progress);
			yield return null;
		}
			
		Destroy(this.gameObject);
		print("Starting Evolution");
		evolution.StartEvolution();
	}

	/// <summary>
	/// Changes to the evolution scene for continuing a saved simulation.
	/// </summary>
	public void ContinueEvolution(Evolution evolution, Action completion) {

		AttachCreatureToEvolution(evolution);

		AsyncOperation sceneLoading = SceneManager.LoadSceneAsync("EvolutionScene");
		sceneLoading.allowSceneActivation = true;
		DontDestroyOnLoad(evolution.gameObject);

		StartCoroutine(WaitForEvolutionSceneToLoadForLoad(sceneLoading, completion));
		DontDestroyOnLoad(this);
	}

	IEnumerator WaitForEvolutionSceneToLoadForLoad(AsyncOperation loadingOperation, Action completion) {

		while(!loadingOperation.isDone){

			//print(loadingOperation.progress);
			yield return null;
		}

		Destroy(this.gameObject);
		print("Continuing Evolution");

		completion();

	}


	public void PromptCreatureSave() {
		saveDialog.gameObject.SetActive(true);
		saveDialog.ResetErrors();
	}

	/// <summary>
	/// Attempts to save the current creature. Shows an error screen if something went wrong.
	/// </summary>
	/// <param name="name">Name.</param>
	public void SaveCreature(string name) {

		saveDialog.ResetErrors();

		if (name == "") {
			saveDialog.ShowErrorMessage("The Creature name is empty.");
			return;
		}

		try {
			CreatureSaver.WriteSaveFile(name, joints, bones, muscles);

		} catch (IllegalFilenameException e) {
			saveDialog.ShowErrorMessage("The name can't contain . (dots) or _ (underscores).");
			return;
		}

		// The save was successful
		saveDialog.gameObject.SetActive(false);

		buttonManager.Refresh();

	}

	public void LoadCreature(Dropdown dropDown) {

		DeleteCreature();

		//var name = CreatureSaver.GetCreatureNames()[dropDown.value];
		var name = ButtonManager.CreateDropDownOptions()[dropDown.value];
		lastCreatureName = name;

		// The first option in the Dropdown list is going to be an empty creature
		if (dropDown.value == 0) {
			return;
		}

		CreatureSaver.LoadCreature(name, this);
	}

	private void TestCreatureSave() {

		var filename = "Testfile.txt";
		var saveFolder = "CreatureSaves";
		var path = Path.Combine(Application.dataPath, saveFolder);
		path = Path.Combine(path, filename);

		if (File.Exists(path)) {
			var reader = new StreamReader(path);
			var contents = reader.ReadToEnd();
			reader.Close();

			print("The file exists. Content: \n" + contents);
		} else {
			File.WriteAllText(path, "Example Text");
			print("The file was created.");
		}

	} 


}
