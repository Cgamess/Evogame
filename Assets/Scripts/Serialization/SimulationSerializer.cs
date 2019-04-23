﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The SimulationSerializer provides function for saving and loading the state of a simulation in / from a file.
/// 
/// The Evolution Save files have the following format (VERSION 2):
/// 
/// Content: 
/// 
/// v save format version (v 2)
/// -separator-
/// Encoded Evolution Settings
/// -separator-
/// Encoded Neural Network Settings
/// -separator-
/// CreatureSaveData - created by the CreatureSaver class
/// -separator-
/// A list of the best chromosomes (for each chromosome:  chromosome : (CreatureStats encoded))
/// -separator-
/// A list of the current chromosomes (The chromosomes of all creatures of the last simulating generation)
/// </summary>
public class SimulationSerializer {

	/// <summary>
	/// The name of the folder that holds the creature save files.
	/// </summary>
	private const string SAVE_FOLDER = "EvolutionSaves";

	private const string FILE_EXTENSION = ".evol";

	private static readonly Regex EXTENSION_PATTERN = new Regex(FILE_EXTENSION);

	private static string RESOURCE_PATH = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

	static SimulationSerializer() {
		MigrateSimulationSaves();
		CopyDefaultSimulations();
	}

	/// <summary>
	/// Saves the encoded simulation to a file with the specified name.
	/// </summary>
	/// <param name="name">The filename without an extension.</param>
	/// <param name="encodedData"></param>
	public static void SaveSimulationFile(string name, string encodedData, bool overwrite = false) { 

		name = EXTENSION_PATTERN.Replace(name, "");

		if (!overwrite) {
			name = GetAvailableSimulationName(name);
		}
		var path = PathToSimulationSave(name);

		CreateSaveFolder();
		File.WriteAllText(path, encodedData);
	}

	public static void SaveSimulation(SimulationData data) {

		string contents = data.Encode();
		string creatureName = data.CreatureDesign.Name;
		string dateString = System.DateTime.Now.ToString("MMM dd, yyyy");
		string taskString = EvolutionTaskUtil.StringRepresentation(data.Settings.Task);
		int generation = data.BestCreatures.Count + 1;
		string filename = string.Format("{0} - {1} - {2} - Gen({3})", creatureName, taskString, dateString, generation);

		// Save without overwriting existing saves
		SaveSimulationFile(filename, contents, false);
	}

	/// <summary>
	/// Saves the given information about an evolution simulation of a creature in a file, so that
	/// it can be loaded and continued at the same generation again.
	/// </summary>
	/// <remarks>
	/// The file is always written in the latest save version format.
	/// </remarks>
	/// <exception cref="IllegalFilenameException">Thrown if the filename contains 
	/// illegal characters</exception>
	/// <returns>The name of the saved file.</returns>
	// public static string WriteSaveFile(string creatureName, 
	// 								   SimulationSettings settings, 
	// 								   NeuralNetworkSettings networkSettings, 
	// 								   int generationNumber, 
	// 								   string creatureSaveData, 
	// 								   List<ChromosomeStats> bestChromosomes, 
	// 								   List<string> currentChromosomes) {

	// 	if (string.IsNullOrEmpty(creatureName)) 
	// 		throw new IllegalFilenameException();

	// 	var splitOptions = new SplitOptions();

	// 	var date = System.DateTime.Now.ToString("yyyy-MM-dd");

	// 	var name = string.Format("{0} - {1} - {2} - Gen({3})", creatureName, settings.Task, date, generationNumber);
	// 	name = GetAvailableSimulationName(name);

	// 	var stringBuilder = new StringBuilder();

	// 	// Add the version number
	// 	stringBuilder.AppendLine(string.Format("v {0}", version.ToString()));
	// 	stringBuilder.Append(splitOptions.COMPONENT_SEPARATOR);

	// 	// Add the encoded evolution settings
	// 	stringBuilder.AppendLine(settings.Encode());
	// 	stringBuilder.Append(splitOptions.COMPONENT_SEPARATOR);

	// 	// Add the encoded neural network settings
	// 	stringBuilder.AppendLine(networkSettings.Encode());
	// 	stringBuilder.Append(splitOptions.COMPONENT_SEPARATOR);

	// 	// Add the creature save data 
	// 	stringBuilder.AppendLine(creatureSaveData);
	// 	stringBuilder.Append(splitOptions.COMPONENT_SEPARATOR);

	// 	// Add the list of best chromosomes
	// 	foreach (var chromosome in bestChromosomes) {
	// 		stringBuilder.AppendLine(chromosome.ToString());
	// 	}
	// 	stringBuilder.Append(splitOptions.COMPONENT_SEPARATOR);

	// 	// Add the list of current chromosomes
	// 	foreach (var chromosome in currentChromosomes) {
	// 		stringBuilder.AppendLine(chromosome);
	// 	}
	// 	stringBuilder.Append(splitOptions.COMPONENT_SEPARATOR);

	// 	SaveSimulationFile(name, stringBuilder.ToString());

	// 	return name;
	// }

	/// <summary>
	/// Returns a simulation save filename that is still available based on the
	/// specified suggested name. (Both without extensions)
	/// </summary>
	private static string GetAvailableSimulationName(string suggestedName) {
		
		var existingNames = GetEvolutionSaveFilenames();
		int counter = 2;
		var finalName = suggestedName;
		while (existingNames.Contains(finalName)) {
			finalName = string.Format("{0} ({1})", suggestedName, counter);
			counter++;
		}
		return finalName;
	}

	/// <summary>
	/// Returns the SimulationData of a previously saved simulation.
	/// </summary>
	/// <param name="name">The name of the saved simulation without the file extension.</param>
	public static SimulationData ParseSimulationFromSaveFile(string name) {

		var path = PathToSimulationSave(name);
		var contents = File.ReadAllText(path);


	}

	/// <summary>
	/// Loads a previously saved simulation from an existing file and continues the
	/// evolution process.
	/// </summary>
	/// <param name="name">The name of the saved simulation without the file extension.</param>
	public static void LoadSimulationFromSaveFile(string name, CreatureEditor editor) {

		var path = PathToSimulationSave(name);
		var contents = File.ReadAllText(path);

		
			
		

		

		
	}

	/// <summary>
	/// Returns a list of filenames containing simulation save data. 
	/// </summary>
	/// <returns>The evolution save filenames.</returns>
	public static List<string> GetEvolutionSaveFilenames() {

		return FileUtil.GetFilenamesInDirectory(RESOURCE_PATH, FILE_EXTENSION);

		// CreateSaveFolder();

		// var info = new DirectoryInfo(RESOURCE_PATH);
		// var fileInfo = info.GetFiles();
		
		// var files = fileInfo.Where(f => f.Name.EndsWith(".evol")).ToList();

		// files.Sort((f1,f2) => f2.LastAccessTime.CompareTo(f1.LastAccessTime)); // Sort descending

		// return files.Select(f => EXTENSION_PATTERN.Replace(f.Name, "")).ToList();
	}

	/// <summary>
	/// Renames the creature design with the specified name (Without extension).
	/// Existing files are overwritten.
	/// </summary>
	public static void RenameSimulationSave(string oldName, string newName) {
		var oldPath = PathToSimulationSave(oldName);
		var newPath = PathToSimulationSave(newName);

		if (File.Exists(oldPath))
			File.Move(oldPath, newPath);
	}

	/// <summary>
	/// Returns true if a simulation save with the specified name (without extension) 
	/// already exists.
	/// </summary>
	/// <param name="name"></param>
	public static bool SimulationSaveExists(string name) {
		return GetEvolutionSaveFilenames().Contains(name);
	}

	/// <summary>
	/// Deletes the save file with the specified name. This can not be undone!
	/// </summary>
	/// <param name="filename">The name of the evolution save file to be deleted. 
	/// (Without an extension)</param>
	public static void DeleteSaveFile(string name) {

		var path = PathToSimulationSave(name);
		File.Delete(path);
	}

	/// <summary>
	/// Copies the default simulation files from the resources folder into the savefile directory
	/// </summary>
	private static void CopyDefaultSimulations() {

		CreateSaveFolder();

		var names = new [] {
			"FROGGER - RUNNING - Default - Gen(70).evol"	
		};

		foreach (var name in names) {

			var savePath = GetSavePathForFile(name);

			if (!System.IO.File.Exists(savePath)) {
				var loadPath = Path.Combine("DefaultSaves", name);
				var resFile = Resources.Load(loadPath) as TextAsset;

				File.WriteAllText(savePath, resFile.text);
			}
		}
	}

	/// <summary>
	/// Returns the path to a simulation save file with the specified name 
	/// (without an extension).
	/// </summary>
	public static string PathToSimulationSave(string name) {
		return Path.Combine(RESOURCE_PATH, string.Format("{0}.evol", name));
	}

	/// <summary>
	/// Returns the path to a simulation save file with the specified filename
	/// (including the extension).
	/// </summary>
	public static string GetSavePathForFile(string filename) { 
		return Path.Combine(RESOURCE_PATH, filename);
	}

	/// <summary>
	/// Creates the save location for the creature saves if it doesn't exist already.
	/// </summary>
	private static void CreateSaveFolder() {
		Directory.CreateDirectory(RESOURCE_PATH);
	}

	/// <summary>
	/// Updates the extension for all existing save files from .txt to .evol
	/// </summary>
	private static void MigrateSimulationSaves() {
		if (Settings.DidMigrateSimulationSaves) return;
		Debug.Log("Beginning simulation save migration.");

		var filenames = new DirectoryInfo(RESOURCE_PATH).GetFiles().Select(f => f.Name);
		var txtReplace = new Regex(".txt");
		foreach (var filename in filenames) {
			var newName = txtReplace.Replace(filename, ".evol");
			var oldPath = GetSavePathForFile(filename);
			var newPath = GetSavePathForFile(newName);

			if (File.Exists(oldPath))
				File.Move(oldPath, newPath);
		}

		Settings.DidMigrateSimulationSaves = true;
	}
}
