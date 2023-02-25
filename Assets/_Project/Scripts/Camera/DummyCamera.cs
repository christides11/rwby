using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Fusion;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace rwby
{
    [OrderAfter(typeof(KinematicCharacterMotor))]
    public class DummyCamera : SimulationBehaviour
    {
        public Camera camera;
        public CinemachineBrain brain;
        public CamHandleCutout cutoutHandler;

        public virtual void Initialize()
        {
            Runner.AddSimulationBehaviour(cutoutHandler, null);
            GameManager.singleton.settingsManager.OnSettingsSet += UpdateCameraSettings;
            UpdateCameraSettings();
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