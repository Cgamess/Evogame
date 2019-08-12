using UnityEngine;

namespace Keiwando.Evolution {

    public class UniversalBrain: Brain {

        public override int NumberOfInputs => 11;
        public override int NumberOfOutputs {
            get { return base.NumberOfOutputs + 1; }
        }

        private float customRaycastAngle = 0f;

        /// <summary>
        /// Inputs:
        /// 
        /// - Raycast Distances:
        ///     - Distance to ground
        ///     - Forward
        ///     - Down-Forward
        ///     - Down-Back
        ///     - Back
        ///     - Custom
        /// - dX velocity
        /// - dY velocity
        /// - angular velocity
        /// - number of points touching ground
        /// - creature rotation
        /// </summary>
        protected override void UpdateInputs() {

            var basicInputs = creature.CalculateBasicBrainInputs();
            
            var center = new Vector3(creature.GetXPosition(), creature.GetYPosition());

            var forward = creature.RaycastDistance(center, new Vector3(1, 0));
            var downForward = creature.RaycastDistance(center, new Vector3(1, -1));
            var downBack = creature.RaycastDistance(center, new Vector3(-1, -1));
            var back = creature.RaycastDistance(center, new Vector3(-1, 0));

            var direction = new Vector3(Mathf.Cos(customRaycastAngle), Mathf.Sin(customRaycastAngle));
            // Debug.DrawRay(center, direction * 5, Color.magenta);
            var custom = creature.RaycastDistance(center, direction);

            var inputs = Network.Inputs;
            inputs[0] = creature.DistanceFromGround();
            inputs[1] = forward;
            inputs[2] = downForward;
            inputs[3] = downBack;
            inputs[4] = back;
            inputs[5] = custom;

            inputs[6] = basicInputs.VelocityX;
            inputs[7] = basicInputs.VelocityY;
            inputs[8] = basicInputs.AngularVelocity;
            inputs[9] = basicInputs.PointsTouchingGroundCount;
            inputs[10] = basicInputs.Rotation;
        }

        protected override void ApplyOutputs(float[] outputs) {
            base.ApplyOutputs(outputs);

            customRaycastAngle = outputs[outputs.Length - 1] * 2 * Mathf.PI;
        }
    }
}