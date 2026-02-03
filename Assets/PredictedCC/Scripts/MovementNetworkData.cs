using UnityEngine;

namespace PredictedCharacterController
{
    public readonly struct MovementReplicateData
    {
        public readonly MovementInput Input;
        public readonly MovementReconcileData Predicted;

        public MovementReplicateData(MovementInput input, MovementReconcileData predicted)
        {
            Input = input;
            Predicted = predicted;
        }
    }

    public readonly struct MovementReconcileData
    {
        public readonly Vector3 Position;
        public readonly float VerticalVelocity;

        public readonly uint Tick;

        public MovementReconcileData(Vector3 position, float verticalVelocity, uint tick)
        {
            Position = position;
            VerticalVelocity = verticalVelocity;
            Tick = tick;
        }
    }
}