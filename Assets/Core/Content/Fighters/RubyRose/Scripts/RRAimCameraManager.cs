using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Fusion;
using Rewired;
using UnityEngine;

namespace rwby.core
{
    public class RRAimCameraManager : BaseCameraManager
    {
        public CinemachineVirtualCamera virtualCamera;
        public CinemachineInputProvider inputProvider;
        [NonSerialized] public CinemachineBrain cinemachineBrain;
        [NonSerialized] public CinemachinePOV virtualCameraPOV;
        [NonSerialized] public CinemachineShake virtualCameraShake;

        private CameraSwitcher switcher;
        private FighterManager followTarget;
        
        public override void Initialize(CameraSwitcher switcher)
        {
            this.switcher = switcher;
            cinemachineBrain = switcher.cam.GetComponent<CinemachineBrain>();
            
            virtualCameraPOV = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            virtualCameraShake = virtualCamera.GetComponent<CinemachineShake>();

            var t = virtualCamera.GetComponentsInChildren<FusionCinemachineCollider>();
            foreach (var v in t)
            {
                v.runner = Runner;
            }
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override void Render()
        {
            base.Render();
        }

        public virtual void Update()
        {
            
        }

        public override void AssignControlTo(ClientManager clientManager, int playerID)
        {
            base.AssignControlTo(clientManager, playerID);
        }

        public override void SetTarget(FighterManager fighterManager)
        {
            base.SetTarget(fighterManager);
            virtualCamera.Follow = fighterManager.transform;
            virtualCamera.LookAt = fighterManager.transform;
            followTarget = fighterManager;
        }
        
        
    }
}