﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public interface SaveDialogDelegate {
	void DidConfirmSave(SaveDialog dialog, string name);
	bool CanEnterCharacter(SaveDialog dialog, int index, char c);
	void DidChangeValue(SaveDialog dialog, string value);
}

public class SaveDialog : MonoBehaviour {

	public static SaveDialog shared;

	public SaveDialogDelegate Delegate { get; set; }

	[SerializeField] private InputField inputField;

	[SerializeField] private Text errorMessage;

	void Awake() {
		if (shared == null || shared == this) {
			shared = this;
		} else {
			Destroy(this.gameObject);
		}
	}

	public void Show(SaveDialogDelegate Delegate) {
		gameObject.SetActive(true);
		this.Delegate = Delegate;
		ResetErrors();
		KeyInputManager.shared.Register();

		inputField.onValidateInput += delegate (string input, int charIndex, char addedChar) {
			if (Delegate.CanEnterCharacter(this, charIndex, addedChar)) {
				return addedChar;
			} else {
				return '\0';
			}
		};

		inputField.onValueChanged.AddListener(delegate {
			
			Delegate.DidChangeValue(this, inputField.text);
		});
	}

	public void Close() {
		Delegate = null;
		KeyInputManager.shared.Deregister();
		gameObject.SetActive(false);
	}

	public void OnSaveClicked() {
		errorMessage.enabled = false;

		Delegate.DidConfirmSave(this, inputField.text);
	}

	public void OnCancelClicked() {
		ResetErrors();
		Close();
	}

	public void ShowErrorMessage(string message) {
		errorMessage.text = message;
		errorMessage.enabled = true;
	}

	public void ResetErrors() {
		errorMessage.enabled = false;
	}
}
