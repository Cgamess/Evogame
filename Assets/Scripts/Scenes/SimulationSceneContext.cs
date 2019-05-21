using System;

namespace Keiwando.Evolution.Scenes {

    public class SimulationSceneContext: ISceneContext {

        private readonly SimulationData data;

        public SimulationSceneContext(SimulationData data) {
            this.data = data;
        }

        public CreatureStats GetStatsForBestOfGeneration(int generation) {
            if (generation < 0 || generation > data.BestCreatures.Count) {
                return null;
            }
            return data.BestCreatures[generation - 1].Stats;
        }

        public float GetDistanceOfBest(int generation) {
            var stats = GetStatsForBestOfGeneration(generation);
            if (stats == null) return float.NaN;
            switch (this.data.Settings.Task) {
            case EvolutionTask.Running: 
                return stats.horizontalDistanceTravelled;
            case EvolutionTask.Jumping:
            case EvolutionTask.ObstacleJump: 
            case EvolutionTask.Climbing:
                return stats.verticalDistanceTravelled;
            }
            return stats.horizontalDistanceTravelled;
        }

        public int GetCurrentGeneration() {
            return this.data.BestCreatures.Count + 1;
        }
    }
}