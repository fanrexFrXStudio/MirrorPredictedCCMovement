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

        [SerializeField]
        private bool detachVisual;

        private PlayerPredictedMovement movement;

        private void Awake()
        {
            movement = GetComponent<PlayerPredictedMovement>();

            if (!isServerOnly && detachVisual)
                visual.SetParent(null);
        }

        private void OnDestroy()
        {
            if (!isServerOnly && detachVisual)
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
            if (detachVisual)
            {
                // visual отделен от игрока — работаем в world space (оригинальная логика)
                float distance = Vector3.Distance(visual.position, transform.position);
                if (distance > teleportThreshold)
                {
                    visual.position = transform.position;
                    return;
                }
                float step = (interpolation + (distance * multiple)) * Time.deltaTime;
                visual.position = Vector3.MoveTowards(visual.position, transform.position, step);
            }
            else
            {
                // visual прикреплен как child к transform — работаем в local space
                // при нулевом localPosition визуал точно совпадает с parent
                float distance = visual.localPosition.magnitude;
                if (distance > teleportThreshold)
                {
                    visual.localPosition = Vector3.zero;
                    return;
                }
                float step = (interpolation + (distance * multiple)) * Time.deltaTime;
                visual.localPosition = Vector3.MoveTowards(visual.localPosition, Vector3.zero, step);
            }
        }
    }
}