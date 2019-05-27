using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class DistanceMarkerSpawner: BaseStructure {

        private static readonly string ENCODING_ID = "evolution::structure::distancemarkerspawner";

        public float MarkerDistance { get; private set; }
        public float DistanceAngleFactor { get; private set; }

        static DistanceMarkerSpawner() {
            SimulationScene.RegisterStructure(ENCODING_ID, delegate(JObject json) {
                return Decode(json);
            });
        }

        public DistanceMarkerSpawner(Transform transform, float markerDistance = 5f, float angleFactor = 1f): base(transform) {
            this.MarkerDistance = markerDistance;
            this.DistanceAngleFactor = angleFactor;
        }

        public override string GetEncodingKey() {
            return ENCODING_ID;
        }

        private static class CodingKey {
            public const string MarkerDistance = "markerDistance";
            public const string AngleFactor = "distanceAngleFactor";
        }

        public override JObject Encode() {
            var json = base.Encode();
            json[CodingKey.MarkerDistance] = MarkerDistance;
            json[CodingKey.AngleFactor] = DistanceAngleFactor;
            return json;
        }

        public static DistanceMarkerSpawner Decode(JObject json) {
            var transform = BaseStructure.DecodeTransform(json);
            var markerDistance = json[CodingKey.MarkerDistance].ToObject<float>();
            var angleFactor = json[CodingKey.AngleFactor].ToObject<float>();
            return new DistanceMarkerSpawner(transform, markerDistance, angleFactor);
        }

        public override IStructureBuilder GetBuilder() {
            return new DistanceMarkerSpawnerBuilder(this);
        }

        public class DistanceMarkerSpawnerBuilder: BaseStructureBuilder<DistanceMarkerSpawner> {

            protected override string prefabPath => "Prefabs/Structures/DistanceMarkerSpawner";

            public DistanceMarkerSpawnerBuilder(DistanceMarkerSpawner spawner): base(spawner) {}

            public override GameObject Build(ISceneContext context) {
                var spawner = base.Build(context).GetComponent<Keiwando.Evolution.DistanceMarkerSpawner>();
                spawner.MarkerDistance = this.structure.MarkerDistance;
                spawner.DistanceAngleFactor = this.structure.DistanceAngleFactor;
                spawner.Context = context;
                spawner.gameObject.layer = context.GetBackgroundLayer();
                return spawner.gameObject;
            }
        }
    }
}