using Mirror;
using PredictedCamera;
using System.Collections.Generic;
using UnityEngine;

namespace PredictedAbstractCharacterController
{
    public class MovementInputBuffer<TInput, TState>
        where TInput : IMovementInput<TState>
    {
        public int Count => inputsBuffer.Count;

        private readonly List<TInput> inputsBuffer = new();
        private const ushort maxBufferSize = 1024;

        public void Write(TInput replicate)
        {
            inputsBuffer.Add(replicate);

            if (inputsBuffer.Count > maxBufferSize)
                inputsBuffer.RemoveAt(0);
        }

        public void Update(int index, TInput input) => inputsBuffer[index] = input;

        public void ClearOlder(int index) => inputsBuffer.RemoveRange(0, index + 1);

        public TInput Get(int index) => inputsBuffer[index];
        public int GetIndex(uint tick)
        {
            for (var i = 0; i < inputsBuffer.Count; i++)
            {
                if (inputsBuffer[i].GetBaseInput().Tick == tick)
                    return i;
            }

            return -1;
        }
    }

    [RequireComponent(typeof(CharacterController))]
    public abstract class MovementBase<TInput, TState> : NetworkBehaviour
        where TInput : struct, IMovementInput<TState>
        where TState : struct, IMovementState
    {
        #region Var

        [SerializeField]
        private Transform orientation;

        [SerializeField]
        private PlayerFirstPersonCamera playerCamera;

        [SerializeField]
        private bool canJump = true, useGravity = true;

        [Range(1, 50), SerializeField]
        private float jumpForce = 5, gravity = 10;

        [Space, Range(.01f, .5f), SerializeField]
        private float reconcileThreshold = .05f;

        private const float onGroundVerticalVelocity = -1;

        protected readonly MovementInputBuffer<TInput, TState> buffer = new();

        protected CharacterController controller;
        protected uint localTick;
        protected float verticalVelocity;

        #endregion

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        protected abstract float GetSpeed(TInput input);

        protected abstract TInput GetInput();
        protected abstract TState GetState();

        protected virtual void ApplyState(TState state)
        {
            transform.position = state.GetBaseState().Position;
            verticalVelocity = state.GetBaseState().VerticalVelocity;
        }

        protected MovementBaseInput GetBaseInput() => new()
        {
            JumpPerformed = Input.GetKey(KeyCode.Space),
            Move = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
            Yaw = playerCamera.Yaw
        };

        protected MovementBaseState GetBaseState() => new()
        {
            Position = transform.position,
            VerticalVelocity = verticalVelocity,
        };

        protected abstract void CallCmdWriteInput(TInput input);
        protected abstract void CallTargetReconcile(TState state);

        protected void CmdWriteInput(TInput input) => buffer.Write(input);

        protected virtual void Prediction()
        {
            var input = GetInput();
            var baseInput = input.GetBaseInput();

            baseInput.Tick = localTick;
            input.SetBaseInput(baseInput);

            Simulate(input);

            if (isServer)
                return;

            input.SetPredictedState(GetState());
            buffer.Write(input);
            CallCmdWriteInput(input);

            localTick++;
        }

        protected virtual void ServerFixedUpdate()
        {
            if (buffer.Count == 0)
                return;

            var input = buffer.Get(0);
            buffer.ClearOlder(0);

            Simulate(input);

            var state = GetState();
            var baseState = state.GetBaseState();
            baseState.Tick = input.GetBaseInput().Tick;

            state.SetBaseState(baseState);
            CallTargetReconcile(state);
        }

        protected virtual void Simulate(TInput input)
        {
            var baseInput = input.GetBaseInput();
            var moveDirection = GetDirection(baseInput.Move, baseInput.Yaw) * GetSpeed(input);

            if (controller.isGrounded)
                verticalVelocity = (baseInput.JumpPerformed && canJump) ? jumpForce : onGroundVerticalVelocity;
            else if (useGravity)
                verticalVelocity -= gravity * Time.fixedDeltaTime;

            moveDirection.y = verticalVelocity;
            controller.Move(moveDirection * Time.fixedDeltaTime);
        }

        protected virtual void TargetReconcile(TState serverState)
        {
            var serverBaseState = serverState.GetBaseState();
            var index = buffer.GetIndex(serverBaseState.Tick);

            if (index == -1)
                return;

            var inputAtTick = buffer.Get(index);
            var predictedBaseState = inputAtTick.GetPredictedState().GetBaseState();

            var errorDistance = Vector3.Distance(serverBaseState.Position, predictedBaseState.Position);

            buffer.ClearOlder(index);

            if (errorDistance <= reconcileThreshold)
                return;

            Debug.LogWarning($"[MovementBase:] Reconcile: {errorDistance:F4} at Tick: {serverBaseState.Tick}");

            var wasEnabled = controller.enabled;
            controller.enabled = false;

            ApplyState(serverState);
            verticalVelocity = serverBaseState.VerticalVelocity;

            controller.enabled = wasEnabled;

            for (int i = 0; i < buffer.Count; i++)
            {
                var input = buffer.Get(i);

                Simulate(input);

                input.SetPredictedState(GetState());
                buffer.Update(i, input);
            }
        }

        protected virtual void FixedUpdate()
        {
            if (isOwned)
                Prediction();

            if (isServerOnly)
                ServerFixedUpdate();
        }

        protected virtual Vector3 GetDirection(Vector2 moveInput, float yaw)
        {
            var rotation = Quaternion.Euler(0, yaw, 0);

            var forward = rotation * Vector3.forward;
            var right = rotation * Vector3.right;

            return (forward * moveInput.y + right * moveInput.x).normalized;
        }
    }
}
