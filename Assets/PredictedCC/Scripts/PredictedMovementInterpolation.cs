using Mirror;
using UnityEngine;

namespace PredictedCharacterController.New
{
    [RequireComponent(typeof(PlayerPredictedMovement))]
    public sealed class PredictedMovementInterpolation : NetworkBehaviour
    {
        [SerializeField]
        private bool enableOwnerInterpolation = true, enableObserverInterpolation = true;

        // multiple - множитель, чем больше рассинхрон тем более резче будет интерполяция
        [Range(.1f, 50), SerializeField]
        private float ownerInterpolation = 1, ownerInterpolationMultiple = 2, ownerTeleportThreshold = 2;

        [Space, Range(.1f, 50), SerializeField]
        private float observerInterpolation = 1, observerInterpolationMultiple = 2, observerTeleportThreshold = 2;

        [Space, SerializeField]
        private Transform visual;

        private void Awake()
        {
            if (!isServerOnly)
                visual.SetParent(null);
        }

        private void OnDestroy()
        {
            if (!isServerOnly)
                Destroy(visual.gameObject);
        }

        private void LateUpdate()
        {
            if (isServerOnly)
                return;

            if (isOwned)
            {
                if (enableOwnerInterpolation)
                    Interpolate(ownerInterpolation, ownerInterpolationMultiple, ownerTeleportThreshold);
            }
            else
            {
                if (enableObserverInterpolation)
                    Interpolate(observerInterpolation, observerInterpolationMultiple, observerTeleportThreshold);
            }
        }

        private void Interpolate(float interpolation, float multiple, float teleportThreshold)
        {
            InterpolatePosition(interpolation, multiple, teleportThreshold);
        }

        private void InterpolatePosition(float interpolation, float multiple, float teleportThreshold)
        {
            var distance = Vector3.Distance(visual.position, transform.position);

            if (distance > teleportThreshold)
            {
                visual.position = transform.position;
                return;
            }

            var step = (interpolation + (distance * multiple)) * Time.deltaTime;
            visual.position = Vector3.MoveTowards(visual.position, transform.position, step);
        }
    }
}