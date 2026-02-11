using Mirror;
using UnityEngine;

namespace PredictedCharacterController
{
    public sealed class MovementInterpolation : NetworkBehaviour
    {
        [SerializeField]
        private Transform visual;

        private Vector3 currentPosition, lastPosition;

        private void Awake()
        {
            if (isServerOnly)
                return;

            visual.SetParent(null);

            lastPosition = currentPosition = transform.position;
        }

        private void OnDestroy()
        {
            if (visual && !isServerOnly)
                Destroy(visual.gameObject);
        }

        private void FixedUpdate()
        {
            if (isServerOnly)
                return;

            lastPosition = currentPosition;
        }

        private void LateUpdate()
        {
            if (isServerOnly)
                return;

            currentPosition = transform.position;

            var time = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            time = Mathf.Clamp01(time);

            visual.position = Vector3.Lerp(lastPosition, currentPosition, time);
        }
    }
}