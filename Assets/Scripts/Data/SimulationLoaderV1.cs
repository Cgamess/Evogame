using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Evolution Save files have the following format (VERSION 1):
/// Filename: CreatureName - Date - Generation
/// 
/// Content: 
/// 
/// simulation task
/// -separator-
/// time per generation
/// -separator-
/// CreatureSaveData - created by the CreatureSaver class
/// -separator-
/// A list of the best chromosomes (for each chromosome:  chromosome : fitness)
/// -separator-
/// A list of the current chromosomes (The chromosomes of all creatures of the last simulating generation)
/// </summary>
public class SimulationLoaderV1 {

    /// <summary>
	/// Loads the simulation from save file of format version 1.
	/// </summary>
	/// <param name="name">The name of the simualtion save.</param>
	/// <param name="content">The Content of the save file.</param>
	public static void LoadSimulationFromSaveFile(string name, string content, SimulationSerializer.SplitOptions splitOptions, CreatureEditor editor) { 

		var creatureName = name.Split('-')[0].Replace(" ", "");
		if (string.IsNullOrEmpty(creatureName))
			creatureName = "Unnamed";

		var components = content.Split(splitOptions.SPLIT_ARRAY, System.StringSplitOptions.None);

		// extract the save data from the file contents.
		var taskType = EvolutionTaskUtil.TaskForNumber(int.Parse(components[0].Replace(Environment.NewLine, "")));

		var timePerGen = int.Parse(components[1].Replace(Environment.NewLine, ""));

		var creatureData = components[2];
		// CreatureSaver.LoadCreatureFromContents(creatureData, creatureBuilder);
		// TODO: Replace this with actual design parsed from the save file
		var creatureDesign = new CreatureDesign("Unnamed", new List<JointData>(), new List<BoneData>(), new List<MuscleData>());

		var bestChromosomesData = new List<string>(components[3].Split(splitOptions.NEWLINE_SPLIT, StringSplitOptions.None));
		var bestChromosomes = new List<ChromosomeData>();

		foreach (var chromosomeData in bestChromosomesData) {

			if (chromosomeData != "") {

				var chromosomeInfo = ChromosomeInfo.FromString(chromosomeData);
				var chromosomeStats = new ChromosomeStats(chromosomeInfo.chromosome, new CreatureStats());
				chromosomeStats.stats.fitness = chromosomeInfo.fitness;
				var data = new ChromosomeData(chromosomeInfo.chromosome, chromosomeStats.stats);
				bestChromosomes.Add(data);
			}
		}

		var chromosomeComponents = components[4].Split(splitOptions.NEWLINE_SPLIT, StringSplitOptions.None);
		var currentChromosomes = new List<string>();

		foreach (var chromosome in chromosomeComponents) {

			if (chromosome != "") {
				currentChromosomes.Add(chromosome);
			}
		}

		var currentGeneration = bestChromosomes.Count + 1;

		var settings = new SimulationSettings();
		settings.task = taskType;
		settings.simulationTime = timePerGen;
		settings.populationSize = currentChromosomes.Count;

		var networkSettings = new NeuralNetworkSettings();
		

		var simulationData = new SimulationData(settings, networkSettings, creatureDesign, 
												bestChromosomes, currentChromosomes.ToArray());

		editor.StartSimulation(simulationData);
	}
}