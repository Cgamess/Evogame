﻿using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using Keiwando.NativeFileSO;

public class SimulationFileManager : MonoBehaviour, FileSelectionViewControllerDelegate {

	[SerializeField]
	private CreatureBuilder creatureBuilder;
	[SerializeField]
	private Evolution evolution;

	[SerializeField]
	private FileSelectionViewController viewController;

	private static readonly Regex RENAME_REGEX = new Regex(".txt|.evol");

	private int selectedIndex = 0;
	private List<string> filenames;

	void Start() {
		NativeFileSOMobile.shared.FilesWereOpened += delegate (OpenedFile[] files) {
			foreach (var file in files) { 
				var extension = file.Extension.ToLower();
				if (extension.Equals(".txt") || extension.Equals(".evol")) {
					// TODO: Validate file contents
					EvolutionSaver.SaveSimulationFile(file.Name, file.ToUTF8String());
				}
			}
		};
	}

	public void ShowUI() {
		RefreshCache();
		viewController.Show(this);
	}

	// MARK: - FileSelectionViewControllerDelegate

	public string GetTitle(FileSelectionViewController controller) {
		return "Choose a saved simulation";
	}

	public string GetEmptyMessage(FileSelectionViewController controller) {
		return "You haven't saved any simulations yet";
	}

	public int GetNumberOfItems(FileSelectionViewController controller) {
		return filenames.Count;
	}

	public string GetTitleForItemAtIndex(FileSelectionViewController controller,
										 int index) {
		return RENAME_REGEX.Replace(filenames[index], "");
	}
	public int GetIndexOfSelectedItem(FileSelectionViewController controller) {
		return selectedIndex;
	}

	public void DidSelectItem(FileSelectionViewController controller, int index) {
		selectedIndex = index;
	}

	public void DidEditTitleAtIndex(FileSelectionViewController controller, int index) {
		// TODO: Rename file & check if filename is available
		RefreshCache();
	}

	public void LoadButtonClicked(FileSelectionViewController controller) {

		var filename = filenames[selectedIndex];
		StartCoroutine(LoadOnNextFrame(filename));
	}

	private IEnumerator LoadOnNextFrame(string filename) {

		yield return new WaitForEndOfFrame();

		EvolutionSaver.LoadSimulationFromSaveFile(filename, creatureBuilder, evolution);
	}

	public void ImportButtonClicked(FileSelectionViewController controller) {

		SupportedFileType[] supportedFileTypes = {
			SupportedFileType.PlainText,
			CustomEvolutionFileType.evol
		};

		NativeFileSO.shared.OpenFiles(supportedFileTypes,
		  delegate (bool filesWereOpened, OpenedFile[] files) { 
			if (filesWereOpened) {
				foreach (OpenedFile file in files) {
					EvolutionSaver.SaveSimulationFile(file.Name, file.ToUTF8String());	
					RefreshCache();
					viewController.Refresh();
				}
			}
		});
	}

	public void ExportButtonClicked(FileSelectionViewController controller) {

		var filename = filenames[selectedIndex];
		string path = EvolutionSaver.GetSavePathForFile(filename);

		FileToSave file = new FileToSave(path, SupportedFileType.PlainText);

		NativeFileSO.shared.SaveFile(file);
	}

	public void DeleteButtonClicked(FileSelectionViewController controller) {
		var filename = filenames[selectedIndex];
		EvolutionSaver.DeleteSaveFile(filename);
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
		filenames = EvolutionSaver.GetEvolutionSaveFilenames();
	}
}