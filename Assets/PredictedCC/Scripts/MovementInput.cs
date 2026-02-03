using System;
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
}
