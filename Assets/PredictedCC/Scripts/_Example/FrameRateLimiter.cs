using Mirror;
using UnityEngine;

namespace PredictedCharacterController.Example
{
    public class FrameRateLimiter : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int minFPS = 10;
        [SerializeField] private int maxFPS = 600;

        private float currentFPS;
        private float deltaTime;

        private void Start()
        {
            if (Application.targetFrameRate <= 0)
                Application.targetFrameRate = 60;
        }

        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            currentFPS = 1.0f / deltaTime;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 250, 250, 150), GUI.skin.box);

            GUI.color = currentFPS < 30 ? Color.red : Color.green;
            GUILayout.Label($"Current FPS: {Mathf.Ceil(currentFPS)}");
            GUI.color = Color.white;

            GUILayout.Space(5);

            GUILayout.Label($"Target FPS Limit: {Application.targetFrameRate}");

            var target = (float)Application.targetFrameRate;
            target = GUILayout.HorizontalSlider(target, minFPS, maxFPS);

            if (Application.targetFrameRate != (int)target)
            {
                Application.targetFrameRate = (int)target;
                QualitySettings.vSyncCount = 0;
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("30"))
                Application.targetFrameRate = 30;

            if (GUILayout.Button("60"))
                Application.targetFrameRate = 60;

            if (GUILayout.Button("144"))
                Application.targetFrameRate = 144;

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}