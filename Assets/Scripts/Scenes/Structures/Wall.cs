using Keiwando.JSON;

using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    [RegisterInScene(ENCODING_ID)]
    public class Wall: BaseStructure {

        private const string ENCODING_ID = "evolution::structure::wall";

        public Wall(Transform transform): base(transform) {}

        public override string GetEncodingKey() {
            return ENCODING_ID;
        }

        public static Wall Decode(JObject json) {
            var transform = BaseStructure.DecodeTransform(json);
            return new Wall(transform);
        }

        public override IStructureBuilder GetBuilder() {
            return new WallBuilder(this);
        }

        public class WallBuilder: BaseStructureBuilder<Wall> {

            protected override string prefabPath => "Prefabs/Structures/Wall";

            public WallBuilder(Wall wall): base(wall) {}
        }
    }
}