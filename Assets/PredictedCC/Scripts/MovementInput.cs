using UnityEngine;

namespace PredictedCharacterController
{
    public readonly struct MovementInput
    {
        public readonly Vector2 MoveInput;
        public readonly bool JumpPerformed;

        public MovementInput(Vector2 move, bool jumpPerformed)
        {
            MoveInput = move;
            JumpPerformed = jumpPerformed;
        }
    }

    public readonly struct MovementReplicateNetData
    {
        public readonly MovementInput Input;
        public readonly uint Tick;

        public MovementReplicateNetData(MovementInput input, uint tick)
        {
            Input = input;
            Tick = tick;
        }
    }
}