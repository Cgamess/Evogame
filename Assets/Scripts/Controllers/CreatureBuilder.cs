﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class CreatureBuilder {

	/// <summary>
	/// A counter used to assign a unique id to each body component created with this CreatureBuilder.
	/// Needs to be incremented after each use. Can be reset if needed. Starts at 0.
	/// This needs to be updated when a creature design is loaded!
	/// </summary>
	private int idCounter = 0;

	public string Name { get; set; } = "Unnamed";

	/// <summary>
	/// The joints of the creature that have been placed in the scene.
	/// </summary>
	private List<Joint> joints = new List<Joint>();	
	/// <summary>
	/// The bones that have been placed in the scene.
	/// </summary>
	private List<Bone> bones = new List<Bone>();
	/// <summary>
	/// The muscles that have been placed in the scene.
	/// </summary>
	private List<Muscle> muscles = new List<Muscle>();

	/// <summary> 
	/// The Bone that is currently being placed. 
	/// </summary>
	private Bone currentBone;

	/// <summary>
	/// The Muscle that is currently being placed.
	/// </summary>
	private Muscle currentMuscle;

	/// <summary>
	/// The minimum distance between two joints when they are placed 
	/// (Can be moved closer together using "Move").
	/// </summary>
	private float jointNonOverlapRadius = 0.6f;

	/// <summary>
	/// The bone thickness.
	/// </summary>
	public static float CONNECTION_WIDTH = 0.5f;

	public CreatureBuilder() {}

	public CreatureBuilder(CreatureDesign design) {

		this.Name = design.Name;

		foreach (var jointData in design.Joints) {
			this.joints.Add(Joint.CreateFromData(jointData));
		}

		foreach (var boneData in design.Bones) {
			CreateBoneFromData(boneData);	
		}

		foreach (var muscleData in design.Muscles) {
			CreateMuscleFromData(muscleData);
		}

		// Update the idCounter
		int maxJointID = design.Joints.Max(data => data.id);
		int maxBoneID = design.Bones.Max(data => data.id);
		int maxMuscleID = design.Muscles.Max(data => data.id);
		idCounter = Mathf.Max(Mathf.Max(maxJointID, maxBoneID), maxMuscleID) + 1;
	}

	
	#region Joint Placement

	/// <summary>
	/// Attempts to place a joint at the specified world position.
	/// </summary>
	/// <remarks>Returns whether a joint was placed.</remarks>
	public bool TryPlacingJoint(Vector3 position) {

		// Make sure the joint doesn't overlap another one
		bool noOverlap = true;
		foreach (var joint in joints) {
			if ((joint.center - position).magnitude < jointNonOverlapRadius) {
				noOverlap = false;
				break;
			}
		}

		if (noOverlap) {
			PlaceJoint(position);
		}

		return noOverlap;
	}

	/// <summary>
	/// Places a new joint at the specified point.
	/// </summary>
	private void PlaceJoint(Vector3 position) {

		var jointData = new JointData(idCounter++, position, 1f);
		joints.Add(Joint.CreateFromData(jointData));
	}

	#endregion
	#region Bone Placement

	/// <summary>
	/// Attempts to place a bone beginning at the current position.
	/// </summary>
	public void TryStartingBone(Joint joint) {
		
		if (joint != null) {

			CreateBoneFromJoint(joint);
			PlaceConnectionBetweenPoints(currentBone.gameObject, joint.center, joint.center, CONNECTION_WIDTH);
		}
	}

	/// <summary>
	/// Instantiates a bone at the specified point.
	/// </summary>
	private void CreateBoneFromJoint(Joint joint){
		
		// TODO: Keep Muscle joint weight in mind
		var boneData = new BoneData(idCounter++, joint.JointData.id, joint.JointData.id, 2f);
		currentBone = Bone.CreateAtPoint(joint.center, boneData);
		currentBone.startingJoint = joint;
	}

	private void CreateBoneFromData(BoneData data) {
		// Find the connecting joints
		var startingJoint = FindJointWithId(data.startJointID);
		var endingJoint = FindJointWithId(data.endJointID);
		if (startingJoint == null || endingJoint == null) {
			Debug.Log(string.Format("The connecting joints for bone {0} were not found!", data.id));
			return;
		}
		var bone = CreateBoneBetween(startingJoint, endingJoint, data);
		bone.ConnectToJoints();
		bones.Add(bone);
	}

	private Bone CreateBoneBetween(Joint startingJoint, Joint endingJoint, BoneData data) {

		var bone = Bone.CreateAtPoint(startingJoint.center, data);
		PlaceConnectionBetweenPoints(bone.gameObject, startingJoint.center, endingJoint.center, CONNECTION_WIDTH);
		bone.startingJoint = startingJoint;
		bone.endingJoint = endingJoint;
		return bone;
	}

	/// <summary>
	/// Updates the bone that is currently being placed to end at the 
	/// current mouse/touch position.
	/// </summary>
	public void UpdateCurrentBoneEnd(Vector3 position, Joint hoveringJoint) {

		if (currentBone != null) {
			// check if user is hovering over an ending joint which is not the same as the starting
			// joint of the currentBone
			var endPoint = position;

			if (hoveringJoint != null && !hoveringJoint.Equals(currentBone.startingJoint)) {
				endPoint = hoveringJoint.center;
				currentBone.endingJoint = hoveringJoint;
				var oldData = currentBone.BoneData;
				var newData = new BoneData(oldData.id, oldData.startJointID, hoveringJoint.JointData.id, oldData.weight);
				currentBone.BoneData = newData;
			} 

			PlaceConnectionBetweenPoints(currentBone.gameObject, currentBone.startingPoint, endPoint, CONNECTION_WIDTH);	
		}	
	}

	/// <summary>
	/// Transforms the given gameObject between the specified points. 
	/// (Points are flattened to 2D).
	/// </summary>
	/// <param name="connection">The object to place as between the start and end point</param>
	/// <param name="width">The thickness of the connection.</param>
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

	/// <summary>
	/// Checks to see if the current bone is valid (attached to two joints) and if so 
	/// adds it to the list of bones.
	/// </summary>
	/// <returns>Returns whether the current bone was placed.</returns>
	public bool PlaceCurrentBone() {

		if (currentBone == null) return false;

		if (currentBone.endingJoint == null || 
			currentBone.endingJoint.Equals(currentBone.startingJoint)) {
			// The connection has no connected ending -> Destroy
			UnityEngine.Object.Destroy(currentBone.gameObject);
			currentBone = null;
			return false;
		} else {
			currentBone.ConnectToJoints();
			bones.Add(currentBone);
			currentBone = null;
			// The creature was modified
			return true;
		}
	} 

	#endregion
	#region Muscle Placement

	/// <summary>
	/// Attempts to place a muscle starting at the specified position.
	/// </summary>
	public void TryStartingMuscle(Bone bone) {

		// find the selected bone
		// Bone bone = HoveringUtil.GetHoveringObject<Bone>(bones);

		if (bone != null) {
			CreateMuscleFromBone(bone);
			var startPos = bone.muscleJoint.transform.position;
			currentMuscle.SetLinePoints(startPos, startPos);
		}
	}

	/// <summary>
	/// Instantiates a muscle at the specified point.
	/// </summary>
	private void CreateMuscleFromBone(Bone bone) {

		var muscleData = new MuscleData(idCounter++, bone.BoneData.id, bone.BoneData.id, Muscle.Defaults.MaxForce, true);
		currentMuscle = Muscle.CreateFromData(muscleData);
		var muscleJoint = bone.muscleJoint;
		currentMuscle.startingJoint = muscleJoint;
		currentMuscle.SetLinePoints(muscleJoint.transform.position, muscleJoint.transform.position);
	}

	/// <summary>
	/// Updates the muscle that is currently being placed to end at the 
	/// specified position
	/// </summary>
	public void UpdateCurrentMuscleEnd(Vector3 endPoint, Bone hoveringBone) {

		if (currentMuscle != null) {
			// Check if user is hovering over an ending joint which is not the same as the starting
			// joint of the currentMuscle
			var endingPoint = endPoint;

			if (hoveringBone != null) {
				
				MuscleJoint joint = hoveringBone.muscleJoint;

				if (!joint.Equals(currentMuscle.startingJoint)) {
					endingPoint = joint.transform.position;
					currentMuscle.endingJoint = joint;	
					var oldData = currentMuscle.MuscleData;
					var newData = new MuscleData(
						oldData.id, oldData.startBoneID, hoveringBone.BoneData.id, 
						oldData.strength, oldData.canExpand
					); 
					currentMuscle.MuscleData = newData;
				} else {
					currentMuscle.endingJoint = null;
				}
			} else {
				currentMuscle.endingJoint = null;
			}

			currentMuscle.SetLinePoints(currentMuscle.startingJoint.transform.position, endingPoint);
		}
	}

	/// <summary>
	/// Checks to see if the current muscle is valid (attached to two joints) and if so
	/// adds it to the list of muscles.
	/// </summary>
	/// <returns>Returns whether the current muscle was placed</returns>
	public bool PlaceCurrentMuscle() {
			
		if (currentMuscle == null) return false;

		if (currentMuscle.endingJoint == null) {
			// The connection has no connected ending -> Destroy
			UnityEngine.Object.Destroy(currentMuscle.gameObject);
			currentMuscle = null;
			return false;
		} else {

			// Validate the muscle doesn't exist already
			foreach (Muscle muscle in muscles) {
				if (muscle.Equals(currentMuscle)) {
					UnityEngine.Object.Destroy(currentMuscle.gameObject);
					currentMuscle = null;
					return false;
				}
			}

			currentMuscle.ConnectToJoints();
			currentMuscle.AddCollider();
			muscles.Add(currentMuscle);
			currentMuscle = null;
			// The creature was modified
			return true;
		}
	}

	private void CreateMuscleFromData(MuscleData data) {
		// Find the connecting joints
		var startingBone = FindBoneWithId(data.startBoneID);
		var endingBone = FindBoneWithId(data.endBoneID);
		
		if (startingBone == null || endingBone == null) {
			Debug.Log(string.Format("The connecting joints for bone {0} were not found!", data.id));
			return;
		}
		var muscle = CreateMuscleBetween(startingBone, endingBone, data);
		muscle.ConnectToJoints();
		muscle.AddCollider();
		muscles.Add(muscle);
	}

	private Muscle CreateMuscleBetween(Bone startingBone, Bone endingBone, MuscleData data) {

		var muscle = Muscle.CreateFromData(data);
		var startJoint = startingBone.muscleJoint;
		var endJoint = endingBone.muscleJoint;

		muscle.startingJoint = startJoint;
		muscle.endingJoint = endJoint;

		muscle.SetLinePoints(startJoint.transform.position, endJoint.transform.position);
		return muscle;
	}

	#endregion
	#region Move

	/// <summary>
	/// Moves the currently selected components to the specified position.
	/// </summary>
	public void MoveSelection(ICollection<Joint> jointsToMove, Vector3 movement) {

		foreach (var joint in jointsToMove) {
			var newPosition = joint.transform.position + movement;
			joint.MoveTo(newPosition);		
		}
	}

	/// <summary>
	/// Resets all properties used while moving a body component.
	/// </summary>
	/// <returns>Returns whether the creature design was changed.</returns>
	public bool MoveEnded(ICollection<Joint> jointsToMove) {

		if (jointsToMove == null) return false;

		var didChange = jointsToMove.Count > 0;
		if (didChange) {
			foreach (var joint in jointsToMove) {
				var oldData = joint.JointData;
				var newData = new JointData(oldData.id, joint.center, oldData.weight);
				joint.JointData = newData;
			}
		}
		return didChange;
	}

	#endregion

	public void Reset() {
		DeleteAll();
		idCounter = 0;
	}

	/// <summary>
	/// Deletes all body components placed with this builder.
	/// </summary>
	public void DeleteAll() {

		// Deleting joints will recursively delete all connected 
		// body parts as well
		foreach (var joint in joints) {
			joint.Delete();
		}

		RemoveDeletedObjects();
		idCounter = 0;
	}

	public void Delete(List<BodyComponent> selection) {

		for (int i = 0; i < selection.Count; i++) {
			var item = selection[i];
			if (item == null) continue;

			item.Delete();
			
		}
		RemoveDeletedObjects();
	}

	/// <summary>
	/// Deletes the placed body components that are currently being
	/// hovered over.
	/// </summary>
	/// <returns>Returns whether the creature design was modified.</returns>
	public bool DeleteHoveringBodyComponent() {

		BodyComponent joint = HoveringUtil.GetHoveringObject<Joint>(joints);
		BodyComponent bone = HoveringUtil.GetHoveringObject<Bone>(bones);
		BodyComponent muscle = HoveringUtil.GetHoveringObject<Muscle>(muscles);

		BodyComponent toDelete = joint != null ? joint : ( bone != null ? bone : muscle ) ;

		if (toDelete != null) {
			toDelete.Delete();
			RemoveDeletedObjects();
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			// The creature was modified
			return true;
		} else {
			return false;
		}
	}

	public void SetBodyComponents(List<Joint> joints, List<Bone> bones, List<Muscle> muscles) {
		this.joints = joints;
		this.bones = bones;
		this.muscles = muscles;
	}

	/// <summary>
	/// Creates a Creature object from the currently placed bodyparts.
	/// </summary>
	public Creature Build() {

		ResetHoverableColliders();

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

		return creature;
	}

	/// <summary>
	/// Returns a CreatureDesign representation of the currently placed
	/// body parts 
	/// </summary>
	public CreatureDesign GetDesign() {
		
		var name = this.Name;
		var jointData = this.joints.Select(j => j.JointData).ToList();
		var boneData = this.bones.Select(b => b.BoneData).ToList();
		var muscleData = this.muscles.Select(m => m.MuscleData).ToList();
		return new CreatureDesign(name, jointData, boneData, muscleData);
	}

	#region Hover Configuration

	/// <summary>
	/// Enabled / Disables highlighting on hover for the specified body components
	/// </summary>
	public void EnableHighlighting(bool joint, bool bone, bool muscle) {

		HoveringUtil.SetShouldHighlight(this.joints, joint);
		HoveringUtil.SetShouldHighlight(this.bones, bone);
		HoveringUtil.SetShouldHighlight(this.muscles, muscle);
	}

	/// <summary>
	/// Sets the specified mouse hover texture on all body components created 
	/// by this builder.
	/// </summary>
	public void SetMouseHoverTextures(Texture2D texture) {

		HoveringUtil.SetMouseHoverTexture(joints, texture);
		HoveringUtil.SetMouseHoverTexture(bones, texture);
		HoveringUtil.SetMouseHoverTexture(muscles, texture);
	}

	/// <summary>
	/// Resets the hoverable colliders on joints and bones.
	/// </summary>
	private void ResetHoverableColliders() {

		HoveringUtil.ResetHoverableColliders(joints);
		HoveringUtil.ResetHoverableColliders(bones);
	}

	#endregion

	#region Utils

	private Joint FindJointWithId(int id) {
		foreach (var joint in joints) {
			if (joint.JointData.id == id) {
				return joint;
			}
		}
		return null;
	}

	private Bone FindBoneWithId(int id) {
		foreach (var bone in bones) {
			if (bone.BoneData.id == id) {
				return bone;
			} 
		}
		return null;
	}

	/// <summary>
	/// Removes the already destroyed object that are still left in the lists.
	/// </summary>
	private void RemoveDeletedObjects() {

		bones = BodyComponent.RemoveDeletedObjects<Bone>(bones);
		joints = BodyComponent.RemoveDeletedObjects<Joint>(joints);
		muscles = BodyComponent.RemoveDeletedObjects<Muscle>(muscles);
	}

	#endregion
}
