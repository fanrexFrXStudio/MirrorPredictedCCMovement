using Mirror;
using UnityEngine;

namespace PredictedAbstractCharacterController
{
    public sealed class HumanMovement : MovementBase<HumanMovementInput, HumanMovementState>
    {
        [Header("Human"), SerializeField]
        private float speed = 5;

        [Command]
        protected override void CallCmdWriteInput(HumanMovementInput input) => CmdWriteInput(input);

        [TargetRpc]
        protected override void CallTargetReconcile(HumanMovementState state) => TargetReconcile(state);

        protected override HumanMovementInput GetInput()
        {
            var input = new HumanMovementInput();
            input.SetBaseInput(GetBaseInput());

            return input;
        }

        protected override HumanMovementState GetState()
        {
            var state = new HumanMovementState();
            state.SetBaseState(GetBaseState());

            return state;
        }

        protected override float GetSpeed(HumanMovementInput input)
        {
            return speed;
        }
    }
}
