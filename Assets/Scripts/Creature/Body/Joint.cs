﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Joint : BodyComponent {

	private const string PATH = "Prefabs/Joint";

	public Vector3 center { 
		get { return transform.position; } 
	}

	public JointData JointData { get; set; }

	public Rigidbody Body {
		get { return body; }
	}
	private Rigidbody body;

	public bool isCollidingWithGround { get; private set; }
	public bool isCollidingWithObstacle { get; private set; }

	private Dictionary<Bone, UnityEngine.Joint> joints = new Dictionary<Bone, UnityEngine.Joint>();

	private Vector3 resetPosition;
	private Quaternion resetRotation;
	private bool iterating;

	public static Joint CreateFromData(JointData data) {
		
		var joint = ((GameObject) Instantiate(Resources.Load(PATH), data.position, Quaternion.identity)).GetComponent<Joint>();
		joint.JointData = data;
		return joint;
	}

	public static Joint CreateFromString(string data) {
		
		var parts = data.Split('%');
		//print(data);
		// Format: ID - pos.x - pos.y - pos.z
		var x = float.Parse(parts[1]);
		var y = float.Parse(parts[2]);
		var z = float.Parse(parts[3]);

		var id = int.Parse(parts[0]);

		var jointData = new JointData(id, new Vector3(x,y,z), 1f);
		var joint = CreateFromData(jointData);

		return joint;
	}

	public override void Start () {
		base.Start();

		resetPosition = transform.position;
		resetRotation = transform.rotation;

		body = GetComponent<Rigidbody>();
	}

	public void Reset() {
		transform.SetPositionAndRotation(resetPosition, resetRotation);
		body.velocity = Vector3.zero;
	}

	/// <summary>
	/// Moves the joint to the specified position and updates all of the connected objects.
	/// </summary>
	public void MoveTo(Vector3 newPosition) {

		transform.position = newPosition;

		foreach (var connectedBone in joints.Keys) {
			connectedBone.RefreshBonePlacement();
		}
	}

	public void Connect(Bone bone) {

		HingeJoint joint = gameObject.AddComponent<HingeJoint>();
		//ConfigurableJoint joint = gameObject.AddComponent<ConfigurableJoint>();
		joint.anchor = Vector3.zero;
		joint.axis = new Vector3(0, 0, 1);
		joint.autoConfigureConnectedAnchor = true;
		joint.useSpring = false;
		//var spring = joint.spring;
		//spring.spring = 1000f;
		//joint.spring = spring;
		//joint.connectedAnchor = new Vector3(0, 1.14f, 0);
		joint.enablePreprocessing = true;
		joint.enableCollision = false;

		joint.connectedBody = bone.gameObject.GetComponent<Rigidbody>();

		joints.Add(bone, joint);
	}

	/** Disconnects the bone from the joint. */
	public void Disconnect(Bone bone) {

		UnityEngine.Joint joint = joints[bone];
		Destroy(joint);
		if (!iterating)
			joints.Remove(bone);
	}
		
	/// <summary>
	/// Deletes the joint and all attached objects from the scene.
	/// </summary>
	public override void Delete() {
		base.Delete();

		iterating = true;

		List<Bone> toDelete = new List<Bone>();
		// disconnect the bones
		foreach(Bone bone in joints.Keys) {

			bone.Delete();
			toDelete.Add(bone);
		}

		foreach(Bone bone in toDelete) {
			joints.Remove(bone);
		}

		iterating = false;

		Destroy(gameObject);
	}

	public override void PrepareForEvolution() {

		body = GetComponent<Rigidbody>();
		body.isKinematic = false;
	}

	/// <summary>
	/// Generates a string that holds all the information needed to save and rebuild this BodyComponent.
	/// Format: Own ID % pos.x % pos.y % pos.z
	/// </summary>
	/// <returns>The save string.</returns>
	public override string GetSaveString() {
		// TODO: Fix
		var pos = transform.position;
		return string.Format("{0}%{1}%{2}%{3}", JointData.id, pos.x, pos.y, pos.z);
	}

		
	void OnTriggerEnter(Collider collider) {

		if (collider.CompareTag("Ground")) {
			isCollidingWithGround = true;
		} else if (collider.CompareTag("Obstacle")) {
			isCollidingWithObstacle = true;
		}
	}

	void OnTriggerExit(Collider collider) {

		//if (collider.tag.ToUpper() == "OBSTACLE") 
		if (collider.CompareTag("Obstacle")) 
			isCollidingWithObstacle = false;
		else
			isCollidingWithGround = false;	
	}
}
