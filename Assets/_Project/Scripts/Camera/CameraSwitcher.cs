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

        public void Disable()
        {
            if (currentCamera == -1) return;
            playerCameras[currentCamera].Deactivate();
            currentCamera = -1;
        }

        public void SwitchTo(int id)
        {
            if(currentCamera != -1) playerCameras[currentCamera].Deactivate();
            currentCamera = id;
            if(currentCamera != -1) playerCameras[currentCamera].Activate();
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