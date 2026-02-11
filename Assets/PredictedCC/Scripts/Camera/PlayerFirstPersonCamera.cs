using Mirror;
using UnityEngine;

namespace PredictedCamera
{
    public sealed class PlayerFirstPersonCamera : NetworkBehaviour
    {
        [SerializeField]
        private float sensivity, clamp;

        [SerializeField]
        private Transform cameraPoint;

        public bool CanLook;

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        public override void OnStartLocalPlayer() => UpdateCameraState(true);
        public override void OnStopLocalPlayer() => UpdateCameraState(false);

        private void UpdateCameraState(bool link)
        {
            if (!Camera.main)
                return;

            if (link)
            {
                Camera.main.transform.SetPositionAndRotation(cameraPoint.position, cameraPoint.rotation);
                Camera.main.transform.SetParent(cameraPoint);

                return;
            }

            Camera.main.transform.SetParent(null);
        }


        private void Update()
        {
            if (isOwned)
                Simulate(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        }

        private void Simulate(float mouseX, float mouseY)
        {
            if (!isOwned || !CanLook)
                return;

            mouseX *= sensivity;
            mouseY *= sensivity;

            Yaw += mouseX;
            Pitch -= mouseY;

            Pitch = Mathf.Clamp(Pitch, -clamp, clamp);

            cameraPoint.localEulerAngles = new(Pitch, Yaw, 0f);
        }
    }
}
