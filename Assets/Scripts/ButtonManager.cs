﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ButtonManager : MonoBehaviour {

	public SelectableButton jointButton;
	public SelectableButton boneButton;
	public SelectableButton muscleButton;

	public SelectableButton deleteButton;

	public SelectableButton selectedButton;

	public CreatureBuilder creatureBuilder;

	private Dictionary<SelectableButton, CreatureBuilder.BodyPart> buttonMap;

	// Use this for initialization
	void Start () {

		buttonMap = new Dictionary<SelectableButton, CreatureBuilder.BodyPart>();

		buttonMap.Add(jointButton, CreatureBuilder.BodyPart.Joint);
		buttonMap.Add(boneButton, CreatureBuilder.BodyPart.Bone);
		buttonMap.Add(muscleButton, CreatureBuilder.BodyPart.Muscle);
		buttonMap.Add(deleteButton, CreatureBuilder.BodyPart.None);

		selectedButton.Selected = true;

		foreach (SelectableButton button in buttonMap.Keys) {
			button.manager = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void selectButton(SelectableButton button) {

		if (!button.Equals(selectedButton)) {
			
			button.Selected = true;
			selectedButton.Selected = false;
			selectedButton = button;

			creatureBuilder.SelectedPart = buttonMap[button];
		}
	}

	public void selectButton(CreatureBuilder.BodyPart part) {
		
		foreach ( SelectableButton button in buttonMap.Keys) {
			
			if (buttonMap[button].Equals(part)) {
				selectButton(button);
				break;
			}
		}
	}
}
