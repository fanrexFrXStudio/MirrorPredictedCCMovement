using UnityEngine;

namespace PredictedAbstractCharacterController
{
    public interface IMovementInput<TState>
    {
        MovementBaseInput GetBaseInput();
        void SetBaseInput(MovementBaseInput input);

        TState GetPredictedState();
        void SetPredictedState(TState state);
    }

    public interface IMovementState
    {
        MovementBaseState GetBaseState();
        void SetBaseState(MovementBaseState state);
    }

    public struct MovementBaseInput
    {
        public Vector2 Move;
        public bool JumpPerformed;
        public float Yaw;

        public uint Tick;
    }

    public struct MovementBaseState
    {
        public Vector3 Position;
        public float VerticalVelocity;

        public uint Tick;
    }
}
