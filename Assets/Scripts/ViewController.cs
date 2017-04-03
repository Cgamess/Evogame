﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewController : MonoBehaviour {

	[SerializeField] private Text generationLabel;
	[SerializeField] private Text EVOLGenerationLabel;

	[SerializeField] private Text BCGenerationLabel;
	[SerializeField] private InputField BCInputField;

	[SerializeField] private Text BCErrorMessage;
	private Color ErrorMessageColor;
	private Coroutine FadeRoutine;

	[SerializeField] private Text FitnessLabel;

	[SerializeField] private GameObject AutoplaySettings;
	[SerializeField] private Text AutoplayDurationLabel;

	private Evolution evolution;

	// Use this for initialization
	void Start () {

		evolution = GameObject.Find("Evolution").GetComponent<Evolution>();

		ErrorMessageColor = BCErrorMessage.color;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdateGeneration(int generation) {
		
		var text = string.Format("Generation {0}", generation);

		generationLabel.text = text;
		EVOLGenerationLabel.text = text;
	}

	public void UpdateBCGeneration(int generation) {
		
		BCGenerationLabel.text = string.Format("Best of Gen. {0}", generation);
		BCInputField.text = generation.ToString();
	}

	public void UpdateFitness(float fitness) {

		var percentageFitness = Mathf.Round(fitness * 10000f) / 100f;
		FitnessLabel.text = string.Format("Fitness: {0}%", percentageFitness);
	}

	public void ShowErrorMessage(string message) {
		BCErrorMessage.gameObject.SetActive(true);
		BCErrorMessage.text = string.Format(message);

		if (FadeRoutine != null) StopCoroutine(FadeRoutine);

		StartCoroutine(WaitBeforeErrorFadeOut(1));
	}

	public void HideErrorMessage() {
		
		BCErrorMessage.gameObject.SetActive(false);

		if (FadeRoutine != null) {
			StopCoroutine(FadeRoutine);
		}
	}

	IEnumerator FadeOutErrorMessage(float duration) {

		float start = Time.time;
		float elapsed = 0f;

		while (elapsed < duration) {

			elapsed = Time.time - start;
			float normalizedTime = Mathf.Clamp(elapsed / duration, 0, 1);

			BCErrorMessage.color = Color.Lerp(ErrorMessageColor, Color.clear, normalizedTime);

			yield return null;
		}

		BCErrorMessage.gameObject.SetActive(false);
		BCErrorMessage.color = ErrorMessageColor;
	}

	IEnumerator WaitBeforeErrorFadeOut(int seconds) {

		yield return new WaitForSeconds(seconds);

		FadeRoutine = StartCoroutine(FadeOutErrorMessage(3.5f));
	}

	public void FocusOnNextCreature() {
		evolution.FocusOnNextCreature();
	}

	public void FocusOnPreviousCreature() {
		evolution.FocusOnPreviousCreature();
	}

	public void ViewAutoPlaySettings (bool active){
		
		AutoplaySettings.gameObject.SetActive(active);
	}

	public void UpdateAutoPlayDurationLabel(float duration) {
		AutoplayDurationLabel.text = string.Format("Duration {0}s", duration);
	}

	public void GoBackToCreatureBuilding() {
		evolution.GoBackToCreatureBuilding();
	}
}
