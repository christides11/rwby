using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using NaughtyAttributes;

namespace rwby
{
    [OrderBefore(typeof(BaseCameraManager))]
    public class CameraSwitcher : SimulationBehaviour
    {
        public CameraInputManager cameraInputManager;
        [ReadOnly] public DummyCamera cam;
        [ReadOnly] public FighterManager target;

        public Dictionary<int, BaseCameraManager> playerCameras = new Dictionary<int, BaseCameraManager>();
        public int currentCamera;

        public void WhenCameraModeChanged(FighterManager fm)
        {
            SwitchTo(fm.cameraMode);
        }
        
        public void RegisterCamera(int id, BaseCameraManager camHandler)
        {
            camHandler.id = id;
            camHandler.cam = cam;
            playerCameras.Add(id, camHandler);
            camHandler.Deactivate();
        }

        public float lowStrength;
        public float mediumStrength;
        public float highStrength;
        public override void Render()
        {
            base.Render();

            if (target)
            {
                if (target.shakeDefinition.shakeStrength == CameraShakeStrength.None
                    || Runner.Tick > target.shakeDefinition.endFrame)
                {
                    ResetCameraShake();
                    return;
                }

                var currentValue = (Runner.Tick - target.shakeDefinition.startFrame);
                var targetValue = (target.shakeDefinition.endFrame - target.shakeDefinition.startFrame);

                var alpha = Runner.Simulation.StateAlpha;
                var t1 = currentValue / targetValue;
                var t2 = (currentValue + 1) / targetValue;

                var t = (t2 * alpha + t1 * (1.0f - alpha));
                switch (target.shakeDefinition.shakeStrength)
                {
                    case CameraShakeStrength.Low:
                        ShakeCamera(lowStrength, t);
                        break;
                    case CameraShakeStrength.Medium:
                        ShakeCamera(mediumStrength, t);
                        break;
                    case CameraShakeStrength.High:
                        ShakeCamera(highStrength, t);
                        break;
                }
            }
        }

        public virtual void ResetCameraShake()
        {
            for (int i = 0; i < playerCameras.Count; i++)
            {
                playerCameras[i].StopShaking();
            }
        }

        public virtual void ShakeCamera(float strength, float time)
        {
            for (int i = 0; i < playerCameras.Count; i++)
            {
                playerCameras[i].ShakeCamera(strength, time);
            }
        }

        public void Disable()
        {
            if (currentCamera == -1) return;
            playerCameras[currentCamera].Deactivate();
            currentCamera = -1;
        }

        public void SwitchTo(int id)
        {
            Vector2 lookDir = Vector2.zero;
            if (currentCamera != -1) lookDir = playerCameras[currentCamera].GetLookDirection();
            if(currentCamera != -1) playerCameras[currentCamera].Deactivate();
            currentCamera = id;
            if(currentCamera != -1) playerCameras[currentCamera].Activate();
            if(currentCamera != -1) playerCameras[currentCamera].SetLookDirection(lookDir);
        }

        public virtual void AssignControlTo(ClientManager clientManager, int playerID)
        {
            cameraInputManager.AssignControlTo(clientManager, playerID);
            for (int i = 0; i < playerCameras.Count; i++)
            {
                playerCameras[i].AssignControlTo(clientManager, playerID);
            }
        }

        public virtual void SetTarget(FighterManager fighterManager)
        {
            for (int i = 0; i < playerCameras.Count; i++)
            {
                playerCameras[i].SetTarget(fighterManager);
            }
            target = fighterManager;
        }
    }
}