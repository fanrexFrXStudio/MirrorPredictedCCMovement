using Mirror;
using UnityEngine;

namespace PredictedCharacterController.New
{
    /// <summary>
    /// Very simple example script player movement
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(NetworkTransformBase))]
    public sealed class PlayerMovement : NetworkBehaviour
    {
        [SerializeField]
        private float moveSpeed = 5, jumpForce = 5, onGroundVerticalVelocity = -1, gravityScale = -10, reconcileThreshold = .1f;

        // buffer and verticalVelocity is different on different connections. We can write and read everything from one variable
        // old: MovementInputBuffer localBuffer = new(), serverBuffer = new(); float verticalVelocity, serverVerticalVelocity
        private CharacterController controller;
        private readonly MovementInputBuffer buffer = new();

        private uint localTick;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        public override void OnStartLocalPlayer()
        {
            // Local player dont need in NetTransform
            GetComponent<NetworkTransformBase>().enabled = false;
        }

        private void FixedUpdate()
        {
            if (isOwned)
                Prediction();

            if (isServerOnly)
                ServerFixedUpdate();
        }

        private void Prediction()
        {
            var input = GetMovementInput();

            Simulate(input);

            // if its host, just return
            if (isServer)
                return;

            var replicate = new MovementReplicateData(input, new(transform.position, verticalVelocity, localTick));

            buffer.Write(replicate);
            CmdBufferWriteReplicate(new(input, localTick));

            localTick++;
        }

        // One simulate on all connections, just by input
        private void Simulate(MovementInput input)
        {
            var moveInput = new Vector3(input.MoveInput.x, 0, input.MoveInput.y);

            moveInput = Vector3.ClampMagnitude(moveInput, 1f);

            if (controller.isGrounded)
                verticalVelocity = input.JumpPerformed ? jumpForce : onGroundVerticalVelocity;
            else
                verticalVelocity += gravityScale * Time.fixedDeltaTime;

            var moveDirection = transform.TransformDirection(moveInput) * moveSpeed;

            moveDirection.y = verticalVelocity;
            controller.Move(moveDirection * Time.fixedDeltaTime);
        }

        [Command]
        private void CmdBufferWriteReplicate(MovementReplicateNetData netData) =>
            buffer.Write(new(netData.Input, new(default, default, netData.Tick)));

        [Server]
        private void ServerFixedUpdate()
        {
            if (buffer.Count == 0)
                return;

            var replicate = buffer.Get(0);
            buffer.ClearOlder(0);

            Simulate(replicate.Input);
            TargetReconcile(new(transform.position, verticalVelocity, replicate.Predicted.Tick));
        }

        [TargetRpc]
        private void TargetReconcile(MovementReconcileData reconcile)
        {
            var inputIndex = buffer.GetIndex(reconcile.Tick);

            if (inputIndex == -1)
                return;

            var replicate = buffer.Get(inputIndex);
            var unsyncDistance = Vector3.Distance(reconcile.Position, replicate.Predicted.Position);

            buffer.ClearOlder(inputIndex);

            if (unsyncDistance <= reconcileThreshold)
                return;

            Debug.LogWarning($"Reconcile! Error: {unsyncDistance}");

            var enabled = controller.enabled;

            controller.enabled = false;
            transform.position = reconcile.Position;
            controller.enabled = enabled;

            verticalVelocity = reconcile.VerticalVelocity;

            // Replay all inputs
            for (var i = 0; i < buffer.Count; i++)
            {
                Simulate(buffer.Get(i).Input);
                buffer.UpdateLastPosition(i, transform.position);
            }
        }

        private MovementInput GetMovementInput() =>
            new(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), Input.GetKeyDown(KeyCode.Space));
    }
}