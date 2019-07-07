using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Keiwando.Evolution.Scenes;

namespace Keiwando.Evolution {

    [Serializable]
    public class SimulationData {

        public int Version { get; private set; } = 3;

        public SimulationSettings Settings { get; set; }
        public NeuralNetworkSettings NetworkSettings { get; set; }
        public CreatureDesign CreatureDesign { get; set; }
        public SimulationScene SceneDescription { get; set; }

        public List<ChromosomeData> BestCreatures { get; set; }
        public string[] CurrentChromosomes { get; set; }

        public readonly int LastV2SimulatedGeneration;

        public SimulationData(
            SimulationSettings settings, 
            NeuralNetworkSettings networkSettings, 
            CreatureDesign design, 
            SimulationScene sceneDescription
        ) {
            this.Settings = settings;
            this.NetworkSettings = networkSettings;
            this.CreatureDesign = design;
            this.SceneDescription = sceneDescription;
            this.BestCreatures = new List<ChromosomeData>();
            this.CurrentChromosomes = new string[0];
            this.LastV2SimulatedGeneration = 0;
        }

        public SimulationData(
            SimulationSettings settings, 
            NeuralNetworkSettings networkSettings, 
            CreatureDesign design,
            SimulationScene sceneDescription,
            List<ChromosomeData> bestCreatures, 
            string[] currentChromosomes,
            int lastV2SimulatedGeneration = 0
        ): this(settings, networkSettings, design, sceneDescription) {
            this.BestCreatures = bestCreatures;
            this.CurrentChromosomes = currentChromosomes;
            this.LastV2SimulatedGeneration = lastV2SimulatedGeneration;
        }

        #region Encode & Decode

        private static class CodingKey {
            public const string Version = "version";
            public const string Settings = "simulationSettings";
            public const string NetworkSettings = "networkSettings";
            public const string CreatureDesign = "creatureDesign";
            public const string SceneDescription = "scene";
            public const string BestCreatures = "bestCreatures";
            public const string CurrentChromosomes = "currentChromosomes";
            public const string LastV2SimulatedGeneration = "lastV2SimulationGeneration";
        }

        public JObject Encode() {

            JObject json = new JObject();
            
            json[CodingKey.Version] = this.Version;
            json[CodingKey.Settings] = this.Settings.Encode();
            json[CodingKey.NetworkSettings] = this.NetworkSettings.Encode();
            json[CodingKey.CreatureDesign] = this.CreatureDesign.Encode();
            json[CodingKey.SceneDescription] = this.SceneDescription.Encode();
            json[CodingKey.BestCreatures] = JArray.FromObject(
                this.BestCreatures.Select(chromosome => chromosome.Encode()).ToList()
            );
            json[CodingKey.CurrentChromosomes] = JArray.FromObject(this.CurrentChromosomes);
            json[CodingKey.LastV2SimulatedGeneration] = this.LastV2SimulatedGeneration;

            return json;
        }

        public static SimulationData Decode(string encoded) {
            return Decode(JObject.Parse(encoded));   
        }

        public static SimulationData Decode(JObject json) {

            return new SimulationData(
                SimulationSettings.Decode((JObject)json[CodingKey.Settings]),
                NeuralNetworkSettings.Decode((JObject)json[CodingKey.NetworkSettings]),
                CreatureDesign.Decode((JObject)json[CodingKey.CreatureDesign]),
                SimulationScene.Decode((JObject)json[CodingKey.SceneDescription]),
                (json[CodingKey.BestCreatures] as JArray).Select(encoded => ChromosomeData.Decode(encoded as JObject)).ToList(),
                json[CodingKey.CurrentChromosomes].ToObject<string[]>(),
                json[CodingKey.LastV2SimulatedGeneration].ToObject<int>()
            );
        }

        #endregion
    }
}