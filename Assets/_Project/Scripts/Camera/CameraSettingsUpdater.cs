using Fusion;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace rwby
{
    public class CameraSettingsUpdater : SimulationBehaviour
    {
        public Camera camera;

        public virtual void Start()
        {
            GameManager.singleton.settingsManager.OnSettingsSet += UpdateCameraSettings;
            UpdateCameraSettings();
        }

        private void OnDestroy()
        {
            GameManager.singleton.settingsManager.OnSettingsSet -= UpdateCameraSettings;
        }

        private void UpdateCameraSettings()
        {
            var settingsMenu = GameManager.singleton.settingsManager;

            var urpCam = camera.GetComponent<UniversalAdditionalCameraData>();
            urpCam.antialiasing = AntialiasingMode.None;
            switch (settingsMenu.Settings.antiAliasingPP)
            {
                case 1:
                    urpCam.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    break;
                case 2:
                    urpCam.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    urpCam.antialiasingQuality = AntialiasingQuality.High;
                    break;
            }
        }
    }
}