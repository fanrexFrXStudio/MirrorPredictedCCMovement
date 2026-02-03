using Mirror;
using UnityEngine;

namespace PredictedCharacterController.New
{
    [RequireComponent(typeof(CharacterController), typeof(NetworkTransformBase))]
    public sealed class PlayerPredictedMovement : NetworkBehaviour
    {
        [SerializeField]
        private float moveSpeed = 5, jumpForce = 5, onGroundVerticalVelocity = -1, gravityScale = -10, reconcileThreshold = .1f;

        private CharacterController controller;
        private NetworkTransformBase networkTransform;
        private readonly MovementInputBuffer localBuffer = new();

        private readonly MovementInputBuffer serverBuffer = new();
        private uint localTick;
        private float verticalVelocity, serverVerticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        public override void OnStartLocalPlayer()
        {
            networkTransform = GetComponent<NetworkTransformBase>();
            networkTransform.enabled = false;
        }

        private void FixedUpdate()
        {
            var delta = Time.fixedDeltaTime;

            if (isOwned)
            {
                var input = GetMovementInput();

                Simulate(input, delta, ref verticalVelocity);

                if (isServer)
                    return;

                var newReplicate = new MovementReplicateData(input, new(transform.position, verticalVelocity, localTick));

                localBuffer.Write(newReplicate);
                localTick++;

                CmdBufferWriteReplicate(newReplicate);
            }

            if (isServerOnly)
                ServerTick(delta);
        }

        private void Simulate(MovementInput input, float delta, ref float vVelocity)
        {
            const float minMoveInputMagnitude = .01f;
            var moveInput = new Vector3(input.MoveInput.x, 0, input.MoveInput.y);

            moveInput = Vector3.ClampMagnitude(moveInput, 1f);

            if (controller.isGrounded)
                vVelocity = input.JumpPerformed ? jumpForce : onGroundVerticalVelocity;
            else
                vVelocity += gravityScale * delta;

            var moveDirection = moveInput.magnitude > minMoveInputMagnitude
                ? transform.TransformDirection(moveInput) * moveSpeed
                : Vector3.zero;

            moveDirection.y = vVelocity;
            controller.Move(moveDirection * delta);
        }

        [Command]
        private void CmdBufferWriteReplicate(MovementReplicateData replicate) => serverBuffer.Write(replicate);

        [Server]
        private void ServerTick(float delta)
        {
            if (serverBuffer.Count == 0)
                return;

            var replicate = serverBuffer.Get(0);
            serverBuffer.ClearOlder(0);

            Simulate(replicate.Input, delta, ref serverVerticalVelocity);
            TargetReconcile(new(transform.position, serverVerticalVelocity, replicate.Predicted.Tick));
        }

        [TargetRpc]
        private void TargetReconcile(MovementReconcileData reconcile)
        {
            var inputIndex = localBuffer.GetIndex(reconcile.Tick);

            if (inputIndex == -1)
                return;

            var replicate = localBuffer.Get(inputIndex);
            var unsyncDistance = Vector3.Distance(reconcile.Position, replicate.Predicted.Position);

            localBuffer.ClearOlder(inputIndex);

            if (unsyncDistance <= reconcileThreshold)
                return;

            Debug.LogWarning($"Reconcile! Error: {unsyncDistance}");

            var enabled = controller.enabled;

            controller.enabled = false;
            transform.position = reconcile.Position;
            controller.enabled = enabled;

            verticalVelocity = reconcile.VerticalVelocity;

            var delta = Time.fixedDeltaTime;

            for (var i = 0; i < localBuffer.Count; i++)
            {
                Simulate(localBuffer.Get(i).Input, delta, ref verticalVelocity);
                localBuffer.UpdateLastPosition(i, transform.position);
            }
        }

        private MovementInput GetMovementInput() =>
            new(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), Input.GetKeyDown(KeyCode.Space));
    }
}