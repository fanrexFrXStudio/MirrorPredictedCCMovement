namespace PredictedAbstractCharacterController
{
    public struct HumanMovementInput : IMovementInput<HumanMovementState>
    {
        public MovementBaseInput SERIALIZE_Input;
        public HumanMovementState SERIALIZE_State;

        public readonly MovementBaseInput GetBaseInput() => SERIALIZE_Input;
        public void SetBaseInput(MovementBaseInput input) => SERIALIZE_Input = input;

        public readonly HumanMovementState GetPredictedState() => SERIALIZE_State;
        public void SetPredictedState(HumanMovementState state) => SERIALIZE_State = state;
    }

    public struct HumanMovementState : IMovementState
    {
        public MovementBaseState SERIALIZE_State;

        public readonly MovementBaseState GetBaseState() => SERIALIZE_State;
        public void SetBaseState(MovementBaseState state) => SERIALIZE_State = state;
    }
}
