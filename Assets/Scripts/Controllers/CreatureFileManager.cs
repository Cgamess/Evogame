﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Keiwando.NativeFileSO;

public class CreatureFileManager : MonoBehaviour, 
								   FileSelectionViewControllerDelegate, 
								   SaveDialogDelegate {

	[SerializeField]
	private CreatureEditor editor;

	[SerializeField]
	private FileSelectionViewController viewController;
	[SerializeField]
	private SaveDialog saveDialog;
	[SerializeField]
	private UIFade importIndicator;

	private int selectedIndex = 0;
	private List<string> creatureNames = new List<string>();

	void Start() {
		NativeFileSOMobile.shared.FilesWereOpened += delegate (OpenedFile[] files) {
			var didImport = false;
			foreach (var file in files) { 
				var extension = file.Extension.ToLower();
				if (extension.Equals(".creat")) {
					// TODO: Validate file contents
					CreatureSaver.SaveCreatureDesign(file.Name, file.ToUTF8String());
					didImport = true;
				}
			}
			RefreshCache();
			viewController.Refresh();
			if (didImport) {
				importIndicator.FadeInOut();
			}
		};
	}

	public void ShowUI() {
		RefreshCache();
		viewController.Show(this);
	}

	public void PromptCreatureSave() {
		saveDialog.Show(this);
	}

	// MARK: - SaveDialogDelegate

	public void DidConfirmSave(SaveDialog dialog, string name) {

		if (string.IsNullOrEmpty(name)) {
			dialog.ShowErrorMessage("Please enter a name for your creature!");
			return;
		}

		try {
			editor.SaveCurrentDesign(name);
		} catch (IllegalFilenameException e) {
			dialog.ShowErrorMessage(string.Format("The creature name cannot contain: {0}", 
			new string(CreatureSaver.INVALID_NAME_CHARACTERS)));
			return;
		}

		dialog.Close();
	}

	public bool CanEnterCharacter(SaveDialog dialog, int index, char c) {
		return !CreatureSaver.INVALID_NAME_CHARACTERS.Contains(c);
	}

	public void DidChangeValue(SaveDialog dialog, string value) {
		if (CreatureSaver.CreatureExists(value)) {
			dialog.ShowErrorMessage(string.Format("The existing save for {0} will be overwritten!", value));
		} else {
			dialog.ResetErrors();
		}
	}

	// MARK: - FileSelectionViewControllerDelegate

	public string GetTitle(FileSelectionViewController controller) {
		return "Creature Designs";
	}

	public string GetEmptyMessage(FileSelectionViewController controller) {
		return "You haven't saved any creature designs yet";
	}

	public int GetNumberOfItems(FileSelectionViewController controller) {
		return creatureNames.Count;
	}

	public string GetTitleForItemAtIndex(FileSelectionViewController controller,
										 int index) {
		return creatureNames[index];
	}
	public int GetIndexOfSelectedItem(FileSelectionViewController controller) {
		return selectedIndex;
	}

	public void DidSelectItem(FileSelectionViewController controller, int index) {
		selectedIndex = index;
	}

	public bool IsCharacterValidForName(FileSelectionViewController controller, char c) {
		return !CreatureSaver.INVALID_NAME_CHARACTERS.Contains(c);
	}

	public bool IsNameAvailable(FileSelectionViewController controller, string newName) {
		return !CreatureSaver.CreatureExists(newName);
	}

	public void DidEditTitleAtIndex(FileSelectionViewController controller, int index, string newName) {
		
		if (!IsNameAvailable(controller, newName)) return;

		var currentName = creatureNames[index];
		CreatureSaver.RenameCreatureDesign(currentName, newName);
		RefreshCache();
	}

	public void LoadButtonClicked(FileSelectionViewController controller) {

		var name = creatureNames[selectedIndex];
		viewController.Close();
		StartCoroutine(LoadOnNextFrame(name));
	}

	private IEnumerator LoadOnNextFrame(string name) {

		yield return new WaitForEndOfFrame();

		// TODO: Get CreatureDesign file
		//editor.LoadDesign(design);
	}

	public void ImportButtonClicked(FileSelectionViewController controller) {

		SupportedFileType[] supportedFileTypes = {
			CustomEvolutionFileType.creat
		};

		NativeFileSO.shared.OpenFiles(supportedFileTypes,
		delegate (bool filesWereOpened, OpenedFile[] files) {
			if (filesWereOpened) {
			  	foreach (OpenedFile file in files) {

					CreatureSaver.SaveCreatureDesign(file.Name, file.ToUTF8String());
				  	RefreshCache();
				  	viewController.Refresh();
					importIndicator.FadeInOut();
			 	}
			}
	  	});
	}

	public void ExportButtonClicked(FileSelectionViewController controller) {

		var name = creatureNames[selectedIndex];
		string path = CreatureSaver.PathToCreatureDesign(name);

		FileToSave file = new FileToSave(path, CustomEvolutionFileType.creat);

		NativeFileSO.shared.SaveFile(file);
	}

	public void DeleteButtonClicked(FileSelectionViewController controller) {
		var name = creatureNames[selectedIndex];
		CreatureSaver.DeleteCreatureSave(name);
		selectedIndex = 0;
		RefreshCache();
	}

	public void DidClose(FileSelectionViewController controller) {
		Reset();
	}

	private void Reset() {
		selectedIndex = 0;
		RefreshCache();
	}

	private void RefreshCache() {
		creatureNames = CreatureSaver.GetCreatureNames();
	}
}
